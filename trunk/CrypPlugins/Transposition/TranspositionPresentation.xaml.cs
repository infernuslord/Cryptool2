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
        /// <summary>
        /// Visualisationmodul for Transposition.c
        /// </summary>
        public TranspositionPresentation()
        {
            InitializeComponent(); 
           SizeChanged += sizeChanged;
           
        }

        public void sizeChanged(Object sender, EventArgs eventArgs)
        {
            this.Stack.RenderTransform = new ScaleTransform(this.ActualWidth / this.Stack.ActualWidth,
                                                       this.ActualHeight / this.Stack.ActualHeight);
        }
        

        
        private TextBlock[,] teba;
        private int von;
        private int nach;
        private int schleife = 0;
        private int outcount;
        private int outcount1;
        private int outcount2;
        private int outcount3;
        private int outcount4;
        private int outcount5;
        private int countup;
        private int countup1;
        private int precountup;
        private bool Stop = false;
        private int per;
        private int act;
        private int rein;
        private int reout;
        private TextBlock[] reina;
        private TextBlock[] reouta;
        private int speed = 1;
        private int rowper;
        private int colper;
        private byte[,] read_in_matrix;
        private byte[,] permuted_matrix;
        private Brush[,] mat_back;
        private DoubleAnimation fadeIn;
        private DoubleAnimation fadeOut;
        private int[] key;

        /// <summary>
        /// Getter of the Speed the Visualisation is running
        /// </summary>
        /// <param name="speed"></param>
        public void UpdateSpeed(int speed)
        {
            this.speed = speed;
        }

        /// <summary>
        /// Initialisation of all Params the Visualisation needs from the Caller
        /// </summary>
        /// <param name="read_in_matrix"></param>
        /// <param name="permuted_matrix"></param>
        /// <param name="keyword"></param>
        /// <param name="per"></param>
        /// <param name="rein"></param>
        /// <param name="reout"></param>
        private void init(byte[,] read_in_matrix, byte[,] permuted_matrix, String keyword, int per, int rein, int reout, int act, int[] key) 
        {
            
            DoubleAnimation fadeIn = new DoubleAnimation();
            fadeIn.From = 0.0;
            fadeIn.To = 1.0;
            fadeIn.Duration = new Duration(TimeSpan.FromMilliseconds((1001 - speed)*4));

            

            DoubleAnimation fadeOut = new DoubleAnimation();
            fadeOut.From = 1.0;
            fadeOut.To = 0.0;
            fadeOut.Duration = new Duration(TimeSpan.FromMilliseconds((1001 - speed)*4));

            if (act == 0)
                    {
                        this.rein = rein;
                        this.reout = reout;
                    }
                    else 
                    {
                        this.rein = reout;
                        this.reout = rein;
                    }
            this.fadeIn = fadeIn;
            this.fadeOut = fadeOut;
            this.read_in_matrix=read_in_matrix;
            this.permuted_matrix = permuted_matrix;
            this.per = per;
            this.act = act;
            this.key = key;

            countup = 0;
            countup1 = 0;
            outcount2 = 0;
            outcount5 = 0;
            precountup = 0;

            Stop = false;
            if (keyword == null)
                Stop = true;

            textBox2.Clear();
            if (per == 1  )
                {
                   
                    rowper = 0;
                    colper = 2;
                    if (this.reout == 1)
                        outcount = 0;
                    else
                        outcount = 2;

                    if (this.reout == 1)
                        outcount4 = 0;
                    else 
                        outcount4 = 2;
                    
                    if(this.rein == 1)
                        outcount1 = 0;
                    else
                        outcount1 = 2;

                    if (this.rein == 1)
                        outcount3 = 0;

                    else
                        outcount3 = 2;
                }
            else
                {
                    rowper = 2;
                    colper = 0;

                    if (this.reout == 1)
                        outcount = 2;
                    else
                        outcount = 0;

                    if (this.reout == 1)
                        outcount4 = 2;
                    else
                        outcount4 = 0;

                    if (this.rein == 1)
                        outcount1 = 2;
                    else
                        outcount1 = 0;

                    if (this.rein == 1)
                        outcount3 = 2;
                    else
                        outcount3 = 0;
                    }
        }

        public void main(byte[,] read_in_matrix, byte[,] permuted_matrix, int[] key, String keyword, byte[] input, byte[] output, int per, int rein, int reout,int act)
        {

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                    this.my_Stop(this, EventArgs.Empty);
                    init(read_in_matrix,permuted_matrix,keyword, per, rein, reout,act,key);
                    create(read_in_matrix, permuted_matrix, key, keyword, input, output);
                    
                    schleife = 0;
                    //textBox1.Text = input;
                    //readIn();
            }, null);
        }

        private void postReadOut() 
        { 
            ColorAnimation myColorAnimation = new ColorAnimation();
            myColorAnimation.From = Colors.Orange;
            myColorAnimation.To = Colors.Transparent;
            myColorAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));
            Boolean no = true;

            if (reouta != null)
            {  
                SolidColorBrush brush = new SolidColorBrush();
                brush.Color = Colors.Transparent;

                for (int i = precountup; i < countup1; i++)
                {
                   reouta[i].Background = brush;
                }
                              
                if (reout == 0)
                {                   
                    for (int i = 0; i < permuted_matrix.GetLength(0); i++)
                    {
                        if (i == permuted_matrix.GetLength(0) - 1 && outcount5 == permuted_matrix.GetLength(1) - 1 && !Stop)
                        {
                            myColorAnimation.Completed += new EventHandler(the_End);
                            no = false;
                        }
                    }
                }
                else
                {
                        for (int i = 0; i < permuted_matrix.GetLength(1); i++)
                        {
                            if (i == permuted_matrix.GetLength(1) - 1 && outcount5 == permuted_matrix.GetLength(0) - 1 && !Stop)
                            {
                                myColorAnimation.Completed += new EventHandler(the_End);
                                no = false;
                            }
                        }
                }

                if (no)
                myColorAnimation.Completed += new EventHandler(my_Help7);
                if (!Stop)
                    brush.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation);
            }
        }

        private void preReadOut()
        {
            ColorAnimation myColorAnimation_green = new ColorAnimation();
            myColorAnimation_green.From = Colors.LawnGreen;
            myColorAnimation_green.To = Colors.Yellow;
            myColorAnimation_green.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));

            ColorAnimation myColorAnimation_blue = new ColorAnimation();
            myColorAnimation_blue.From = Colors.AliceBlue;
            myColorAnimation_blue.To = Colors.Yellow;
            myColorAnimation_blue.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));

            SolidColorBrush brush_green = new SolidColorBrush();
            SolidColorBrush brush_blue = new SolidColorBrush();

            if (teba != null)
            {
                if (reout == 0)
                {                    
                    for (int i = rowper; i < teba.GetLength(0); i++)
                    {
                        if (mat_back[i - rowper, outcount4 - colper] == Brushes.LawnGreen)
                            teba[i, outcount4].Background = brush_green;
                        else
                            teba[i, outcount4].Background = brush_blue;                       
                    }                    
                        myColorAnimation_green.Completed += new EventHandler(my_Help6);
                                        
                    if (!Stop)
                    {
                        brush_green.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation_green);
                        brush_blue.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation_blue);
                    }
                }
                else
                {                    
                    for (int i = colper; i < teba.GetLength(1); i++)
                    {
                        if (mat_back[outcount4 - rowper, i - colper] == Brushes.LawnGreen)
                            teba[outcount4, i].Background = brush_green;
                        else
                            teba[outcount4, i].Background = brush_blue;
                    }
                   
                        myColorAnimation_blue.Completed += new EventHandler(my_Help6);
                    if (!Stop)
                    {
                        brush_blue.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation_blue);
                        brush_green.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation_green);

                    }
                }
            }

        }
        
        public void readout()
        {
            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = 1.0;
            myDoubleAnimation.To = 0.0;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1001-speed));

            DoubleAnimation fadeIn = new DoubleAnimation();
            fadeIn.From = 0.0;
            fadeIn.To = 1.0;
            fadeIn.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));                  

            if (teba != null)
            {
                if (reout == 1)
                {
                    for (int i = 0; i < permuted_matrix.GetLength(1); i++)
                    {
                        if (Convert.ToInt64(permuted_matrix[outcount5, i]) != 0)
                            {
                                reouta[countup1].BeginAnimation(TextBlock.OpacityProperty, fadeIn);
                                countup1++;
                            }    
                    }

                    for (int i = colper; i < teba.GetLength(1); i++)
                    {
                        if (i == teba.GetLength(1) - 1 && !Stop)
                        {
                            myDoubleAnimation.Completed += new EventHandler(my_Help1);
                        }
                        teba[outcount, i].BeginAnimation(TextBlock.OpacityProperty, myDoubleAnimation);

                    }
                }
                else
                {
                    for (int i = 0; i < permuted_matrix.GetLength(0); i++)
                    {
                        if (Convert.ToInt64(permuted_matrix[ i,outcount5]) != 0)
                            {
                                reouta[countup1].BeginAnimation(TextBlock.OpacityProperty, fadeIn);
                                countup1++;
                            }                               
                    }

                    for (int i = rowper; i < teba.GetLength(0); i++)
                    {
                        if (i == teba.GetLength(0) - 1 && !Stop)
                        {
                            myDoubleAnimation.Completed += new EventHandler(my_Help1);
                        }
                        teba[i, outcount].BeginAnimation(TextBlock.OpacityProperty, myDoubleAnimation);
                    }
                }
            }
        }
        /// <summary>
        /// (Insertion Sort) algorithm for sorting the rows OR columns by index during the permutationphase
        /// </summary>
        /// <param name="i"></param>
        public void sort(int i)
        {
            if (per == 1)
            {
                if (teba != null && key != null)
                {
                    if (act == 0)
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
                                preani(i, s);

                            }
                            else
                            {
                                schleife++;
                                sort(schleife);
                            }

                        }

                        else if (!Stop) { preReadOut(); }
                    }
                    else
                    {
                        if (i < teba.GetLength(0) - 1)
                        {
                            if (Convert.ToInt32(teba[i, 0].Text) != key[i])
                            {
                                int s = 0;
                                for (int ix = i + 1; ix < teba.GetLength(0); ix++)
                                {
                                    if (Convert.ToInt32(teba[ix, 0].Text) == key[i])
                                    {
                                        s = ix;
                                    }
                                }
                                preani(i, s);

                            }
                            else
                            {
                                textBox2.Text += key[i];
                                schleife++;
                                sort(schleife);
                            }

                        }

                        else if (!Stop) { preReadOut(); }
                    }
                }

            }
            else
            {
                preReadOut();
            }
        }
        /// <summary>
        /// "emergengy break" 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void my_Stop(object sender, EventArgs e)
        {
            myGrid.Children.Clear();
            myGrid.ColumnDefinitions.Clear();
            myGrid.RowDefinitions.Clear();
            mywrap1.Children.Clear();
            mywrap2.Children.Clear();

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
                postReadOut();
        }

        private void my_Help2(object sender, EventArgs e)
        {
            outcount1++;
            if (!Stop)
                postReadIn();
        }

        private void my_Help3(object sender, EventArgs e)
        {
            sizeChanged(this, EventArgs.Empty);
            
            if (!Stop)
            preReadIn();
        }
        
        private void my_Help4(object sender, EventArgs e)
        {
            
            outcount2++;
            if (!Stop)
            readIn();
        }

        private void my_Help7(object sender, EventArgs e)
        {
            precountup = countup1; 
            
            outcount5++;
            if (!Stop)
                preReadOut();
        }
         
        private void my_Help5(object sender, EventArgs e)
        {
            
            outcount3++;
            if (!Stop)
            preReadIn();
        }

        private void my_Help6(object sender, EventArgs e)
        {
           
            outcount4++;
            if (!Stop)
                readout();
        }

        private void my_Help8(object sender, EventArgs e)
        {
            ani();
        }

        private void my_Help(object sender, EventArgs e)
        {
            schleife++;
            if (!Stop)
               postani();
        }

        private void my_Help9(object sender, EventArgs e)
        {
            if (!Stop)
                sort(schleife);
        }

        private void the_End(object sender, EventArgs e)
        {
            Stack.BeginAnimation(OpacityProperty, fadeOut);
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

                        if(i>1)
                        {
                            Brush help2;
                            help2= mat_back[nach, i-2];
                            mat_back[nach, i-2] = mat_back[von, i-2];
                            mat_back[von, i-2] = help2;
                        }

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

        public void preani(int von, int nach) 
        {
            this.von = von;
            this.nach = nach;
            
            ColorAnimation myColorAnimation_gy = new ColorAnimation();
            myColorAnimation_gy.From = Colors.LawnGreen;
            myColorAnimation_gy.To = Colors.Yellow;
            myColorAnimation_gy.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));

            ColorAnimation myColorAnimation_by = new ColorAnimation();
            myColorAnimation_by.From = Colors.AliceBlue;
            myColorAnimation_by.To = Colors.Yellow;
            myColorAnimation_by.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));

            ColorAnimation myColorAnimation_ty = new ColorAnimation();
            myColorAnimation_ty.From = Colors.Transparent;
            myColorAnimation_ty.To = Colors.Yellow;
            myColorAnimation_ty.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));


            SolidColorBrush brush_gy = new SolidColorBrush();
            SolidColorBrush brush_by = new SolidColorBrush();
            SolidColorBrush brush_ty = new SolidColorBrush();

            ColorAnimation myColorAnimation_go = new ColorAnimation();
            myColorAnimation_go.From = Colors.LawnGreen;
            myColorAnimation_go.To = Colors.Orange;
            myColorAnimation_go.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));

            ColorAnimation myColorAnimation_bo = new ColorAnimation();
            myColorAnimation_bo.From = Colors.AliceBlue;
            myColorAnimation_bo.To = Colors.Orange;
            myColorAnimation_bo.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));

            ColorAnimation myColorAnimation_to = new ColorAnimation();
            myColorAnimation_to.From = Colors.Transparent;
            myColorAnimation_to.To = Colors.Orange;
            myColorAnimation_to.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));


            SolidColorBrush brush_go = new SolidColorBrush();
            SolidColorBrush brush_bo = new SolidColorBrush();
            SolidColorBrush brush_to = new SolidColorBrush();

            if (teba != null)
                if (per == 1)
                    for (int i = 0; i < teba.GetLength(1); i++)
                    {
                        if (i > 1)
                        {
                            if (mat_back[von, i - 2].Equals(Brushes.LawnGreen))
                                teba[von, i].Background = brush_gy;

                            if (mat_back[von, i - 2].Equals(Brushes.AliceBlue))
                                teba[von, i].Background = brush_by;

                            if (mat_back[nach, i - 2].Equals(Brushes.LawnGreen))
                                teba[nach, i].Background = brush_go;

                            if (mat_back[nach, i - 2].Equals(Brushes.AliceBlue))
                                teba[nach, i].Background = brush_bo;
                        
                        }
                        else
                        {
                            teba[von, i].Background = brush_ty;
                            teba[nach, i].Background = brush_to;
                        }
                    }
            
            brush_ty.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation_ty); 
            brush_gy.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation_gy);
            brush_to.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation_to);
            brush_go.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation_go);
            brush_bo.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation_bo); 
            myColorAnimation_by.Completed += new EventHandler(my_Help8);
            brush_by.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation_by); 

        }

        public void postani() 
        
        {
            ColorAnimation myColorAnimation_gy = new ColorAnimation();
            myColorAnimation_gy.From = Colors.Yellow;
            myColorAnimation_gy.To = Colors.LawnGreen;
            myColorAnimation_gy.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));

            ColorAnimation myColorAnimation_by = new ColorAnimation();
            myColorAnimation_by.From = Colors.Yellow;
            myColorAnimation_by.To = Colors.AliceBlue;
            myColorAnimation_by.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));

            ColorAnimation myColorAnimation_ty = new ColorAnimation();
            myColorAnimation_ty.From = Colors.Yellow;
            myColorAnimation_ty.To = Colors.Transparent;
            myColorAnimation_ty.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));


            SolidColorBrush brush_gy = new SolidColorBrush();
            SolidColorBrush brush_by = new SolidColorBrush();
            SolidColorBrush brush_ty = new SolidColorBrush();

            ColorAnimation myColorAnimation_go = new ColorAnimation();
            myColorAnimation_go.From = Colors.Orange;
            myColorAnimation_go.To = Colors.LawnGreen;
            myColorAnimation_go.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));

            ColorAnimation myColorAnimation_bo = new ColorAnimation();
            myColorAnimation_bo.From = Colors.Orange;
            myColorAnimation_bo.To = Colors.AliceBlue;
            myColorAnimation_bo.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));

            ColorAnimation myColorAnimation_to = new ColorAnimation();
            myColorAnimation_to.From = Colors.Orange;
            myColorAnimation_to.To = Colors.Transparent;
            myColorAnimation_to.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));


            SolidColorBrush brush_go = new SolidColorBrush();
            SolidColorBrush brush_bo = new SolidColorBrush();
            SolidColorBrush brush_to = new SolidColorBrush();

            if (teba != null)
                if (per == 1)
                    for (int i = 0; i < teba.GetLength(1); i++)
                    {
                        if (i > 1)
                        {
                            if (mat_back[nach, i - 2].Equals(Brushes.LawnGreen))
                                teba[nach, i].Background = brush_gy;

                            if (mat_back[nach, i - 2].Equals(Brushes.AliceBlue))
                                teba[nach, i].Background = brush_by;

                            if (mat_back[von, i - 2].Equals(Brushes.LawnGreen))
                                teba[von, i].Background = brush_go;

                            if (mat_back[von, i - 2].Equals(Brushes.AliceBlue))
                                teba[von, i].Background = brush_bo;

                        }
                        else
                        {
                            teba[nach, i].Background = brush_ty;
                            teba[von, i].Background = brush_to;
                        }
                    }

            brush_ty.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation_ty);
            brush_gy.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation_gy);
            brush_to.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation_to);
            brush_go.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation_go);
            brush_bo.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation_bo);
            myColorAnimation_by.Completed += new EventHandler(my_Help9);
            brush_by.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation_by); 

        }
        /// <summary>
        /// animation being used in the permutationphase while sorting
        /// </summary>
        /// <param name="von"></param>
        /// <param name="nach"></param>
        public void ani()
        {
            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = 1.0;
            myDoubleAnimation.To = 0.0;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1001-speed));

           
            if (teba != null)
                if (per == 1)
                    for (int i = 0; i < teba.GetLength(1); i++)
                    {
                        teba[von, i].BeginAnimation(TextBlock.OpacityProperty, myDoubleAnimation);
                        if (i == teba.GetLength(1) - 1 && !Stop)
                        {
                            myDoubleAnimation.Completed += new EventHandler(my_Completed);
                        }
                        teba[nach, i].BeginAnimation(TextBlock.OpacityProperty, myDoubleAnimation);
                    }
// die folgende ELSE ist abgeändert. ich: ungewiss ihrer absoluten richtigkeit.klappte einmal "zur hälfte" .. lösch' wie du bock hast.
                else 
                {
                    for (int i = 0; i < teba.GetLength(0); i++) 
                    {
                        teba[i, von].BeginAnimation(TextBlock.OpacityProperty, myDoubleAnimation); 
                        if (i == teba.GetLength(0) - 1 && !Stop) 
                        {
                            myDoubleAnimation.Completed += new EventHandler(my_Completed);
                        }
                        teba[i, nach].BeginAnimation(TextBlock.OpacityProperty, myDoubleAnimation);
                    }
                }
        }
        /// <summary>
        /// coloranimation for the text in the left wrapper to be "eaten out" and getting marked
        /// </summary>
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
        /// <summary>
        /// method for fading text out from the left wrapper and fading into the grid (where it's already in but transparent)
        /// </summary>
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
            ColorAnimation myColorAnimation_green = new ColorAnimation();
            myColorAnimation_green.From = Colors.Yellow;
            myColorAnimation_green.To = Colors.LawnGreen;
            myColorAnimation_green.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));

            ColorAnimation myColorAnimation_blue = new ColorAnimation();
            myColorAnimation_blue.From = Colors.Yellow;
            myColorAnimation_blue.To = Colors.AliceBlue;
            myColorAnimation_blue.Duration = new Duration(TimeSpan.FromMilliseconds(1001 - speed));

            SolidColorBrush brush_green = new SolidColorBrush();
            SolidColorBrush brush_blue = new SolidColorBrush();

            if (teba != null)
            {
                if (rein == 0)
                {
                    Boolean no = true;
                    for (int i = rowper; i < teba.GetLength(0); i++)
                    {
                        if (mat_back[i-rowper, outcount3-colper] == Brushes.LawnGreen)
                            teba[i, outcount3].Background = brush_green;
                        else
                            teba[i, outcount3].Background = brush_blue;
                        if (i == teba.GetLength(0) - 1 && outcount3 == teba.GetLength(1) - 1 && !Stop)
                        {
                            sort(schleife);
                            no = false;
                        }
                    }
                    if (no)
                    {
                        myColorAnimation_green.Completed += new EventHandler(my_Help5);                       
                    }
                    if (!Stop)
                    {
                        brush_green.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation_green);
                        brush_blue.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation_blue);
                    }
                }
                else
                {
                    Boolean no = true;
                    for (int i = colper; i < teba.GetLength(1); i++)
                    {
                        if (mat_back[outcount3 - rowper, i - colper] == Brushes.LawnGreen)
                            teba[ outcount3, i].Background = brush_green;
                        else
                            teba[outcount3, i].Background = brush_blue;
                        if (i == teba.GetLength(1) - 1 && outcount3 == teba.GetLength(0) - 1 && !Stop)
                        {
                            sort(schleife);
                            no = false;
                        }
                    }
                    if (no)
                        myColorAnimation_blue.Completed += new EventHandler(my_Help5);
                    if (!Stop)
                    {
                        brush_blue.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation_blue);
                        brush_green.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation_green);
                    }
                }
            }
        }
        /// <summary>
        /// method for creating the grid
        /// </summary>
        /// <param name="read_in_matrix"></param>
        /// <param name="permuted_matrix"></param>
        /// <param name="key"></param>
        /// <param name="keyword"></param>
        /// <param name="input"></param>
        /// <param name="output"></param>
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
                                         
                        
                        TextBlock txt = new TextBlock();
                        String s = key[i].ToString();
                        
                        txt.VerticalAlignment = VerticalAlignment.Center;
                        if (act == 0)
                            txt.Text = s;
                        else
                            txt.Text = "" + (i + 1);
                        txt.FontSize = 12;
                        txt.FontWeight = FontWeights.ExtraBold;
                        txt.TextAlignment = TextAlignment.Center;
                        txt.Width = 17;
                       // txt.Opacity = 0.0;
                        
                        if (per == 1)
                        {
                            Grid.SetRow(txt, 0);
                            Grid.SetColumn(txt, i);
                            myGrid.Children.Add(txt);
                            teba[i, 0] = txt;
                            //teba[i, 0].BeginAnimation(TextBlock.OpacityProperty,fadeIn);                            
                        }
                        else                         
                        {
                            Grid.SetRow(txt, i);
                            Grid.SetColumn(txt, 0);
                            myGrid.Children.Add(txt);
                            teba[0, i] = txt;
                            //teba[0, i].BeginAnimation(TextBlock.OpacityProperty, fadeIn);
                        }
                    }
                    
                    if (keyword != null)
                    {                        
                        char[] ch = keyword.ToCharArray();
                        if (act == 1)
                            Array.Sort(ch);

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
                    mat_back = new Brush[read_in_matrix.GetLength(0), read_in_matrix.GetLength(1)];
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
                            if (ix % 2 == x)
                                    mat_back[ix,i]= Brushes.AliceBlue;
                            else
                                mat_back[ix,i] = Brushes.LawnGreen;
                            txt.Background = Brushes.Yellow;
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
                        //reina[i].Opacity = 0.0;
                        mywrap1.Children.Add(txt);
                       // if (i == input.Length-1)
                         //   { fadeIn.Completed += new EventHandler(my_Help3); }
                        //if (!Stop)
                       // reina[i].BeginAnimation(TextBlock.OpacityProperty,fadeIn);
                    }

                    reouta = new TextBlock[output.Length];
                    for (int i = 0; i < output.Length; i++)
                    {
                        TextBlock txt = new TextBlock();
                        txt.FontSize = 12;
                        txt.FontWeight = FontWeights.ExtraBold;
                        txt.Text = Convert.ToChar(output[i]).ToString();
                        reouta[i] = txt;
                        reouta[i].Background = Brushes.Orange;
                        reouta[i].Opacity = 0.0;
                        mywrap2.Children.Add(txt);   
                    }
                }

                fadeIn.Completed += new EventHandler(my_Help3);
                if(!Stop)
                Stack.BeginAnimation(OpacityProperty, fadeIn);
            }
         , null);
        }
    }
}
