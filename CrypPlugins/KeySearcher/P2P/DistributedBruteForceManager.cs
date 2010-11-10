using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Windows.Threading;
using Cryptool.P2P;
using Cryptool.PluginBase;
using KeySearcher.Helper;
using KeySearcher.KeyPattern;
using KeySearcher.P2P.Exceptions;
using KeySearcher.P2P.Helper;
using KeySearcher.P2P.Presentation;
using KeySearcher.P2P.Storage;
using KeySearcher.P2P.Tree;
using KeySearcherPresentation.Controls;
using System.Timers;

namespace KeySearcher.P2P
{
    internal class DistributedBruteForceManager
    {
        private readonly StorageKeyGenerator keyGenerator;
        private readonly KeySearcher keySearcher;
        private readonly KeySearcherSettings settings;
        private readonly KeyQualityHelper keyQualityHelper;
        private readonly P2PQuickWatchPresentation quickWatch;
        private readonly KeyPatternPool patternPool;
        private readonly StatusContainer status;
        internal readonly StatisticsGenerator StatisticsGenerator;
        internal readonly Stopwatch StopWatch;

        private KeyPoolTree keyPoolTree;

        public DistributedBruteForceManager(KeySearcher keySearcher, KeyPattern.KeyPattern keyPattern, KeySearcherSettings settings,
                                            KeyQualityHelper keyQualityHelper, P2PQuickWatchPresentation quickWatch)
        {
            this.keySearcher = keySearcher;
            this.settings = settings;
            this.keyQualityHelper = keyQualityHelper;
            this.quickWatch = quickWatch;

            // TODO when setting is still default (21), it is only displayed as 21 - but the settings-instance contains 0 for that key!
            if (settings.ChunkSize == 0)
            {
                settings.ChunkSize = 21;
            }

            StopWatch = new Stopwatch();
            status = new StatusContainer();
            status.IsCurrentProgressIndeterminate = true;

            keyGenerator = new StorageKeyGenerator(keySearcher, settings);
            patternPool = new KeyPatternPool(keyPattern, new BigInteger(Math.Pow(2, settings.ChunkSize)));
            StatisticsGenerator = new StatisticsGenerator(status, quickWatch, keySearcher, settings, this);
            quickWatch.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(UpdateStatusContainerInQuickWatch));
        }

        public void Execute()
        {
            status.CurrentOperation = "Initializing connection to the peer-to-peer system";
            new ConnectionHelper(keySearcher, settings).ValidateConnectionToPeerToPeerSystem();

            if (!P2PManager.IsConnected)
            {
                keySearcher.GuiLogMessage("Unable to use peer-to-peer system.", NotificationLevel.Error);
                status.CurrentOperation = "Unable to use peer-to-peer system";
                return;
            }

            status.CurrentOperation = "Initializing distributed key pool tree";
            keyPoolTree = new KeyPoolTree(patternPool, keySearcher, keyQualityHelper, keyGenerator, status, StatisticsGenerator);

            keySearcher.GuiLogMessage(
                "Total amount of patterns: " + patternPool.Length + ", each containing " + patternPool.PartSize +
                " keys.", NotificationLevel.Info);
            status.CurrentOperation = "Ready for calculation";

            status.StartDate = keyPoolTree.StartDate();
            //----------------------------------------------
            status.JobSubmitterID = keyPoolTree.SubmitterID();
            //-----------------------------------------------

            keyPoolTree.UpdateStatusForNewCalculation();

            Leaf currentLeaf;
            while (!keySearcher.stop)
            {
                status.IsCurrentProgressIndeterminate = true;

                BigInteger displayablePatternId;
                try
                {
                    status.CurrentOperation = "Finding next leaf to calculate";
                    currentLeaf = keyPoolTree.FindNextLeaf();
                    if (currentLeaf == null)
                    {
                        break;
                    }
                    displayablePatternId = currentLeaf.PatternId() + 1;
                }
                catch (AlreadyCalculatedException)
                {
                    keySearcher.GuiLogMessage("Node was already calculated.", NotificationLevel.Info);
                    keyPoolTree.Reset();
                    continue;
                }

                // TODO if reserve returns successfully, start timer to update our reserveration every few minutes
                // if we cannot reacquire our lock in the timer, calculation must be aborted
                if (!currentLeaf.ReserveLeaf())
                {
                    keySearcher.GuiLogMessage(
                        "Pattern #" + displayablePatternId +
                        " was reserved before it could be reserved for this CrypTool instance.",
                        NotificationLevel.Info);
                    keyPoolTree.Reset();
                    continue;
                }

                var reservationTimer = new Timer {Interval = 5*60*1000};    //Every 5 minutes
                reservationTimer.Elapsed += new ElapsedEventHandler(delegate
                                                                        {
                                                                            var oldMessage = status.CurrentOperation;
                                                                            var message = string.Format("Rereserving pattern #{0}", displayablePatternId);
                                                                            keySearcher.GuiLogMessage(message, NotificationLevel.Info);
                                                                            status.CurrentOperation = message;
                                                                            currentLeaf.ReserveLeaf();
                                                                            status.CurrentOperation = oldMessage;
                                                                        });

                keySearcher.GuiLogMessage(
                    "Running pattern #" + displayablePatternId + " of " + patternPool.Length,
                    NotificationLevel.Info);
                status.CurrentChunk = displayablePatternId;
                status.CurrentOperation = "Calculating pattern " + status.CurrentChunk;

                try
                {
                    LinkedList<KeySearcher.ValueKey> result;

                    status.IsCurrentProgressIndeterminate = false;
                    StopWatch.Start();
                    reservationTimer.Start();
                    try
                    {
                        result = keySearcher.BruteForceWithLocalSystem(patternPool[currentLeaf.PatternId()], true);
                    }
                    finally
                    {
                        reservationTimer.Stop();
                        reservationTimer.Dispose();
                        StopWatch.Stop();
                        status.IsCurrentProgressIndeterminate = true;
                    }

                    if (!keySearcher.stop)
                    {
                        status.CurrentOperation = "Processing results of calculation";
                        KeyPoolTree.ProcessCurrentPatternCalculationResult(currentLeaf, result);
                        StatisticsGenerator.ProcessPatternResults(result);

                        status.CurrentOperation = "Calculating global statistics";
                        StatisticsGenerator.CalculateGlobalStatistics(displayablePatternId);

                        status.LocalFinishedChunks++;
                        keySearcher.GuiLogMessage(
                            string.Format("Best match: {0} with {1}", result.First.Value.key, result.First.Value.value),
                            NotificationLevel.Info);

                        status.CurrentOperation = "Updating status in DHT";
                        keyPoolTree.UpdateStatus(currentLeaf);
                    }
                    else
                    {
                        keySearcher.GuiLogMessage("Brute force was stopped, not saving results...",
                                                  NotificationLevel.Info);
                        status.ProgressOfCurrentChunk = 0;
                        currentLeaf.GiveLeafFree();
                        var message = string.Format("Removed reservation of pattern #{0}", displayablePatternId);
                        keySearcher.GuiLogMessage(message, NotificationLevel.Info);
                        status.CurrentOperation = message;
                    }
                }
                catch (ReservationRemovedException)
                {
                    keySearcher.GuiLogMessage("Reservation removed by another node (while calculating). " +
                                              "To avoid a state in limbo, proceeding to first available leaf...",
                                              NotificationLevel.Info);
                    keyPoolTree.Reset();
                    continue;
                }
                catch (UpdateFailedException e)
                {
                    keySearcher.GuiLogMessage("Could not store results: " + e.Message, NotificationLevel.Info);
                    keyPoolTree.Reset();
                    continue;
                }

                // Push statistics to database
                status.CurrentOperation = "Pushing statistics to evaluation database";
                DatabaseStatistics.PushToDatabase(status, StopWatch.ElapsedMilliseconds, keyPoolTree.Identifier, settings, keySearcher);
            }

            // Set progress to 100%
            if (!keySearcher.stop && keyPoolTree.IsCalculationFinished())
            {
                keySearcher.showProgress(keySearcher.costList, 1, 1, 1);
                keySearcher.GuiLogMessage("Calculation complete.", NotificationLevel.Info);
                keyPoolTree.UpdateStatusForFinishedCalculation();
            }

            StatisticsGenerator.CalculationStopped();
            status.ProgressOfCurrentChunk = 0;
            status.IsSearchingForReservedNodes = false;
            status.IsCurrentProgressIndeterminate = false;
            status.CurrentOperation = "Idle";
            status.RemainingTimeTotal = new TimeSpan(0);
        }

        private void UpdateStatusContainerInQuickWatch()
        {
            quickWatch.DataContext = status;
            quickWatch.UpdateSettings(keySearcher, settings);
        }
    }
}