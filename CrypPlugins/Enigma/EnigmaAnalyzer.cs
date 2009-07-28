using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// additional needed libs
using System.Diagnostics;

//Cryptool 2.0 specific includes
using Cryptool.PluginBase;


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
            public double IC;

            #region IComparable<analysisConfigSettings> Member

            public int CompareTo(analysisConfigSettings other)
            {
                return this.IC.CompareTo(other.IC);
            }

            #endregion
        }

        #endregion

        #region Private member variables

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
            double newIC = calculateIC(result);

            if (analysisCandidates.Count >= maxAnalysisEntries)
            {
                // List is full, check if we need to remove one
                if (newIC > analysisCandidates[0].IC)
                {
                    double currentMax = analysisCandidates[analysisCandidates.Count - 1].IC;

                    analysisConfigSettings csetting = new analysisConfigSettings();
                    csetting.IC = newIC;
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


                    if (newIC > currentMax)
                    {
                        // new best option
                        string status = String.Format("ANALYSIS: ==> Found better rotor settings: {0},{1},{2}; {3},{4},{5}; Key: {6}; I.C.={7} <==",
                        (rotorEnum)csetting.Rotor3, (rotorEnum)csetting.Rotor2, (rotorEnum)csetting.Rotor1,
                        csetting.Ring3.ToString("00"), csetting.Ring2.ToString("00"), csetting.Ring1.ToString("00"),
                        csetting.Key, newIC.ToString());
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
                csetting.IC = newIC;
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
                    (rotorEnum)bestOption.Rotor3, (rotorEnum)bestOption.Rotor2, (rotorEnum)bestOption.Rotor1, bestOption.Key, bestOption.IC.ToString());
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

                    double newIC = calculateIC(result);

                    if (newIC > enigmaConfig.IC)
                    {
                        //better value, hence update the data
                        enigmaConfig.IC = newIC;
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

                    double newIC = calculateIC(result);

                    if (newIC > enigmaConfig.IC)
                    {
                        //better value, hence update the data
                        enigmaConfig.IC = newIC;
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
                            double newIC = calculateIC(result);

                            if (newIC > enigmaConfig.IC)
                            {
                                //better value, hence update the data
                                enigmaConfig.IC = newIC;
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
                enigmaConfig.IC.ToString(),
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
        private void analyzePlugs(analysisConfigSettings enigmaConfig, int maxPlugs, string text)
        {
            string tmp;
            bool plugFound = false;
            int trials = 0;

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

                        core.setInternalConfig(enigmaConfig.Rotor1, enigmaConfig.Rotor2, enigmaConfig.Rotor3, 0, settings.Reflector, enigmaConfig.Ring1, enigmaConfig.Ring2, enigmaConfig.Ring3, 1, plugboard.ToString());


                        int r1pos = settings.Alphabet.IndexOf(enigmaConfig.Key[2]);
                        int r2pos = settings.Alphabet.IndexOf(enigmaConfig.Key[1]);
                        int r3pos = settings.Alphabet.IndexOf(enigmaConfig.Key[0]);
                        string result = core.Encrypt(r1pos, r2pos, r3pos, 0, text);

                        double newIC = calculateIC(result);
                        trials++;

                        if (newIC > enigmaConfig.IC)
                        {
                            enigmaConfig.IC = newIC;
                            enigmaConfig.PlugBoard = plugboard.ToString();
                            plugFound = true;
                        }
                    }
                }


                string msg = String.Format("ANALYSIS: Plugs setting in round {0} after {1} trials: {2} | {3},{4},{5} | {6},{7},{8} | {9} | {10}",
                    (n + 1), trials, enigmaConfig.IC.ToString(),
                    (rotorEnum)enigmaConfig.Rotor3, (rotorEnum)enigmaConfig.Rotor2, (rotorEnum)enigmaConfig.Rotor1,
                    enigmaConfig.Ring3, enigmaConfig.Ring2, enigmaConfig.Ring1,
                    enigmaConfig.Key, pluginFacade.pB2String(enigmaConfig.PlugBoard));
                pluginFacade.LogMessage(msg, NotificationLevel.Info);

                // no plug could lead to a better result, hence abort plug search.
                if (!plugFound)
                    break;
            }
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
            double unmodifiedIC = calculateIC(core.Encrypt(r1pos, r2pos, r3pos, 0, text));

            // now o2p
            core.setInternalConfig(enigmaConfig.Rotor1, enigmaConfig.Rotor2, enigmaConfig.Rotor3, 0, settings.Reflector, enigmaConfig.Ring1, enigmaConfig.Ring2, enigmaConfig.Ring3, 1, o2pPlugPlugboard.ToString());
            double o2pIC = calculateIC(core.Encrypt(r1pos, r2pos, r3pos, 0, text));

            // now c2o
            core.setInternalConfig(enigmaConfig.Rotor1, enigmaConfig.Rotor2, enigmaConfig.Rotor3, 0, settings.Reflector, enigmaConfig.Ring1, enigmaConfig.Ring2, enigmaConfig.Ring3, 1, c2oPlugboard.ToString());
            double c2oIC = calculateIC(core.Encrypt(r1pos, r2pos, r3pos, 0, text));

            string bestPlugBoard = enigmaConfig.PlugBoard;
            double newIC;

            if (c2oIC > unmodifiedIC)
            {
                if (c2oIC > o2pIC)
                {
                    bestPlugBoard = c2oPlugboard.ToString();
                    newIC = c2oIC;
                }
                else
                {
                    bestPlugBoard = o2pPlugPlugboard.ToString();
                    newIC = o2pIC;
                }
            }
            else
            {
                if (unmodifiedIC > o2pIC)
                {
                    bestPlugBoard = unmodifiedPlugboard;
                    newIC = unmodifiedIC;
                }
                else
                {
                    bestPlugBoard = o2pPlugPlugboard.ToString();
                    newIC = o2pIC;
                }
            }


            if (newIC > enigmaConfig.IC)
            {
                enigmaConfig.IC = newIC;
                enigmaConfig.PlugBoard = bestPlugBoard;
                found = true;
            }

            //string msg = String.Format("ANALYSIS PlUG CONFLICT: Unmodified [{0}] => {1}; Variant A [{2}] => {3}; Variant B[{4}] => {5} || Selected [{6}]",
            //    pB2String(unmodifiedPlugboard), unmodifiedIC,
            //    pB2String(c2oPlugboard.ToString()), c2oIC,
            //    pB2String(o2pPlugPlugboard.ToString()), o2pIC,
            //    pB2String(bestPlugBoard));

            //LogMessage(msg, NotificationLevel.Info);

            return found;
        }

        #region Helper methods

        /// <summary>
        /// The method calculates and returns the index of coincidences of a given text
        /// </summary>
        /// <param name="input">The index if coincidences of this text will be calculated</param>
        /// <returns>The index of coincidences</returns>
        private double calculateIC(string input)
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
                    cfg.IC.ToString(),
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
        public string Analyze(string preformatedText)
        {
            pluginFacade.LogMessage("=========> ANALYSIS OF ENIGMA MESSAGE STARTED <=========", NotificationLevel.Info);


            // some initialisation
            analysisCandidates.Clear();
            stop = false;

            if (settings.AnalyzeRotors)
            {
                pluginFacade.LogMessage("ANALYSIS: ====> Stage 1 - Searching used rotors <====", NotificationLevel.Info);
                analyzeRotors(preformatedText);
            }
            else
            {
                pluginFacade.LogMessage("ANALYSIS: ====> Skipping stage 1 - Using rotors  from settings <====", NotificationLevel.Info);
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

            // just for debugging
            //analysisCandidates[analysisCandidates.Count-1].Rotor1 = 2;
            //analysisCandidates[analysisCandidates.Count - 1].Rotor2 = 0;
            //analysisCandidates[analysisCandidates.Count - 1].Rotor3 = 1;
            //analysisCandidates[analysisCandidates.Count - 1].Key = "BKF";

            printBestCandidates();

            // put the core in quiet mode, since now many internal changes occur
            core.VerboseLevel = VerboseLevels.Quiet;

            if (settings.AnalyzeRings)
            {
                pluginFacade.LogMessage("ANALYSIS: ====> Stage 2 - Searching ring positions <====", NotificationLevel.Info);

                for (int j = analysisCandidates.Count - 1; j >= 0; j--)
                {
                    analysisCandidates[j].PlugBoard = settings.Alphabet; // empty plugs
                    analyzeRings(analysisCandidates[j], preformatedText);
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


            if (settings.AnalyzePlugs)
            {
                pluginFacade.LogMessage("ANALYSIS: ====> Stage 3 - Searching used plugs <====", NotificationLevel.Info);

                for (int j = analysisCandidates.Count - 1; j >= 0; j--)
                {
                    analyzePlugs(analysisCandidates[j], settings.MaxSearchedPlugs, preformatedText);
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


            pluginFacade.LogMessage("=========> ANALYSIS OF ENIGMA MESSAGE DONE <=========", NotificationLevel.Info);

            // switch back to verbose core
            core.VerboseLevel = VerboseLevels.VeryVerbose;

            // decrypt with best option
            analysisConfigSettings bestConfig = analysisCandidates[analysisCandidates.Count - 1];

            core.setInternalConfig(bestConfig.Rotor1, bestConfig.Rotor2, bestConfig.Rotor3, 0, settings.Reflector, bestConfig.Ring1, bestConfig.Ring2, bestConfig.Ring3, 1, bestConfig.PlugBoard);
            int r1p = settings.Alphabet.IndexOf(bestConfig.Key[2]);
            int r2p = settings.Alphabet.IndexOf(bestConfig.Key[1]);
            int r3p = settings.Alphabet.IndexOf(bestConfig.Key[0]);
            return core.Encrypt(r1p, r2p, r3p, 0, preformatedText);
        }

        /// <summary>
        /// Stops a currently running analysis and outputs the result so far
        /// </summary>
        public void StopAnalysis()
        {
            stop = true;
        }

        /// <summary>
        /// Returns the Index of coincidences of a given text
        /// </summary>
        /// <param name="text">The text</param>
        /// <returns>The index of coincidences</returns>
        public double IndexOfCoincidences(string text)
        {
            return calculateIC(text);
        }

        #endregion


    }
}
