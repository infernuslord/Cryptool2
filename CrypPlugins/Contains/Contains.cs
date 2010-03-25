/*
   Copyright 2008 Thomas Schmid, University of Siegen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.Windows.Controls;
using System.ComponentModel;
using System.IO;
using Contains.Aho_Corasick;
using Cryptool.PluginBase.Analysis;
using System.Collections;
using Cryptool.PluginBase.Miscellaneous;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Contains
{
  [Author("Thomas Schmid", "thomas.schmid@cryptool.org", "Uni Siegen", "http://www.uni-siegen.de")]
  [PluginInfo(false, "Contains", "Search strings in a dictionary.", "URL", "Contains/icon.png", "Contains/subset.png", "Contains/no_subset.png")]
  public class Contains : IAnalysisMisc
  {    
    private ContainsSettings settings;
    private StringSearch stringSearch;
    private string inputString = "";
    private string dictionaryInputString;
    private ContainsPresentation presentation = new ContainsPresentation();
    private Hashtable hashTable = new Hashtable();
    private Dictionary<string, NotificationLevel> dicWarningsAndErros = new Dictionary<string, NotificationLevel>();

    public Contains()
    {
      settings = new ContainsSettings();
    }

    [PropertyInfo(Direction.InputData, "Text input", "Input a string to search for in selected Dictionary.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public string InputString
    {
      get { return this.inputString; }
      set
      {
        if (value != inputString)
        {          
          if (value != null && settings.ToLower) this.inputString = value.ToLower();
          else this.inputString = value;
          OnPropertyChanged("InputString");
        }
      }
    }

    [PropertyInfo(Direction.InputData, "Dictionary", "The search for known words is based on this dictionary.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public string DictionaryInputString
    {
      get { return this.dictionaryInputString; }
      set
      {
        if (value != dictionaryInputString)
        {
          if (value != null && settings.ToLower) this.dictionaryInputString = value.ToLower();
          else this.dictionaryInputString = value;

          Stopwatch stopWatch = new Stopwatch();
          EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(Properties.Resources.building_search_structure, this, NotificationLevel.Info));
          stopWatch.Start();
          SetSearchStructure();
          stopWatch.Stop();
          EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(string.Format(Properties.Resources.finished_building_search_structure, new object[] { stopWatch.Elapsed.Seconds.ToString()}) , this, NotificationLevel.Info));
          OnPropertyChanged("DictionaryInputString");          
        }
      }
    }
    
    [PropertyInfo(Direction.InputData, "Number of hits to find.", "Needs to find n-hits to return true (Overrides settings value if input is given).", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public int Hits
    {
      get { return settings.Hits; }
      set
      {
        if (value != settings.Hits && value >= 1)
        {
          settings.Hits = value;
          OnPropertyChanged("Hits");
        }
        string msg = "Error: got hit value < 1";
        if (value < 1 && !dicWarningsAndErros.ContainsKey(msg))
          dicWarningsAndErros.Add(msg, NotificationLevel.Error);
      }
    }

    /// <summary>
    /// Builds the search structure, e.g. Hashtable or AhoCorasick. This takes about 6sec for a 
    /// 4MB Dictionary file.
    /// 
    /// Add sync Attribute, because on 
    /// foreach (string item in theWords)
    ///   if (!hashTable.ContainsKey(item))
    ///     hashTable.Add(item, null);
    /// appeared erros after firering a lot events in loop
    /// </summary>    
    [MethodImpl(MethodImplOptions.Synchronized)]
    private void SetSearchStructure()
    {
      try
      {
        stringSearch = null;
        hashTable = null;

        if (dictionaryInputString != null && inputString != null)
        {
          if (settings.Search == ContainsSettings.SearchType.AhoCorasick)
          {            
            // if DicDelimiter is set we have to split the input string
            if (settings.DelimiterDictionary.Length == 1)
              stringSearch = new StringSearch(dictionaryInputString.Split(settings.DelimiterDictionary[0]));
            else
            {
              string[] arr = new string[1];
              arr[0] = dictionaryInputString;
              stringSearch = new StringSearch(arr);
            }
          }
          else if (settings.Search == ContainsSettings.SearchType.Hashtable)
          {
            hashTable = new Hashtable();
            string[] theWords = null;
            if (settings.DelimiterDictionary.Length == 1)
              theWords = dictionaryInputString.Split(settings.DelimiterInputString[0]);
            else
            {
              theWords = new string[1];
              theWords[0] = dictionaryInputString;
            }
            
            foreach (string item in theWords)
              if (!hashTable.ContainsKey(item))
                hashTable.Add(item, null);            
          }
        }
      }
      catch (Exception exception)
      {
        EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(exception.Message, this, NotificationLevel.Error));
        EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(exception.StackTrace, this, NotificationLevel.Error));
      }
    }

    private bool result;
    [PropertyInfo(Direction.OutputData, "Search result", "The search result is based on the current input and the given parameters.", "", false, false, DisplayLevel.Expert, QuickWatchFormat.Text, null)]
    public bool Result
    {
      get { return result; }
      set
      {
        result = value;
        OnPropertyChanged("Result");
        if (result)
          EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, 1));
        else
          EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, 2));
      }
    }



//Angelov:
    private int hitCount = 0;

    
    [PropertyInfo(Direction.OutputData, "Number of Hits", "The search result is based on the current input and the given parameters.", "", false, false, DisplayLevel.Expert, QuickWatchFormat.Text, null)]
    public int HitCount
    {
        get { return hitCount; }
        set
        {
            hitCount = value;
            OnPropertyChanged("HitCount");
            
        }
    } 
//End Angelov



    #region IPlugin Members

    public event StatusChangedEventHandler OnPluginStatusChanged;

    public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

    public event PluginProgressChangedEventHandler OnPluginProgressChanged;

    public ISettings Settings
    {
      get { return settings; }
    }

    public UserControl Presentation
    {
      get { return presentation; }
    }

    public UserControl QuickWatchPresentation
    {
      get { return presentation; }
    }

    public void PreExecution()
    {
      EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, 0));
      // set hits to zero
      List<StringSearchResult> list = new List<StringSearchResult>();
      presentation.SetData(list.ToArray());
    }

    // If this attribute is not used large loops, containing this plugin, will
    // produce threads that aren't finished before next execution takes place.
    // So after pressing stop button there may be a lot threads in queue that have still
    // to be executed
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Execute()
    {
      try
      {
        List<StringSearchResult> listReturn = new List<StringSearchResult>();
        string[] arrSearch = null;
        List<string> wordsFound = new List<string>();
        
        if (InputString != null && DictionaryInputString != null)
        {
          if (settings.Search == ContainsSettings.SearchType.AhoCorasick && stringSearch != null)
          {
            listReturn.AddRange(stringSearch.FindAll(InputString));
          }
          else if (settings.Search == ContainsSettings.SearchType.Hashtable && hashTable != null)
          {            
            if (settings.DelimiterInputString != null && settings.DelimiterInputString.Length == 1)
              arrSearch = InputString.Split(settings.DelimiterInputString[0]);
            if (arrSearch != null)
            {
              for (int i = 0; i < arrSearch.Length; i++)
              {
                if (hashTable.ContainsKey(arrSearch[i])) 
                {                  
                  if (settings.CountWordsOnlyOnce)
                  {
                    if (!wordsFound.Contains(arrSearch[i]))
                    {
                      wordsFound.Add(arrSearch[i]);
                      listReturn.Add(new StringSearchResult(i, arrSearch[i]));
                    }
                  }                  
                  else
                  {
                    listReturn.Add(new StringSearchResult(i, arrSearch[i]));
                  }
                }
              }
            }
            else
            {
              if (hashTable.ContainsKey(InputString)) listReturn.Add(new StringSearchResult(0, InputString));
            }
          }
            
           


          // set target-hits bases on current setting
          int currentTargetHits = int.MinValue;
          if (settings.HitPercentFromInputString && arrSearch != null)
          {
            currentTargetHits = (int)((double)arrSearch.Length / 100.0 * settings.Hits);
          }
          else
          {
            currentTargetHits = settings.Hits;
          }
          presentation.TargetHits = currentTargetHits;

          if (listReturn.Count < currentTargetHits)
          {
            // presentation.SetHits(list.Count);
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(Math.Min((double)listReturn.Count / (double)settings.Hits * 100.0, 100.0), 100));
            Result = false;
          }
          else
          {
            // presentation.SetData(list.ToArray());
            Result = true;
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(100, 100));
          }
          presentation.SetData(listReturn.ToArray());

         //Angelov: 

          HitCount = listReturn.Count;

         //End Angelov

        }
        else
        {
          foreach (KeyValuePair<string, NotificationLevel> kvp in dicWarningsAndErros)
          {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(kvp.Key, this, kvp.Value));
          }
          dicWarningsAndErros.Clear();
          if (InputString == null)
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(Properties.Resources.no_input_string, this, NotificationLevel.Error));
          if (DictionaryInputString == null)
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(Properties.Resources.no_dictionary, this, NotificationLevel.Error));
        }
      }
      catch (Exception exception)
      {
        EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(exception.Message, this, NotificationLevel.Error));
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

    protected void OnPropertyChanged(string name)
    {
      EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
    }

    #endregion
  }
}
