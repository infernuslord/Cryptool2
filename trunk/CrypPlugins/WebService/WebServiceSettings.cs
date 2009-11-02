using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace WebService
{
  public class WebServiceSettings:ISettings
    {
     //   public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
       
        #region ISettings Member
        private bool changes;
        public bool HasChanges
        {
            get
            {
                return changes;
            }
            set
            {
                changes = value;

            }
        }

        #endregion
        
        private string methodName;
        [TaskPane("Methodenname", "Benennen Sie ihre Web Methode", "Konfigurieren", 1, false, DisplayLevel.Expert, ControlType.TextBox,"")]
        public string MethodName
        {
            get
            {
                return methodName;
            }
            set
            {
                methodName= value;
                OnPropertyChanged("MethodName");
            }
        }

        private int paramString;
        [TaskPane("Eingabeparameter vom Typ string", "Anzahl der Parameter vom Typ string", "Konfigurieren", 3, false, DisplayLevel.Expert, ControlType.ComboBox, new string[] {"0","1", "2"})]
        public int String
        {
            get { return paramString; }
            set
            {paramString=value;
                OnPropertyChanged("String");
            }
        }
        private int paramDouble;
        [TaskPane("Eingabeparameter vom Typ double", "Anzahl der Parameter vom Typ double", "Konfigurieren", 4, false, DisplayLevel.Expert, ControlType.ComboBox, new string[] { "0", "1", "2" })]
        public int Double
        {
            get { return paramDouble; }
            set
            {
                paramDouble = value;
                OnPropertyChanged("Double");
            }
        }
        private int integer;
      //  [ContextMenu("Eingabeparameter", "Erwartete Eingabeparameter", 0, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, "True", "False")]
       [TaskPane("Eingabeparameter vom Typ int", "Anzahl der Parameter vom Typ int","Konfigurieren", 2, false, DisplayLevel.Expert, ControlType.ComboBox, new string[] {"0","1","2"})]
        public int Integer
        {
            get
            {
                return integer;
            }
            set
            {
                integer = value;
                OnPropertyChanged("Integer");
            }
        }

       private int test;
       [TaskPane("Rückgabeparameter", "Wählen Sie den Typen des Rückgabeparameters", "Konfigurieren", 5, false, DisplayLevel.Expert, ControlType.ComboBox, new string[] { "void","int", "string", "float", "double"})]
       public int Test
       {
           get
           {
               return test;
           }
           set
           {
               test = value;
               OnPropertyChanged("Test");
           }
       }
       [TaskPane("WSDL veröffentlichen", "", "", 0, false, DisplayLevel.Beginner, ControlType.Button)]
       public void exportWSDL()
       {
           OnPropertyChanged("exportWSDL");
       }

       //[TaskPane("Schlüsselpaar erzeugen", "Erstellt ein RSA-Schlüsselpaar", "Key Management", 0, false, DisplayLevel.Beginner, ControlType.Button)]
       //public void createKey()
       //{
       //    OnPropertyChanged("createKey");
       //    {

       //    }
       //}

       [TaskPane("Öffentlichen Schlüssel bereitstellen", "Exportiert den öffentlichen Schlüssel", "Key Management", 1, false, DisplayLevel.Beginner, ControlType.Button)]
       public void publishKey()
       {
           OnPropertyChanged("publishKey");
           {

           }
       }

       [TaskPane("Vorgefertigte Methode", "Erstellt eine Testmethode, die zwei int Parameter entgegennimmt, diese addiert und das Ergebnis zurückgibt.","Konfigurieren", 0, false, DisplayLevel.Beginner, ControlType.Button)]
       public void MethodenStub()
       {
           OnPropertyChanged("MethodenStub");

       }
       
       private string targetFileName;
      [TaskPane("Target File Name","Target to write WSDL",null,0,false,DisplayLevel.Beginner,ControlType.SaveFileDialog, FileExtension="Cryptool Alphabet (*.cta)|*.cta")]
       public string TargetFilename
       {
           get { return targetFileName; }
           set
           {
               targetFileName = value;
               OnPropertyChanged("TargetFilename");
           }
       }
      private string userCode;
      
      public string UserCode
      {
          get { return userCode; }
          set
          {
              userCode = value;
              OnPropertyChanged("UserCode");
          }
      }
      //private int animationSpeed = 3;
      //[TaskPane("Animationsspeed", "Set the speed for animations", "Animation", 9, false, DisplayLevel.Beginner, ControlType.NumericUpDown, Cryptool.PluginBase.ValidationType.RangeInteger, 1, 5)]
      //public int AnimationSpeed
      //{
      //    get
      //    {
      //        return animationSpeed;
      //    }
      //    set
      //    {
      //        animationSpeed = value;
      //        OnPropertyChanged("AnimationSpeed");
      //    }
      //}
      private bool compiled;

      public bool Compiled
      {
          get { return compiled; }
          set
          {
              compiled = value;
              OnPropertyChanged("Compiled");

          }
      }
        #region INotifyPropertyChanged Member


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
