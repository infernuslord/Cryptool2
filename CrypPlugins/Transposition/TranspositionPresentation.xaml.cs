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
        private int outcount2;
        private int outcount3;
        private int countup;
        private bool Stop = false;
        private int per;
        private int rein;
        private int reout;
        private TextBlock[] reina;
        private TextBlock[] reouta;
        private int speed = 1;
        private int rowper;
        private int colper;
        private byte[,] read_in_matrix;
        private DoubleAnimation fadeIn;
        private DoubleAnimation fadeOut;


        public void UpdateSpeed(int speed)
        {
            this.speed = speed;
        }

        private void init(byte[,] read_in_matrix, String keyword, int per, int rein, int reout) 
        {
            DoubleAnimation fadeIn = new DoubleAnimation();
            fadeIn.From = 0.0;
            fadeIn.To = 1.0;
            fadeIn.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));

            DoubleAnimation fadeOut = new DoubleAnimation();
            fadeOut.From = 0.0;
            fadeOut.To = 1.0;
            fadeOut.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));

            this.fadeIn = fadeIn;
            this.fadeOut = fadeOut;
            this.read_in_matrix=read_in_matrix;
            this.per = per;
            this.rein = rein;
            this.reout = reout;

            countup = 0;
            outcount2 = 0;
            
            Stop = false;
            if (keyword == null)
                Stop = true;

            textBox2.Clear();
            if (per == 1  )
                {
                   
                    rowper = 0;
                    colper = 2;
                    if (reout == 1)
                        outcount = 0;
                    if (reout == 0)
                        outcount = 2;
                    
                    if(rein == 1)
                        outcount1 = 0;
                    
                    if(rein == 0)
                        outcount1 = 2;

                    if (rein == 1)
                        outcount3 = 0;

                    if (rein == 0)
                        outcount3 = 2;
                    
                    
                }
            else
                {
                    rowper = 2;
                    colper = 0;

                   
                    if (reout == 1)
                        outcount = 2;
                    if (reout == 0)
                        outcount = 0;

                    if (rein == 1)
                        outcount1 = 2;
                    if (rein == 0)
                        outcount1 = 0;
                    
                    if (rein == 1)
                        outcount3 = 2;
                    if (rein == 0)
                        outcount3 = 0;
                    
                    
                    }
        }

        public void main(byte[,] read_in_matrix, byte[,] permuted_matrix, int[] key, String keyword, byte[] input, byte[] output, int per, int rein, int reout)
        {

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                    this.my_Stop(this, EventArgs.Empty);
                    init(read_in_matrix,keyword, per, rein, reout);
                    create(read_in_matrix, permuted_matrix, key, keyword, input, output);

                    schleife = 0;
                    //textBox1.Text = input;
                    //readIn();
     
                
            }, null);
        }


        
        public void readout()
        {
            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = 1.0;
            myDoubleAnimation.To = 0.0;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1001-speed));

            if (teba != null)
            {
                if (reout == 1)
                    for (int i = colper; i < teba.GetLength(1); i++)
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
                    for (int i = rowper; i < teba.GetLength(0); i++)
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
            if (per == 1)
            {
                if (teba != null)
                {
                    if (i < teba.GetLength(0) - 1)
                    {
                        if (Convert.ToInt32(teba[i, 0].Text) != i + 1)
                        {
                            int s = 0;
                            for (int ix = i + 1; ix < teba.GetLength(0); ix++)
                            {
                                if (Convert.ToInt32(teba[ix, 0].Text) == i + 1)
                                {
                                    s = ix;
                                }
                            }
                            ani(i, s);

                        }
                        else
                        {
                            schleife++;
                            sort(schleife);
                        }

                    }


                    else if (!Stop) { readout(); }
                }
            }
            else 
            
            {
                readout();
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
                postReadIn();

        }

        private void my_Help3(object sender, EventArgs e)
        {
            textBox2.Text = "feuer";
            if (!Stop)
            preReadIn();
        }
        
        private void my_Help4(object sender, EventArgs e)
        {
            textBox2.Text = "feuer";
            outcount2++;
            if (!Stop)
            readIn();
        }
         private void my_Help5(object sender, EventArgs e)
        {
            textBox2.Text = "feuer";
            outcount3++;
            if (!Stop)
            preReadIn();
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
                myFadein.Duration = new Duration(TimeSpan.FromMilliseconds(1001-speed));


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
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1001-speed));

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

        public void preReadIn()
        {

            ColorAnimation myColorAnimation = new ColorAnimation();
            myColorAnimation.From = Colors.Transparent;
            myColorAnimation.To = Colors.Orange;
            myColorAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));


            if (reina != null)
            {
                SolidColorBrush brush = new SolidColorBrush();
                brush.Color = Colors.Transparent;

                if (rein == 0)
                {
                    for (int i = 0; i < read_in_matrix.GetLength(0); i++)
                    {
                        if (Convert.ToInt64(read_in_matrix[ i,outcount2]) != 0)
                        {
                            reina[countup].Background = brush;
                            countup++;
                        }
                    }
                }
                
                else
                    {
                        for (int i = 0; i < read_in_matrix.GetLength(1); i++)
                        {
                            if (Convert.ToInt64(read_in_matrix[outcount2, i]) != 0)
                            {
                                reina[countup].Background = brush;
                                countup++;
                            }
                        }
                    }
                myColorAnimation.Completed += new EventHandler(my_Help4);
                if (!Stop)
                brush.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation);
                
            }
        }


        public void readIn()
        {

            DoubleAnimation fadeIn = new DoubleAnimation();
            fadeIn.From = 0.0;
            fadeIn.To = 1.0;
            fadeIn.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));

            DoubleAnimation myFadeOut = new DoubleAnimation();
            myFadeOut.From = 1.0;
            myFadeOut.To = 0.0;
            myFadeOut.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));

            for (int i = 0; i < countup; i++)
            {
                    if(reina[i].Opacity!=0.0)  
                    reina[i].BeginAnimation(TextBlock.OpacityProperty, myFadeOut); 
                
            }

                if (teba != null)
                {


                    if (rein == 0)
                    {
                        for (int i = rowper; i < teba.GetLength(0); i++)
                        {


                            if (i == teba.GetLength(0) - 1 && outcount1 < teba.GetLength(1) - 1 && !Stop)
                            {
                                fadeIn.Completed += new EventHandler(my_Help2);
                            }

                            teba[i, outcount1].BeginAnimation(TextBlock.OpacityProperty, fadeIn);

                            if (i == teba.GetLength(0) - 1 && outcount1 == teba.GetLength(1) - 1 && !Stop)
                            {
                                postReadIn();
                            }

                        }
                    }



                    else
                    {
                        for (int i = colper; i < teba.GetLength(1); i++)
                        {


                            if (i == teba.GetLength(1) - 1 && outcount1 < teba.GetLength(0) - 1 && !Stop)
                            {
                                fadeIn.Completed += new EventHandler(my_Help2);
                            }
                            teba[outcount1, i].BeginAnimation(TextBlock.OpacityProperty, fadeIn);

                            try
                            {
                                textBox1.Text = textBox1.Text.Remove(0, 1);
                            }

                            catch (ArgumentOutOfRangeException) { }

                            if (i == teba.GetLength(1) - 1 && outcount1 == teba.GetLength(0) - 1 && !Stop)
                            { postReadIn(); }

                        }
                    }



                }
        }

        public void postReadIn()
        {
            ColorAnimation myColorAnimation = new ColorAnimation();
            myColorAnimation.From = Colors.DarkOrange;
            myColorAnimation.To = Colors.Transparent;
            myColorAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));

            SolidColorBrush brush = new SolidColorBrush();

            if (teba != null)
            {

                if (rein == 0)
                {
                    Boolean no = true;
                    for (int i = rowper; i < teba.GetLength(0); i++)
                    {

                        teba[i, outcount3].Background = brush;

                        if (i == teba.GetLength(0) - 1 && outcount3 == teba.GetLength(1) - 1 && !Stop)
                        {
                            sort(schleife);
                            no = false;
                        }

                    }
                    if(no)
                    myColorAnimation.Completed += new EventHandler(my_Help5);
                    if (!Stop)
                    brush.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation);
                }



                else
                {
                    Boolean no = true;
                    for (int i = colper; i < teba.GetLength(1); i++)
                    {


                        teba[ outcount3,i].Background = brush;

                        if (i == teba.GetLength(1) - 1 && outcount3 == teba.GetLength(0) - 1 && !Stop)
                        {
                            sort(schleife);
                            no = false;
                        }

                    }
                    if (no)
                        myColorAnimation.Completed += new EventHandler(my_Help5);
                    if (!Stop)
                    brush.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation);
                }
            }
        }

        public void create(byte[,] read_in_matrix, byte[,] permuted_matrix, int[] key, String keyword, byte[] input, byte[] output)
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
                    mywrap1.Children.Clear();
                    mywrap2.Children.Clear();


                    teba = new TextBlock[read_in_matrix.GetLength(0) + rowper, read_in_matrix.GetLength(1) + colper];

                    for (int i = 0; i < read_in_matrix.GetLength(0) + rowper;i++ )
                    {
                       myGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    }

                    for (int i = 0; i < read_in_matrix.GetLength(1) + colper; i++)
                    {
                        
                        myGrid.RowDefinitions.Add(new RowDefinition());
                    }
                    

                    for (int i = 0; i < key.Length; i++)
                    {
                        if (mainGrid.Width < 800)
                        {
                            myGrid.Width += 12;
                            mainGrid.Width += 12;
                        }
                       
                        
                        TextBlock txt = new TextBlock();
                        String s = key[i].ToString();
                        
                        txt.VerticalAlignment = VerticalAlignment.Center;
                        txt.Text = s;
                        txt.FontSize = 12;
                        txt.FontWeight = FontWeights.ExtraBold;
                        txt.TextAlignment = TextAlignment.Center;
                        txt.Width = 17;
                        txt.Opacity = 0.0;

                        if (per == 1)
                        {
                            Grid.SetRow(txt, 0);
                            Grid.SetColumn(txt, i);
                            myGrid.Children.Add(txt);
                            teba[i, 0] = txt;
                            teba[i, 0].BeginAnimation(TextBlock.OpacityProperty,fadeIn);
                            
                        }

                        else 
                        
                        {
                            Grid.SetRow(txt, i);
                            Grid.SetColumn(txt, 0);
                            myGrid.Children.Add(txt);
                            teba[0, i] = txt;
                            teba[0, i].BeginAnimation(TextBlock.OpacityProperty, fadeIn);
                        }

                    }

                    
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
                            txt.Width = 17;

                            if (per == 1)
                            {
                                Grid.SetRow(txt, 1);
                                Grid.SetColumn(txt, i);
                                myGrid.Children.Add(txt);
                                teba[i, 1] = txt;
                                teba[i, 1].BeginAnimation(TextBlock.OpacityProperty, fadeIn);
                            }
                            
                            else 
                            {
                                Grid.SetRow(txt, i);
                                Grid.SetColumn(txt, 1);
                                myGrid.Children.Add(txt);
                                teba[1, i] = txt;
                                teba[1, i].BeginAnimation(TextBlock.OpacityProperty, fadeIn);
                            }
                        }
                    }

                    for (int i = 0; i < read_in_matrix.GetLength(1); i++)
                    {
                       
                        int x = 0;
                      
                        if (i % 2 == 0)
                            x = 1;
                        for (int ix = 0; ix < read_in_matrix.GetLength(0); ix++)
                        {

                            TextBlock txt = new TextBlock();
                            txt.VerticalAlignment = VerticalAlignment.Center;
                            if (Convert.ToInt64(read_in_matrix[ix, i]) != 0)
                                txt.Text = Convert.ToChar(read_in_matrix[ix, i]).ToString();
                            else
                                txt.Text = "";
                           // if (ix % 2 == x)
                             //   txt.Background = Brushes.AliceBlue;
                            //else
                              //  txt.Background = Brushes.LawnGreen;
                            txt.Background = Brushes.DarkOrange;
                            txt.FontSize = 12;
                            txt.Opacity = 1.0;
                            txt.FontWeight = FontWeights.ExtraBold;
                            txt.TextAlignment = TextAlignment.Center;
                            txt.Width = 17;

                            Grid.SetRow(txt, (i + colper));
                            Grid.SetColumn(txt, (ix + rowper));
                            myGrid.Children.Add(txt);
                            teba[(ix + rowper), (i + colper)] = txt;
                            teba[(ix + rowper), (i + colper)].Opacity = 0.0;
                        }
                    }
                    
                   
                   reina = new TextBlock[input.Length];
                    for (int i = 0; i < input.Length; i++)
                    {
                        TextBlock txt = new TextBlock();
                        txt.FontSize = 12;
                        txt.FontWeight = FontWeights.ExtraBold;
                        txt.Text = Convert.ToChar(input[i]).ToString();
                        reina[i] = txt;
                        reina[i].Background = Brushes.Transparent;
                        reina[i].Opacity = 0.0;
                        mywrap1.Children.Add(txt);
                        if (i == input.Length-1)
                            { fadeIn.Completed += new EventHandler(my_Help3); }
                        if (!Stop)
                        reina[i].BeginAnimation(TextBlock.OpacityProperty,fadeIn);


                    }

                    reouta = new TextBlock[output.Length];
                    for (int i = 0; i < output.Length; i++)
                    {
                        TextBlock txt = new TextBlock();
                        txt.FontSize = 12;
                        txt.FontWeight = FontWeights.ExtraBold;
                        txt.Text = Convert.ToChar(output[i]).ToString();
                        reouta[i] = txt;
                        reouta[i].Background = Brushes.Transparent;
                        reouta[i].Opacity = 0.0;
                        mywrap2.Children.Add(txt);
                      
                       
                    }
                }
            }
         , null);
        }

    }

}
