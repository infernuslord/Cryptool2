using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// additional needed libs
using System.Diagnostics;

//Cryptool 2.0 specific includes
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Enigma
{
    public class IntermediateResultEventArgs : EventArgs
    {
        public string Result { get; set; }
    }


    public class EnigmaAnalyzer
    {

        #region Private classes/structs

        private class analysisConfigSettings : IComparable<analysisConfigSettings>
        {
            public int Rotor1;
            public int Rotor2;
            public int Rotor3;
            public int Ring1;
            public int Ring2;
            public int Ring3;
            public string Key;
            public string PlugBoard = "-- no plugs --";
            public double Score;

            #region IComparable<analysisConfigSettings> Member

            public int CompareTo(analysisConfigSettings other)
            {
                return this.Score.CompareTo(other.Score);
            }

            #endregion
        }

        #endregion

        #region Private member variables and constants

        Enigma pluginFacade;
        EnigmaSettings settings;
        EnigmaCore core;

        private bool stop = false;

        private List<analysisConfigSettings> analysisCandidates = new List<analysisConfigSettings>();
        private const int maxAnalysisEntries = 10; // should go in settings under "Analysis Options"

        #endregion

        #region Private member methods

        /// <summary>
        /// This method basically brute-forces all possible rotors
        /// </summary>
        /// <param name="text">The ciphertext</param>
        private void analyzeRotors(string text)
        {
            // Start the stopwatch
            Stopwatch sw = Stopwatch.StartNew();
            int trials = 0;

            for (int i = 0; i < 8; i++)
            {
                //Rotor 3 (slowest)
                if (!includeRotor(i)) continue;
                settings.Rotor3 = i;
                for (int j = 0; j < 8; j++)
                {
                    // Rotor 2 (middle)
                    if (!includeRotor(j) || j == i) continue;
                    settings.Rotor2 = j;

                    for (int k = 0; k < 8; k++)
                    {
                        // Rotor 1 (fastest)
                        if (!includeRotor(k) || k == i || k == j) continue;
                        settings.Rotor1 = k;

                        //set the internal Config to the new rotors
                        core.setInternalConfig(k, j, i, 0, settings.Reflector,
                            settings.AnalyzeRings ? 1 : settings.Ring1,
                            settings.AnalyzeRings ? 1 : settings.Ring2,
                            settings.AnalyzeRings ? 1 : settings.Ring3,
                            settings.Ring4,
                            settings.AnalyzePlugs ? settings.Alphabet : settings.PlugBoard);

                        analyzeKeys(text);
                        trials++;

                        pluginFacade.ShowProgress(i * Math.Pow(8, 2) + j * 8 + k, Math.Pow(8, 3));

                        if (stop) break;
                    } // Rotor 1
                    if (stop) break;
                } // Rotor 2
                if (stop) break;
            } // Rotor 3

            // Stop the stopwatch
            sw.Stop();

            string msg = String.Format("Processed {0} rotor permutations in {1}!",
                trials, sw.Elapsed.ToString());
            pluginFacade.LogMessage(msg, NotificationLevel.Info);
        }

        /// <summary>
        /// This method brute-forces all possible rotor positions (i.e. the key)
        /// The rotors themselfs need to be setup prior to calling this method
        /// </summary>
        /// <param name="text">The ciphertext</param>
        private void analyzeKeys(string text)
        {
            /////////////////////////////////////////
            // now run through all rotor positions..

            // Start the stopwatch
            Stopwatch sw = Stopwatch.StartNew();
            int trials = 0;

            // Rotor 1 positions (fastest)
            for (int l = 0; l < 26; l++)
            {
                for (int m = 0; m < 26; m++)
                {
                    for (int n = 0; n < 26; n++)
                    {
                        checkKey(l, m, n, text);
                        trials++;
                        if (stop) break;
                    }
                    if (stop) break;
                }
                if (stop) break;
            } // Rotor1 positions

            // Stop the stopwatch
            sw.Stop();

            string msg = String.Format("Processed {0} rotor positions for {1},{2},{3} in {4}!",
                trials, (rotorEnum)core.Rotor3, (rotorEnum)core.Rotor2, (rotorEnum)core.Rotor1, sw.Elapsed.ToString());
            pluginFacade.LogMessage(msg, NotificationLevel.Info);
        }

        /// <summary>
        /// This method performs a trial encryption with the given rotor positions (i.e. the key)
        /// If the trial encryption results in a better result as before encountered, the current settings will
        /// remembered in analysisCandidates-List. 
        /// </summary>
        /// <param name="rotor1Pos">Integer value for rotor 1 position (values range from 0 to 25)</param>
        /// <param name="rotor2Pos">Integer value for rotor 2 position (values range from 0 to 25)</param>
        /// <param name="rotor3Pos">Integer value for rotor 3 position (values range from 0 to 25)</param>
        /// <param name="text">The ciphertext</param>
        /// <returns>The result of encrypting/decrypting the ciphertext with the given key</returns>
        private string checkKey(int rotor1Pos, int rotor2Pos, int rotor3Pos, string text)
        {
            string result = core.Encrypt(rotor1Pos, rotor2Pos, rotor3Pos, 0, text);
            double newScore = calculateScore(result, settings.KeySearchMethod);

            if (analysisCandidates.Count >= maxAnalysisEntries)
            {
                // List is full, check if we need to remove one
                if (newScore > analysisCandidates[0].Score)
                {
                    double currentMax = analysisCandidates[analysisCandidates.Count - 1].Score;

                    analysisConfigSettings csetting = new analysisConfigSettings();
                    csetting.Score = newScore;
                    csetting.Rotor1 = core.Rotor1;
                    csetting.Rotor2 = core.Rotor2;
                    csetting.Rotor3 = core.Rotor3;
                    csetting.Ring1 = core.Ring1;
                    csetting.Ring2 = core.Ring2;
                    csetting.Ring3 = core.Ring3;
                    csetting.PlugBoard = core.Plugboard;
                    csetting.Key = settings.Alphabet[rotor3Pos].ToString() + settings.Alphabet[rotor2Pos].ToString() + settings.Alphabet[rotor1Pos].ToString();

                    analysisCandidates.Add(csetting);
                    analysisCandidates.Sort();

                    // remove the smallest one
                    analysisCandidates.RemoveAt(0);


                    if (newScore > currentMax)
                    {
                        // new best option
                        string status = String.Format("ANALYSIS: ==> Found better rotor settings: {0},{1},{2}; {3},{4},{5}; Key: {6}; I.C.={7} <==",
                        (rotorEnum)csetting.Rotor3, (rotorEnum)csetting.Rotor2, (rotorEnum)csetting.Rotor1,
                        csetting.Ring3.ToString("00"), csetting.Ring2.ToString("00"), csetting.Ring1.ToString("00"),
                        csetting.Key, newScore.ToString());
                        pluginFacade.LogMessage(status, NotificationLevel.Info);

                        printBestCandidates();

                        // fire the event, so someting becomes visible..
                        if (OnIntermediateResult != null)
                        {
                            OnIntermediateResult(this, new IntermediateResultEventArgs() { Result = result });
                        }
                    }


                }
            }
            else
            {
                //there is room left, hence add the element

                analysisConfigSettings csetting = new analysisConfigSettings();
                csetting.Score = newScore;
                csetting.Rotor1 = core.Rotor1;
                csetting.Rotor2 = core.Rotor2;
                csetting.Rotor3 = core.Rotor3;
                csetting.Ring1 = core.Ring1;
                csetting.Ring2 = core.Ring2;
                csetting.Ring3 = core.Ring3;
                csetting.PlugBoard = core.Plugboard;
                csetting.Key = settings.Alphabet[rotor3Pos].ToString() + settings.Alphabet[rotor2Pos].ToString() + settings.Alphabet[rotor1Pos].ToString();

                analysisCandidates.Add(csetting);
                analysisCandidates.Sort();

                if (analysisCandidates.Count == maxAnalysisEntries)
                {
                    printBestCandidates();

                    // current best option
                    analysisConfigSettings bestOption = analysisCandidates[analysisCandidates.Count - 1];

                    string status = String.Format("ANALYSIS: Best candidates is filled. Best option so far: {0},{1},{2}; Key: {3}; I.C.={4}",
                    (rotorEnum)bestOption.Rotor3, (rotorEnum)bestOption.Rotor2, (rotorEnum)bestOption.Rotor1, bestOption.Key, bestOption.Score.ToString());
                    pluginFacade.LogMessage(status, NotificationLevel.Debug);

                    // fire the event, so someting becomes visible..
                    if (OnIntermediateResult != null)
                    {
                        OnIntermediateResult(this, new IntermediateResultEventArgs() { Result = result });
                    }
                }

            }

            return result;
        }        

        /// <summary>
        /// Performs an hill-climbing approach to determine the ring settings
        /// </summary>
        /// <param name="enigmaConfig">A partial enigma configuration to be used</param>
        /// <param name="text">The ciphertext</param>
        private void analyzeRings(analysisConfigSettings enigmaConfig, string text)
        {

            // get the current rotor positions
            int r1pos = settings.Alphabet.IndexOf(enigmaConfig.Key[2]);
            int r2pos = settings.Alphabet.IndexOf(enigmaConfig.Key[1]);
            int r3pos = settings.Alphabet.IndexOf(enigmaConfig.Key[0]);

            if (settings.AnalyzeKey)
            {
                // turn fast rotor
                for (int i = 1; i <= settings.Alphabet.Length; i++)
                {
                    core.setInternalConfig(enigmaConfig.Rotor1, enigmaConfig.Rotor2, enigmaConfig.Rotor3, 0, settings.Reflector, i, 1, 1, 1, enigmaConfig.PlugBoard);


                    int rotatedR1Pos;
                    if (settings.AnalyzeKey)
                        // rotate the fast rotor with the ring
                        rotatedR1Pos = (r1pos + (i - 1)) % settings.Alphabet.Length;
                    else
                        rotatedR1Pos = r1pos;


                    string result = core.Encrypt(rotatedR1Pos, r2pos, r3pos, 0, text);

                    double newScore = calculateScore(result, settings.KeySearchMethod);

                    if (newScore > enigmaConfig.Score)
                    {
                        //better value, hence update the data
                        enigmaConfig.Score = newScore;
                        enigmaConfig.Ring1 = i;
                        enigmaConfig.Key = settings.Alphabet[r3pos].ToString() + settings.Alphabet[r2pos].ToString() + settings.Alphabet[rotatedR1Pos].ToString();
                    }

                }

                // update the current rotor positions (only rotor 1 might have changed)
                r1pos = settings.Alphabet.IndexOf(enigmaConfig.Key[2]);

                // turn middle rotor
                for (int i = 1; i <= settings.Alphabet.Length; i++)
                {
                    core.setInternalConfig(enigmaConfig.Rotor1, enigmaConfig.Rotor2, enigmaConfig.Rotor3, 0, settings.Reflector, enigmaConfig.Ring1, i, 1, 1, enigmaConfig.PlugBoard);

                    int rotatedR2Pos;
                    if (settings.AnalyzeKey)
                        // rotate the middle rotor with the ring
                        rotatedR2Pos = (r2pos + (i - 1)) % settings.Alphabet.Length;
                    else
                        rotatedR2Pos = r2pos;

                    string result = core.Encrypt(r1pos, rotatedR2Pos, r3pos, 0, text);

                    double newScore = calculateScore(result, settings.KeySearchMethod);

                    if (newScore > enigmaConfig.Score)
                    {
                        //better value, hence update the data
                        enigmaConfig.Score = newScore;
                        enigmaConfig.Ring2 = i;
                        enigmaConfig.Key = settings.Alphabet[r3pos].ToString() + settings.Alphabet[rotatedR2Pos].ToString() + settings.Alphabet[r1pos].ToString();
                    }
                }
            }
            else
            {
                // in case the key is fixed, we search all combinations, i.e. 26*26*26
                for (int i = 1; i <= settings.Alphabet.Length; i++)
                {
                    for (int j = 1; j <= settings.Alphabet.Length; j++)
                    {
                        for (int k = 1; k <= settings.Alphabet.Length; k++)
                        {
                            core.setInternalConfig(enigmaConfig.Rotor1, enigmaConfig.Rotor2, enigmaConfig.Rotor3, 0, settings.Reflector, k, j, i, 1, enigmaConfig.PlugBoard);
                            string result = core.Encrypt(r1pos, r2pos, r3pos, 0, text);
                            double newScore = calculateScore(result, settings.KeySearchMethod);

                            if (newScore > enigmaConfig.Score)
                            {
                                //better value, hence update the data
                                enigmaConfig.Score = newScore;
                                enigmaConfig.Ring1 = k;
                                enigmaConfig.Ring2 = j;
                                enigmaConfig.Ring2 = i;
                                enigmaConfig.Key = settings.Alphabet[r3pos].ToString() + settings.Alphabet[r2pos].ToString() + settings.Alphabet[r1pos].ToString();
                            }
                        }
                    }
                }
            }

            // print best option
            string msg = String.Format("ANALYSIS: Best ring setting: {0} | {1},{2},{3} | {4},{5},{6} | {7} | {8}",
                enigmaConfig.Score.ToString(),
                (rotorEnum)enigmaConfig.Rotor3, (rotorEnum)enigmaConfig.Rotor2, (rotorEnum)enigmaConfig.Rotor1,
                enigmaConfig.Ring3.ToString("00"), enigmaConfig.Ring2.ToString("00"), enigmaConfig.Ring1.ToString("00"),
                enigmaConfig.Key, pluginFacade.pB2String(enigmaConfig.PlugBoard));
            pluginFacade.LogMessage(msg, NotificationLevel.Info);

        }

        /// <summary>
        /// Performs a hill-climbing approach to determine the plug-settings
        /// </summary>
        /// <param name="enigmaConfig">A partial enigma configuration to be used</param>
        /// <param name="maxPlugs">The maximum numer of plugs to be searched. Note that if no more improvement can be found the algortihm may terminate earlier</param>
        /// <param name="text">The cipertext</param>
        /// <returns>best decrypted result string</returns>
        private string analyzePlugs(analysisConfigSettings enigmaConfig, int maxPlugs, string text)
        {
            string tmp;
            bool plugFound = false;
            int trials = 0;
            string bestResult = encrypt(enigmaConfig, text, enigmaConfig.PlugBoard);
            enigmaConfig.Score = calculateScore(bestResult, settings.PlugSearchMethod);

            for (int n = 0; n < maxPlugs; n++)
            {

                //LogMessage(String.Format("ANALYSIS: ====> Stage 3.{0} - Searching plugs <====",(n+1)), NotificationLevel.Info);

                tmp = enigmaConfig.PlugBoard;
                plugFound = false;
                trials = 0; //reset the counter, so we count each round individually

                for (int i = 0; i < settings.Alphabet.Length; i++)
                {
                    for (int j = i + 1; j < settings.Alphabet.Length; j++)
                    {
                        //create a "clean" plugboard
                        StringBuilder plugboard = new StringBuilder(tmp);

                        //if both selected letters are pluged, ignore them
                        if (plugboard[i] != settings.Alphabet[i] && plugboard[j] != settings.Alphabet[j])
                            continue;

                        if (plugboard[i] != settings.Alphabet[i])
                        {
                            plugFound = plugFound | resolvePlugConflict(i, j, enigmaConfig, plugboard.ToString(), text);
                            trials += 3;
                            continue;
                        }

                        if (plugboard[j] != settings.Alphabet[j])
                        {
                            plugFound = plugFound | resolvePlugConflict(j, i, enigmaConfig, plugboard.ToString(), text);
                            trials += 3;
                            continue;
                        }

                        //swap i with j
                        plugboard[i] = settings.Alphabet[j];
                        plugboard[j] = settings.Alphabet[i];

                        string result = encrypt(enigmaConfig, text, plugboard.ToString());
                        double newScore = calculateScore(result, settings.PlugSearchMethod);
                        trials++;

                        if (newScore > enigmaConfig.Score)
                        {
                            enigmaConfig.Score = newScore;
                            enigmaConfig.PlugBoard = plugboard.ToString();
                            bestResult = result;
                            plugFound = true;
                        }

                        if (stop)
                            break;
                    }
                    if (stop)
                        break;
                }


                string msg = String.Format("ANALYSIS: Plugs setting in round {0} after {1} trials: {2} | {3},{4},{5} | {6},{7},{8} | {9} | {10}",
                    (n + 1), trials, enigmaConfig.Score.ToString(),
                    (rotorEnum)enigmaConfig.Rotor3, (rotorEnum)enigmaConfig.Rotor2, (rotorEnum)enigmaConfig.Rotor1,
                    enigmaConfig.Ring3, enigmaConfig.Ring2, enigmaConfig.Ring1,
                    enigmaConfig.Key, pluginFacade.pB2String(enigmaConfig.PlugBoard));
                pluginFacade.LogMessage(msg, NotificationLevel.Info);

                // no plug could lead to a better result, hence abort plug search.
                if (!plugFound || stop)
                    break;
            }

            return bestResult;
        }

        private string encrypt(analysisConfigSettings enigmaConfig, string text, string plugboard)
        {
            core.setInternalConfig(enigmaConfig.Rotor1, enigmaConfig.Rotor2, enigmaConfig.Rotor3, 0, settings.Reflector, enigmaConfig.Ring1, enigmaConfig.Ring2, enigmaConfig.Ring3, 1, plugboard);

            int r1pos = settings.Alphabet.IndexOf(enigmaConfig.Key[2]);
            int r2pos = settings.Alphabet.IndexOf(enigmaConfig.Key[1]);
            int r3pos = settings.Alphabet.IndexOf(enigmaConfig.Key[0]);
            return core.Encrypt(r1pos, r2pos, r3pos, 0, text);
        }

        /// <summary>
        /// Determines the best plug-configuration out of three possibilities.
        /// </summary>
        /// <param name="conflictLetterPos">A letter, which already has been plugged with some other letter</param>
        /// <param name="otherLetterPos">The letter we want to test</param>
        /// <param name="enigmaConfig">A partial enigma configuration to be used</param>
        /// <param name="unmodifiedPlugboard">A plugboard which should be used in this stage</param>
        /// <param name="text">The cipertext</param>
        /// <returns>Returns true, if a better plug-combination could be found, false otherwise</returns>
        private bool resolvePlugConflict(int conflictLetterPos, int otherLetterPos, analysisConfigSettings enigmaConfig, string unmodifiedPlugboard, string text)
        {
            bool found = false;

            int pluggedLetterPos = settings.Alphabet.IndexOf(unmodifiedPlugboard[conflictLetterPos]);


            // plug otherLetter together with pluggedLetter and restore the coflictLetter
            StringBuilder o2pPlugPlugboard = new StringBuilder(unmodifiedPlugboard);
            o2pPlugPlugboard[conflictLetterPos] = settings.Alphabet[conflictLetterPos]; // restore conflictLetter
            o2pPlugPlugboard[otherLetterPos] = settings.Alphabet[pluggedLetterPos];     // swap other with
            o2pPlugPlugboard[pluggedLetterPos] = settings.Alphabet[otherLetterPos];     // plugged


            // plug conflictLetter with otherLetter and restore pluggedLetter one
            StringBuilder c2oPlugboard = new StringBuilder(unmodifiedPlugboard);
            c2oPlugboard[pluggedLetterPos] = settings.Alphabet[pluggedLetterPos]; // restore pluggedLetter
            c2oPlugboard[conflictLetterPos] = settings.Alphabet[otherLetterPos];  // swap conflictLetter
            c2oPlugboard[otherLetterPos] = settings.Alphabet[conflictLetterPos];  // with otherLetter


            // now we habe three different plug-posibilities and need to determine 
            // the best one, which remains set, hence we do 3 trial encryptions


            // get the current key
            int r1pos = settings.Alphabet.IndexOf(enigmaConfig.Key[2]);
            int r2pos = settings.Alphabet.IndexOf(enigmaConfig.Key[1]);
            int r3pos = settings.Alphabet.IndexOf(enigmaConfig.Key[0]);


            // start with the unmodified
            core.setInternalConfig(enigmaConfig.Rotor1, enigmaConfig.Rotor2, enigmaConfig.Rotor3, 0, settings.Reflector, enigmaConfig.Ring1, enigmaConfig.Ring2, enigmaConfig.Ring3, 1, unmodifiedPlugboard);
            double unmodifiedScore = calculateScore(core.Encrypt(r1pos, r2pos, r3pos, 0, text), settings.PlugSearchMethod);

            // now o2p
            core.setInternalConfig(enigmaConfig.Rotor1, enigmaConfig.Rotor2, enigmaConfig.Rotor3, 0, settings.Reflector, enigmaConfig.Ring1, enigmaConfig.Ring2, enigmaConfig.Ring3, 1, o2pPlugPlugboard.ToString());
            double o2pScore = calculateScore(core.Encrypt(r1pos, r2pos, r3pos, 0, text), settings.PlugSearchMethod);

            // now c2o
            core.setInternalConfig(enigmaConfig.Rotor1, enigmaConfig.Rotor2, enigmaConfig.Rotor3, 0, settings.Reflector, enigmaConfig.Ring1, enigmaConfig.Ring2, enigmaConfig.Ring3, 1, c2oPlugboard.ToString());
            double c2oScore = calculateScore(core.Encrypt(r1pos, r2pos, r3pos, 0, text), settings.PlugSearchMethod);

            string bestPlugBoard = enigmaConfig.PlugBoard;
            double newScore;

            if (c2oScore > unmodifiedScore)
            {
                if (c2oScore > o2pScore)
                {
                    bestPlugBoard = c2oPlugboard.ToString();
                    newScore = c2oScore;
                }
                else
                {
                    bestPlugBoard = o2pPlugPlugboard.ToString();
                    newScore = o2pScore;
                }
            }
            else
            {
                if (unmodifiedScore > o2pScore)
                {
                    bestPlugBoard = unmodifiedPlugboard;
                    newScore = unmodifiedScore;
                }
                else
                {
                    bestPlugBoard = o2pPlugPlugboard.ToString();
                    newScore = o2pScore;
                }
            }


            if (newScore > enigmaConfig.Score)
            {
                enigmaConfig.Score = newScore;
                enigmaConfig.PlugBoard = bestPlugBoard;
                found = true;
            }

            //string msg = String.Format("ANALYSIS PLUG CONFLICT: Unmodified [{0}] => {1}; Variant A [{2}] => {3}; Variant B[{4}] => {5} || Selected [{6}]",
            //    pluginFacade.pB2String(unmodifiedPlugboard), unmodifiedScore,
            //    pluginFacade.pB2String(c2oPlugboard.ToString()), c2oScore,
            //    pluginFacade.pB2String(o2pPlugPlugboard.ToString()), o2pScore,
            //    pluginFacade.pB2String(bestPlugBoard));

            //pluginFacade.LogMessage(msg, NotificationLevel.Info);

            return found;
        }

        #region Helper methods

        /// <summary>
        /// This method calculates a trigram log2 score of a given text on the basis of a given grams dictionary.
        /// Case is insensitive.
        /// </summary>
        /// <param name="input">The text to be scored</param>
        /// <param name="length">n-gram length</param>
        /// <returns>The trigram score result</returns>
        private double calculateNGrams(string input, int length, int valueSelection)
        {
            double score = 0;
            IDictionary<string, double[]> corpusGrams = pluginFacade.GetStatistics(length);

            // FIXME: case handling?

            HashSet<string> inputGrams = new HashSet<string>();

            foreach (string g in GramTokenizer.tokenize(input, length, false))
            {
                // ensure each n-gram is counted only once
                if (inputGrams.Add(g))
                {
                    if (corpusGrams.ContainsKey(g))
                    {
                        score += corpusGrams[g][valueSelection];
                    }
                }
            }

            return score;
        }

        private IDictionary<string, double[]> calculateConditional(string input, int length)
        {
            IDictionary<string, double[]> inputGrams = new Dictionary<string, double[]>();

            foreach (string g in GramTokenizer.tokenize(input, length, false))
            {
                if (inputGrams.ContainsKey(g))
                {
                    inputGrams[g][Enigma.ABSOLUTE]++;
                }
                else
                {
                    inputGrams[g] = new double[] { 1, 0 };
                }
            }

            double sum = inputGrams.Values.Sum(item => item[Enigma.ABSOLUTE]);
            foreach (double[] values in inputGrams.Values)
            {
                values[Enigma.PERCENTAGED] = values[Enigma.ABSOLUTE] / sum;
            }

            return inputGrams;
        }

        private double calculateSinkov(string input, int length)
        {
            double score = 0;
            IDictionary<string, double[]> corpusGrams = pluginFacade.GetStatistics(length);
            IDictionary<string, double[]> inputGrams = calculateConditional(input, length);

            foreach (KeyValuePair<string, double[]> item in inputGrams)
            {
                if (corpusGrams.ContainsKey(item.Key))
                {
                    score += item.Value[Enigma.PERCENTAGED] * corpusGrams[item.Key][Enigma.SINKOV];
                }
            }

            return score;
        }

        private double calculateEntropy(string input)
        {
            double score = 0;
            IDictionary<string, double[]> inputGrams = calculateConditional(input, 1);

            foreach (double[] values in inputGrams.Values)
            {
                score += values[Enigma.PERCENTAGED] * Math.Log(values[Enigma.PERCENTAGED], 2);
            }

            return score;
        }

        /// <summary>
        /// A helper method, which returns true, if the rotor with the given
        /// number should be included in the analysis as determined
        /// by the analysis settings
        /// </summary>
        /// <param name="i">The rotor number as integer</param>
        /// <returns>True if this rotor shoudl be included, false otherwise</returns>
        private bool includeRotor(int i)
        {
            switch (i)
            {
                case 0: return settings.AnalysisUseRotorI;
                case 1: return settings.AnalysisUseRotorII;
                case 2: return settings.AnalysisUseRotorIII;
                case 3: return settings.AnalysisUseRotorIV;
                case 4: return settings.AnalysisUseRotorV;
                case 5: return settings.AnalysisUseRotorVI;
                case 6: return settings.AnalysisUseRotorVII;
                case 7: return settings.AnalysisUseRotorVIII;
            }
            return false;
        }

        /// <summary>
        /// Prints on the CrypTool console the currently best options for rotor usage, rotor settings
        /// ring settings, and plug settings
        /// </summary>
        private void printBestCandidates()
        {
            StringBuilder message = new StringBuilder("  -- Analysis results --" + Environment.NewLine);
            message.AppendLine("=====================================");

            foreach (analysisConfigSettings cfg in analysisCandidates)
            {
                message.AppendFormat("{0} | {1},{2},{3} | {4},{5},{6} | {7} | {8}" + Environment.NewLine,
                    cfg.Score.ToString(),
                    (rotorEnum)cfg.Rotor3, (rotorEnum)cfg.Rotor2, (rotorEnum)cfg.Rotor1,
                    cfg.Ring3.ToString("00"), cfg.Ring2.ToString("00"), cfg.Ring1.ToString("00"),
                    cfg.Key, pluginFacade.pB2String(cfg.PlugBoard));
            }

            pluginFacade.LogMessage(message.ToString(), NotificationLevel.Info);
        }

        #endregion

        #endregion

        #region Properties
        #endregion

        #region Public events

        public event EventHandler<IntermediateResultEventArgs> OnIntermediateResult;

        #endregion

        #region Public methods


        /// <summary>
        /// Creates a new Enigma nalyzer class
        /// This analyzer performs an cipher-text-only analysis of a given enigma text.
        /// It uses the method as described by James Gillogly in Cryptologia 1995
        /// </summary>
        /// <param name="facade">The enimga plugin which controls this analyzer</param>
        public EnigmaAnalyzer(Enigma facade)
        {
            pluginFacade = facade;
            settings = (EnigmaSettings)pluginFacade.Settings;
            core = new EnigmaCore(facade);
        }

        /// <summary>
        /// Thsi method performs the entire analysis and returs the cleartext as detected by the algorithm
        /// The provided string should be preformated, i.e. should not contain uncecessary characters;
        /// unknown characters in the input will slow down the analysis process
        /// </summary>
        /// <param name="preformatedText">The preformated text for analysis</param>
        /// <returns>The cleartext as decoded with the analyzed key</returns>
        public IEnumerable<string> Analyze(string preformatedText)
        {
            pluginFacade.LogMessage("=========> ANALYSIS OF ENIGMA MESSAGE STARTED <=========", NotificationLevel.Info);

            // some initialisation
            analysisCandidates.Clear();
            stop = false;

            AnalyzeRun(preformatedText);

            pluginFacade.LogMessage("=========> ANALYSIS OF ENIGMA MESSAGE DONE <=========", NotificationLevel.Info);

            // switch back to verbose core
            core.VerboseLevel = VerboseLevels.VeryVerbose;

            // decrypt all
            return analysisCandidates.Select(cfg => encrypt(cfg, preformatedText, cfg.PlugBoard));
        }

        private void AnalyzeRun(string preformatedText)
        {
            if (settings.AnalyzeRotors)
            {
                pluginFacade.LogMessage("ANALYSIS: ====> Stage 1 - Searching used rotors and key <====", NotificationLevel.Info);
                analyzeRotors(preformatedText);
            }
            else
            {
                pluginFacade.LogMessage("ANALYSIS: ====> Stage 1 - Searching key (with fixed rotors) <====", NotificationLevel.Info);
                if (settings.AnalyzeKey)
                {
                    core.setInternalConfig(settings.Rotor1, settings.Rotor2, settings.Rotor3, settings.Rotor4, settings.Reflector,
                                    settings.AnalyzeRings ? 1 : settings.Ring1,
                                    settings.AnalyzeRings ? 1 : settings.Ring2,
                                    settings.AnalyzeRings ? 1 : settings.Ring3,
                                    settings.Ring4,
                                    settings.AnalyzePlugs ? settings.Alphabet : settings.PlugBoard
                        );
                    analyzeKeys(preformatedText);
                }
                else
                {
                    pluginFacade.LogMessage("ANALYSIS: ====> Skipping stage 1 - Using rotors/key from settings <====", NotificationLevel.Info);
                    analysisConfigSettings settingsConfig = new analysisConfigSettings();
                    settingsConfig.Rotor1 = settings.Rotor1;
                    settingsConfig.Rotor2 = settings.Rotor2;
                    settingsConfig.Rotor3 = settings.Rotor3;
                    settingsConfig.Ring1 = settings.AnalyzeRings ? 1 : settings.Ring1;
                    settingsConfig.Ring2 = settings.AnalyzeRings ? 1 : settings.Ring2;
                    settingsConfig.Ring3 = settings.AnalyzeRings ? 1 : settings.Ring3;
                    settingsConfig.Key = settings.Key;

                    analysisCandidates.Add(settingsConfig);
                }
            }

            // just for debugging
            //analysisCandidates[analysisCandidates.Count-1].Rotor1 = 2;
            //analysisCandidates[analysisCandidates.Count - 1].Rotor2 = 0;
            //analysisCandidates[analysisCandidates.Count - 1].Rotor3 = 1;
            //analysisCandidates[analysisCandidates.Count - 1].Key = "BKF";

            printBestCandidates();

            if (stop)
                return;

            // put the core in quiet mode, since now many internal changes occur
            core.VerboseLevel = VerboseLevels.Quiet;

            if (settings.AnalyzeRings)
            {
                pluginFacade.LogMessage("ANALYSIS: ====> Stage 2 - Searching ring positions <====", NotificationLevel.Info);

                if (!stop)
                {
                    for (int j = analysisCandidates.Count - 1; j >= 0; j--)
                    {
                        analysisCandidates[j].PlugBoard = settings.Alphabet; // empty plugs
                        analyzeRings(analysisCandidates[j], preformatedText);
                    }
                }

                analysisCandidates.Sort();
            }
            else
            {
                pluginFacade.LogMessage("ANALYSIS: ====> Skipping stage 2 - Using provided ring settings <====", NotificationLevel.Info);

                for (int j = analysisCandidates.Count - 1; j >= 0; j--)
                {
                    analysisCandidates[j].PlugBoard = settings.Alphabet; // empty plugs
                    analysisCandidates[j].Ring1 = settings.Ring1;
                    analysisCandidates[j].Ring2 = settings.Ring2;
                    analysisCandidates[j].Ring3 = settings.Ring3;
                }
            }

            printBestCandidates();

            if (stop)
                return;

            if (settings.AnalyzePlugs)
            {
                pluginFacade.LogMessage("ANALYSIS: ====> Stage 3 - Searching used plugs <====", NotificationLevel.Info);

                for (int j = analysisCandidates.Count - 1; j >= 0; j--)
                {
                    string result = analyzePlugs(analysisCandidates[j], settings.MaxSearchedPlugs, preformatedText);

                    // fire the event, so someting becomes visible..
                    if (OnIntermediateResult != null)
                    {
                        OnIntermediateResult(this, new IntermediateResultEventArgs() { Result = result });
                    }
                }

                analysisCandidates.Sort();
            }
            else
            {
                pluginFacade.LogMessage("ANALYSIS: ====> Skipping stage 3 - Using provided plugboard <====", NotificationLevel.Info);

                for (int j = analysisCandidates.Count - 1; j >= 0; j--)
                {
                    analysisCandidates[j].PlugBoard = settings.PlugBoard;
                }
            }

            printBestCandidates();
        }

        /// <summary>
        /// Stops a currently running analysis and outputs the result so far
        /// </summary>
        public void StopAnalysis()
        {
            stop = true;
        }

        /// <summary>
        /// The method calculates and returns the index of coincidences of a given text
        /// </summary>
        /// <param name="input">The index if coincidences of this text will be calculated</param>
        /// <returns>The index of coincidences</returns>
        public double calculateIC(string input)
        {
            int[] statistics = new int[settings.Alphabet.Length];
            long cipherTextLength = 0; //input.Length; //n
            long countDoubleCharacters = 0;

            // first count the frequency of (single) letters
            foreach (char c in input)
            {
                int i = settings.Alphabet.IndexOf(char.ToUpper(c));
                if (i >= 0) statistics[i]++;
            }


            // now calculate the index of coincidences
            for (int i = 0; i < statistics.Length; i++)
            {
                cipherTextLength += statistics[i];
                countDoubleCharacters += (statistics[i] * (statistics[i] - 1));
            }

            return ((double)countDoubleCharacters / (double)(cipherTextLength * (cipherTextLength - 1)));
        }

        public double calculateScore(string input, int searchMethod)
        {
            switch (searchMethod)
            {
                case 0:
                    return calculateIC(input);
                case 1:
                    return calculateNGrams(input, 2, Enigma.LOG2);
                case 2:
                    return calculateNGrams(input, 3, Enigma.LOG2);
                case 3:
                    return calculateSinkov(input, 1);
                case 4:
                    return calculateSinkov(input, 2);
                case 5:
                    return calculateEntropy(input);
                default:
                    throw new NotSupportedException("Search method not supported");
            }
        }

        #endregion


    }
}
