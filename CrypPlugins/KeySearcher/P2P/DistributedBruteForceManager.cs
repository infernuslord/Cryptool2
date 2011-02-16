﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Windows.Threading;
using Cryptool.P2P;
using Cryptool.P2P.Internal;
using Cryptool.PluginBase;
using KeySearcher.Helper;
using KeySearcher.KeyPattern;
using KeySearcher.P2P.Exceptions;
using KeySearcher.P2P.Helper;
using KeySearcher.P2P.Presentation;
using KeySearcher.P2P.Storage;
using KeySearcher.P2P.Tree;
using KeySearcherPresentation.Controls;
using KeySearcher.Properties;
using System.Timers;
using Timer = System.Timers.Timer;

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
        private AutoResetEvent systemJoinEvent = new AutoResetEvent(false);

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
            status = new StatusContainer(keySearcher);
            status.IsCurrentProgressIndeterminate = true;

            keyGenerator = new StorageKeyGenerator(keySearcher, settings);
            patternPool = new KeyPatternPool(keyPattern, new BigInteger(Math.Pow(2, settings.ChunkSize)));
            StatisticsGenerator = new StatisticsGenerator(status, quickWatch, keySearcher, settings, this);
            quickWatch.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(UpdateStatusContainerInQuickWatch));
        }

        public void Execute()
        {
            status.CurrentOperation = Resources.Initializing_connection_to_the_peer_to_peer_system;
            new ConnectionHelper(keySearcher, settings).ValidateConnectionToPeerToPeerSystem();

            if (!P2PManager.IsConnected)
            {
                keySearcher.GuiLogMessage(Resources.Unable_to_use_peer_to_peer_system_, NotificationLevel.Error);
                status.CurrentOperation = Resources.Unable_to_use_peer_to_peer_system_;
                return;
            }

            status.CurrentOperation = Resources.Initializing_distributed_key_pool_tree;
            InitializeTree();
            
            bool statupdate = false;
            Leaf currentLeaf;
            keySearcher.InitialiseInformationQuickwatch();
            int utime = settings.UpdateTime > 0 ? settings.UpdateTime : 30;
            var statisticTimer = new Timer { Interval = utime * 60 * 1000 };    //Update of the statistics after every 30 minutes
            statisticTimer.Start();

            while (!keySearcher.stop)
            {
                try
                {
                    if (statupdate)
                    {
                        statisticTimer.Stop();
                        statisticTimer.Dispose();
                        keyPoolTree.Reset();
                        keySearcher.ResetStatistics();
                        keySearcher.SetInitialized(false);
                        status.CurrentOperation = Resources.Updating_statistic;
                        InitializeTree();
                        statupdate = false;
                        keySearcher.InitialiseInformationQuickwatch();
                        statisticTimer = new Timer { Interval = utime * 60 * 1000 };
                        statisticTimer.Start();
                    }

                    status.IsCurrentProgressIndeterminate = true;

                    BigInteger displayablePatternId;
                    try
                    {
                        status.CurrentOperation = Resources.Finding_next_leaf_to_calculate;
                        currentLeaf = keyPoolTree.FindNextLeaf();
                        if (currentLeaf == null)
                        {
                            break;
                        }
                        displayablePatternId = currentLeaf.PatternId() + 1;
                    }
                    catch (AlreadyCalculatedException)
                    {
                        keySearcher.GuiLogMessage(Resources.Node_was_already_calculated_, NotificationLevel.Info);
                        keyPoolTree.Reset();
                        continue;
                    }
                    catch (KeySearcherStopException)  //Fullstopfunction
                    {
                        keySearcher.GuiLogMessage(Resources.Keysearcher_Fullstop__Please_Update_your_Version_, NotificationLevel.Debug);
                        status.CurrentOperation = Resources.PLEASE_UPDATE;
                        keyPoolTree.Reset();
                        keySearcher.Stop();
                        return;
                    }

                    // TODO if reserve returns successfully, start timer to update our reserveration every few minutes
                    // if we cannot reacquire our lock in the timer, calculation must be aborted
                    if (!currentLeaf.ReserveLeaf())
                    {
                        keySearcher.GuiLogMessage(
                            string.Format(Resources.Pattern___0__was_reserved_before_it_could_be_reserved_for_this_CrypTool_instance_, displayablePatternId),
                            NotificationLevel.Info);
                        keyPoolTree.Reset();
                        continue;
                    }

                    bool reservationRemoved = false;
                    var reservationTimer = new Timer { Interval = 18 * 60 * 1000 };    //Every 18 minutes
                    reservationTimer.Elapsed += new ElapsedEventHandler(delegate
                                                                            {
                                                                                var oldMessage = status.CurrentOperation;
                                                                                var message = string.Format(Resources.Rereserving_pattern___0_, displayablePatternId);
                                                                                keySearcher.GuiLogMessage(message, NotificationLevel.Info);
                                                                                status.CurrentOperation = message;
                                                                                try
                                                                                {
                                                                                    if (!currentLeaf.ReserveLeaf())
                                                                                        keySearcher.GuiLogMessage(Resources.Rereserving_pattern_failed_, NotificationLevel.Warning);

                                                                                    //if (!currentLeaf.ReserveLeaf())
                                                                                    //{
                                                                                    //    keySearcher.GuiLogMessage("Rereserving pattern failed! Skipping to next pattern!", 
                                                                                    //        NotificationLevel.Warning);
                                                                                    //    reservationRemoved = true;
                                                                                    //    keySearcher.stop = true;
                                                                                    //}
                                                                                }
                                                                                catch (Cryptool.P2P.Internal.NotConnectedException)
                                                                                {
                                                                                    keySearcher.GuiLogMessage(Resources.Rereserving_pattern_failed__because_there_is_no_connection_,
                                                                                            NotificationLevel.Warning);
                                                                                    //TODO: Register OnSystemJoined event to rereserve pattern immediately after reconnect
                                                                                }
                                                                                status.CurrentOperation = oldMessage;
                                                                            });

                    statisticTimer.Elapsed += new ElapsedEventHandler(delegate
                                                                          {
                                                                              if (!settings.DisableUpdate)
                                                                              {
                                                                                  statupdate = true;
                                                                              }
                                                                          });

                    keySearcher.GuiLogMessage(
                        string.Format(Resources.Running_pattern___0__of__1_, displayablePatternId, patternPool.Length),
                        NotificationLevel.Info);
                    status.CurrentChunk = displayablePatternId;
                    status.CurrentOperation = Resources.Calculating_pattern_ + status.CurrentChunk;

                    try
                    {
                        LinkedList<KeySearcher.ValueKey> result;

                        status.IsCurrentProgressIndeterminate = false;
                        StopWatch.Start();
                        reservationTimer.Start();
                        try
                        {
                            result = keySearcher.BruteForceWithLocalSystem(patternPool[currentLeaf.PatternId()], true);
                            if (reservationRemoved)
                            {
                                keySearcher.stop = false;
                                throw new ReservationRemovedException("");
                            }
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
                            if (!P2PManager.IsConnected)
                            {
                                status.CurrentOperation = Resources.Connection_lost__Waiting_for_reconnection_to_store_the_results_;
                                keySearcher.GuiLogMessage(status.CurrentOperation, NotificationLevel.Info);
                                do
                                {
                                    P2PManager.P2PBase.OnSystemJoined += P2PBase_OnSystemJoined;
                                    systemJoinEvent.WaitOne(1000);
                                } while (!P2PManager.IsConnected);
                            }
                            status.CurrentOperation = Resources.Processing_results_of_calculation;

                            String hostname = Cryptool.PluginBase.Miscellaneous.UniqueIdentifier.GetHostName();
                            Int64 hostid = Cryptool.PluginBase.Miscellaneous.UniqueIdentifier.GetID();
                            if (settings.UseExternalClient)
                            {
                                hostname = keySearcher.ExternalClientHostname;
                                hostid = keySearcher.ExternaClientId;
                            }
                            KeyPoolTree.ProcessCurrentPatternCalculationResult(currentLeaf, result, hostid, hostname);
                            StatisticsGenerator.ProcessPatternResults(result);

                            status.CurrentOperation = Resources.Calculating_global_statistics;
                            StatisticsGenerator.CalculateGlobalStatistics(displayablePatternId);

                            status.LocalFinishedChunks++;
                            keySearcher.GuiLogMessage(
                                string.Format(Resources.Best_match___0__with__1_, result.First.Value.key, result.First.Value.value),
                                NotificationLevel.Info);

                            status.CurrentOperation = Resources.Updating_status_in_DHT;
                            keyPoolTree.UpdateStatus(currentLeaf);
                        }
                        else
                        {
                            keySearcher.GuiLogMessage(Resources.Brute_force_was_stopped__not_saving_results___,
                                                      NotificationLevel.Info);
                            status.ProgressOfCurrentChunk = 0;
                            currentLeaf.GiveLeafFree();
                            var message = string.Format(Resources.Removed_reservation_of_pattern___0_, displayablePatternId);
                            keySearcher.GuiLogMessage(message, NotificationLevel.Info);
                            status.CurrentOperation = message;
                        }
                    }
                    catch (ReservationRemovedException)
                    {
                        keySearcher.GuiLogMessage(Resources.Reservation_removed_by_another_node__while_calculating___To_avoid_a_state_in_limbo__proceeding_to_first_available_leaf___,
                                                  NotificationLevel.Info);
                        keyPoolTree.Reset();
                        continue;
                    }
                    catch (UpdateFailedException e)
                    {
                        keySearcher.GuiLogMessage(Resources.Could_not_store_results__ + e.Message, NotificationLevel.Info);
                        keyPoolTree.Reset();
                        continue;
                    }
                    catch (KeySearcherStopException)  //Fullstopfunction
                    {
                        keySearcher.GuiLogMessage(Resources.Keysearcher_Fullstop__Please_Update_your_Version_, NotificationLevel.Debug);
                        status.CurrentOperation = Resources.PLEASE_UPDATE;
                        keyPoolTree.Reset();
                        keySearcher.Stop();
                        return;
                    }

                    // Push statistics to database
                    status.CurrentOperation = Resources.Pushing_statistics_to_evaluation_database;
                    DatabaseStatistics.PushToDatabase(status, StopWatch.ElapsedMilliseconds, keyPoolTree.Identifier, settings, keySearcher);
                }
                catch (NotConnectedException)
                {
                    status.CurrentOperation = "Connection lost. Waiting for reconnect...";
                    keySearcher.GuiLogMessage(status.CurrentOperation, NotificationLevel.Info);
                    do
                    {
                        P2PManager.P2PBase.OnSystemJoined += P2PBase_OnSystemJoined;
                        systemJoinEvent.WaitOne(1000);
                    } while (!P2PManager.IsConnected);
                }
                catch (InvalidOperationException)
                {
                    //do nothing
                }
            }

            // Set progress to 100%
            if (!keySearcher.stop && keyPoolTree.IsCalculationFinished())
            {
                keySearcher.showProgress(keySearcher.costList, 1, 1, 1);
                keySearcher.GuiLogMessage(Resources.Calculation_complete_, NotificationLevel.Info);
                keyPoolTree.UpdateStatusForFinishedCalculation();
            }

            StatisticsGenerator.CalculationStopped();
            status.ProgressOfCurrentChunk = 0;
            status.IsSearchingForReservedNodes = false;
            status.IsCurrentProgressIndeterminate = false;
            status.CurrentOperation = "Idle";
            statisticTimer.Stop();
            statisticTimer.Dispose();
            status.RemainingTimeTotal = new TimeSpan(0);
        }

        private int FindLocalPatterns()
        {
            //String myAvatar = "CrypTool2";
            String myAvatar = P2PSettings.Default.PeerName;
            long myID = Cryptool.PluginBase.Miscellaneous.UniqueIdentifier.GetID();
            Dictionary<string, Dictionary<long, Information>> myStats = keySearcher.GetStatistics();

            if(myStats.ContainsKey(myAvatar))
            {
                if(myStats[myAvatar].ContainsKey(myID))
                {
                    return myStats[myAvatar][myID].Count;
                }
            }
            return 0;
        }

        private void InitializeTree()
        {
            try
            {
                keyPoolTree = new KeyPoolTree(patternPool, keySearcher, keyQualityHelper, keyGenerator, status, StatisticsGenerator);
            }
            catch (KeySearcherStopException)
            {
                status.CurrentOperation = Resources.PLEASE_UPDATE;
                keySearcher.GuiLogMessage(Resources.Keysearcher_Fullstop__Please_Update_your_Version_, NotificationLevel.Error);
                keySearcher.Stop();
                throw new KeySearcherStopException();
            }


            keySearcher.GuiLogMessage(
                string.Format(Resources.Total_amount_of_patterns___0___each_containing__1__keys_, patternPool.Length, patternPool.PartSize), NotificationLevel.Info);
            status.CurrentOperation = Resources.Ready_for_calculation;

            status.StartDate = keyPoolTree.StartDate();
            keySearcher.SetBeginningDate(keyPoolTree.StartDate());
            status.JobSubmitterID = keyPoolTree.SubmitterID();
            status.LocalFinishedChunks = FindLocalPatterns();

            keyPoolTree.UpdateStatusForNewCalculation();
            keySearcher.SetInitialized(true);
        }

        void P2PBase_OnSystemJoined()
        {
            P2PManager.P2PBase.OnSystemJoined -= P2PBase_OnSystemJoined;
            systemJoinEvent.Set();
        }

        private void UpdateStatusContainerInQuickWatch()
        {
            quickWatch.DataContext = status;
            quickWatch.UpdateSettings(keySearcher, settings);
        }
    }
}