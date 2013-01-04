using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Cryptool.Plugins.StegoPermutation
{
    public class BigIntegerClass
    {
        public BigInteger BigIntegerStruct = 0;

        public BigIntegerClass(BigInteger value)
        {
            this.BigIntegerStruct = value;
        }
    }

    public class StegoPermutationDataContext : INotifyPropertyChanged
    {
        public StegoPermutationDataContext()
        {
            this.list = new Collection<string>();
        }

        private IList list;
        public IList List
        {
            get
            {
                return list;
            }
            set
            {
                if (value == null) return;

                this.number = -1;
                this.list.Clear();
                foreach (string s in value)
                {
                    this.list.Add(s);
                }

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Text"));
                }
            }
        }

        private BigInteger number;
        public BigInteger Number
        {
            get
            {
                return this.number;
            }
            set
            {
                this.list = null;
                this.number = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Text"));
                }
            }
        }

        private string ListItemToChar(int index)
        {
            if (list[index] == null)
            {
                return "_";
            }
            else
            {
                return list[index].ToString();
            }
        }

        public string Text
        {
            get
            {
                if((list != null)&&(list.Count > 0))
                {
                    string text = ListItemToChar(0);
                    for (int n = 1; n < list.Count; n++)
                    {
                        text += "," + ListItemToChar(n);
                    }
                    return text;
                }
                else if (number > -1)
                {
                    return number.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }


    /// <summary>
    /// Interaktionslogik für UserControl1.xaml
    /// </summary>
    public partial class StegoPermutationPresentation : UserControl
    {
        private StegoPermutationDataContext inputList = new StegoPermutationDataContext();
        private StegoPermutationDataContext resultDataContext = new StegoPermutationDataContext();

        public StegoPermutationDataContext InputList
        {
            get
            {
                return this.inputList;
            }
            set
            {
                this.inputList = value;
            }
        }

        public StegoPermutationDataContext ResultList
        {
            get
            {
                return this.resultDataContext;
            }
            set
            {
                this.resultDataContext = value;
            }
        }

        public void UpdateInputList(Collection<string> value)
        {
            this.inputList.List = value;
        }

        public void UpdateResultList(IList value)
        {
            this.resultDataContext.List = value;
        }

        public void UpdateResultNumber(BigIntegerClass value)
        {
            this.resultDataContext.Number = value.BigIntegerStruct;
        }

        public StegoPermutationPresentation()
        {
            InitializeComponent();

            Binding srcTextBinding = new Binding();
            srcTextBinding.Source = this.InputList;
            srcTextBinding.Path = new PropertyPath("Text");
            srcTextBinding.Mode = BindingMode.OneWay;
            srcTextBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            Binding resTextBinding = new Binding();
            resTextBinding.Source = this.ResultList;
            resTextBinding.Path = new PropertyPath("Text");
            resTextBinding.Mode = BindingMode.OneWay;
            resTextBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            txtSrcList.DataContext = this.InputList;
            txtSrcList.SetBinding(TextBox.TextProperty, srcTextBinding);

            txtResultList.DataContext = this.ResultList;
            txtResultList.SetBinding(TextBox.TextProperty, resTextBinding);
        }
    }
}
