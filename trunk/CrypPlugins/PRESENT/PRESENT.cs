/*
   Copyright 2008 Timm Korte, University of Siegen

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

// Cryptool PRESENT Plugin
// Author: Timm Korte, cryptool@easycrypt.de
// PRESENT information: http://www.crypto.rub.de/imperia/md/content/texte/publications/conferences/present_ches2007.pdf

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;
using System.Windows.Controls;
using System.Runtime.CompilerServices;
using Cryptool.PluginBase.Miscellaneous;
using System.Runtime.Remoting.Contexts;
using System.Windows;
using System.Windows.Threading;
using System.Threading;


namespace Cryptool.PRESENT
{
  [Author("Timm Korte", "cryptool@easycrypt.de", "Uni Bochum", "http://www.ruhr-uni-bochum.de")]
  [PluginInfo(false, "PRESENT", "PRESENT is an ultra-lightweight block cipher", "PRESENT/DetailedDescription/Description.xaml", "PRESENT/icon.png", "PRESENT/Images/encrypt.png", "PRESENT/Images/decrypt.png")]
  [EncryptionType(EncryptionType.SymmetricBlock)]
  [Synchronization(SynchronizationAttribute.REQUIRES_NEW)]
  public class PRESENT : ContextBoundObject, IEncryption
  {
    #region Private variables
    private PRESENTSettings settings;
    private PRESENTAnimation presentation;
    private CryptoolStream inputStream;
    private CryptoolStream outputStream;
    private byte[] inputKey;
    private byte[] inputIV;
    private CryptoStream p_crypto_stream_enc;
    private CryptoStream p_crypto_stream_dec;
    private bool stop;
    private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();
    #endregion

    
    public PRESENT()
    {
      this.presentation = new PRESENTAnimation();
      this.settings = new PRESENTSettings();      
    }

    public ISettings Settings
    {
      
      get { return (ISettings)this.settings; }
      
      set { this.settings = (PRESENTSettings)value; }
    }

    [PropertyInfo(Direction.InputData, "Input", "Data to be encrypted or decrypted.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
    public CryptoolStream InputStream
    {
      
      get 
      {
        if (inputStream != null)
        {
          CryptoolStream cs = new CryptoolStream();
          cs.OpenRead(inputStream.FileName);
          listCryptoolStreamsOut.Add(cs);
          return cs;
        }
        else return null;        
      }
      
      set 
      {
        if (value != inputStream)
        {
          this.inputStream = value;
          if (value != null) listCryptoolStreamsOut.Add(value);
          OnPropertyChanged("InputStream");
        }
      }
    }

    [PropertyInfo(Direction.InputData, "Key", "Must be 10 or 16 bytes.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
    public byte[] InputKey
    {
      
      get { return this.inputKey; }
      
      set 
      {
        if (value != inputKey)
        {
          this.inputKey = value;
          // OnPropertyChanged("InputKey");
        }
      }
    }

    [PropertyInfo(Direction.InputData, "IV", "IV to be used in chaining modes, must be 8 bytes.", "", false, false, DisplayLevel.Professional, QuickWatchFormat.Hex, null)]
    public byte[] InputIV
    {
      
      get { return this.inputIV; }
      
      set 
      {
        if (value != inputIV)
        {
          this.inputIV = value;
          OnPropertyChanged("InputIV");
        }
      }
    }

    [PropertyInfo(Direction.OutputData, "Output stream", "Encrypted or decrypted output data", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
    public CryptoolStream OutputStream
    {
      
      get 
      {         
        if (this.outputStream != null && File.Exists(this.outputStream.FileName))
        {
          CryptoolStream cs = new CryptoolStream();
          listCryptoolStreamsOut.Add(cs);
          cs.OpenRead(this.outputStream.FileName);
          return cs;
        }
        if (outputStream != null && !File.Exists(this.outputStream.FileName))
          GuiLogMessage("Can't find the created output filename.", NotificationLevel.Error);
        return null;
      }
      
      set 
      { 
        outputStream = value;
        if (value != null) listCryptoolStreamsOut.Add(value);
        OnPropertyChanged("OutputStream");
      }
    }

    private void ConfigureAlg(SymmetricAlgorithm alg, bool encrypt)
    {
      switch (settings.Mode)
      { // 0="ECB"=default, 1="CBC", 2="CFB", 3="OFB"
        case 1: alg.Mode = CipherMode.CBC; break;
        case 2: alg.Mode = CipherMode.CFB; break;
        case 3: alg.Mode = CipherMode.OFB; break;
        default: alg.Mode = CipherMode.ECB; break;
      }
      switch (settings.Padding)
      { // 0="Zeros"=default, 1="None", 2="PKCS7"
        case 1: alg.Padding = PaddingMode.None; break;
        case 2: alg.Padding = PaddingMode.PKCS7; break;
        default: alg.Padding = PaddingMode.Zeros; break;
      }

      // Check input data
      if (this.inputStream == null)
      { // no input connected
        GuiLogMessage("ERROR - No input data provided", NotificationLevel.Error);
      }
      else if ((this.inputStream.Length % (alg.BlockSize >> 3)) != 0)
      {
        if (!encrypt)
        { // when decrypting, input size must be multiple of blocksize
          GuiLogMessage("ERROR - When decrypting, the input length (" + this.inputStream.Length + " bytes) has to be a multiple of the blocksize (n*" + (alg.BlockSize >> 3) + " bytes).", NotificationLevel.Error);
        }
        else if (alg.Padding == PaddingMode.None)
        { // without padding, input size must be multiple of blocksize
          GuiLogMessage("ERROR - Without padding, the input length (" + this.inputStream.Length + " bytes) has to be a multiple of the blocksize (n*" + (alg.BlockSize >> 3) + " bytes).", NotificationLevel.Error);
          GuiLogMessage("WARNING - Input length (" + this.inputStream.Length + " bytes) is not multiple of blocksize, switching to PKCS7 padding.", NotificationLevel.Warning);
          alg.Padding = PaddingMode.PKCS7;
          settings.Padding = 2;
        }
      }

      // Check Key
      if (this.inputKey == null)
      { //key is required, "null" is an Error
        GuiLogMessage("ERROR - No key provided", NotificationLevel.Error);
        GuiLogMessage("WARNING - No key provided. Using 0x000..00!", NotificationLevel.Warning);
        this.inputKey = new byte[10];
      }
      else if ((this.inputKey.Length != 10) && (this.inputKey.Length != 16))
      { // invalid key length
        GuiLogMessage("ERROR - Invalid key length (" + this.inputKey.Length + " bytes), must be 10 or 16 bytes", NotificationLevel.Error);
        this.inputKey = new byte[10];
      }
      alg.Key = this.inputKey;

      // Check IV
      if (this.inputIV == null)
      { // IV might be optional, "null" = none given
        if (alg.Mode != CipherMode.ECB)
        { // if not using ECB, we need an IV -> generate default 0x00...0
          GuiLogMessage("WARNING - No IV for chaining mode (" + alg.Mode.ToString() + ") provided. Using 0x000..00!", NotificationLevel.Warning);
        }
        this.inputIV = new byte[8];
      }
      else if (this.inputIV.Length != 8)
      { // invalid IV length
        GuiLogMessage("ERROR - Invalid IV length (" + this.inputIV.Length + " bytes), must be 8 bytes", NotificationLevel.Error);
        this.inputIV = new byte[8];
      }
      alg.IV = this.inputIV;
      GuiLogMessage(
          "cipher " + (encrypt ? "encryption" : "decryption") + " info:\n"
          + "\tkey: 0x" + getHex(alg.Key) + "\n"
          + ((alg.Mode == CipherMode.ECB) ? "" : "\tIV: 0x" + getHex(alg.IV) + "\n")
          + "\tchaining mode: " + alg.Mode.ToString() + "\n"
          + "\tpadding mode: " + alg.Padding.ToString(),
          NotificationLevel.Info);      
    }

    
    private string getHex(byte[] p)
    {
      StringBuilder strHex = new StringBuilder();
      for (int i = 0; i < p.Length; i++)
      {
        strHex.Append(p[i].ToString("X2"));
      }
      return strHex.ToString();
    }
    
    public void Encrypt()
    {
      if (this.inputStream != null)
      {
        // Encrypt Stream
        try
        {
          if (this.inputStream.CanSeek) this.inputStream.Position = 0;
          SymmetricAlgorithm p_alg = new PresentManaged();
          ConfigureAlg(p_alg, true);

          if ((this.presentation != null) & (p_alg.KeySize == 80)) {
              byte[] block = new byte[8];
              byte[] key = (byte[])p_alg.Key.Clone();
              int r = InputStream.Read(block, 0, 8);
              if (this.inputStream.CanSeek) this.inputStream.Position = 0;
              if (r < 8) {
                  for (int i = 0; i < r; i++) {
                      block[7 - i] = block[r - i - 1];
                  }
                  byte p;
                  if (p_alg.Padding == PaddingMode.PKCS7) { p = (byte)(8 - r); } else { p = (byte)0; }
                  for (int i = 0; i < 8 - r; i++) block[i] = p;
              }
              this.presentation.Assign_Values(key, block);
          }

          ICryptoTransform p_encryptor = p_alg.CreateEncryptor();
          outputStream = new CryptoolStream();
          listCryptoolStreamsOut.Add(outputStream);
          outputStream.OpenWrite(this.GetPluginInfoAttribute().Caption);
          p_crypto_stream_enc = new CryptoStream((Stream)inputStream, p_encryptor, CryptoStreamMode.Read);
          byte[] buffer = new byte[p_alg.BlockSize / 8];
          int bytesRead;
          int position = 0;
          DateTime startTime = DateTime.Now;
          while ((bytesRead = p_crypto_stream_enc.Read(buffer, 0, buffer.Length)) > 0 && !stop)
          {
            outputStream.Write(buffer, 0, bytesRead);
            if ((int)(inputStream.Position * 100 / inputStream.Length) > position)
            {
              position = (int)(inputStream.Position * 100 / inputStream.Length);
              ProgressChanged(inputStream.Position, inputStream.Length);
            }
          }
          p_crypto_stream_enc.Flush();
          // p_crypto_stream_enc.Close();        
          
          DateTime stopTime = DateTime.Now;
          TimeSpan duration = stopTime - startTime;

          if (!stop)
          {
            GuiLogMessage("Encryption complete! (in: " + inputStream.Length.ToString() + " bytes, out: " + outputStream.Length.ToString() + " bytes)", NotificationLevel.Info);
            GuiLogMessage("Wrote data to file: " + outputStream.FileName, NotificationLevel.Info);
            GuiLogMessage("Time used: " + duration.ToString(), NotificationLevel.Debug);
            outputStream.Close();
            OnPropertyChanged("OutputStream");
          }
          if (stop)
          {
            GuiLogMessage("Aborted!", NotificationLevel.Info);
            outputStream.Close();
          }
        }
        catch (CryptographicException cryptographicException)
        {
          // TODO: For an unknown reason p_crypto_stream can not be closed after exception.
          // Trying so makes p_crypto_stream throw the same exception again. So in Dispose 
          // the error messages will be doubled. 
          // As a workaround we set p_crypto_stream to null here.
          p_crypto_stream_enc = null;
          GuiLogMessage(cryptographicException.Message, NotificationLevel.Error);
        }
        catch (Exception exception)
        {
          GuiLogMessage(exception.Message, NotificationLevel.Error);
        }
        finally
        {
          ProgressChanged(1, 1);
        }
      }
    }

    public void Decrypt()
    {
      if (this.inputStream != null)
      {
        // Decrypt Stream
        try
        {
          if (this.inputStream.CanSeek) this.inputStream.Position = 0;
          SymmetricAlgorithm p_alg = new PresentManaged();
          ConfigureAlg(p_alg, false);
          ICryptoTransform p_decryptor = p_alg.CreateDecryptor();
          outputStream = new CryptoolStream();
          listCryptoolStreamsOut.Add(outputStream);
          outputStream.OpenWrite(this.GetPluginInfoAttribute().Caption);
          p_crypto_stream_dec = new CryptoStream((Stream)inputStream, p_decryptor, CryptoStreamMode.Read);
          byte[] buffer = new byte[p_alg.BlockSize / 8];
          int bytesRead;
          int position = 0;
          DateTime startTime = DateTime.Now;
          while ((bytesRead = p_crypto_stream_dec.Read(buffer, 0, buffer.Length)) > 0 && !stop)
          {
            outputStream.Write(buffer, 0, bytesRead);
            if ((int)(inputStream.Position * 100 / inputStream.Length) > position)
            {
              position = (int)(inputStream.Position * 100 / inputStream.Length);
              ProgressChanged(inputStream.Position, inputStream.Length);
            }
          }
          p_crypto_stream_dec.Flush();
          p_crypto_stream_dec.Close();
          DateTime stopTime = DateTime.Now;
          TimeSpan duration = stopTime - startTime;
          if (!stop)
          {
            GuiLogMessage("Decryption complete! (in: " + inputStream.Length.ToString() + " bytes, out: " + outputStream.Length.ToString() + " bytes)", NotificationLevel.Info);
            GuiLogMessage("Wrote data to file: " + outputStream.FileName, NotificationLevel.Info);
            GuiLogMessage("Time used: " + duration.ToString(), NotificationLevel.Debug);
            outputStream.Close();
            OnPropertyChanged("OutputStream");
          }
          if (stop)
          {
              outputStream.Close();
              GuiLogMessage("Aborted!", NotificationLevel.Info);
          }
        }
        catch (CryptographicException cryptographicException)
        {
          // TODO: For an unknown reason p_crypto_stream can not be closed after exception.
          // Trying so makes p_crypto_stream throw the same exception again. So in Dispose 
          // the error messages will be doubled. 
          // As a workaround we set p_crypto_stream to null here.
          p_crypto_stream_dec = null;
          GuiLogMessage(cryptographicException.Message, NotificationLevel.Error);
        }
        catch (Exception exception)
        {
          GuiLogMessage(exception.Message, NotificationLevel.Error);
        }
        finally
        {
          ProgressChanged(1, 1);
        }
      }
    }

    #region IPlugin Member

#pragma warning disable 67
		public event StatusChangedEventHandler OnPluginStatusChanged;
		public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
		public event PluginProgressChangedEventHandler OnPluginProgressChanged;
#pragma warning restore


    public System.Windows.Controls.UserControl Presentation
    {
        get { return presentation; }
    }

    public UserControl QuickWatchPresentation
    {
      get { return presentation; }
    }

    public void Initialize()
    {
      
    }
 
    public void Dispose()
    {
      try
      {
        stop = false;
        inputKey = null;
        inputIV = null;
        inputStream = null;
        outputStream = null;

        foreach (CryptoolStream cryptoolStream in listCryptoolStreamsOut)
        {
          cryptoolStream.Close();
        }
        listCryptoolStreamsOut.Clear();

        if (p_crypto_stream_dec != null)
        {
          p_crypto_stream_dec.Flush();
          p_crypto_stream_dec.Close();
          p_crypto_stream_dec = null;
        }

        if (p_crypto_stream_enc != null)
        {
          p_crypto_stream_enc.Flush();
          p_crypto_stream_enc.Close();
          p_crypto_stream_enc = null;
        }
      }
      catch (Exception exception)
      {
        GuiLogMessage(exception.Message, NotificationLevel.Error);
      }

      this.stop = false;
    }

    public void Stop()
    {
      this.stop = true;
    }

    public void PreExecution()
    {
      Dispose();
      //stop = false;
      //inputIV = null;
    }

    public void PostExecution()
    {
      Dispose();
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;
    
    public void OnPropertyChanged(string name)
    {
      EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
      //if (PropertyChanged != null)
      //{
      //  PropertyChanged(this, new PropertyChangedEventArgs(name));
      //}
    }
    
    private void GuiLogMessage(string message, NotificationLevel logLevel)
    {
      EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
      //if (OnGuiLogNotificationOccured != null)
      //{
      //  OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
      //}
    }
    
    private void ProgressChanged(double value, double max)
    {
      EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
      //if (OnPluginProgressChanged != null)
      //{
      //  OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));
      //}
    }

    #endregion

    #region IPlugin Members

    
    public void Execute()
    {
      switch (settings.Action)
      {
        case 0:
          Encrypt();
          break;
        case 1:
          Decrypt();
          break;
        default:
          break;
      }      
    }

    
    public void Pause()
    {
      
    }

    #endregion
  }
}
