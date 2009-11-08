using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;

namespace Transposition
{
    /// <summary>
    /// Interaktionslogik für TranspositionPresentation.xaml
    /// </summary>
    public partial class TranspositionPresentation : UserControl
    {
        public TranspositionPresentation()
        {
            InitializeComponent();
        }

        public event EventHandler Completed;
        private TextBlock[,] teba;
        public int timeCounter = 0;
        private int von;
        private int nach;
        private bool PaarSortiert;
        private int schleife=0;
        private int outcount;
        private bool onlyOnce ;

        public void main(char[,] read_in_matrix, char[,] permuted_matrix, int[] key, String keyword, String input, String output,int per, int rein,int reout)
        {
            
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (per == 1 && rein == 0 && reout == 1)
                {
                    outcount = 0;
                    onlyOnce = true;
                    create(read_in_matrix, permuted_matrix, key, keyword, input, output);
                    schleife = 0;
                    sort(schleife);
                }
            }, null);
        }

        

        public void readout(object sender, EventArgs e) 
        {
            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = 1.0;
            myDoubleAnimation.To = 0.0;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1000));

            if (teba != null)
                
                for (int i = 0; i < teba.GetLength(1); i++)
                {
                    
                   
                    if (i == teba.GetLength(1) - 1 && outcount<teba.GetLength(0)-1)
                    {
                        myDoubleAnimation.Completed += new EventHandler(my_Help1);
                    }
                    teba[outcount, i].BeginAnimation(TextBlock.OpacityProperty, myDoubleAnimation);
                    if (i > 1)
                    {
                        textBox2.Text += teba[outcount, i].Text.ToString();
                    }
                }
        }

        public void sort(int i)
        {
            if (teba != null)
            {
                if (i < teba.GetLength(0) - 1)
                {
                    if (Convert.ToInt32(teba[i, 0].Text) > Convert.ToInt32(teba[i + 1, 0].Text))
                    {
                        ani(this, EventArgs.Empty, i, i + 1);
                        PaarSortiert = false;
                    }
                    else 
                        {
                            schleife++;
                            sort(schleife); 
                        }
                }
                else if (!PaarSortiert)
                {
                    schleife = 0;
                    PaarSortiert = true;
                    sort(0);
                }

                if (PaarSortiert && onlyOnce) { readout(this, EventArgs.Empty); onlyOnce = false; }
            }
            
        }
        private void my_Help1(object sender, EventArgs e) 
        { 
            outcount++;
            readout(this, EventArgs.Empty);
           
        }
        private void my_Help(object sender, EventArgs e)
        {
            schleife++;
            sort(schleife);
        }
        
        private void my_Completed(object sender, EventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (teba != null)
                {

                    for (int i = 0; i < teba.GetLength(1); i++)
                    {
                        String help = teba[nach, i].Text.ToString();
                        teba[nach, i].Text = teba[von, i].Text.ToString();
                        teba[von, i].Text = help;
                        textBox1.Text += i;
                    }
                }
                textBox1.Text += "feuer";
                DoubleAnimation myFadein = new DoubleAnimation();
                myFadein.From = 0.0;
                myFadein.To = 1.0;
                myFadein.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
                

                if (teba != null)
                    for (int i = 0; i < teba.GetLength(1); i++)
                    {
                        teba[von, i].BeginAnimation(TextBlock.OpacityProperty, myFadein);
                        if (i == teba.GetLength(1)-1)
                        {
                            myFadein.Completed += new EventHandler(my_Help);
                        } 
                        
                        teba[nach, i].BeginAnimation(TextBlock.OpacityProperty, myFadein);
                    }
               
            }, null);
        }

        public void ani(object sender, EventArgs e, int von, int nach)
        {
            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = 1.0;
            myDoubleAnimation.To = 0.0;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1000));

            this.von = von;
            this.nach = nach;
            
            if(teba!=null)
            for (int i=0; i < teba.GetLength(1); i++)
            {
                
                teba[von, i].BeginAnimation(TextBlock.OpacityProperty, myDoubleAnimation);
                if (i == teba.GetLength(1)- 1)
                {
                    myDoubleAnimation.Completed += new EventHandler(my_Completed);
                }
                teba[nach, i].BeginAnimation(TextBlock.OpacityProperty, myDoubleAnimation);    
            }
        }

        public void create(char[,] read_in_matrix, char[,] permuted_matrix, int[] key, String keyword, String input, String output)
        {
            
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (read_in_matrix != null && key != null)
                {
                    myGrid.Children.Clear();
                    myGrid.RowDefinitions.Clear();
                    myGrid.ColumnDefinitions.Clear();
                    textBox1.Clear();
                    textBox2.Clear();
                    
                    myGrid.ShowGridLines = true;

                    teba = new TextBlock[read_in_matrix.GetLength(0), read_in_matrix.GetLength(1)+2];
                    myGrid.RowDefinitions.Add(new RowDefinition());

                    for (int i = 0; i < key.Length; i++)
                    {
                        myGrid.ColumnDefinitions.Add(new ColumnDefinition());
                        TextBlock txt = new TextBlock();
                        String s = key[i].ToString();
                        txt.VerticalAlignment = VerticalAlignment.Center;
                        txt.Text = s;
                        txt.FontSize = 12;
                        txt.FontWeight = FontWeights.ExtraBold;
                        txt.TextAlignment = TextAlignment.Center;
                        Grid.SetRow(txt, 0);
                        Grid.SetColumn(txt, i);
                        myGrid.Children.Add(txt);
                        teba[i, 0] = txt;
                        
                    }
                    myGrid.RowDefinitions.Add(new RowDefinition());
                    char[] ch = keyword.ToCharArray();

                    for (int i = 0; i < key.Length; i++)
                    {

                        TextBlock txt = new TextBlock();
                        txt.VerticalAlignment = VerticalAlignment.Center;
                        txt.Text = ch[i].ToString();
                        txt.FontSize = 12;
                        txt.FontWeight = FontWeights.ExtraBold;
                        txt.TextAlignment = TextAlignment.Center;
                        Grid.SetRow(txt, 1);
                        Grid.SetColumn(txt, i);
                        myGrid.Children.Add(txt);
                        teba[i, 1] = txt;
                    }

                    for (int i = 0; i < read_in_matrix.GetLength(1); i++)
                    {
                        myGrid.RowDefinitions.Add(new RowDefinition());
                        for (int ix = 0; ix < read_in_matrix.GetLength(0); ix++)
                        {
                            TextBlock txt = new TextBlock();
                            txt.VerticalAlignment = VerticalAlignment.Center;
                            txt.Text = read_in_matrix[ix, i].ToString();
                            txt.FontSize = 12;
                            txt.FontWeight = FontWeights.ExtraBold;
                            txt.TextAlignment = TextAlignment.Center;
                            Grid.SetRow(txt, i + 2);
                            Grid.SetColumn(txt, ix);
                            myGrid.Children.Add(txt);
                            teba[ix, i+2] = txt;
                        }
                    }
                    
                }
             
            }
         , null);
        }
        
    }

}
