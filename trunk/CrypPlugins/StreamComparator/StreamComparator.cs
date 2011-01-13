using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using Cryptool.PluginBase;
using System;
using System.Windows.Media.Imaging;
using System.Text;
using System.Web;
using System.Xml;
using System.Windows.Markup;
using Cryptool.PluginBase.Analysis;
using Cryptool.PluginBase.IO;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.StreamComparator
{
  [Author("Thomas Schmid", "thomas.schmid@cryptool.org", "Uni Siegen", "http://www.uni-siegen.de")]
  [PluginInfo(false, "Stream Comparator", "Compares two inputs Streams and decides if they are equal.", null, "StreamComparator/icon.png", "StreamComparator/Images/equal.png", "StreamComparator/Images/unequal.png", "StreamComparator/Images/contextmenu.png")]
  public class StreamComparator : IDifferential
  {
    #region Private variables
    private bool isBinary;
    private bool stop;
    private CryptoolStream streamOne;
    private CryptoolStream streamTwo;
    private List<CryptoolStream> listCryptoolStreams = new List<CryptoolStream>();
    private StreamComparatorPresentation streamComparatorPresentation;
    #endregion

    private StreamComparatorSettings settings;
    public ISettings Settings 
    {
      get { return settings; }
      set { settings = (StreamComparatorSettings)value; }
    }

    # region ctor

    public StreamComparator()
    {
      streamComparatorPresentation = new StreamComparatorPresentation();
      settings = new StreamComparatorSettings();
    }

    # endregion

    #region Properties

    [PropertyInfo(Direction.InputData, "Stream one", "First stream to compare", "", true, false, QuickWatchFormat.None, null)]
    public CryptoolStream InputOne
    {
      get 
      {
        CryptoolStream cryptoolStream = null;
        if (streamOne != null)
        {
          cryptoolStream = new CryptoolStream();
          cryptoolStream.OpenRead(streamOne.FileName);
          listCryptoolStreams.Add(cryptoolStream);
        }
        return cryptoolStream; 
      }
      set { streamOne = value; }
    }

    [PropertyInfo(Direction.InputData, "Stream two", "Second stream to compare", "", true, false, QuickWatchFormat.None, null)]
    public CryptoolStream InputTwo
    {
      get 
      {
        CryptoolStream cryptoolStream = null;
        if (streamTwo != null)
        {
          cryptoolStream = new CryptoolStream();
          cryptoolStream.OpenRead(streamTwo.FileName);
          listCryptoolStreams.Add(cryptoolStream);
        }
        return cryptoolStream; 
      }
      set { streamTwo = value; }
    }

    private bool inputsAreEqual;
    [PropertyInfo(Direction.OutputData, "Comparator achievement", "Ture if streams are equal, otherwise false.", "", false, false, QuickWatchFormat.Text, null)]
    public bool InputsAreEqual
    {
      get { return inputsAreEqual; }
      set
      {
        inputsAreEqual = value;
        OnPropertyChanged("InputsAreEqual");
      }
    }

    #endregion

    #region IPlugin Members

    public void Execute()
    {
      stop = false;
      isBinary = false;

      // this would result in one extra message on each comparison so just set it to false on 
      // pre-execution 
      // InputsAreEqual = false;

      int streamOneByte;
      int streamTwoByte;

      CryptoolStream workingStreamOne = InputOne;
      CryptoolStream workingStreamTwo = InputTwo;

      if (workingStreamOne != null && workingStreamTwo != null)
      {
        if (workingStreamOne == workingStreamTwo || workingStreamOne.FileName == workingStreamTwo.FileName && settings.Diff)
        {
          GuiTextChanged("Inputs are equal: same file was referenced two times.", NotificationLevel.Info);
          InputsAreEqual = true;
          if (OnPluginProgressChanged != null) OnPluginProgressChanged(this, new PluginProgressEventArgs(1, 1));
          if (settings.Diff) CreateDiffView();          
        }
        else if (workingStreamOne.Length != workingStreamTwo.Length && !settings.Diff)
        {
          GuiTextChanged("Inputs are not equal, because the filesize is different.", NotificationLevel.Info);
          InputsAreEqual = false;
          if (OnPluginProgressChanged != null) OnPluginProgressChanged(this, new PluginProgressEventArgs(1, 1));
          if (settings.Diff) CreateDiffView();
        }
        else
        {
          workingStreamOne.Position = 0;
          workingStreamTwo.Position = 0;
          DateTime startTime = DateTime.Now;
          int position = 0;
          GuiTextChanged("Starting byte comparison of files now...", NotificationLevel.Info);
          // Read and compare a byte from each file until either a
          // non-matching set of bytes is found or until the end of
          // file1 is reached.
          do
          {
            // Read one byte from each file.
            streamOneByte = workingStreamOne.ReadByte();
            streamTwoByte = workingStreamTwo.ReadByte();

            if (streamOneByte == 0) isBinary = true;

            if (OnPluginProgressChanged != null && workingStreamOne.Length > 0 &&
                (int)(workingStreamOne.Position * 100 / workingStreamOne.Length) > position)
            {
              position = (int)(workingStreamOne.Position * 100 / workingStreamOne.Length);
              OnPluginProgressChanged(this,
                new PluginProgressEventArgs(workingStreamOne.Position, workingStreamOne.Length));
            }
          } while ((streamOneByte == streamTwoByte) && (streamOneByte != -1));

          // Return the success of the comparison. "file1byte" is 
          // equal to "file2byte" at this point only if the files are 
          // the same.
          InputsAreEqual = ((streamOneByte - streamTwoByte) == 0);
          DateTime stopTime = DateTime.Now;
          TimeSpan duration = stopTime - startTime;
          GuiTextChanged(
            "Comparison complete. Files are " + (InputsAreEqual ? "equal" : "unequal") + ".", NotificationLevel.Info);
          if (!InputsAreEqual)
            GuiTextChanged("First position a different byte: " + workingStreamOne.Position, NotificationLevel.Info);

          if (OnPluginProgressChanged != null) OnPluginProgressChanged(this, new PluginProgressEventArgs(1, 1));
            GuiTextChanged("Duration: " + duration, NotificationLevel.Info);

          if (settings.Diff) CreateDiffView();
        }

        if (InputsAreEqual)
        {
          // QuickWatchInfo = "Inputs are equal";
          StatusChanged((int)StreamComparatorImage.Equal);
        }
        else
        {
          // QuickWatchInfo = "Inputs are unequal";
          StatusChanged((int)StreamComparatorImage.Unequal);
        }
      }
      if (workingStreamOne == null)
        GuiTextChanged("Stream one is null, no comparison done.", NotificationLevel.Warning);
      if (workingStreamTwo == null)
        GuiTextChanged("Stream two is null, no comparison done.", NotificationLevel.Warning);
    }

    private StringBuilder result;

    /// <summary>
    /// Creates the diff view.
    /// </summary>
    public void CreateDiffView()
    {      
      if (!isBinary || true)
      {
        int maxLength = 65536;
        GuiTextChanged("Generating diff now...", NotificationLevel.Info);
        result = new StringBuilder();
        try
        {
          CryptoolStream cryptoolStream = InputOne;
          CryptoolStream cryptoolStream2 = InputTwo;

          if (cryptoolStream.Length > maxLength || cryptoolStream2.Length > maxLength)
            GuiTextChanged("Streams too big for complete diff, reading end of files only.", NotificationLevel.Warning);

          long startIndex = Math.Max(
            cryptoolStream.Length - maxLength, 
            cryptoolStream2.Length - maxLength);

          StreamReader sr = new StreamReader(cryptoolStream, Encoding.ASCII);          
          StringBuilder strTxt1 = new StringBuilder();

          int size = startIndex > 0 ? (int)(cryptoolStream.Length - startIndex) : (int)cryptoolStream.Length;
          char[] bArr = new char[size];

          if (startIndex > 0) sr.BaseStream.Seek(startIndex, SeekOrigin.Begin);
          sr.Read(bArr, 0, bArr.Length);
          bool test = sr.EndOfStream;
          cryptoolStream.Close();

          for (int i = 0; i < bArr.Length; i++)
            strTxt1.Append(bArr[i]);          

          sr = new StreamReader(cryptoolStream2, Encoding.ASCII);
          if (startIndex > 0) sr.BaseStream.Seek(startIndex, SeekOrigin.Begin);
          StringBuilder strTxt2 = new StringBuilder();

          size = startIndex > 0 ? (int)(cryptoolStream2.Length - startIndex) : (int)cryptoolStream2.Length;
          bArr = new char[size];
          sr.Read(bArr, 0, bArr.Length);
          test = sr.EndOfStream;
          cryptoolStream2.Close();

          for (int i = 0; i < bArr.Length; i++)
            strTxt2.Append(bArr[i]);
          

          string[] aLines = strTxt1.ToString().Split('\n');
          string[] bLines = strTxt2.ToString().Split('\n');

          Diff diff = new Diff();
          Diff.Item[] diffItem = diff.DiffText(strTxt1.ToString(), strTxt2.ToString());

          result.AppendLine("<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">");
          result.AppendLine("<Table CellSpacing=\"0\" FontFamily=\"Tahoma\" FontSize=\"12\" Background=\"White\">");
          result.AppendLine("<Table.Columns><TableColumn Width=\"40\" /><TableColumn/></Table.Columns>");
          result.AppendLine("<TableRowGroup>");


          string color = InputsAreEqual ? "#80FF80" : "#FF8080";

          result.AppendLine("<TableRow Background=\"" + color + "\"><TableCell ColumnSpan=\"2\" TextAlignment=\"Left\">");
          result.AppendLine("<Paragraph FontSize=\"9pt\" FontWeight=\"Bold\">");
          if (InputsAreEqual) result.AppendLine("Input streams are equal.");
          else result.AppendLine("Input streams are unequal.");
          result.AppendLine("</Paragraph></TableCell></TableRow>");
          
          result.AppendLine("<TableRow Background=\"SkyBlue\"><TableCell ColumnSpan=\"2\" TextAlignment=\"Left\">");
          result.AppendLine("<Paragraph FontSize=\"9pt\" FontWeight=\"Bold\">");
          result.AppendLine(streamOne.FileName);
          result.AppendLine("</Paragraph></TableCell></TableRow>");

          result.AppendLine("<TableRow Background=\"SkyBlue\"><TableCell ColumnSpan=\"2\" TextAlignment=\"Left\">");
          result.AppendLine("<Paragraph FontSize=\"9pt\" FontWeight=\"Bold\">");
          result.AppendLine(streamTwo.FileName);
          result.AppendLine("</Paragraph></TableCell></TableRow>");


          int n = 0;
          for (int fdx = 0; fdx < diffItem.Length; fdx++)
          {
            if (stop) break;
            Diff.Item aItem = diffItem[fdx];

            // write unchanged lines
            while ((n < aItem.StartB) && (n < bLines.Length))
            {
              StatusBarProgressbarValueChanged(n, bLines.Length);
              WriteLine(n, DiffMode.NoChange, bLines[n]);
              n++;
            } // while

            // write deleted lines
            for (int m = 0; m < aItem.deletedA; m++)
            {
              StatusBarProgressbarValueChanged(n, bLines.Length);
              WriteLine(-1, DiffMode.Remove, aLines[aItem.StartA + m]);
            } // for

            // write inserted lines
            while (n < aItem.StartB + aItem.insertedB)
            {
              StatusBarProgressbarValueChanged(n, bLines.Length);
              WriteLine(n, DiffMode.Add, bLines[n]);
              n++;
            } // while
          } // while

          // write rest of unchanged lines
          while (n < bLines.Length && !stop)
          {
            StatusBarProgressbarValueChanged(n, bLines.Length);
            WriteLine(n, DiffMode.NoChange, bLines[n]);
            n++;
          } // while
          result.AppendLine("</TableRowGroup></Table></FlowDocument>");
          StatusBarProgressbarValueChanged(1, 2);
          CryptoolStream cs = new CryptoolStream();
          cs.OpenRead(this.GetPluginInfoAttribute().Caption, Encoding.Default.GetBytes(result.ToString()));
          streamComparatorPresentation.SetContent(cs);
          cs.Close();
          StatusBarProgressbarValueChanged(1, 1);
        }
        catch (Exception exception)
        {
          GuiTextChanged(exception.Message, NotificationLevel.Error);
          GuiTextChanged("There were erros while creating the diff. Is this really text input?", NotificationLevel.Error);
        }
      }
      else
      {
        GuiTextChanged("BinaryInput, no comparison done.", NotificationLevel.Warning);
        streamComparatorPresentation.SetBinaryDocument();
      }
    }

    // public string Xaml;

    private enum DiffMode
    {
      Add, Remove, NoChange
    }


    /// <summary>
    /// Writes the line.
    /// </summary>
    /// <param name="lineNumber">The line number.</param>
    /// <param name="dm">The DiffMode.</param>
    /// <param name="aText">A text.</param>
    private void WriteLine(int lineNumber, DiffMode dm, string aText)
    {
      result.Append("<TableRow><TableCell Background=\"lightgray\"><Paragraph>");
      if (lineNumber != -1) result.Append((lineNumber + 1).ToString("0000"));
      else result.Append(" ");

      result.Append("</Paragraph></TableCell><TableCell>");

      if (dm == DiffMode.Add)
        result.Append("<Paragraph Background=\"#80FF80\">");
      else if (dm == DiffMode.Remove)
        result.Append("<Paragraph Background=\"#FF8080\">");
      else
        result.Append("<Paragraph>");

      // result.Append("<TextBlock TextWrapping=\"NoWrap\">");

      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);      
      XmlTextWriter xtw = new XmlTextWriter(sw);      
      xtw.WriteString(removeUnprintablesAndUnicode(aText));
      // xtw.WriteString(aText);
      xtw.Flush();
      xtw.Close();

      result.AppendLine(sb.ToString() + "</Paragraph></TableCell></TableRow>\n");
    }

    /// <summary>
    /// Removes the unprintables and unicode.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns></returns>
    public string removeUnprintablesAndUnicode(string text)
    {
      text = text.Replace("\r", "");
      string outputs = String.Empty;
      for (int jj = 0; jj < text.Length; jj++)
      {
        char ch = text[jj];        
        if (((int)(byte)ch) >= 32 & ((int)(byte)ch) <= 128)
        {
          outputs += ch;
        }
        else
        {
          outputs += ".";
        }
      }
      return outputs;
    }

    public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

    public event PluginProgressChangedEventHandler OnPluginProgressChanged;

    public event StatusChangedEventHandler OnPluginStatusChanged;

    public string Title { get; set; }

    public bool HasChanges
    {
      get { return settings.HasChanges; }
      set { settings.HasChanges = value; }
    }

    public UserControl Presentation
    {
      get { return (UserControl)streamComparatorPresentation; }
      set { }
    }

    public UserControl QuickWatchPresentation
    {
      get { return null; }
    }    

    public void Initialize()
    {
    }

    public void Dispose()
    {
      foreach (CryptoolStream stream in listCryptoolStreams)
      {
        if (stream != null) stream.Close();
      }
      listCryptoolStreams.Clear();
      if (streamOne != null)
      {
        this.streamOne.Close();
        this.streamOne = null;
      }
      if (streamTwo != null)
      {
        this.streamTwo.Close();
        this.streamTwo = null;
      }
    }

    public void Stop()
    {
      stop = true;
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
      EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
    }

    #endregion

    # region methods
    private void GuiTextChanged(string message, NotificationLevel logLevel)
    {
      EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
    }

    private void StatusBarProgressbarValueChanged(double value, double maxValue)
    {
      EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, maxValue));
    }

    private void StatusChanged(int imageIndex)
    {
      EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, imageIndex));
    }
    #endregion

    #region IPlugin Members


    public void PreExecution()
    {
      // QuickWatchInfo = null;
      streamComparatorPresentation.SetNoComparisonYetDocument();
      // InputsAreEqual = false;
    }

    public void PostExecution()
    {
      Dispose(); 
    }

    #endregion

    #region IPlugin Members

    public void Pause()
    {
      
    }

    #endregion
  }

  enum StreamComparatorImage
  {
    Default, 
    Equal, 
    Unequal
  }
}