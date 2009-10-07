﻿using System;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Analysis;
using Cryptool.PluginBase;
using System.Windows.Controls;
using System.ComponentModel;
using Cryptool.PluginBase.Control;
using System.Collections;

namespace KeySearcher
{
    public class KeyPattern
    {
        private class Wildcard
        {
            private char[] values = new char[256];
            private int length;
            private int counter;            

            public Wildcard(string valuePattern)
            {                
                counter = 0;
                length = 0;
                int i = 1;
                while (valuePattern[i] != ']')
                {
                    if (valuePattern[i + 1] == '-')
                    {
                        for (char c = valuePattern[i]; c <= valuePattern[i + 2]; c++)
                            values[length++] = c;
                        i += 2;
                    }
                    else
                        values[length++] = valuePattern[i];
                    i++;
                }
            }

            public char getChar()
            {                
                return (char)values[counter];
            }

            public bool succ()
            {
                counter++;
                if (counter >= length)
                {
                    counter = 0;
                    return true;
                }
                return false;
            }

        }

        private string pattern;
        private string key;        
        private ArrayList wildcardList;

        public KeyPattern(string pattern)
        {
            this.pattern = pattern;
        }

        public string giveWildcardKey()
        {
            string res = "";
            int i = 0;
            while (i < pattern.Length)
            {
                if (pattern[i] != '[')
                    res += pattern[i];
                else
                {
                    res += '*';
                    while (pattern[i] != ']')
                        i++;
                }
                i++;
            }
            return res;
        }

        public bool testKey(string key)
        {
            int kcount = 0;
            int pcount = 0;
            while (kcount < key.Length && pcount < pattern.Length)
            {
                if (pattern[pcount] != '[')
                {
                    if (key[kcount] != '*' && pattern[pcount] != key[kcount])
                        return false;
                    kcount++;
                    pcount++;
                }
                else
                {
                    bool contains = false;
                    pcount++;
                    while (pattern[pcount] != ']')
                    {
                        if (key[kcount] != '*')
                        {
                            if (pattern[pcount + 1] == '-')
                            {
                                if (key[kcount] >= pattern[pcount] && key[kcount] <= pattern[pcount + 2])
                                    contains = true;
                                pcount += 2;
                            }
                            else
                                if (pattern[pcount] == key[kcount])
                                    contains = true;
                        }
                        pcount++;
                    }
                    if (!contains && !(key[kcount] == '*'))
                        return false;
                    kcount++;
                    pcount++;
                }                
            }
            if (pcount != pattern.Length || kcount != key.Length)
                return false;
            return true;
        }

        public void initKeyIteration(string key)
        {
            this.key = key;
            int pcount = 0;
            wildcardList = new ArrayList();
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] == '*')
                    wildcardList.Add(new Wildcard(pattern.Substring(pcount, pattern.IndexOf(']', pcount)+1-pcount)));

                if (pattern[pcount] == '[')
                    while (pattern[pcount] != ']')
                        pcount++;
                pcount++;
            }
        }

        public bool nextKey()
        {
            int wildcardCount = 0;
            bool overflow = ((Wildcard)wildcardList[0]).succ();
            wildcardCount = 1;
            while (overflow && (wildcardCount < wildcardList.Count))
                overflow = ((Wildcard)wildcardList[wildcardCount++]).succ();
            return !overflow;
        }

        public string getKey()
        {
            string res = "";
            int wildcardCount = 0;
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] != '*')
                    res += key[i];
                else
                {
                    Wildcard wc = (Wildcard)wildcardList[wildcardCount++];
                    res += wc.getChar();
                }
            }
            return res;
        }
    }
    
    [Author("Thomas Schmid and Sven Rech", "thomas.schmid@cryptool.org and rech@cryptool.org", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo(true, "KeySearcher", "Bruteforces a decryption algorithm.", null, "KeySearcher/Images/icon.png")]
    public class KeySearcher : IAnalysisMisc
    {
        private bool stop;

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private KeySearcherSettings settings;        

        public KeySearcher()
        {
            settings = new KeySearcherSettings(this);
        }

        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
            if (ControlMaster != null)
            {
                stop = false;
                if (!settings.Pattern.testKey(settings.Key))
                {
                    GuiLogMessage("Wrong key pattern!", NotificationLevel.Error);
                    return;
                }
                settings.Pattern.initKeyIteration(settings.Key);
                string key;
                do
                {
                    key = settings.Pattern.getKey();
                    GuiLogMessage("Try key " + key, NotificationLevel.Debug);
                    byte[] decryption = ControlMaster.Decrypt(ControlMaster.getKeyFromString(key));
                    //TODO: Check cost function here
                } while (settings.Pattern.nextKey() && !stop);
            }
        }

        public void PostExecution()
        {
        }

        public void Pause()
        {
        }

        public void Stop()
        {
            stop = true;
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region IControlEncryption Members

        private IControlEncryption controlMaster;
        [PropertyInfo(Direction.ControlMaster, "Control Master", "Used for bruteforcing", "", DisplayLevel.Beginner)]
        public IControlEncryption ControlMaster
        {
            get { return controlMaster; }
            set
            {
                if (value != null)
                {
                    settings.Pattern = new KeyPattern(value.getKeyPattern());
                    controlMaster = value;
                    OnPropertyChanged("ControlMaster");
                }
            }
        }

        public void GuiLogMessage(string message, NotificationLevel loglevel)
        {
            if (OnGuiLogNotificationOccured != null)
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, loglevel));
        }

        #endregion
    }
}
