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

        
        private TextBlock[,] teba;
        private int von;
        private int nach;
        private int schleife = 0;
        private int outcount;
        private int outcount1;
        private bool Stop = false;
        private int rein;
        private int reout;

        public void main(char[,] read_in_matrix, char[,] permuted_matrix, int[] key, String keyword, String input, String output, int per, int rein, int reout)
        {

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (per == 1  )
                {
                    
                    this.rein = rein;
                    this.reout = reout;
                    textBox2.Clear();
                    if (reout == 1)
                        outcount = 0;
                    if (reout == 0)
                        outcount = 2;
                    
                    if(rein == 1)
                        outcount1 = 0;
                    if(rein == 0)
                        outcount1 = 2;
                    
                    Stop = false;
                    if (keyword == null)
                        Stop = true;
                    create(read_in_matrix, permuted_matrix, key, keyword, input, output);

                    schleife = 0;
                    textBox1.Text = input;
                    readIn();
                }
            }, null);
        }



        public void readout()
        {
            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = 1.0;
            myDoubleAnimation.To = 0.0;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1001-slider1.Value));

            if (teba != null)
            {
                if (reout == 1)
                    for (int i = 2; i < teba.GetLength(1); i++)
                    {


                        if (i == teba.GetLength(1) - 1 && outcount < teba.GetLength(0) - 1 && !Stop)
                        {
                            myDoubleAnimation.Completed += new EventHandler(my_Help1);
                        }
                        teba[outcount, i].BeginAnimation(TextBlock.OpacityProperty, myDoubleAnimation);
                        if (i > 1)
                        {
                            textBox2.Text += teba[outcount, i].Text.ToString();
                        }
                    }


                else
                {
                    for (int i = 0; i < teba.GetLength(0); i++)
                    {


                        if (i == teba.GetLength(0) - 1 && outcount < teba.GetLength(1) - 1 && !Stop)
                        {
                            myDoubleAnimation.Completed += new EventHandler(my_Help1);
                        }
                        teba[i,outcount].BeginAnimation(TextBlock.OpacityProperty, myDoubleAnimation);
                        if (outcount > 1)
                        {
                            textBox2.Text += teba[i,outcount].Text.ToString();
                        }
                    }
                }
            }
        }

        public void sort(int i)
        {
            if (teba != null)
            {
                if (i < teba.GetLength(0) - 1)
                {
                    if (Convert.ToInt32(teba[i, 0].Text) != i+1 )
                    {
                        int s =0;
                        for (int ix = i + 1; ix < teba.GetLength(0); ix++)
                        {
                            if (Convert.ToInt32(teba[ix, 0].Text) == i + 1)
                            {
                                s = ix;
                            }
                        }
                        ani( i, s);
                        
                    }
                    else
                    {
                        schleife++;
                        sort(schleife);
                    }
                    
                }
               

                else if ( !Stop){  readout(); }
            }

        }

        public void my_Stop(object sender, EventArgs e)
        {
            myGrid.Children.Clear();
            myGrid.ColumnDefinitions.Clear();
            myGrid.RowDefinitions.Clear();
            outcount = 0;
            
            schleife = 0;
            textBox1.Clear();
            textBox2.Clear();
            Stop = true;
        }

        private void my_Help1(object sender, EventArgs e)
        {
            outcount++;
            if (!Stop)
                readout();

        }

        private void my_Help2(object sender, EventArgs e)
        {
            outcount1++;
            if (!Stop)
                readIn();

        }

        private void my_Help(object sender, EventArgs e)
        {
            schleife++;
            if (!Stop)
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

                        TextBlock help1 = new TextBlock();
                        help1.Background = teba[nach, i].Background;
                        teba[nach, i].Background = teba[von, i].Background;
                        teba[von, i].Background = help1.Background;

                        
                    }
                }
                
                DoubleAnimation myFadein = new DoubleAnimation();
                myFadein.From = 0.0;
                myFadein.To = 1.0;
                myFadein.Duration = new Duration(TimeSpan.FromMilliseconds(1001-slider1.Value));


                if (teba != null)
                    for (int i = 0; i < teba.GetLength(1); i++)
                    {
                        teba[von, i].BeginAnimation(TextBlock.OpacityProperty, myFadein);
                        if (i == teba.GetLength(1) - 1 && !Stop)
                        {
                            myFadein.Completed += new EventHandler(my_Help);
                        }

                        teba[nach, i].BeginAnimation(TextBlock.OpacityProperty, myFadein);
                    }

            }, null);
        }

        public void ani( int von, int nach)
        {
            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = 1.0;
            myDoubleAnimation.To = 0.0;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1001-slider1.Value));

            this.von = von;
            this.nach = nach;

            if (teba != null)
                for (int i = 0; i < teba.GetLength(1); i++)
                {

                    teba[von, i].BeginAnimation(TextBlock.OpacityProperty, myDoubleAnimation);
                    if (i == teba.GetLength(1) - 1 && !Stop)
                    {
                        myDoubleAnimation.Completed += new EventHandler(my_Completed);
                    }
                    teba[nach, i].BeginAnimation(TextBlock.OpacityProperty, myDoubleAnimation);
                }
        }

        public void readIn()
        {



            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = 0.0;
            myDoubleAnimation.To = 1.0;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1001-slider1.Value));

            if (teba != null)
            {
                if (rein == 0)
                {
                    for (int i = 0; i < teba.GetLength(0); i++)
                    {


                        if (i == teba.GetLength(0) - 1 && outcount1 < teba.GetLength(1) - 1 && !Stop)
                        {
                            myDoubleAnimation.Completed += new EventHandler(my_Help2);
                        }
                        teba[i, outcount1].BeginAnimation(TextBlock.OpacityProperty, myDoubleAnimation);

                        try
                        {
                            textBox1.Text = textBox1.Text.Remove(0, 1);
                        }

                        catch (ArgumentOutOfRangeException) { }

                        if (i == teba.GetLength(0) - 1 && outcount1 == teba.GetLength(1) - 1 && !Stop)
                        { sort(schleife); }

                    }
                }

                else
                {
                    for (int i = 2; i < teba.GetLength(1); i++)
                    {


                        if (i == teba.GetLength(1) - 1 && outcount1 < teba.GetLength(0) - 1 && !Stop)
                        {
                            myDoubleAnimation.Completed += new EventHandler(my_Help2);
                        }
                        teba[outcount1,i].BeginAnimation(TextBlock.OpacityProperty, myDoubleAnimation);

                        try
                        {
                            textBox1.Text = textBox1.Text.Remove(0, 1);
                        }

                        catch (ArgumentOutOfRangeException) { }

                        if (i == teba.GetLength(1) - 1 && outcount1 == teba.GetLength(0) - 1 && !Stop)
                        { sort(schleife); }

                    }
                }
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

                    

                    mainGrid.Height = 100;
                    mainGrid.Width = 420;

                    myGrid.Width = 50;
                    myGrid.Height = 100;
                    myGrid.ShowGridLines = false;

                    teba = new TextBlock[read_in_matrix.GetLength(0), read_in_matrix.GetLength(1) + 2];
                    
                    myGrid.RowDefinitions.Add(new RowDefinition());
                    for (int i = 0; i < key.Length; i++)
                    {
                        if (mainGrid.Width < 800)
                        {
                            myGrid.Width += 12;
                            mainGrid.Width += 12;
                        }
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
                    if (keyword != null)
                    {
                        
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
                    }

                    for (int i = 0; i < read_in_matrix.GetLength(1); i++)
                    {
                        if (mainGrid.Height < 280)
                        {
                            mainGrid.Height += 12;
                            myGrid.Height += 12;
                        }


                        int x = 0;
                        myGrid.RowDefinitions.Add(new RowDefinition());
                        if (i % 2 == 0)
                            x = 1;
                        for (int ix = 0; ix < read_in_matrix.GetLength(0); ix++)
                        {

                            TextBlock txt = new TextBlock();
                            txt.VerticalAlignment = VerticalAlignment.Center;
                            if (char.GetUnicodeCategory(read_in_matrix[ix, i]).ToString() != "Control")
                                txt.Text = read_in_matrix[ix, i].ToString();
                            else
                                txt.Text = "";
                            if (ix % 2 == x)
                                txt.Background = Brushes.AliceBlue;
                            else
                                txt.Background = Brushes.LawnGreen;
                            txt.FontSize = 12;
                            txt.Opacity = 0.0;
                            txt.FontWeight = FontWeights.ExtraBold;
                            txt.TextAlignment = TextAlignment.Center;
                            Grid.SetRow(txt, i + 2);
                            Grid.SetColumn(txt, ix);
                            myGrid.Children.Add(txt);
                            teba[ix, i + 2] = txt;
                        }
                    }

                }

            }
         , null);
        }

    }

}
