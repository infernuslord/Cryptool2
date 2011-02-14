﻿/*using System;
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

namespace Cryptool.Enigma
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class EnigmaPresentation : UserControl
    {
        public EnigmaPresentation()
        {
            InitializeComponent();
        }
    }
}*/
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
using System.Windows.Media.Animation;
using System.Threading;
using System.Windows.Threading;
using System.ComponentModel;



namespace Cryptool.Enigma
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class  EnigmaPresentation : UserControl
    {
        Button[] bList = new Button[26];
        Button[] tList = new Button[26];
        TextBlock[] inputList = new TextBlock[26];
        TextBlock[] outputList = new TextBlock[26];
        int[] switchlist = new int[26];
        int[] umkehrlist = {4,9,12,25,0,11,24,23,21,1,22,5,2,17,16,20,14,13,19,18,15,8,10,7,6,3};

        int aktuell = -1;

        Rectangle[] leuchtenList = new Rectangle[26];

        Line[,] umlList = new Line[26,3];
        List<Line[]> schalterlist = new List<Line[]>();
        List<Line[]> schalterlist2 = new List<Line[]>();

        Rectangle[] dummyrec = new Rectangle[4];

        Boolean stop = false;

        Boolean off = false;

        Boolean mfouron = false;

        double speed = 0.5;

        Line[,] frombat = new Line[9,3];
        Line[,] frombat2 = new Line[3,2];

        Line[,] toBat = new Line[26, 6];
        Line[,] tobat1 = new Line[9, 3];
        Line[,] tobat2 = new Line[3, 2];
        
        Canvas rotorlocks1 ;
        Canvas rotorlocks2;
        Canvas rotorlocks3;
        Canvas rotorlocks4;

        List<TextBlock> inputtebo;
        List<TextBlock> outputtebo = new List<TextBlock>();
        TextBlock[] textBlocksToAnimate = new TextBlock[6];

        Line[] drawLines = new Line[22];

        Rotor2[] rotorarray = new Rotor2[4];
        private Walze walze;
        Image[] rotorimgs = new Image[5];
        Image[] walzeimgs = new Image[3];


        Point startPoint;
        Line[] lList = new Line[26];

        List<Canvas> schalterlist3 = new List<Canvas>();
        List<Canvas> leuchtenlist3 = new List<Canvas>();

        List<UIElement> linesToAnimate = new List<UIElement>();
        List<UIElement> linesToAnimate2 = new List<UIElement>();
        List<Line> linesToThrowAway = new List<Line>();
        DoubleAnimation fadeIn = new DoubleAnimation();
        DoubleAnimation fadeOut = new DoubleAnimation();
        DoubleAnimation nop = new DoubleAnimation();
        AutoResetEvent ars = new AutoResetEvent(false);
        
        private void sizeChanged(Object sender, EventArgs eventArgs)
        {
            this.mainmainmain.RenderTransform = new ScaleTransform(this.ActualWidth / this.mainmainmain.ActualWidth,
                                                            this.ActualHeight / this.mainmainmain.ActualHeight);
            
                
        }

        private void sliderValueChanged(object sender, EventArgs args) 
        {
            speed = slider1.Value;
            if(rotorarray[0]!=null)
            rotorarray[0].fast = slider1.Value * 80;
            if (rotorarray[1] != null)
            rotorarray[1].fast = slider1.Value * 80;
            if (rotorarray[2] != null)
            rotorarray[2].fast = slider1.Value * 80;
            if (walze != null)
            walze.fast = slider1.Value * 80;
        }

        Boolean justonce = true;

        public void settings_OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            EnigmaSettings dummyset = sender as EnigmaSettings;

            if (e.PropertyName == "Ring1down")
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    rotorarray[2].upperclick1(null, EventArgs.Empty);    
                }, null);
            }

            if (e.PropertyName == "Ring1up")
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    rotorarray[2].downerclick1(null, EventArgs.Empty);
                }, null);
            }

            if (e.PropertyName == "Ring2down")
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    rotorarray[1].upperclick1(null, EventArgs.Empty);
                }, null);
            }

            if (e.PropertyName == "Ring2up")
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    rotorarray[1].downerclick1(null, EventArgs.Empty);
                }, null);
            }

            if (e.PropertyName == "Ring3down")
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    rotorarray[0].upperclick1(null, EventArgs.Empty);
                }, null);
            }

            if (e.PropertyName == "Ring3up")
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    rotorarray[0].downerclick1(null, EventArgs.Empty);
                }, null);
            }

            if (e.PropertyName[0] == 'P' && e.PropertyName !="PlugBoardDisplay")
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    Button b = new Button();
                    b.Uid ="" +( Convert.ToInt32(e.PropertyName[9]) -65);
                    b.Content = Convert.ToInt32(e.PropertyName[9]) - 65;

                    List_MouseLeftButtonDown(b,EventArgs.Empty);
                }, null);
            }
            if (e.PropertyName == "PlugBoardDisplay") 
            {
                merken = -1;
            }

            if (e.PropertyName == "Reflector")
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.walze.helpNextAnimation -= helptoanimate5;

                    dropBoxCanvasWalze.Children.Remove(walzeimgs[dummyset.Reflector]);

                    Image img = new Image();
                    img.Height = 100;
                    img.Width = 50;
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri("Images/rotor" + walze.typ + ".jpg", UriKind.Relative);
                    bi.EndInit();
                    img.Source = bi;


                    Canvas.SetLeft(img, 50 * walze.typ);

                    dropBoxCanvasWalze.Children.Add(img);
                    img.Uid = "" + walze.typ;
                    img.Cursor = Cursors.Hand;

                    img.PreviewMouseMove += Walze_MouseMove1;
                    walzeimgs[walze.typ - 1] = img;
                    walzenarea.Children.Remove(walze);

                    Walze walze1 = new Walze(dummyset.Reflector + 1, this.Width, this.Height);
                    walze1.fast = speed * 80;
                    Canvas.SetLeft(walze1, 0);
                    Canvas.SetTop(walze1, 60);
                    walzenarea.Children.Add(walze1);
                    walze1.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
                    walze1.PreviewMouseMove += new MouseEventHandler(Walze_MouseMove);
                    this.walze = walze1;
                    walze.helpNextAnimation += helptoanimate5;


                }, null);
            }

            if (e.PropertyName == "Rotor1")
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    dropBoxCanvas.Children.Remove(rotorimgs[dummyset.Rotor1]);

                    Image img = new Image();
                    img.Height = 100;
                    img.Width = 50;
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri("Images/rotor" + rotorarray[2].map + ".jpg", UriKind.Relative);
                    bi.EndInit();
                    img.Source = bi;


                    Canvas.SetLeft(img, 50 * rotorarray[2].map);

                    dropBoxCanvas.Children.Add(img);
                    img.Uid = "" + rotorarray[2].map;
                    img.Cursor = Cursors.Hand;

                    img.PreviewMouseMove += Rotor_MouseMove1;
                    rotorimgs[rotorarray[2].map - 1] = img;

                    rotorarray[2].helpNextAnimation -= helptoanimate2;
                    rotorarray[2].helpNextAnimation2 -= helptoanimate8;
                    rotorarea.Children.Remove(rotorarray[2]);

                    Rotor2 rotor = new Rotor2(dummyset.Rotor1 + 1, this.Width, this.Height);
                    rotor.fast = speed * 80;
                    rotor.Cursor = Cursors.Hand;
                    rotor.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
                    Canvas.SetLeft(rotor, 460);
                    rotorarea.Children.Add(rotor);
                    rotor.PreviewMouseMove += new MouseEventHandler(Rotor_MouseMove);

                    rotorarray[2] = rotor;
                    rotorarray[2].helpNextAnimation += helptoanimate2;
                    rotorarray[2].helpNextAnimation2 += helptoanimate8;



                }, null);
            }

            if (e.PropertyName == "Rotor2")
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {

                    dropBoxCanvas.Children.Remove(rotorimgs[dummyset.Rotor2]);
                    Image img = new Image();
                    img.Height = 100;
                    img.Width = 50;
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri("Images/rotor" + rotorarray[1].map + ".jpg", UriKind.Relative);
                    bi.EndInit();
                    img.Source = bi;
                    dropBoxCanvas.Children.Add(img);
                    Canvas.SetLeft(img, 50 * rotorarray[1].map);
                    img.Uid = "" + rotorarray[1].map;
                    img.Cursor = Cursors.Hand;

                    img.PreviewMouseMove += Rotor_MouseMove1;

                    rotorimgs[rotorarray[1].map - 1] = img;

                    rotorarray[1].helpNextAnimation -= helptoanimate2;
                    rotorarray[1].helpNextAnimation2 -= helptoanimate8;

                    rotorarea.Children.Remove(rotorarray[1]);

                    Rotor2 rotor = new Rotor2(dummyset.Rotor2 + 1, this.Width, this.Height);
                    rotor.fast = speed * 80;
                    rotor.Cursor = Cursors.Hand;
                    rotor.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
                    Canvas.SetLeft(rotor, 230);
                    rotorarea.Children.Add(rotor);
                    rotor.PreviewMouseMove += new MouseEventHandler(Rotor_MouseMove);

                    rotorarray[1] = rotor;
                    rotorarray[1].helpNextAnimation += helptoanimate3;
                    rotorarray[1].helpNextAnimation2 += helptoanimate7;

                }, null);
            }

            if (e.PropertyName == "Rotor3")
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {

                    dropBoxCanvas.Children.Remove(rotorimgs[dummyset.Rotor3]);

                    Image img = new Image();
                    img.Height = 100;
                    img.Width = 50;
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri("Images/rotor" + rotorarray[0].map + ".jpg", UriKind.Relative);
                    bi.EndInit();
                    img.Source = bi;
                    dropBoxCanvas.Children.Add(img);
                    Canvas.SetLeft(img, 50 * rotorarray[0].map);
                    img.Uid = "" + rotorarray[0].map;
                    img.Cursor = Cursors.Hand;

                    img.PreviewMouseMove += Rotor_MouseMove1;

                    rotorimgs[rotorarray[0].map - 1] = img;

                    rotorarray[0].helpNextAnimation -= helptoanimate2;
                    rotorarray[0].helpNextAnimation2 -= helptoanimate8;

                    rotorarea.Children.Remove(rotorarray[0]);

                    Rotor2 rotor = new Rotor2(dummyset.Rotor3 + 1, this.Width, this.Height);
                    rotor.fast = speed * 80;
                    rotor.Cursor = Cursors.Hand;
                    rotor.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
                    Canvas.SetLeft(rotor, 0);
                    rotorarea.Children.Add(rotor);
                    rotor.PreviewMouseMove += new MouseEventHandler(Rotor_MouseMove);

                    rotorarray[0] = rotor;
                    rotorarray[0].helpNextAnimation += helptoanimate4;
                    rotorarray[0].helpNextAnimation2 += helptoanimate6;
                }, null);
            }

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                dropBoxCanvas.Children.Remove(rotorimgs[dummyset.Rotor3]);
                dropBoxCanvas.Children.Remove(rotorimgs[dummyset.Rotor2]);
                dropBoxCanvas.Children.Remove(rotorimgs[dummyset.Rotor1]);
            }, null);

        }
            
        

        /*
        public void settingsChanged(EnigmaSettings dummyset)
        {
            if (justonce)
            {
                justonce = false;
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                { everythingblack(); Debug.Text = "" + walze.typ; }, null);
                
                if (dummyset.Reflector+1  !=  walze.typ)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.walze.helpNextAnimation -= helptoanimate5;

                        dropBoxCanvasWalze.Children.Remove(walzeimgs[dummyset.Reflector]);

                        Image img = new Image();
                        img.Height = 100;
                        img.Width = 50;
                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        bi.UriSource = new Uri("Images/rotor" + walze.typ + ".jpg", UriKind.Relative);
                        bi.EndInit();
                        img.Source = bi;


                        Canvas.SetLeft(img, 50 * walze.typ);

                        dropBoxCanvasWalze.Children.Add(img);
                        img.Uid = "" + walze.typ;
                        img.Cursor = Cursors.Hand;

                        img.PreviewMouseMove += Walze_MouseMove1;
                        walzeimgs[walze.typ - 1] = img;
                        walzenarea.Children.Remove(walze);

                        Walze walze1 = new Walze(dummyset.Reflector+1, this.Width, this.Height);
                        walze1.fast = speed * 80;
                        Canvas.SetLeft(walze1, 0);
                        Canvas.SetTop(walze1, 60);
                        walzenarea.Children.Add(walze1);
                        walze1.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
                        walze1.PreviewMouseMove += new MouseEventHandler(Walze_MouseMove);
                        this.walze = walze1;
                        walze.helpNextAnimation += helptoanimate5;
                        

                    }, null);
                }

                if (dummyset.Rotor1 + 1 != rotorarray[2].map)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        dropBoxCanvas.Children.Remove(rotorimgs[dummyset.Rotor1]);

                        Image img = new Image();
                        img.Height = 100;
                        img.Width = 50;
                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        bi.UriSource = new Uri("Images/rotor" + rotorarray[2].map + ".jpg", UriKind.Relative);
                        bi.EndInit();
                        img.Source = bi;


                        Canvas.SetLeft(img, 50 * rotorarray[2].map);

                        dropBoxCanvas.Children.Add(img);
                        img.Uid = "" + rotorarray[2].map;
                        img.Cursor = Cursors.Hand;

                        img.PreviewMouseMove += Rotor_MouseMove1;
                        rotorimgs[rotorarray[2].map - 1] = img;

                        rotorarray[2].helpNextAnimation -= helptoanimate2;
                        rotorarray[2].helpNextAnimation2 -= helptoanimate8;
                        rotorarea.Children.Remove(rotorarray[2]);

                        Rotor2 rotor = new Rotor2(dummyset.Rotor1 + 1, this.Width, this.Height);
                        rotor.fast = speed * 80;
                        rotor.Cursor = Cursors.Hand;
                        rotor.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
                        Canvas.SetLeft(rotor, 460);
                        rotorarea.Children.Add(rotor);
                        rotor.PreviewMouseMove += new MouseEventHandler(Rotor_MouseMove);

                        rotorarray[2] = rotor;
                        rotorarray[2].helpNextAnimation += helptoanimate2;
                        rotorarray[2].helpNextAnimation2 += helptoanimate8;

                        
                        
                    }, null);
                }

                if (dummyset.Rotor2 + 1 != rotorarray[1].map)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {

                        dropBoxCanvas.Children.Remove(rotorimgs[dummyset.Rotor2]);
                        Image img = new Image();
                        img.Height = 100;
                        img.Width = 50;
                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        bi.UriSource = new Uri("Images/rotor" + rotorarray[1].map + ".jpg", UriKind.Relative);
                        bi.EndInit();
                        img.Source = bi;
                        dropBoxCanvas.Children.Add(img);
                        Canvas.SetLeft(img, 50 * rotorarray[1].map);
                        img.Uid = "" + rotorarray[1].map;
                        img.Cursor = Cursors.Hand;

                        img.PreviewMouseMove += Rotor_MouseMove1;

                        rotorimgs[rotorarray[1].map - 1] = img;

                        rotorarray[1].helpNextAnimation -= helptoanimate2;
                        rotorarray[1].helpNextAnimation2 -= helptoanimate8;
                        
                        rotorarea.Children.Remove(rotorarray[1]);

                        Rotor2 rotor = new Rotor2(dummyset.Rotor2 + 1, this.Width, this.Height);
                        rotor.fast = speed * 80;
                        rotor.Cursor = Cursors.Hand;
                        rotor.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
                        Canvas.SetLeft(rotor, 230);
                        rotorarea.Children.Add(rotor);
                        rotor.PreviewMouseMove += new MouseEventHandler(Rotor_MouseMove);

                        rotorarray[1] = rotor;
                        rotorarray[1].helpNextAnimation += helptoanimate3;
                        rotorarray[1].helpNextAnimation2 += helptoanimate7;
                        
                    }, null);
                }

                if (dummyset.Rotor3 + 1 != rotorarray[0].map)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {

                        dropBoxCanvas.Children.Remove(rotorimgs[dummyset.Rotor3]);

                        Image img = new Image();
                        img.Height = 100;
                        img.Width = 50;
                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        bi.UriSource = new Uri("Images/rotor" + rotorarray[0].map + ".jpg", UriKind.Relative);
                        bi.EndInit();
                        img.Source = bi;
                        dropBoxCanvas.Children.Add(img);
                        Canvas.SetLeft(img, 50 * rotorarray[0].map);
                        img.Uid = "" + rotorarray[0].map;
                        img.Cursor = Cursors.Hand;

                        img.PreviewMouseMove += Rotor_MouseMove1;

                        rotorimgs[rotorarray[0].map - 1] = img;

                        rotorarray[0].helpNextAnimation -= helptoanimate2;
                        rotorarray[0].helpNextAnimation2 -= helptoanimate8;
                        
                        rotorarea.Children.Remove(rotorarray[0]);

                        Rotor2 rotor = new Rotor2(dummyset.Rotor3 + 1, this.Width, this.Height);
                        rotor.fast = speed * 80;
                        rotor.Cursor = Cursors.Hand;
                        rotor.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
                        Canvas.SetLeft(rotor, 0);
                        rotorarea.Children.Add(rotor);
                        rotor.PreviewMouseMove += new MouseEventHandler(Rotor_MouseMove);

                        rotorarray[0] = rotor;
                        rotorarray[0].helpNextAnimation += helptoanimate4;
                        rotorarray[0].helpNextAnimation2 += helptoanimate6;
                    }, null);
                }
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    dropBoxCanvas.Children.Remove(rotorimgs[dummyset.Rotor3]);
                    dropBoxCanvas.Children.Remove(rotorimgs[dummyset.Rotor2]);
                    dropBoxCanvas.Children.Remove(rotorimgs[dummyset.Rotor1]);
                }, null);
                justonce = true;
            }
        }
        */


        public EnigmaPresentation()
        {
            InitializeComponent();
            SizeChanged += sizeChanged;
            //mainWindow.WindowStyle = WindowStyle.None;
            slider1.ValueChanged += this.sliderValueChanged;

            slider1.Minimum = 0.5;
            slider1.Maximum = 6;

            Stop.Click += stopclick;
            destop.Click += deStopClick;
            m4on.Click += m4onClick;
            play.Click += playClick;
            //mainmain.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(DragSource_PreviewMouseLeftButtonDown);
            //mainmain.PreviewMouseMove += new MouseEventHandler(DragSource_PreviewMouseMove);



            int x = 30;
            int y = 50;

            int modx = 0;
            int mody = 0;
            int modz = 0;

            

            for (int i = 0; i < 26; i++)
            {
                
                switchlist[i] = i;
                Button b = new Button();
                b.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
                b.Click += List_MouseLeftButtonDown;
                b.Background = Brushes.LightBlue;
                b.Height = y;
                b.Width = x;
                b.PreviewMouseMove +=new MouseEventHandler(List_MouseMove);
                b.Drop += List_Drop;
                //b.DragEnter+=List_DragEnter;
                //b.AllowDrop=true;
                b.Uid = ""+i;
                b.Content = Convert.ToChar(i+80)+"" ;
                b.Content = i;
                b.Cursor = Cursors.Hand;
                b.Opacity = 1;


                //b.Style.Resources.Source = 

                //Style mystyle = new Style();
                //mystyle.Resources.Source = StaticResource GlassButton;
                //b.BorderThickness = new Thickness(10.0);
                steckerbrett.Children.Add(b);
                bList[i] = b;

                Line l = new Line();
                l.Name = "l" + i;
                l.X1 = x/2+i*x;
                l.Y1 = 0;
                l.X2 = x/2+i*x;
                l.Y2 = 200;
                
                SolidColorBrush redBrush = new SolidColorBrush();

                redBrush.Color = Colors.Black;

              
                redBrush.Color = Colors.Black;

                TextBlock t = new TextBlock();
                t.Text =  ""+Convert.ToChar(i + 65) ;
                t.Height = x;
                t.Width = x;
                t.Background = Brushes.SkyBlue;
                t.TextAlignment = TextAlignment.Center;
                if(i%2 == 0)
                    t.Background = Brushes.LightSeaGreen;
                alpha.Children.Add(t);
                TextBlock t1 = new TextBlock();
                t1.Text = Convert.ToChar(i + 65) + "";
                t1.Height = x;
                t1.Width = x;
                t.FontSize= 20;
                t1.FontSize = 20;
                t1.Background = Brushes.SkyBlue;
                t1.TextAlignment = TextAlignment.Center;
                if (i % 2 == 0)
                    t1.Background = Brushes.LightSeaGreen;
                
                alpha2.Children.Add(t1);
                inputList[i] = t;
                outputList[i] = t1;
                //alpha2.Children.Add(t1);
                // Set Line's width and color

                l.StrokeThickness = 1;

                l.Stroke = redBrush;

                Line l2 = new Line();
                l2.X1 = x / 2 + i * x;
                l2.Y1 = 0;
                l2.X2 = x / 2 + i * x;
                l2.Y2 = 20 + umkehrlist[i] * 10;

                l2.StrokeThickness = 1;

                //l2.Stroke = redBrush;
                
                umlList[i, 0] = l2;
                if(umkehrlist[i]<i)
                maingrid2.Children.Add(l2);

                Line l3 = new Line();
                l3.X1 = x / 2 + umkehrlist[i] * x;
                l3.Y1 = 20 + umkehrlist[i] * 10;
                l3.X2 = x / 2 + i * x;
                l3.Y2 = 20 + umkehrlist[i] * 10;

                l3.StrokeThickness = 1;

               // l3.Stroke = redBrush;

                umlList[i, 1] = l3;
                if(umkehrlist[i]<i)
                maingrid2.Children.Add(l3);

                lList[i] = l;
                maingrid.Children.Add(l);
                
                NameScope.GetNameScope(this).RegisterName("l"+i, l);


                Line l4 = new Line();
                l4.X1 = x / 2 + umkehrlist[i] * x;
                l4.Y1 = 0;
                l4.X2 = x / 2 + umkehrlist[i] * x;
                l4.Y2 = 20+umkehrlist[i] * 10;

                l4.StrokeThickness = 1;

               // l4.Stroke = redBrush;

                umlList[i, 2] = l4;
                if (umkehrlist[i]<i)
                maingrid2.Children.Add(l4);

                Button taste = new Button();
                taste.Height = 39;
                taste.Width = 39;
                taste.Uid = i+"";
                taste.Content = ""+Convert.ToChar(i + 65);
                taste.Click += tasteClick;
                taste.FontSize=25;
                Canvas.SetLeft(taste,50*i);

                

                //Canvas hschalter = schalter();

                if (i %3==0)
                {
                    Canvas hschalter = schalter(0,i);
                    Canvas hleuchte = leuchte(0,i);

                    Canvas.SetLeft(taste, 90 * modx-5);
                    Canvas.SetLeft(hschalter, 90 * modx -10);
                    Canvas.SetTop(hschalter, 0);
                    Canvas.SetLeft(hleuchte, 90 * modx - 5);

                    tastaturrow0.Children.Add(taste);
                    schalterrow0.Children.Add(hschalter);
                    schalterlist3.Add(hschalter);
                    leuchtenlist3.Add(hleuchte);

                    leuchten.Children.Add(hleuchte);
                    modx++;
                }

                if (i %3==1)
                {
                    Canvas hschalter = schalter(1,i);
                    Canvas hleuchte = leuchte(1, i);

                    Canvas.SetLeft(taste, 90 * mody  + 25);
                    Canvas.SetLeft(hschalter, 90 * mody+20);
                    Canvas.SetLeft(hleuchte, 90 * mody + 25);

                    Canvas.SetTop(hschalter, 0);
                    Canvas.SetTop(hleuchte, 58);

                    tastaturrow1.Children.Add(taste);
                    schalterrow1.Children.Add(hschalter);
                    schalterlist3.Add(hschalter);
                    leuchtenlist3.Add(hleuchte);
                    leuchten.Children.Add(hleuchte);

                    mody++;
                    
                }
                if (i % 3 == 2)
                {
                    Canvas hschalter = schalter(2,i);
                    Canvas hleuchte = leuchte(2, i);

                    Canvas.SetLeft(taste, 90 * modz + 55);
                    Canvas.SetLeft(hschalter, 90 * modz + 50);
                    Canvas.SetLeft(hleuchte, 90 * modz + 55);

                    Canvas.SetTop(hleuchte, 118);

                    leuchten.Children.Add(hleuchte);

                    tastaturrow2.Children.Add(taste);
                    schalterrow2.Children.Add(hschalter);
                    leuchtenlist3.Add(hleuchte);
                    schalterlist3.Add(hschalter);
                    modz++;
                }

                

                tList[i] = taste;
            }

            batterieMalen();

             rotorlocks1 = rotorlocks();
             rotorlocks2 = rotorlocks();
             rotorlocks3 = rotorlocks();

            

            Canvas.SetLeft(rotorlocks1, 200);
            Canvas.SetLeft(rotorlocks2, 430);
            Canvas.SetLeft(rotorlocks3, 660);

            rotorarea.Children.Add(rotorlocks1);
            rotorarea.Children.Add(rotorlocks2);
            rotorarea.Children.Add(rotorlocks3);

            Rotor2 rotor = new Rotor2(3, this.Width, this.Height);
            rotor.fast = speed * 80;
            rotor.Cursor =  Cursors.Hand;
            Canvas.SetLeft(rotor,0);
            rotorarea.Children.Add(rotor);
            rotorarray[0] = rotor;

            rotor.PreviewMouseMove += new MouseEventHandler(Rotor_MouseMove);
            rotor.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
            
            dropBox.Drop += List_Drop2;
            dropBoxWalze.Drop += List_Drop21;


            Walze walze = new Walze(2, this.Width, this.Height);
            walze.fast = speed * 80;
            Canvas.SetLeft(walze, 0);
            Canvas.SetTop(walze, 60);
            walzenarea.Children.Add(walze);
            walze.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
            walze.PreviewMouseMove += new MouseEventHandler(Walze_MouseMove);
            this.walze = walze;

            Rotor2 rotor1 = new Rotor2(2, this.Width, this.Height);
            rotor1.fast = speed * 80;
            rotor1.Cursor = Cursors.Hand;
            rotor1.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
            Canvas.SetLeft(rotor1, 230);
            rotorarea.Children.Add(rotor1);
            rotor1.PreviewMouseMove += new MouseEventHandler(Rotor_MouseMove);

            rotorarray[1] = rotor1;

            Rotor2 rotor2 = new Rotor2(1, this.Width, this.Height);
            rotor2.fast = speed * 80;
            rotor2.Cursor = Cursors.Hand;
            rotor2.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
            Canvas.SetLeft(rotor2, 460);
            rotorarea.Children.Add(rotor2);
            rotor2.PreviewMouseMove += new MouseEventHandler(Rotor_MouseMove);
            rotorarray[2] = rotor2;

            rotorarray[2].helpNextAnimation += helptoanimate2;
            rotorarray[1].helpNextAnimation += helptoanimate3;
            rotorarray[0].helpNextAnimation += helptoanimate4;
            rotorarray[1].helpNextAnimation2 += helptoanimate7;
            rotorarray[2].helpNextAnimation2 += helptoanimate8;
            rotorarray[0].helpNextAnimation2 += helptoanimate6;

            walze.helpNextAnimation += helptoanimate5;

            /*
            TextBlock tebo = new TextBlock();

            tebo.Background = Brushes.LightSkyBlue;
            tebo.Height = 100;
            tebo.Width = 50;
            tebo.TextAlignment = TextAlignment.Center;
            tebo.FontSize = 50;
            tebo.Text = "IV";
            tebo.Uid = "4";
            Canvas.SetLeft(tebo, 50 * 4);
            dropBoxCanvas.Children.Add(tebo);
            tebo.Cursor = Cursors.Hand;
            Image sponge = new Image();
            
                
                


            TextBlock tebo2 = new TextBlock();
            tebo2.Background = Brushes.LightSkyBlue;
            tebo2.Height = 100;
            tebo2.Width = 50;
            tebo2.TextAlignment = TextAlignment.Center;
            tebo2.FontSize = 50;
            tebo2.Uid = "5";
            tebo2.Text = "V";
            Canvas.SetLeft(tebo2, 50 * 5);
            dropBoxCanvas.Children.Add(tebo2);
            tebo2.Cursor = Cursors.Hand;
            

            TextBlock tebo3 = new TextBlock();

            tebo3.Background = Brushes.LightSkyBlue;
            tebo3.Height = 100;
            tebo3.Width = 50;
            tebo3.TextAlignment = TextAlignment.Center;
            tebo3.FontSize = 50;
            tebo3.Uid = "1";
            tebo3.Text = "A";
            Canvas.SetLeft(tebo3, 50 * 1);
            dropBoxCanvasWalze.Children.Add(tebo3);
            tebo3.Cursor = Cursors.Hand;

           

            TextBlock tebo4 = new TextBlock();

            tebo4.Background = Brushes.LightSkyBlue;
            tebo4.Height = 100;
            tebo4.Width = 50;
            tebo4.TextAlignment = TextAlignment.Center;
            tebo4.FontSize = 50;
            tebo4.Uid = "3";
            tebo4.Text = "C";
            Canvas.SetLeft(tebo4, 50 * 3);
            dropBoxCanvasWalze.Children.Add(tebo4);
            tebo4.Cursor = Cursors.Hand;
*/
            Image img1 = new Image();
            img1.Height = 100;
            img1.Width = 50;
            BitmapImage bi11= new BitmapImage();
            bi11.BeginInit();
            bi11.UriSource = new Uri("Images/rotor5.jpg", UriKind.Relative);
            bi11.EndInit();
            img1.Source = bi11;
            dropBoxCanvasWalze.Children.Add(img1);
            Canvas.SetLeft(img1, 50 * 1);
            img1.Uid = "1";
            img1.Cursor = Cursors.Hand;

            Image img2 = new Image();
            img2.Height = 100;
            img2.Width = 50;
            BitmapImage bi12 = new BitmapImage();
            bi12.BeginInit();
            bi12.UriSource = new Uri("Images/rotor4.jpg", UriKind.Relative);
            bi12.EndInit();
            img2.Source = bi12;
            dropBoxCanvasWalze.Children.Add(img2);
            Canvas.SetLeft(img2, 50 * 3);
            img2.Uid = "3";
            img2.Cursor = Cursors.Hand;

            Image img4 = new Image();
            img4.Height = 100;
            img4.Width = 50;
            BitmapImage bi1 = new BitmapImage();
            bi1.BeginInit();
            bi1.UriSource = new Uri("Images/rotor5.jpg", UriKind.Relative);
            bi1.EndInit();
            img4.Source = bi1;
            dropBoxCanvas.Children.Add(img4);
            Canvas.SetLeft(img4, 50 * 5);
            img4.Uid = "5";
            img4.Cursor = Cursors.Hand;

            Image img3 = new Image();
            img3.Height = 100;
            img3.Width = 50;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri("Images/rotor4.jpg", UriKind.Relative);
            bi.EndInit();
            img3.Source = bi;
            dropBoxCanvas.Children.Add(img3);
            Canvas.SetLeft(img3, 50 * 4);
            img3.Uid = "4";
            img3.Cursor = Cursors.Hand;

            rotorimgs[3] = img3;
            rotorimgs[4] = img4;

            walzeimgs[0] = img1;
            walzeimgs[2] = img2;
            img3.PreviewMouseMove += Rotor_MouseMove1;
            img4.PreviewMouseMove += Rotor_MouseMove1;

            //tebo.PreviewMouseMove += Rotor_MouseMove1;
            //tebo2.PreviewMouseMove += Rotor_MouseMove1;

            

            img1.PreviewMouseMove += Walze_MouseMove1;
            img2.PreviewMouseMove += Walze_MouseMove1;


            nop.Duration = new Duration(TimeSpan.FromMilliseconds((1000)));
            nop.Completed += tasteClick2;
                

            fadeIn.From = 0.0;
            fadeIn.To = 1.0;
            fadeIn.Duration = new Duration(TimeSpan.FromMilliseconds((1000)));

            fadeOut.From = 1.0;
            fadeOut.To = 0.0;
            fadeOut.Duration = new Duration(TimeSpan.FromMilliseconds((1000)));

            String input = "LJPQHSVDWCLYXZQFXHIUVWDJOBJNZXRCWEOTVNJCIONTFQNSXWISXKHJDAGDJVAKUKVMJAJHSZQQJHZOIAVZOWMSCKASRDNXKKSRFHCXCMPJGXYIJCCKISYYSHETXVVOVDQLZYTNJXNUWKZRXUJFXMBDIBRVMJKRHTCUJQPTEEIYNYNJBEAQJCLMUODFWMARQCFOBWN";
            inputtebo = new List<TextBlock>();

            for (int i = 0; i < input.Length; i++) 
            {
                if (i % 5 == 0 && i != 0)
                {
                    TextBlock t1 = new TextBlock();
                    t1.Text = " ";
                    t1.FontSize = 40;
                    inputPanel.Children.Add(t1);

                }

                TextBlock t = new TextBlock();
                t.FontFamily = new FontFamily("Courier New");
                t.Text = input[i] + "" ;
                t.FontSize = 41;
                inputPanel.Children.Add(t);
                inputtebo.Add(t);
                
            }

        }



        private Canvas rotorlocks() 
        {
            Canvas temp = new Canvas();
            StackPanel stack = new StackPanel();
            Canvas.SetTop(stack,60);
            stack.Orientation = Orientation.Vertical;
            for (int i = 0; i < 26; i++)
            {
                TextBlock t = new TextBlock();
                t.Text = "" + Convert.ToChar(i + 65);
                t.Width = 30.0;
                t.Height = 29.4;


                t.FontSize = 20;
                t.Background = Brushes.SkyBlue;
                t.TextAlignment = TextAlignment.Center;
                if (i % 2 == 0)
                    t.Background = Brushes.LightSeaGreen;


                stack.Children.Add(t);
            }
            temp.Children.Add(stack);
            return temp;
        }

        private void batterieMalen() 
        {
            Line l = new Line();
            l.Stroke = Brushes.Black;
            l.X1 = 80;
            l.Y2 = 10;
            l.X2 = 80;
            l.Y1 = 68;
            l.Opacity = 0.3;

            tobat2[0, 0] = l;
            batterie.Children.Add(l);

            Line l01 = new Line();
            l01.Stroke = Brushes.Black;
            l01.X1 = 80;
            l01.Y2 = 68;
            l01.X2 = 80;
            l01.Y1 = 128;
            l01.Opacity = 0.3;

            tobat2[1, 0] = l01;
            batterie.Children.Add(l01);

            Line l02 = new Line();
            l02.Stroke = Brushes.Black;
            l02.X1 = 80;
            l02.Y2 = 128;
            l02.X2 = 80;
            l02.Y1 = 180;
            l02.Opacity = 0.3;

            tobat2[2, 0] = l02;
            batterie.Children.Add(l02);


            Line l1 = new Line();
            l1.Stroke = Brushes.Black;
            l1.X2 = 77;
            l1.Y1 = 10;
            l1.X1 = 80;
            l1.Y2 = 10;
            l1.Opacity = 0.3;

            tobat2[0, 1] = l1;
            batterie.Children.Add(l1);

            Line l2 = new Line();
            l2.Stroke = Brushes.Black;
            l2.X1 = 80;
            l2.Y1 = 68;
            l2.X2 = 80;
            l2.Y2 = 68;
            l2.Opacity = 0.3;

            tobat2[1, 1] = l2;
            batterie.Children.Add(l2);


            Line l3 = new Line();
            l3.Stroke = Brushes.Black;
            l3.X2 = 47;
            l3.Y1 = 128;
            l3.X1 = 80;
            l3.Y2 = 128;
            l3.Opacity = 0.3;

            tobat2[2, 1] = l3;
            batterie.Children.Add(l3);


            Line l4 = new Line();
            l4.Stroke = Brushes.Black;
            l4.X1 = 50;
            l4.Y1 = 180;
            l4.X2 = 110;
            l4.Y2 = 180;
            l4.StrokeThickness = 3;
            

            batterie.Children.Add(l4);

            Line l5 = new Line();
            l5.Stroke = Brushes.Black;
            l5.X1 = 65;
            l5.Y1 = 190;
            l5.X2 = 95;
            l5.Y2 = 190;
            l5.StrokeThickness = 3;


            batterie.Children.Add(l5);


            Line l6 = new Line();
            l6.Stroke = Brushes.Black;
            l6.X1 = 80;
            l6.Y1 = 190;
            l6.X2 = 80;
            l6.Y2 = 274;
            l6.Opacity = 0.3;

            frombat2[0, 0] = l6;
            batterie.Children.Add(l6);

            Line l61 = new Line();
            l61.Stroke = Brushes.Black;
            l61.X1 = 80;
            l61.Y1 = 274;
            l61.X2 = 80;
            l61.Y2 = 363;
            l61.Opacity = 0.3;

            frombat2[1, 0] = l61;
            batterie.Children.Add(l61);

            Line l62 = new Line();
            l62.Stroke = Brushes.Black;
            l62.X1 = 80;
            l62.Y1 = 363;
            l62.X2 = 80;
            l62.Y2 = 452;
            l62.Opacity = 0.3;

            frombat2[2, 0] = l62;
            batterie.Children.Add(l62);

            Line l70 = new Line();
            l70.Stroke = Brushes.Black;
            l70.X1 = 80;
            l70.Y1 = 274;
            l70.X2 = 80;
            l70.Y2 = 274;
            l70.Opacity = 0.3;

            frombat2[0, 1] = l70;
            batterie.Children.Add(l70);

            Line l71 = new Line();
            l71.Stroke = Brushes.Black;
            l71.X1 = 80;
            l71.Y1 = 363;
            l71.X2 = 80;
            l71.Y2 = 363;
            l71.Opacity = 0.3;

            frombat2[1, 1] = l71;
            batterie.Children.Add(l71);


            Line l72 = new Line();
            l72.Stroke = Brushes.Black;
            l72.X1 = 80;
            l72.Y1 = 452;
            l72.X2 = 34;
            l72.Y2 = 452;
            l72.Opacity = 0.3;

            frombat2[2, 1] = l72;
            batterie.Children.Add(l72);


        }

        private Canvas leuchte(int swint,int i)
        { 
            Canvas myCanvas = new Canvas();

            Rectangle k = new Rectangle();
            k.RadiusX = 20;
            k.RadiusY = 20;
            k.Height = 40;
            k.Width = 40;
            k.Stroke = Brushes.Black;
            //k.Fill = Brushes.DimGray;
            Canvas.SetTop(k,20);
            Canvas.SetLeft(k, 3);

            leuchtenList[i] = k;
            myCanvas.Children.Add(k);

            TextBlock b = new TextBlock();
            b.Text = Convert.ToChar(i + 65)+"";
            b.FontSize = 25;
            b.Width = 40;
            b.TextAlignment = TextAlignment.Center;
            //b.Background = Brushes.GreenYellow;
            Canvas.SetTop(b, 22);
            Canvas.SetLeft(b, 3);

            myCanvas.Children.Add(b);

            Line l = new Line();
            l.Stroke = Brushes.Black;
            l.X1 = 23;
            l.Y1 = 10;
            l.X2 = 23;
            l.Y2 = 20;
            l.Opacity = 0.3;

            toBat[i, 5] = l;
            myCanvas.Children.Add(l);

            Line l1 = new Line();
            l1.Stroke = Brushes.Black;
            l1.X2 = 23;
            l1.Y1 = 10;
            l1.X1 = 112;
            l1.Y2 = 10;
            l1.Opacity = 0.3;

            switch (swint)
            {
                case 0: tobat1[i / 3, swint] = l1;
                    break;
                case 1: tobat1[i / 3, swint] = l1;
                    break;
                case 2: tobat1[i / 3, swint] = l1;
                    break;
            }

            if (i == 25) 
            {
                l1.X1 = 86;
            }

            myCanvas.Children.Add(l1);


            return myCanvas;
        }

        private Canvas schalter(int swint, int i) 
        {
            Line[] line = new Line[2];
            Line[] line2 = new Line[6];

            Canvas myCanvas = new Canvas();
            myCanvas.Height = 40;
            myCanvas.Width = 40;
            Line l = new Line();
            l.Stroke = Brushes.Black;
            l.X1 = 40;
            l.Y1 = 5;
            l.X2 = 40;
            l.Y2 = 18;
            
            myCanvas.Children.Add(l);
            
            line[0] = l;
            Line ln0 = new Line();
            ln0.Stroke = Brushes.Black;
            ln0.X1 = 40;
            ln0.Y1 = 5;
            ln0.X2 = 25;
            ln0.Y2 = 5;
            myCanvas.Children.Add(ln0);

            Line ln1 = new Line();
            ln1.Stroke = Brushes.Black;
            ln1.X1 = 25;
            ln1.Y1 = 5;
            ln1.X2 = 25;
            ln1.Y2 = 0;
            myCanvas.Children.Add(ln1);

            Line l2 = new Line();
            l2.Stroke = Brushes.Black;
            l2.X1 = 40;
            l2.Y1 = 18;
            l2.X2 = 5;
            l2.Y2 = 23;
            myCanvas.Children.Add(l2);

            line[1] = l2;
            line2[0] = l2;
            schalterlist.Add(line);


            Rectangle r = new Rectangle();
            r.Stroke = Brushes.Black;
            r.RadiusX = 3;
            r.RadiusY = 3;
            Canvas.SetLeft(r, 25);
            Canvas.SetTop(r, 13);
            r.Height = 7;
            r.Width = 7;


            myCanvas.Children.Add(r);

            

            Line l4 = new Line();
            l4.Stroke = Brushes.Black;
            l4.X2 = 28;
            l4.Y1 = 10;
            l4.X1 = 50;
            l4.Y2 = 10;

            toBat[i, 0] = l4;
            myCanvas.Children.Add(l4);

            Line ln4 = new Line();
            ln4.Stroke = Brushes.Black;
            ln4.X1 = 29;
            ln4.Y2 = 14;
            ln4.X2 = 29;
            ln4.Y1 = 10;

            toBat[i, 1] = ln4;
            myCanvas.Children.Add(ln4);

            Line ln41 = new Line();
            ln41.Stroke = Brushes.Black;
            ln41.X1 = 50;
            ln41.Y2 = 10;
            ln41.X2 = 50;
            ln41.Y1 = -45;

            toBat[i, 2] = ln41;
            myCanvas.Children.Add(ln41);

            Line ln42 = new Line();
            ln42.Stroke = Brushes.Black;
            ln42.X2 = 50;
            ln42.Y2 = -45;
            ln42.X1 = 28;
            ln42.Y1 = -45;

            toBat[i, 3] = ln42;
            myCanvas.Children.Add(ln42);

            Line ln43 = new Line();
            ln43.Stroke = Brushes.Black;
            ln43.X1 = 28;
            ln43.Y2 = -45;
            ln43.X2 = 28;
            switch (swint) 
            { 
                case 0:
                    ln43.Y1 = -180;
                    break;
                case 1:
                    ln43.Y1 = -210;
                    break;
                case 2:
                    ln43.Y1 = -240;
                    break;

            }
            
            ln43.Opacity = 0.7;
            toBat[i, 4] = ln43;
            myCanvas.Children.Add(ln43);
            

            Rectangle r1 = new Rectangle();
            r1.Stroke = Brushes.Black;
            r1.RadiusX = 3;
            r1.RadiusY = 3;
            Canvas.SetLeft(r1, 25);
            Canvas.SetTop(r1, 25);
            r1.Height = 7;
            r1.Width = 7;


            myCanvas.Children.Add(r1);

            Line l5 = new Line();
            l5.Stroke = Brushes.Black;
            l5.X1 = 14;
            l5.Y1 = 28;
            l5.X2 = 25;
            l5.Y2 = 28;
            myCanvas.Children.Add(l5);
            line2[2] = l5;

            Line l6 = new Line();
            l6.Stroke = Brushes.Black;
            l6.X1 = 14;
            l6.Y1 = 35;
            l6.X2 = 14;
            l6.Y2 = 28;
            myCanvas.Children.Add(l6);

            line2[3] = l6;
            schalterlist2.Add(line2);


            Line ln6 = new Line();
            ln6.Stroke = Brushes.Black;
            ln6.X1 = 104;
            ln6.Y1 = 35;
            ln6.X2 = 14;
            ln6.Y2 = 35;
            ln6.Opacity = 0.3;
            myCanvas.Children.Add(ln6);

            switch (swint)
            {
                case 0: frombat[i / 3,swint] = ln6;
                    break;
                case 1: frombat[i / 3, swint] = ln6;
                    break;
                case 2: frombat[i / 3, swint] = ln6;
                    break;
            }


            if (i == 24)
            {
                ln6.X1 = 120;
            }

            if (i == 25) 
            {
                ln6.X1 = 90;
            }

            Line l7 = new Line();
            l7.Stroke = Brushes.Black;
            l7.X1 = 5;
            l7.Y1 = 23;
            l7.X2 = 5;
            l7.Y2 = 45;
            myCanvas.Children.Add(l7);
            line2[5] = l7;

            Line l8 = new Line();
            l8.Stroke = Brushes.Black;
            l8.X1 = 5;
            l8.Y1 = 45;
            l8.X2 = 25;
            l8.Y2 = 45;
            myCanvas.Children.Add(l8);
            line2[4] = l8;

            Line l9 = new Line();
            l9.Stroke = Brushes.Black;
            l9.X1 = 25;
            l9.Y1 = 45;
            l9.X2 = 25;
            switch(swint)
            {
                case 0: l9.Y2 = 250;
                    break;
                case 1: l9.Y2 = 140;
                    break;
                case 2: l9.Y2 = 50;
                    break;
            }
            myCanvas.Children.Add(l9);
            line2[1] = l9;
            return myCanvas;
        }

       

        private void tastedruecken(object sender, KeyEventArgs e)
        {
            Key k = e.Key;
            string s = k.ToString();
            int x = (int)s[0] - 65;
            
            tasteClick(bList[x], EventArgs.Empty);
            Debug.Text = s;
        }
        

        private void List_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            startPoint = e.GetPosition(null);
        }

        private int merken=-1;

        private void everythingblack() 
        {

            StackPanel dummyLock1 = (StackPanel)rotorlocks3.Children[0];
            
            foreach (TextBlock t in dummyLock1.Children) 
            {
                t.Background = Brushes.LightSeaGreen;
                String s = t.Text;
                if ((int)s[0] % 2 == 0)
                {
                    t.Background = Brushes.SkyBlue;
                }
            }
             dummyLock1 = (StackPanel)rotorlocks2.Children[0];

            foreach (TextBlock t in dummyLock1.Children)
            {
                t.Background = Brushes.LightSeaGreen;
                String s = t.Text;
                if ((int)s[0] % 2 == 0)
                {
                    t.Background = Brushes.SkyBlue;
                }
            }
             dummyLock1 = (StackPanel)rotorlocks1.Children[0];

            foreach (TextBlock t in dummyLock1.Children)
            {
                t.Background = Brushes.LightSeaGreen;
                String s = t.Text;
                if ((int)s[0] % 2 == 0)
                {
                    t.Background = Brushes.SkyBlue;
                }
            }

            if(rotorlocks4 != null)
            {
                dummyLock1 = (StackPanel)rotorlocks4.Children[0];

                foreach (TextBlock t in dummyLock1.Children)
                {
                    t.Background = Brushes.LightSeaGreen;
                    String s = t.Text;
                    if ((int)s[0] % 2 == 0)
                    {
                        t.Background = Brushes.SkyBlue;
                    }
                }
            }

            foreach(Line l in linesToThrowAway)
            {

                foreach (Canvas c in leuchtenlist3)
                {
                    if (c == l.Parent)
                    {
                        c.Children.Remove(l);
                    }
                }

                if (batterie == l.Parent)
                {
                    batterie.Children.Remove(l);
                }
                
                foreach (Canvas c in schalterlist3)
                {
                    if (c == l.Parent)
                    {
                        c.Children.Remove(l);
                    }
                }
                if (mainmainmain == l.Parent)
                {
                    mainmainmain.Children.Remove(l);
                }
                if (maingrid == l.Parent)
                {
                    maingrid.Children.Remove(l);
                }
                if (maingrid2 == l.Parent)
                {
                    maingrid2.Children.Remove(l);
                }
                
                System.GC.Collect();
                

            }
            linesToThrowAway.Clear();
            linesToAnimate.Clear();
            linesToAnimate2.Clear();
            
            if(walze != null)
            walze.resetColors();

            foreach (Rotor2 r in rotorarray)
            {
                if(r != null)
                r.resetColors();
            }

            if(drawLines!=null)
            {
                foreach (Line l in drawLines)
                {
                   
                    mainmainmain.Children.Remove(l);
                    maingrid2.Children.Remove(l);
                }
            }



            foreach (Rectangle r in leuchtenList)
            {
                r.Fill = Brushes.White;
            }

            foreach (Line l in toBat)
            {
                l.Stroke =Brushes.Black;
                l.StrokeThickness = 1.0;
            }

            foreach (Line l in frombat2)
            {
                l.Stroke = Brushes.Black;
                l.StrokeThickness = 1.0;
            }

            foreach (Line[] l in schalterlist2)
            {
                l[0].Stroke=Brushes.Black;
                l[1].Stroke = Brushes.Black;
                l[2].Stroke = Brushes.Black;
                l[3].Stroke = Brushes.Black;
                l[4].Stroke = Brushes.Black;
                l[5].Stroke = Brushes.Black;
                l[0].StrokeThickness = 1.0;
                l[1].StrokeThickness = 1.0;
                l[2].StrokeThickness = 1.0;
                l[3].StrokeThickness = 1.0;
                l[4].StrokeThickness = 1.0;
                l[5].StrokeThickness = 1.0;


            }

            foreach (Line l in frombat)
            {
                if (l != null)
                {
                    l.Stroke = Brushes.Black;
                    l.StrokeThickness = 1.0;
                }
            }
            foreach (Line l in tobat1)
            {
                if (l != null)
                {
                    l.Stroke = Brushes.Black;
                    l.StrokeThickness = 1.0;
                }
            }

            foreach (Line l in tobat2)
            {
                if (l != null)
                {
                    l.Stroke = Brushes.Black;
                    l.StrokeThickness = 1.0;
                }
            }


            foreach (Line[] l in schalterlist) 
            {
                DoubleAnimation animax = new DoubleAnimation();
                animax.Duration = new Duration(TimeSpan.FromMilliseconds((0)));
                animax.From = l[0].Y2;
                animax.To = 18;
                l[0].BeginAnimation(Line.Y2Property,animax);
                l[1].BeginAnimation(Line.Y1Property, animax);
                l[0].Y2 = 18;
                l[1].Y1 = 18;
                l[0].StrokeThickness = 1.0;
                l[1].StrokeThickness = 1.0;
            }
            for (int i = 0; i < outputList.Length;i++ )
            {
                outputList[i].Background = Brushes.SkyBlue;
                if (i%2==0)
                    outputList[i].Background = Brushes.LightSeaGreen;
            }
            for (int i = 0; i < inputList.Length; i++)
            {
                inputList[i].Background = Brushes.SkyBlue;
                if (i % 2 == 0)
                    inputList[i].Background = Brushes.LightSeaGreen;
            }
            
            foreach (Line t in lList)
            {
                t.Stroke = Brushes.Black;
                t.StrokeThickness = 1.0;
            }
            foreach (Button t in bList)
            {
                t.Background = Brushes.LightBlue;
            }
            if(off)
            for (int i = 0; i < 1; i++)
            {
                try
                {
                    foreach (Object o in mainmainmain.Children)
                    {
                        if (o is Line)
                        {
                            Line l = o as Line;
                            if (l.StrokeThickness == 5.0)
                            {
                                mainmainmain.Children.Remove(l);
                            }
                        }
                    }

                    foreach (Object o in maingrid2.Children)
                    {
                        if (o is Line)
                        {
                            Line l = o as Line;
                            if (l.StrokeThickness == 5.0)
                            {
                                maingrid2.Children.Remove(l);
                            }
                        }
                    }

                }
                catch (Exception e) { i--; }
            }

            /*
            foreach (Line t in umlList)
            {
                t.Stroke = Brushes.Black;
            }*/
            
        }

        Button temp = new Button();
        Boolean blupp = true;

        private void stopclick(object sender, EventArgs e)
        {
            stop = true;
            everythingblack();
            blupp = true;
        }

        private void m4onClick(object sender, EventArgs e)
        {
            mfouron = true;
            everythingblack();
            rotorlocks4 = rotorlocks();

            Canvas.SetLeft(rotorlocks4, 890);
           
            rotorarea.Children.Add(rotorlocks4);

            Rotor2 rotor4 = new Rotor2(4, this.Width, this.Height);
            rotor4.fast = speed * 80;
            rotor4.Cursor = Cursors.Hand;
            rotor4.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
            Canvas.SetLeft(rotor4, 688);
            rotorarea.Children.Add(rotor4);
            
            rotor4.PreviewMouseMove += new MouseEventHandler(Rotor_MouseMove);

            rotorarray[2].helpNextAnimation -= helptoanimate2;
            rotorarray[1].helpNextAnimation -= helptoanimate3;
            rotorarray[0].helpNextAnimation -= helptoanimate4;
            rotorarray[1].helpNextAnimation2 -= helptoanimate7;
            rotorarray[2].helpNextAnimation2 -= helptoanimate8;
            rotorarray[0].helpNextAnimation2 -= helptoanimate6;

            
            rotorarray[3] = rotorarray[2];
            rotorarray[2] = rotorarray[1];
            rotorarray[1] = rotorarray[0];
            rotorarray[0] = rotor4;

            Canvas.SetLeft(rotorarray[2], 460); Canvas.SetLeft(rotorarray[1], 230); Canvas.SetLeft(rotorarray[0], 0); Canvas.SetLeft(rotorarray[3], 688);

            rotorarray[3].helpNextAnimation += helptoanimatem4;
            rotorarray[2].helpNextAnimation += helptoanimate2;
            rotorarray[1].helpNextAnimation += helptoanimate3;
            rotorarray[0].helpNextAnimation += helptoanimate4;
            rotorarray[0].helpNextAnimation2 += helptoanimate6;
            rotorarray[1].helpNextAnimation2 += helptoanimate7;
            rotorarray[2].helpNextAnimation2 += helptoanimate8;
            rotorarray[3].helpNextAnimation2 += helptoanimatem42;
            

            textBlocksToAnimate = new TextBlock[8];
        }

        Boolean playbool = false;

        private void playClick(object sender, EventArgs e)
        {
            everythingblack();
            ColorAnimation colorani = new ColorAnimation();
            SolidColorBrush brush = new SolidColorBrush();
            brush.Color = Colors.Transparent;

            colorani.From = Colors.Transparent;
            colorani.To = Colors.Orange;
            colorani.Duration = new Duration(TimeSpan.FromMilliseconds((speed * 160)));
            inputtebo[0].Background = brush;
            colorani.Completed += prefadeout;
            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorani);
            


            //inputtebo[0].Opacity = 0.0;
            playbool = true;
        }

        private void prefadeout(object sender, EventArgs e)
        {
            int x = Convert.ToInt32(inputtebo[0].Text[0]) - 65;
            tasteClick(bList[x], EventArgs.Empty);

            fadeOut.From = 1.0;
            fadeOut.To = 1.0;
            fadeOut.Duration = new Duration(TimeSpan.FromMilliseconds((speed * 160)));

            inputtebo[0].BeginAnimation(OpacityProperty,fadeOut);

        }

        private void prefadeout1(object sender, EventArgs e)
        {
            everythingblack();
            DoubleAnimation fadeOut2 = new DoubleAnimation();
            fadeOut2.From = 1.0;
            fadeOut2.To = 1.0;
            fadeOut2.Duration = new Duration(TimeSpan.FromMilliseconds((speed * 160)));
            fadeOut2.Completed += prefadeout2;

            inputtebo[inputcounter].BeginAnimation(OpacityProperty, fadeOut2);
            
            

        }

        private void prefadeout2(object sender, EventArgs e)
        {
            int x = Convert.ToInt32(inputtebo[inputcounter].Text[0]) - 65;
            tasteClick(bList[x], EventArgs.Empty);
            
        }

        int inputcounter = 0;
        Char outputchar;

        private void nextPlease(object sender, EventArgs e)
        {
            Debug.Text = ""+outputchar;
            inputcounter++;
            if (inputtebo.Count > inputcounter) 
            { 
                
                //int x = Convert.ToInt32(inputtebo[inputcounter].Text[0]) - 65;
                //tasteClick(bList[x], EventArgs.Empty);
                //inputtebo[inputcounter].Opacity = 0.0;
                /*ColorAnimation colorani = new ColorAnimation();
                SolidColorBrush brush = new SolidColorBrush();
                brush.Color = Colors.Transparent;

                colorani.From = Colors.Transparent;
                colorani.To = Colors.Orange;
                colorani.Duration = new Duration(TimeSpan.FromMilliseconds((1000)));
                inputtebo[inputcounter].Background = brush;
                //colorani.Completed += nextPlease0;
                brush.BeginAnimation(SolidColorBrush.ColorProperty, colorani);
                */

                if ((inputcounter-1) % 5 == 0 && inputcounter != 0)
                {
                    TextBlock t1 = new TextBlock();
                    t1.Text = " ";
                    t1.FontSize = 40;
                    outputPanel.Children.Add(t1);

                }

                TextBlock t = new TextBlock();
                t.Background = Brushes.Orange;
                t.Opacity = 0.0;
                t.Text = outputchar + "";
                t.FontSize = 42;
                t.FontFamily = new FontFamily("Courier New");

                outputPanel.Children.Add(t);
                outputtebo.Add(t);
                DoubleAnimation fadeIn2 = new DoubleAnimation();
                fadeIn2.From = 0.0;
                fadeIn2.To = 1.0;
                fadeIn2.Duration = new Duration(TimeSpan.FromMilliseconds((speed * 160)));
                fadeIn2.Completed += nextPlease0;
                t.BeginAnimation(OpacityProperty,fadeIn2);
            }
        }

        private void nextPlease0(object sender, EventArgs e)
        {
                ColorAnimation colorani = new ColorAnimation();
                SolidColorBrush brush = new SolidColorBrush();
                brush.Color = Colors.Orange;

                colorani.From = Colors.Orange;
                colorani.To = Colors.Transparent;
                colorani.Duration = new Duration(TimeSpan.FromMilliseconds((speed * 160)));
                outputtebo[outputtebo.Count-1].Background = brush;
                colorani.Completed += nextPlease1;
                brush.BeginAnimation(SolidColorBrush.ColorProperty, colorani);
       }
        private void nextPlease1(object sender, EventArgs e)
        {
            ColorAnimation colorani0 = new ColorAnimation();
            SolidColorBrush brush0 = new SolidColorBrush();
            brush0.Color = Colors.Orange;

            colorani0.From = Colors.Orange;
            colorani0.To = Colors.Transparent;
            colorani0.Duration = new Duration(TimeSpan.FromMilliseconds((speed * 160)));
            
            brush0.BeginAnimation(SolidColorBrush.ColorProperty, colorani0);
            inputtebo[inputcounter - 1].Background = brush0;

            DoubleAnimation fadeOut2 = new DoubleAnimation();
            fadeOut2.From = 1.0;
            fadeOut2.To = 0.5;
            fadeOut2.Duration = new Duration(TimeSpan.FromMilliseconds((speed * 160)));
            inputtebo[inputcounter-1].BeginAnimation(OpacityProperty, fadeOut2);

            ColorAnimation colorani = new ColorAnimation();
            SolidColorBrush brush = new SolidColorBrush();
            brush.Color = Colors.Transparent;

            colorani.From = Colors.Transparent;
            colorani.To = Colors.Orange;
            colorani.Duration = new Duration(TimeSpan.FromMilliseconds((speed * 160)));
            inputtebo[inputcounter].Background = brush;
            colorani.Completed += prefadeout1;
            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorani);
        }
        private void deStopClick(object sender, EventArgs e)
        {
            off = true;
            speed = 0;
            nop.Duration = new Duration(TimeSpan.FromMilliseconds((80)));
            
            Debug.Text  = off + "" ;
        }

        private void tasteClick(object sender, EventArgs e)
        {
           
            if (blupp)
            {
                if (!mfouron)
                {
                    if (rotorarray[0] != null && rotorarray[1] != null && rotorarray[2] != null && walze != null)
                    {

                        stop = true;
                        blupp = false;
                        temp = sender as Button;
                        if(!playbool)
                        everythingblack();
                        rotorarray[2].upperclick(sender, EventArgs.Empty);
                        if (rotorarray[0].next)
                        {

                            //rotorarray[1].upperclick(sender, EventArgs.Empty);
                        }

                        if (rotorarray[2].next)
                        {
                            rotorarray[1].upperclick(sender, EventArgs.Empty);
                        }

                        if (rotorarray[1].next)
                        {
                            rotorarray[1].upperclick(sender, EventArgs.Empty);
                            rotorarray[0].upperclick(sender, EventArgs.Empty);

                        }





                        Line l = new Line();
                        mainmainmain.Children.Add(l);

                        DoubleAnimation animax = new DoubleAnimation();
                        DoubleAnimation animax2 = new DoubleAnimation();

                        Line[] line = schalterlist[Int32.Parse(temp.Uid)];
                        //line[0].Y2 = 27;
                        //line[1].Y1 = 27;

                        animax.From = line[0].Y2;
                        animax.To = 27;

                        animax2.From = line[0].Y1;
                        animax2.To = 27;

                        animax.Duration = new Duration(TimeSpan.FromMilliseconds((speed*80)));
                        line[0].BeginAnimation(Line.Y2Property, animax);

                        animax2.Duration = new Duration(TimeSpan.FromMilliseconds((speed*80)));
                        line[1].BeginAnimation(Line.Y1Property, animax);


                        l.BeginAnimation(OpacityProperty, nop);
                        stop = false;
                    }
                }
                else
                {
                    if (rotorarray[0] != null && rotorarray[1] != null && rotorarray[2] != null &&
                        rotorarray[3] != null && walze != null)
                    {

                        stop = true;
                        blupp = false;
                        temp = sender as Button;
                        everythingblack();
                        rotorarray[3].upperclick(sender, EventArgs.Empty);
                        if (rotorarray[3].next)
                        {

                            rotorarray[2].upperclick(sender, EventArgs.Empty);
                        }

                        if (rotorarray[2].next)
                        {
                            rotorarray[1].upperclick(sender, EventArgs.Empty);
                        }

                        if (rotorarray[1].next)
                        {
                            rotorarray[1].upperclick(sender, EventArgs.Empty);
                            rotorarray[0].upperclick(sender, EventArgs.Empty);

                        }





                        Line l = new Line();
                        mainmainmain.Children.Add(l);

                        DoubleAnimation animax = new DoubleAnimation();
                        DoubleAnimation animax2 = new DoubleAnimation();

                        Line[] line = schalterlist[Int32.Parse(temp.Uid)];
                        //line[0].Y2 = 27;
                        //line[1].Y1 = 27;

                        animax.From = line[0].Y2;
                        animax.To = 27;

                        animax2.From = line[0].Y1;
                        animax2.To = 27;

                        animax.Duration = new Duration(TimeSpan.FromMilliseconds((speed * 80)));
                        line[0].BeginAnimation(Line.Y2Property, animax);

                        animax2.Duration = new Duration(TimeSpan.FromMilliseconds((speed * 80)));
                        line[1].BeginAnimation(Line.Y1Property, animax);


                        l.BeginAnimation(OpacityProperty, nop);
                        stop = false;
                    }
                }
            }
            else 
            {
                stopclick(this, EventArgs.Empty);
            }
        }
    
            

        private void tasteClick2(object sender, EventArgs e)
        {
            if (!stop)
            {
                Button button = temp as Button;


                for (int i = 0; i <= Int32.Parse(button.Uid) % 3; i++)
                {
                    frombat2[i, 0].Stroke = Brushes.Green;
                    linesToAnimate.Add(frombat2[i, 0]);
                    frombat2[i, 0].Opacity = 1.0;
                }



                frombat2[Int32.Parse(button.Uid) % 3, 1].Stroke = Brushes.Green;
                linesToAnimate.Add(frombat2[Int32.Parse(button.Uid) % 3, 1]);

                if (Int32.Parse(button.Uid) % 3 != 2)
                {
                    frombat[8, Int32.Parse(button.Uid) % 3].Stroke = Brushes.Green;
                    linesToAnimate.Add(frombat[8, Int32.Parse(button.Uid) % 3]);
                }

                for (int i = 7; i >= Int32.Parse(button.Uid) / 3; i--)
                {
                    frombat[i, Int32.Parse(button.Uid) % 3].Stroke = Brushes.Green;
                    linesToAnimate.Add(frombat[i, Int32.Parse(button.Uid) % 3]);
                }
                Line[] line = schalterlist[Int32.Parse(button.Uid)];
                line[0].Y2 = 27;
                line[1].Y1 = 27;

                Line[] line2 = schalterlist2[Int32.Parse(button.Uid)];
                line2[0].Stroke = Brushes.Green;
                line2[1].Stroke = Brushes.Green;
                line2[2].Stroke = Brushes.Green;
                line2[3].Stroke = Brushes.Green;
                line2[4].Stroke = Brushes.Green;
                line2[5].Stroke = Brushes.Green;

                linesToAnimate.Add(line2[3]);
                linesToAnimate.Add(line2[2]);
                linesToAnimate.Add(line2[0]);



                linesToAnimate.Add(line2[5]);

                linesToAnimate.Add(line2[4]);
                linesToAnimate.Add(line2[1]);



                //if (switchlist[umkehrlist[switchlist[Int32.Parse(button.Uid)]]] % 3 != 2)
                //  tobat1[8, switchlist[umkehrlist[switchlist[Int32.Parse(button.Uid)]]] % 3].Stroke = Brushes.Red;






                // int aus = DrawLines(switchlist[Int32.Parse(button.Uid)]);


                //inputList[Int32.Parse(button.Uid)].Background = Brushes.Green;
                linesToAnimate.Add(inputList[Int32.Parse(button.Uid)]);
                lList[switchlist[Int32.Parse(button.Uid)]].Stroke = Brushes.Green;
                linesToAnimate.Add(lList[switchlist[Int32.Parse(button.Uid)]]);
                //bList[switchlist[Int32.Parse(button.Uid)]].Background = Brushes.Green;
                linesToAnimate.Add(bList[switchlist[Int32.Parse(button.Uid)]]);
                //outputList[switchlist[Int32.Parse(button.Uid)]].Background = Brushes.Green;
                linesToAnimate.Add(outputList[switchlist[Int32.Parse(button.Uid)]]);
                /*umlList[switchlist[Int32.Parse(button.Uid)],0].Stroke = Brushes.Red ;
                umlList[switchlist[Int32.Parse(button.Uid)], 2].Stroke = Brushes.Red;
                umlList[switchlist[Int32.Parse(button.Uid)], 1].Stroke = Brushes.Red;
                umlList[umkehrlist[switchlist[Int32.Parse(button.Uid)]], 1].Stroke = Brushes.Red;
                umlList[umkehrlist[switchlist[Int32.Parse(button.Uid)]], 0].Stroke = Brushes.Red;
                umlList[umkehrlist[switchlist[Int32.Parse(button.Uid)]], 2].Stroke = Brushes.Red;
                */

                int aus = DrawLines2(switchlist[Int32.Parse(button.Uid)]);

                

                linesToAnimate2.Add(outputList[aus]);
                linesToAnimate2.Add(bList[aus]);


                linesToAnimate2.Add(lList[aus]);
                linesToAnimate2.Add(inputList[switchlist[aus]]);



                Line[] line3 = schalterlist2[switchlist[aus]];
                line3[0].Stroke = Brushes.Red;
                line3[1].Stroke = Brushes.Red;
                //line3[2].Stroke = Brushes.Red;
                //line3[3].Stroke = Brushes.Red;
                line3[4].Stroke = Brushes.Red;
                line3[5].Stroke = Brushes.Red;

                linesToAnimate2.Add(line3[1]);
                linesToAnimate2.Add(line3[4]);
                linesToAnimate2.Add(line3[5]);
                linesToAnimate2.Add(line3[0]);


                toBat[switchlist[aus], 0].Stroke = Brushes.Red;
                toBat[switchlist[aus], 1].Stroke = Brushes.Red;
                toBat[switchlist[aus], 2].Stroke = Brushes.Red;
                toBat[switchlist[aus], 3].Stroke = Brushes.Red;
                toBat[switchlist[aus], 4].Stroke = Brushes.Red;
                toBat[switchlist[aus], 5].Stroke = Brushes.Red;

                linesToAnimate2.Add(toBat[switchlist[aus], 1]);
                linesToAnimate2.Add(toBat[switchlist[aus], 0]);
                linesToAnimate2.Add(toBat[switchlist[aus], 2]);
                linesToAnimate2.Add(toBat[switchlist[aus], 3]);
                linesToAnimate2.Add(toBat[switchlist[aus], 4]);

                linesToAnimate2.Add(leuchtenList[switchlist[aus]]);
                linesToAnimate2.Add(toBat[switchlist[aus], 5]);

                outputchar = Convert.ToChar(switchlist[aus] + 65);


                List<Line> dummy = new List<Line>();


                for (int i = 7; i >= switchlist[aus] / 3; i--)
                {
                    tobat1[i, switchlist[aus] % 3].Stroke = Brushes.Red;
                    dummy.Add(tobat1[i, switchlist[aus] % 3]);

                }
                for (int i = dummy.Count - 1; i >= 0; i--)
                    linesToAnimate2.Add(dummy[i]);


                if (switchlist[aus] % 3 != 2)
                {
                    tobat1[8, switchlist[aus] % 3].Stroke = Brushes.Red;
                    linesToAnimate2.Add(tobat1[8, switchlist[aus] % 3]);
                }
                linesToAnimate2.Add(tobat2[switchlist[aus] % 3, 1]);
                List<Line> dummy2 = new List<Line>();
                for (int i = 2; i >= switchlist[aus] % 3; i--)
                {
                    tobat2[i, 0].Stroke = Brushes.Red;
                    dummy2.Add(tobat2[i, 0]);
                    tobat2[i, 0].Opacity = 1.0;
                }


                for (int i = dummy2.Count - 1; i >= 0; i--)
                    linesToAnimate2.Add(dummy2[i]);

                tobat2[switchlist[aus] % 3, 1].Stroke = Brushes.Red;


                //leuchtenList[switchlist[aus]].Fill = Brushes.Red;

                //outputList[aus].Background = Brushes.Red;
                //bList[aus].Background = Brushes.Red;
                lList[aus].Stroke = Brushes.Red;

                //inputList[switchlist[aus]].Background = Brushes.Red;
                whostoanimate = 0;
                if (!off)
                    animatelines(0);

                else 
                {
                    foreach (Object o in linesToAnimate)
                    {
                        if (o is Line)
                        {
                            Line l = o as Line;
                            l.Stroke = Brushes.Green;
                            l.StrokeThickness = 5;
                        }
                        if (o is Button)
                        {
                            Button b = o as Button;
                            b.Background = Brushes.Green;

                        }
                        if (o is TextBlock)
                        {
                            TextBlock b = o as TextBlock;
                            b.Background = Brushes.Green;

                        }
                    }

                    foreach (Object o in linesToAnimate2)
                    {
                        if (o is Line)
                        {
                            Line l = o as Line;
                            l.Stroke = Brushes.Red;
                            l.StrokeThickness = 5;
                        }
                        if (o is Button)
                        {
                            Button b = o as Button;
                            b.Background = Brushes.Red;

                        }
                        if (o is TextBlock)
                        {
                            TextBlock b = o as TextBlock;
                            b.Background = Brushes.Red;

                        }
                        if (o is Rectangle)
                        {
                            Rectangle r = o as Rectangle;
                            r.Fill = Brushes.Yellow;

                        }

                    }

                    textBlocksToAnimate[0].Background = Brushes.Green;
                    textBlocksToAnimate[1].Background = Brushes.Green;
                    textBlocksToAnimate[2].Background = Brushes.Green;
                    textBlocksToAnimate[3].Background = Brushes.Red;
                    textBlocksToAnimate[4].Background = Brushes.Red;
                    textBlocksToAnimate[5].Background = Brushes.Red;
                    
                }
                //blupp = true;
            }
        }

        private int DrawLines2(int fromboard)
        {
            if (!stop)
            {
                int m4 = 0;
                if (mfouron)
                    m4 = 228;
                Line l = new Line();
                l.Stroke = Brushes.Green;
                l.X1 = fromboard * 30 + 15;
                l.Y1 = -5;
                l.X2 = fromboard * 30 + 15;
                l.Y2 = 30.5;
                maingrid2.Children.Add(l);
                linesToAnimate.Add(l);
                drawLines[0] = l;

                Line l1 = new Line();
                l1.Stroke = Brushes.Green;
                l1.X1 = fromboard * 30 + 15;
                l1.Y1 = 30.5;
                l1.X2 = 0;
                l1.Y2 = 30.5;
                maingrid2.Children.Add(l1);

                drawLines[1] = l1;
                linesToAnimate.Add(l1);
                Line l2 = new Line();
                l2.Stroke = Brushes.Green;
                l2.X1 = 1300;
                l2.Y1 = 811.5;
                l2.X2 = 990 + m4;
                l2.Y2 = 811.5;
                mainmainmain.Children.Add(l2);

                drawLines[2] = l2;
                linesToAnimate.Add(l2);

                Line l3 = new Line();
                l3.Stroke = Brushes.Green;
                l3.X1 = 990 + m4;
                l3.Y1 = 811.5;
                l3.X2 = 990 + m4;
                l3.Y2 = 74 + rotorarray[2].maparray[fromboard, 0] * 29.5;
                mainmainmain.Children.Add(l3);
                linesToAnimate.Add(l3);

                drawLines[3] = l3;

                Line l4 = new Line();
                l4.Stroke = Brushes.Green;
                l4.X1 = 990 + m4;
                l4.Y1 = 74 + rotorarray[2].maparray[fromboard, 0] * 29.5;
                l4.X2 = 950 + m4;
                l4.Y2 = 74 + rotorarray[2].maparray[fromboard, 0] * 29.5;
                mainmainmain.Children.Add(l4);
                linesToAnimate.Add(l4);
                drawLines[4] = l4;

                int fromboardhelp = fromboard;
                //textBlocksToAnimate[0].Background = Brushes.Green;

                StackPanel dummyLock1 = (StackPanel)rotorlocks3.Children[0];
                int ein = 0;
                if(mfouron)
                {
                    dummyLock1 = (StackPanel)rotorlocks4.Children[0];
                    textBlocksToAnimate[6] = (TextBlock)dummyLock1.Children[fromboard];
                    int dummyfromboard = 0;
                    dummyfromboard = rotorarray[3].mapto(fromboard, off);
                    fromboard = dummyfromboard; 
                    
                }
                dummyLock1 = (StackPanel)rotorlocks3.Children[0];
                textBlocksToAnimate[0] = (TextBlock)dummyLock1.Children[fromboard];
                ein = rotorarray[2].mapto(fromboard,off);
                dummyLock1 = (StackPanel)rotorlocks2.Children[0];
                textBlocksToAnimate[1] = (TextBlock)dummyLock1.Children[ein];
                //textBlocksToAnimate[1].Background = Brushes.Green;

                int ein1 = 0;
                ein1 = rotorarray[1].mapto(ein,off);
                dummyLock1 = (StackPanel)rotorlocks1.Children[0];
                textBlocksToAnimate[2] = (TextBlock)dummyLock1.Children[ein1];
                //textBlocksToAnimate[2].Background = Brushes.Green;

                int ein2 = 0;
                ein2 = rotorarray[0].mapto(ein1,off);
                int ein3 = walze.umkehrlist0(ein2,off);

                int ein4 = rotorarray[0].maptoreverse(ein3,off);
                dummyLock1 = (StackPanel)rotorlocks1.Children[0];
                textBlocksToAnimate[3] = (TextBlock)dummyLock1.Children[ein4];
                //textBlocksToAnimate[3].Background = Brushes.Red;


                int ein5 = rotorarray[1].maptoreverse(ein4, off);

                dummyLock1 = (StackPanel)rotorlocks2.Children[0];
                textBlocksToAnimate[4] = (TextBlock)dummyLock1.Children[ein5];
                //textBlocksToAnimate[4].Background = Brushes.Red;


                int ein6 = rotorarray[2].maptoreverse(ein5, off);

                dummyLock1 = (StackPanel)rotorlocks3.Children[0];
                textBlocksToAnimate[5] = (TextBlock)dummyLock1.Children[ein6];
                //textBlocksToAnimate[5].Background = Brushes.Red;

                if (mfouron)
                {
                    int dummyein6 = 0;
                    dummyein6 = rotorarray[3].maptoreverse(ein6, off);
                    ein6 = dummyein6 ;
                    dummyLock1 = (StackPanel)rotorlocks4.Children[0];
                    textBlocksToAnimate[7] = (TextBlock)dummyLock1.Children[ein6];
                }


                if (ein6 > fromboardhelp)
                {
                    Line l41 = new Line();
                    l41.Stroke = Brushes.Red;
                    l41.X2 = 950 +m4;
                    l41.Y1 = 74 + ein6 * 29.5;
                    l41.X1 = 970 + m4;
                    l41.Y2 = 74 + ein6 * 29.5;
                    mainmainmain.Children.Add(l41);
                    linesToAnimate2.Add(l41);
                    drawLines[17] = l41;

                    Line l31 = new Line();
                    l31.Stroke = Brushes.Red;
                    l31.X2 = 970 + m4;
                    l31.Y2 = 74 + ein6 * 29.5;
                    l31.X1 = 970 + m4;
                    l31.Y1 = 821.5;
                    mainmainmain.Children.Add(l31);
                    linesToAnimate2.Add(l31);
                    drawLines[18] = l31;

                    Line l21 = new Line();
                    l21.Stroke = Brushes.Red;
                    l21.X2 = 970 + m4;
                    l21.Y1 = 821.5;
                    l21.X1 = 1300;
                    l21.Y2 = 821.5;
                    mainmainmain.Children.Add(l21);
                    linesToAnimate2.Add(l21);
                    drawLines[19] = l21;

                    Line l11 = new Line();
                    l11.Stroke = Brushes.Red;
                    l11.X2 = 0;
                    l11.Y1 = 40.5;
                    l11.X1 = ein6 * 30 + 15;
                    l11.Y2 = 40.5;
                    maingrid2.Children.Add(l11);
                    linesToAnimate2.Add(l11);
                    drawLines[20] = l11;

                    Line ln1 = new Line();
                    ln1.Stroke = Brushes.Red;
                    ln1.X2 = ein6 * 30 + 15;
                    ln1.Y2 = 40.5;
                    ln1.X1 = ein6 * 30 + 15;
                    ln1.Y1 = -5;
                    maingrid2.Children.Add(ln1);
                    linesToAnimate2.Add(ln1);
                    drawLines[21] = ln1;
                }
                else
                {
                    Line l41 = new Line();
                    l41.Stroke = Brushes.Red;
                    l41.X1 = 1050 + m4;
                    l41.Y1 = 74 + ein6 * 29.5;
                    l41.X2 = 950 + m4;
                    l41.Y2 = 74 + ein6 * 29.5;
                    mainmainmain.Children.Add(l41);
                    linesToAnimate2.Add(l41);
                    drawLines[17] = l41;

                    Line l31 = new Line();
                    l31.Stroke = Brushes.Red;
                    l31.X2 = 1050 + m4;
                    l31.Y2 = 74 + ein6 * 29.5;
                    l31.X1 = 1050 + m4;
                    l31.Y1 = 800.5;
                    mainmainmain.Children.Add(l31);
                    linesToAnimate2.Add(l31);
                    drawLines[18] = l31;

                    Line l21 = new Line();
                    l21.Stroke = Brushes.Red;
                    l21.X2 = 1050 + m4;
                    l21.Y1 = 800.5;
                    l21.X1 = 1300;
                    l21.Y2 = 800.5;
                    mainmainmain.Children.Add(l21);
                    linesToAnimate2.Add(l21);
                    drawLines[19] = l21;

                    Line l11 = new Line();
                    l11.Stroke = Brushes.Red;
                    l11.X2 = 0;
                    l11.Y1 = 19.5;
                    l11.X1 = ein6 * 30 + 15;
                    l11.Y2 = 19.5;
                    maingrid2.Children.Add(l11);
                    linesToAnimate2.Add(l11);
                    drawLines[20] = l11;

                    Line ln1 = new Line();
                    ln1.Stroke = Brushes.Red;
                    ln1.X1 = ein6 * 30 + 15;
                    ln1.Y2 = 19.5;
                    ln1.X2 = ein6 * 30 + 15;
                    ln1.Y1 = -5;
                    maingrid2.Children.Add(ln1);
                    linesToAnimate2.Add(ln1);
                    drawLines[21] = ln1;
                }
                return ein6;
            }
            else 
            {
                return 5;
            }
        }
        int whostoanimate = 1;
        

        private void animatelines(int counter) 
        {
            if (!stop)
            {
                if (counter < linesToAnimate.Count)
                {
                    if (linesToAnimate[counter] is Line)
                        animateThisLine((Line)linesToAnimate[counter]);
                    if (linesToAnimate[counter] is TextBlock)
                        animateThisTebo2((TextBlock)linesToAnimate[counter], true);
                    if (linesToAnimate[counter] is Button)
                        animateThisTebo2((Button)linesToAnimate[counter], true);

                    //Debug.Text += "i";
                }
                else
                {
                    if (!mfouron)
                    {
                        animateThisTebo(textBlocksToAnimate[0], true);
                        rotorarray[2].startAnimation();
                    }
                    else
                    {
                        animateThisTebo(textBlocksToAnimate[6], true);
                        rotorarray[3].startAnimation();
                    }

                    Debug.Text += "i";

                }
            }
        }

        private void helptoanimatem4(object sender, EventArgs e)
        {
            animateThisTebo(textBlocksToAnimate[0], true);
            rotorarray[2].startAnimation();
        }

        private void helptoanimate2(object sender, EventArgs e) 
        {
            if (!stop)
            {
                animateThisTebo(textBlocksToAnimate[1], true);
                rotorarray[1].startAnimation();

                Debug.Text += "i";
            }
            
            
        }

        private void helptoanimate3(object sender, EventArgs e)
        {
            if (!stop)
            {
                animateThisTebo(textBlocksToAnimate[2], true);
                rotorarray[0].startAnimation();
                Debug.Text += "i";
            }
            
        }

        private void helptoanimate4(object sender, EventArgs e)
        {

            if (!stop)
            {
                walze.startanimation();
                Debug.Text += "i";
            }
        }

        private void helptoanimate5(object sender, EventArgs e)
        {
            if (!stop)
            {
                rotorarray[0].startAnimationReverse();
                Debug.Text += "i";
            }
        }




        private void helptoanimate6(object sender, EventArgs e)
        {
            if (!stop)
            {
                animateThisTebo(textBlocksToAnimate[3], false);
                rotorarray[1].startAnimationReverse();
                Debug.Text += "i";
            }
        }

        private void helptoanimate7(object sender, EventArgs e)
        {
            if (!stop)
            {
                animateThisTebo(textBlocksToAnimate[4], false);
                rotorarray[2].startAnimationReverse();
                whostoanimate2 = 0;
                Debug.Text += "i";
            }
        }

        private int whostoanimate2 = 0;

        private void helptoanimate8(object sender, EventArgs e)
        {
            if (!stop)
            {
                if (!mfouron)
                {
                    animateThisTebo(textBlocksToAnimate[5], false);
                    animateThisLine2((Line) linesToAnimate2[0]);
                }
                else
                {
                    animateThisTebo(textBlocksToAnimate[5], false);
                    rotorarray[3].startAnimationReverse();
                }
            }
        }
        private void helptoanimatem42(object sender, EventArgs e)
        {
            if (!stop)
            {
                animateThisTebo(textBlocksToAnimate[7], false);
                animateThisLine2((Line)linesToAnimate2[0]);
            }
        }
        

        private void animateLines2()
        {
           

            if (whostoanimate2 < linesToAnimate2.Count &&!stop) 
            {
                //animateThisLine2((Line)linesToAnimate2[whostoanimate2]);
                if (linesToAnimate2[whostoanimate2] is Line)
                    animateThisLine2((Line)linesToAnimate2[whostoanimate2]);
                if (linesToAnimate2[whostoanimate2] is TextBlock)
                    animateThisTebo2((TextBlock)linesToAnimate2[whostoanimate2], false);
                if (linesToAnimate2[whostoanimate2] is Button)
                    animateThisTebo2((Button)linesToAnimate2[whostoanimate2], false);
                if (linesToAnimate2[whostoanimate2] is Rectangle)
                    animateThisTebo2((Rectangle)linesToAnimate2[whostoanimate2], false);
            }
            
        }

        private void helptoanimate21(object sender, EventArgs e)
        {
            if (!stop)
            {
                mydouble.Completed -= helptoanimate21;
                blupp = true;
                whostoanimate2++;
                animateLines2();
            }

        }

        private void helptoanimate(object sender, EventArgs e)
        {
            if (!stop)
            {
                mydouble.Completed -= helptoanimate;
                whostoanimate++;
                animatelines(whostoanimate);
            }
                
        }
        DoubleAnimation mydouble;
        private void animateThisLine2(Line l)
        {

            if (!stop)
            {

                Line l1 = new Line();

                Canvas.SetLeft(l, Canvas.GetLeft(l));

                Canvas.SetTop(l, Canvas.GetTop(l));

                l1.StrokeThickness = 5.0;
                l1.Stroke = Brushes.Tomato;


                l1.X1 = l.X2;
                l1.X2 = l.X2;

                l1.Y1 = l.Y2;
                l1.Y2 = l.Y2;

                double abst = Math.Sqrt(Math.Pow(l.X2 - l.X1, 2) + Math.Pow(l.Y2 - l.Y1, 2));
                if (abst == 0)
                    abst = 1;

                mydouble = new DoubleAnimation();
                mydouble.From = l.X2;
                mydouble.To = l.X1;
                mydouble.Duration = new Duration(TimeSpan.FromMilliseconds((abst * speed)));

                DoubleAnimation mydouble1 = new DoubleAnimation();
                mydouble1.From = l.Y2;
                mydouble1.To = l.Y1;
                mydouble1.Duration = new Duration(TimeSpan.FromMilliseconds((abst * speed)));

                mydouble.Completed += helptoanimate21;

                foreach (Canvas c in schalterlist3)
                {
                    if (c == l.Parent)
                    {
                        c.Children.Add(l1);
                    }
                }

                foreach (Canvas c in leuchtenlist3)
                {
                    if (c == l.Parent)
                    {
                        c.Children.Add(l1);
                    }
                }

                if (mainmainmain == l.Parent)
                {
                    mainmainmain.Children.Add(l1);
                }
                if (maingrid == l.Parent)
                {
                    maingrid.Children.Add(l1);
                }
                if (maingrid2 == l.Parent)
                {
                    maingrid2.Children.Add(l1);
                }

                if (batterie == l.Parent)
                {
                    batterie.Children.Add(l1);
                }

                if (l == batterie.Children[2]&&playbool)
                {
                    Debug.Text = "tatütatatütatatatü";
                    mydouble.Completed -= helptoanimate21;
                    mydouble.Completed += nextPlease;
                }

                l1.BeginAnimation(Line.X2Property, mydouble);
                l1.BeginAnimation(Line.Y2Property, mydouble1);
                linesToThrowAway.Add(l1);
            }
        }

        
        

        private void animateThisLine(Line l) 
        {

            if (!stop)
            {

                Line l1 = new Line();

                Canvas.SetLeft(l, Canvas.GetLeft(l));

                Canvas.SetTop(l, Canvas.GetTop(l));

                l1.StrokeThickness = 5.0;
                l1.Stroke = Brushes.LawnGreen;


                l1.X1 = l.X1;
                l1.X2 = l.X1;

                l1.Y1 = l.Y1;
                l1.Y2 = l.Y1;

                mydouble = new DoubleAnimation();
                mydouble.From = l.X1;
                mydouble.To = l.X2;
                Debug.Text = "" + Math.Sqrt(Math.Pow(l.X2 - l.X1, 2) + Math.Pow(l.Y2 - l.Y1, 2));
                double abst = Math.Sqrt(Math.Pow(l.X2 - l.X1, 2) + Math.Pow(l.Y2 - l.Y1, 2));
                if (abst == 0)
                    abst = 1;
                mydouble.Duration = new Duration(TimeSpan.FromMilliseconds((abst * speed)));

                DoubleAnimation mydouble1 = new DoubleAnimation();
                mydouble1.From = l.Y1;
                mydouble1.To = l.Y2;
                mydouble1.Duration = new Duration(TimeSpan.FromMilliseconds((abst * speed)));
                mydouble.Completed += helptoanimate;

                foreach (Canvas c in schalterlist3)
                {
                    if (c == l.Parent)
                    {
                        c.Children.Add(l1);
                    }
                }

                foreach (Canvas c in leuchtenlist3)
                {
                    if (c == l.Parent)
                    {
                        c.Children.Add(l1);
                    }
                }

                if (mainmainmain == l.Parent)
                {
                    mainmainmain.Children.Add(l1);
                }
                if (maingrid == l.Parent)
                {
                    maingrid.Children.Add(l1);
                }
                if (maingrid2 == l.Parent)
                {
                    maingrid2.Children.Add(l1);
                }

                if (batterie == l.Parent)
                {
                    batterie.Children.Add(l1);
                }

                l1.BeginAnimation(Line.X2Property, mydouble);
                l1.BeginAnimation(Line.Y2Property, mydouble1);
                linesToThrowAway.Add(l1);
            }
        }

        private void animateThisTebo2(Rectangle tebo, Boolean c)
        {
            if (!stop)
            {
                SolidColorBrush brush = new SolidColorBrush();
                brush.Color = Colors.White;

                ColorAnimation colorAni = new ColorAnimation();

                colorAni.From = Colors.White;
                colorAni.To = Colors.Yellow;

                colorAni.Duration = new Duration(TimeSpan.FromMilliseconds(speed * 80));

                colorAni.Completed += helptoanimate21;
                tebo.Fill = brush;
                brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAni);
            }
        }

        private void animateThisTebo2(Button tebo, Boolean c)
        {
            if (!stop)
            {
                SolidColorBrush brush = new SolidColorBrush();
                brush.Color = Colors.Transparent;

                ColorAnimation colorAni = new ColorAnimation();

                colorAni.From = Colors.SkyBlue;
                if (tebo.Background == Brushes.LightSeaGreen)
                    colorAni.From = Colors.LightSeaGreen;
                if (c)
                    colorAni.To = Colors.YellowGreen;
                else
                    colorAni.To = Colors.Tomato;
                colorAni.Duration = new Duration(TimeSpan.FromMilliseconds(speed * 80));
                if (c)
                    colorAni.Completed += helptoanimate;
                else
                    colorAni.Completed += helptoanimate21;
                tebo.Background = brush;
                brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAni);
            }
        }

        private void animateThisTebo2(TextBlock tebo, Boolean c)
        {
            if (!stop)
            {
                SolidColorBrush brush = new SolidColorBrush();
                brush.Color = Colors.Transparent;

                ColorAnimation colorAni = new ColorAnimation();

                colorAni.From = Colors.SkyBlue;
                if (tebo.Background == Brushes.LightSeaGreen)
                    colorAni.From = Colors.LightSeaGreen;
                if (c)
                    colorAni.To = Colors.YellowGreen;
                else
                    colorAni.To = Colors.Tomato;
                colorAni.Duration = new Duration(TimeSpan.FromMilliseconds(speed * 80));
                if (c)
                    colorAni.Completed += helptoanimate;
                else
                    colorAni.Completed += helptoanimate21;

                tebo.Background = brush;
                brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAni);
            }
        }

        private void animateThisTebo(TextBlock tebo,Boolean c)
        {
            if (!stop)
            {
                SolidColorBrush brush = new SolidColorBrush();
                brush.Color = Colors.Transparent;

                ColorAnimation colorAni = new ColorAnimation();

                colorAni.From = Colors.SkyBlue;
                if (tebo.Background == Brushes.LightSeaGreen)
                    colorAni.From = Colors.LightSeaGreen;
                if (c)
                    colorAni.To = Colors.YellowGreen;
                else
                    colorAni.To = Colors.Tomato;
                colorAni.Duration = new Duration(TimeSpan.FromMilliseconds(speed * 80));

                tebo.Background = brush;
                brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAni);
            }
        }




        /*
        private int DrawLines(int fromboard)
        {
            
            Line l = new Line();
            l.Stroke = Brushes.Green;
            l.X1 = fromboard*30+15 ;
            l.Y1 = -5;
            l.X2 = fromboard * 30 + 15;
            l.Y2 = 30.5;
            maingrid2.Children.Add(l);

            drawLines[0] = l;

            Line l1 = new Line();
            l1.Stroke = Brushes.Green;
            l1.X1 = fromboard * 30 + 15;
            l1.Y1 = 30.5;
            l1.X2 = -15;
            l1.Y2 = 30.5;
            maingrid2.Children.Add(l1);

            drawLines[1] = l1;

            Line l2 = new Line();
            l2.Stroke = Brushes.Green;
            l2.X1 = 990;
            l2.Y1 = 811.5;
            l2.X2 = 1100;
            l2.Y2 = 811.5;
            mainmainmain.Children.Add(l2);

            drawLines[2] = l2;

            Line l3 = new Line();
            l3.Stroke = Brushes.Green;
            l3.X1 = 990 ;
            l3.Y1 = 74 + rotorarray[2].maparray[fromboard, 0] * 29.5;
            l3.X2 = 990;
            l3.Y2 = 811.5;
            mainmainmain.Children.Add(l3);

            drawLines[3] = l3;

            Line l4 = new Line();
            l4.Stroke = Brushes.Green;
            l4.X1 = 990;
            l4.Y1 = 74 + rotorarray[2].maparray[fromboard, 0] * 29.5;
            l4.X2 = 860;
            l4.Y2 = 74 + rotorarray[2].maparray[fromboard, 0] * 29.5;
            mainmainmain.Children.Add(l4);

            drawLines[4] = l4;

            rotorarray[2].coloring(Brushes.LawnGreen,fromboard);

            Line l5 = new Line();
            l5.Stroke = Brushes.Green;
            l5.X1 = 800;
            l5.Y1 = 74 + rotorarray[2].maparray[fromboard, 0] * 29.5;
            l5.X2 = 710;
            l5.Y2 = 74 + rotorarray[2].maparray[fromboard, 0] * 29.5;
            mainmainmain.Children.Add(l5);

            drawLines[5] = l5;

            int ein = 0;

            Line l6 = new Line();
            l6.Stroke = Brushes.Green;
            l6.X1 = 710;
            l6.Y1 = 74 + rotorarray[2].maparray[fromboard, 0] * 29.5;
            l6.X2 = 710;
            l6.Y2 = 74 + rotorarray[2].maparray[fromboard, 1] * 29.5;
            ein =rotorarray[2].maparray[fromboard, 1];
            
            mainmainmain.Children.Add(l6);

            drawLines[6] = l6;

            Line l7 = new Line();
            l7.Stroke = Brushes.Green;
            l7.X1 = 710;
            l7.Y1 = 74 + rotorarray[2].maparray[fromboard, 1] * 29.5;
            l7.X2 = 660;
            l7.Y2 = 74 + rotorarray[2].maparray[fromboard, 1] * 29.5;
            mainmainmain.Children.Add(l7);

            drawLines[7] = l7;
            rotorarray[1].coloring(Brushes.LawnGreen, rotorarray[2].maparray[fromboard, 1]);

            Line l51 = new Line();
            l51.Stroke = Brushes.Green;
            l51.X1 = 600;
            l51.Y1 = 74 + rotorarray[2].maparray[fromboard, 1] * 29.5;
            l51.X2 = 500;
            l51.Y2 = 74 + rotorarray[2].maparray[fromboard, 1] * 29.5;
            mainmainmain.Children.Add(l51);

            drawLines[8] = l51;

            Line l61 = new Line();
            l61.Stroke = Brushes.Green;
            l61.X1 = 500;
            l61.Y1 = 74 + rotorarray[1].maparray[ein, 0] * 29.5;
            l61.X2 = 500;
            l61.Y2 = 74 + rotorarray[1].maparray[ein, 1] * 29.5;
            Debug.Text = rotorarray[1].maparray[ein, 1] + "";
            mainmainmain.Children.Add(l61);

            drawLines[9] = l61;

            Line l71 = new Line();
            l71.Stroke = Brushes.Green;
            l71.X1 = 500;
            l71.Y1 = 74 + rotorarray[1].maparray[ein, 1] * 29.5;
            l71.X2 = 460;
            l71.Y2 = 74 + rotorarray[1].maparray[ein, 1] * 29.5;
            mainmainmain.Children.Add(l71);

            drawLines[10] = l71;

            rotorarray[0].coloring(Brushes.LawnGreen, rotorarray[1].maparray[ein, 1]);

            int ein6 = rotorarray[1].maparray[ein, 1];

            Line l512 = new Line();
            l512.Stroke = Brushes.Green;
            l512.X1 = 400;
            l512.Y1 = 74 + rotorarray[1].maparray[ein, 1] * 29.5;
            l512.X2 = 310;
            l512.Y2 = 74 + rotorarray[1].maparray[ein, 1] * 29.5;
            mainmainmain.Children.Add(l512);

            drawLines[8] = l51;

            Line l612 = new Line();
            l612.Stroke = Brushes.Green;
            l612.X1 = 310;
            l612.Y1 = 74 + rotorarray[1].maparray[ein, 1] * 29.5;
            l612.X2 = 310;
            l612.Y2 = 74 + rotorarray[0].maparray[ein6, 1] * 29.5;
            Debug.Text = rotorarray[1].maparray[ein, 1] + "";
            mainmainmain.Children.Add(l612);

            drawLines[9] = l61;

            Line l712 = new Line();
            l712.Stroke = Brushes.Green;
            l712.X1 = 310;
            l712.Y1 = 74 + rotorarray[0].maparray[ein6, 1] * 29.5;
            l712.X2 = 260;
            l712.Y2 = 74 + rotorarray[0].maparray[ein6, 1] * 29.5;
            mainmainmain.Children.Add(l712);

            drawLines[10] = l71;

            int ein7 = walze.umkehrlist[rotorarray[0].maparray[ein6, 1]];

            Line l513 = new Line();
            l513.Stroke = Brushes.Red;
            l513.X1 = 260;
            l513.Y1 = 74 + rotorarray[0].maparray[ein7, 0] * 29.5;
            l513.X2 = 350;
            l513.Y2 = 74 + rotorarray[0].maparray[ein7, 0] * 29.5;
            mainmainmain.Children.Add(l513);

            drawLines[8] = l51;

            Line l613 = new Line();
            l613.Stroke = Brushes.Red;
            l613.X1 = 350;
            l613.Y1 = 74 + rotorarray[0].maparray[ein7, 0] * 29.5;
            l613.X2 = 350;
            l613.Y2 = 74 + rotorarray[0].maptoreverse(ein7) * 29.5;
            Debug.Text = rotorarray[1].maparray[ein, 1] + "";
            mainmainmain.Children.Add(l613);

            drawLines[9] = l61;

            Line l713 = new Line();
            l713.Stroke = Brushes.Red;
            l713.X1 = 350;
            l713.Y1 = 74 + rotorarray[0].maptoreverse(ein7) * 29.5;
            l713.X2 = 400;
            l713.Y2 = 74 + rotorarray[0].maptoreverse(ein7) * 29.5;
            mainmainmain.Children.Add(l713);

            drawLines[10] = l71;

            int ein2 = rotorarray[0].maptoreverse(ein7);

            rotorarray[0].coloring(Brushes.Red, ein2);

            Debug.Text = ein2+"test";

            Line l52 = new Line();
            l52.Stroke = Brushes.Red;
            l52.X1 = 460;
            l52.Y1 = 74 + rotorarray[0].maparray[ein2, 0] * 29.5;
            l52.X2 = 550;
            l52.Y2 = 74 + rotorarray[0].maparray[ein2, 0] * 29.5;
            mainmainmain.Children.Add(l52);

            drawLines[11] = l52;

            Line l62 = new Line();
            l62.Stroke = Brushes.Red;
            l62.X1 = 550;
            l62.Y1 = 74 + rotorarray[1].maparray[ein2, 0] * 29.5;
            l62.X2 = 550;
            int ein3=rotorarray[1].maptoreverse(ein2);
            

            l62.Y2 = 74 + rotorarray[1].maparray[ein3, 0] * 29.5;

            mainmainmain.Children.Add(l62);
            drawLines[12] = l62;


            Line l72 = new Line();
            l72.Stroke = Brushes.Red;
            l72.X1 = 550;
            l72.Y1 = 74 + rotorarray[1].maparray[ein3, 0] * 29.5;
            l72.X2 = 600;
            l72.Y2 = 74 + rotorarray[1].maparray[ein3, 0] * 29.5;
            mainmainmain.Children.Add(l72);
            drawLines[13] = l72;

            rotorarray[1].coloring(Brushes.Red, ein3);

            Line l53 = new Line();
            l53.Stroke = Brushes.Red;
            l53.X1 = 660;
            l53.Y1 = 74 + rotorarray[0].maparray[ein3, 0] * 29.5;
            l53.X2 = 740;
            l53.Y2 = 74 + rotorarray[0].maparray[ein3, 0] * 29.5;
            mainmainmain.Children.Add(l53);
            drawLines[14] = l53;

            int ein4 = rotorarray[2].maptoreverse(ein3);

            Line l63 = new Line();
            l63.Stroke = Brushes.Red;
            l63.X1 = 740;
            l63.Y1 = 74 + rotorarray[0].maparray[ein3, 0] * 29.5;
            l63.X2 = 740;
            l63.Y2 = 74 + rotorarray[1].maparray[ein4, 0] * 29.5;
            
            

            mainmainmain.Children.Add(l63);
            drawLines[15] = l63;

            Line l73 = new Line();
            l73.Stroke = Brushes.Red;
            l73.X1 = 740;
            l73.Y1 = 74 + rotorarray[1].maparray[ein4, 0] * 29.5;
            l73.X2 = 800;
            l73.Y2 = 74 + rotorarray[1].maparray[ein4, 0] * 29.5;
            mainmainmain.Children.Add(l73);
            drawLines[16] = l73;

            int ein5 = rotorarray[2].maptoreverse(ein4);
            rotorarray[2].coloring(Brushes.Red, ein4);


            Line l41 = new Line();
            l41.Stroke = Brushes.Red;
            l41.X1 = 940;
            l41.Y1 = 74 + rotorarray[2].maparray[ein5, 1] * 29.5;
            l41.X2 = 860;
            l41.Y2 = 74 + rotorarray[2].maparray[ein5, 1] * 29.5;
            mainmainmain.Children.Add(l41);

            drawLines[17] = l41;

            Line l31 = new Line();
            l31.Stroke = Brushes.Red;
            l31.X1 = 940;
            l31.Y1 = 74 + rotorarray[2].maparray[ein5, 1] * 29.5;
            l31.X2 = 940;
            l31.Y2 = 821.5;
            mainmainmain.Children.Add(l31);

            drawLines[18] = l31;

            Line l21 = new Line();
            l21.Stroke = Brushes.Red;
            l21.X1 = 940;
            l21.Y1 = 821.5;
            l21.X2 = 1100;
            l21.Y2 = 821.5;
            mainmainmain.Children.Add(l21);

            drawLines[19] = l21;

            Line l11 = new Line();
            l11.Stroke = Brushes.Red;
            l11.X1 = rotorarray[2].maparray[ein5, 1] * 30 + 15;
            l11.Y1 = 40.5;
            l11.X2 = -15;
            l11.Y2 = 40.5;
            maingrid2.Children.Add(l11);

            drawLines[20] = l11;

            Line ln1 = new Line();
            ln1.Stroke = Brushes.Red;
            ln1.X1 = rotorarray[2].maparray[ein5, 1] * 30 + 15;
            ln1.Y1 = -5;
            ln1.X2 = rotorarray[2].maparray[ein5, 1] * 30 + 15;
            ln1.Y2 = 40.5;
            maingrid2.Children.Add(ln1);

            drawLines[21] = ln1;

            return rotorarray[2].maparray[ein5, 1];
        }
        */

        private void List_MouseLeftButtonDown(object sender, EventArgs e)
        {
           // Debug.Text = "test";
            everythingblack();
            if (merken == -1)
            {
                Button button = sender as Button;
                merken = Int32.Parse(button.Uid);
                if (!button.Uid.Equals(button.Content.ToString()))
                {                   switchbuttons(Int32.Parse(button.Content.ToString()), Int32.Parse(button.Uid));}
               
                button.Background = Brushes.LawnGreen;
            }

            else 
            {
                
                Button button = sender as Button;
                bList[merken].Background = Brushes.LightBlue;
                if ((button.Content.ToString().Equals(button.Uid) || Int32.Parse(button.Content.ToString()).Equals(merken)))
                { switchbuttons(Int32.Parse(button.Uid), merken); Debug.Text = "test1"; }

                else 
                {
                    Debug.Text = "test1";
                    switchbuttons(Int32.Parse(button.Content.ToString()), merken);
                    switchbuttons(Int32.Parse(button.Uid), Int32.Parse(button.Content.ToString()));
                }
                //switchbuttons(Int32.Parse(button.Uid), merken);
                merken =-1;
            }
        }

        

        private void List_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;
            Button button = sender as Button;
            Button button2 = new Button();
            button2.Width = button.Width;
            button2.Height = button.Height;
            button2.Opacity = 0.0;
            

            if (e.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(diff.X)+4 > SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y)+4 > SystemParameters.MinimumVerticalDragDistance&& button.IsPressed)
            {
                // Get the dragged ListViewItem
            
                everythingblack();
                //lList[Int32.Parse(button.Uid)].X2 = mousePos.X;
                // Find the data behind the ListViewItem

                // Let's define our DragScope .. In this case it is every thing inside our main window .. 
                DragScope = Application.Current.MainWindow.Content as FrameworkElement;
                System.Diagnostics.Debug.Assert(DragScope != null);

                // We enable Drag & Drop in our scope ...  We are not implementing Drop, so it is OK, but this allows us to get DragOver 
                bool previousDrop = DragScope.AllowDrop;
                //DragScope.AllowDrop = true;
                maingrid.AllowDrop = true;
                

                // Let's wire our usual events.. 
                // GiveFeedback just tells it to use no standard cursors..  

                //GiveFeedbackEventHandler feedbackhandler = new GiveFeedbackEventHandler(DragSource_GiveFeedback);
                //this.DragSource.GiveFeedback += feedbackhandler;

                // The DragOver event ... 
                DragEventHandler draghandler = new DragEventHandler(Window1_DragOver);
                DragScope.PreviewDragOver += draghandler;
                DragScope.PreviewMouseLeftButtonUp += aktuellupdate;

                // Drag Leave is optional, but write up explains why I like it .. 
                //DragEventHandler dragleavehandler = new DragEventHandler(DragScope_DragLeave);
                //DragScope.DragLeave += dragleavehandler;

                // QueryContinue Drag goes with drag leave... 
                //QueryContinueDragEventHandler queryhandler = new QueryContinueDragEventHandler(DragScope_QueryContinueDrag);
                //DragScope.QueryContinueDrag += queryhandler;
                steckerbrett.Children.Remove(button);
                steckerbrett.Children.Insert(Int32.Parse(button.Uid), button2);
                
                //Here we create our adorner.. 
                _adorner = new DragAdorner(DragScope, (UIElement)button, true, 1, this.ActualWidth, this.ActualHeight);
                
                _layer = AdornerLayer.GetAdornerLayer(DragScope as Visual);
                _layer.Add(_adorner);
                

                IsDragging = true;
                //_dragHasLeftScope = false;
                //Finally lets drag drop 
                if (!button.Uid.Equals(button.Content.ToString()))
                    switchbuttons(Int32.Parse(button.Content.ToString()), Int32.Parse(button.Uid));
                aktuell = Int32.Parse(button.Content.ToString());
                
                DataObject data = new DataObject("myFormat", button.Uid);
                DragDropEffects de = DragDrop.DoDragDrop(maingrid, data, DragDropEffects.Move);
                maingrid.AllowDrop = false;
                steckerbrett.Children.Remove(button2);
                steckerbrett.Children.Insert(Int32.Parse(button.Uid), button);
                // Clean up our mess :) 
                //DragScope.AllowDrop = previousDrop;
                if (_adorner != null)
                    AdornerLayer.GetAdornerLayer(DragScope).Remove(_adorner);
                
                _adorner = null;
                lList[Int32.Parse(button.Content.ToString())].X2=15+Int32.Parse(button.Content.ToString()) * 30;
                lList[Int32.Parse(button.Content.ToString())].Y2 = 200;
                //           DragSource.GiveFeedback -= feedbackhandler;
                //         DragScope.DragLeave -= dragleavehandler;
                //       DragScope.QueryContinueDrag -= queryhandler;
                DragScope.PreviewDragOver -= draghandler;

                //IsDragging = false;
                

                // Initialize the drag & drop operation
                //DataObject dragData = new DataObject("myFormat", button.Uid);
            
                //DragDrop.DoDragDrop(button, dragData , DragDropEffects.Move);
            }
        }

        private void Rotor_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;
            Rotor2 rotor = sender as Rotor2;
            

            if (e.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(diff.X)  > SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y)  > SystemParameters.MinimumVerticalDragDistance )
            {
                // Get the dragged ListViewItem
                
                everythingblack();
                //lList[Int32.Parse(button.Uid)].X2 = mousePos.X;
                // Find the data behind the ListViewItem

                // Let's define our DragScope .. In this case it is every thing inside our main window .. 
                //DragScope = dropBox as FrameworkElement;
                DragScope = Application.Current.MainWindow.Content as FrameworkElement;
                System.Diagnostics.Debug.Assert(DragScope != null);

                // We enable Drag & Drop in our scope ...  We are not implementing Drop, so it is OK, but this allows us to get DragOver 
                bool previousDrop = DragScope.AllowDrop;
                //DragScope.AllowDrop = true;

                // Let's wire our usual events.. 
                // GiveFeedback just tells it to use no standard cursors..  

                //GiveFeedbackEventHandler feedbackhandler = new GiveFeedbackEventHandler(DragSource_GiveFeedback);
                //this.DragSource.GiveFeedback += feedbackhandler;

                // The DragOver event ... 
                DragEventHandler draghandler = new DragEventHandler(Window1_DragOver2);
                DragScope.PreviewDragOver += draghandler;
                DragScope.PreviewMouseLeftButtonUp += aktuellupdate;

                // Drag Leave is optional, but write up explains why I like it .. 
                //DragEventHandler dragleavehandler = new DragEventHandler(DragScope_DragLeave);
                //DragScope.DragLeave += dragleavehandler;

                // QueryContinue Drag goes with drag leave... 
                //QueryContinueDragEventHandler queryhandler = new QueryContinueDragEventHandler(DragScope_QueryContinueDrag);
                //DragScope.QueryContinueDrag += queryhandler;
                rotorarea.Children.Remove(rotor);
                Rectangle dummy = new Rectangle();
                
                dummy.Width = 200;
                dummy.Height = 900;
                dummy.Opacity = 0.0;
                dummy.Stroke=Brushes.Green;
                dummy.StrokeThickness = 5; 
                dummy.Fill=Brushes.LawnGreen;
                //dummy.AllowDrop = true;
                rotorarea.AllowDrop = true;
                dropBoxCanvas.AllowDrop = true;

                Canvas.SetLeft(dummy,Canvas.GetLeft(rotor));
                int helpint = 5;
                rotor.PreviewMouseLeftButtonDown -= List_PreviewMouseLeftButtonDown;
                rotorarea.Children.Add(dummy);

                if (rotorarray[0] == rotor)
                {
                    dummy.Drop += List_Drop30;
                    rotor.helpNextAnimation -=  helptoanimate4;
                    rotor.helpNextAnimation2 -= helptoanimate6;
                    dummyrec[0] = dummy;
                    rotorarray[0] = null;
                    helpint = 0;
                }

             
                if (rotorarray[1] == rotor)
                    {
                        dummy.Drop += List_Drop31;
                        rotor.helpNextAnimation -= helptoanimate3;
                        dummyrec[1] = dummy;
                         rotor.helpNextAnimation2 -= helptoanimate7;
                         rotorarray[1] = null;
                         helpint = 1;
                    }

                if (rotorarray[2] == rotor)
                        {
                            dummy.Drop += List_Drop32;
                            rotor.helpNextAnimation2 -= helptoanimate8;
                            rotor.helpNextAnimation -= helptoanimate2;
                            dummyrec[2] = dummy;
                            rotorarray[2] = null;
                            helpint = 2;
                        }

                if (rotorarray[3] == rotor)
                {
                    dummy.Drop += List_Drop33;
                    rotor.helpNextAnimation2 -= helptoanimate8;
                    rotor.helpNextAnimation -= helptoanimate2;
                    dummyrec[3] = dummy; 
                    rotorarray[3] = null;
                    helpint = 3;
                }

                //steckerbrett.Children.Insert(Int32.Parse(button.Uid), button2);

                //Here we create our adorner.. 
                _adorner = new DragAdorner(DragScope, (UIElement)rotor.iAm, true, 1, this.ActualWidth, this.ActualHeight);

                _layer = AdornerLayer.GetAdornerLayer(DragScope as Visual);
                _layer.Add(_adorner);

                suc = false;
                IsDragging = true;
                
                //_dragHasLeftScope = false;
                //Finally lets drag drop 
                //if (!button.Uid.Equals(button.Content.ToString()))
                  //  switchbuttons(Int32.Parse(button.Content.ToString()), Int32.Parse(button.Uid));

                
                DataObject data = new DataObject("myFormat", rotor.returnMap()+"");
                DragDropEffects de = DragDrop.DoDragDrop(rotorarea, data, DragDropEffects.Move);
                Debug.Text += "k";



                if (!suc) 
                {
                    if (0 == helpint)
                    {
                        dummy.Drop -= List_Drop30;
                        dummyrec[0] = null;
                        rotorarray[0] = rotor;
                    }
                    if (1 == helpint)
                    {
                        dummy.Drop -= List_Drop31;
                        dummyrec[1] = null;
                        rotorarray[1] = rotor;
                    }

                    if (2 == helpint)
                    {
                        dummy.Drop -= List_Drop32;
                        dummyrec[2] = null;
                        rotorarray[2] = rotor;
                    }

                    rotorarea.Children.Remove(dummy);
                    rotorarea.Children.Add(rotor);
                    dropBoxCanvas.AllowDrop = false;
                }

                rotorarea.AllowDrop = false;

                // Clean up our mess :) 
                //DragScope.AllowDrop = previousDrop;
                if (_adorner != null)
                    AdornerLayer.GetAdornerLayer(DragScope).Remove(_adorner);
                
                _adorner = null;
               
                //           DragSource.GiveFeedback -= feedbackhandler;
                //         DragScope.DragLeave -= dragleavehandler;
                //       DragScope.QueryContinueDrag -= queryhandler;
                DragScope.PreviewDragOver -= draghandler;

                //IsDragging = false;


                // Initialize the drag & drop operation
                //DataObject dragData = new DataObject("myFormat", button.Uid);

                //DragDrop.DoDragDrop(button, dragData , DragDropEffects.Move);
                 
            }
        }

        private void Walze_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;
            Walze rotor = sender as Walze;


            if (e.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                // Get the dragged ListViewItem

                everythingblack();
                //lList[Int32.Parse(button.Uid)].X2 = mousePos.X;
                // Find the data behind the ListViewItem

                // Let's define our DragScope .. In this case it is every thing inside our main window .. 
                DragScope = mainCanvas as FrameworkElement;
                System.Diagnostics.Debug.Assert(DragScope != null);

                // We enable Drag & Drop in our scope ...  We are not implementing Drop, so it is OK, but this allows us to get DragOver 
                bool previousDrop = DragScope.AllowDrop;
                //DragScope.AllowDrop = true;

                // Let's wire our usual events.. 
                // GiveFeedback just tells it to use no standard cursors..  

                //GiveFeedbackEventHandler feedbackhandler = new GiveFeedbackEventHandler(DragSource_GiveFeedback);
                //this.DragSource.GiveFeedback += feedbackhandler;

                // The DragOver event ... 
                DragEventHandler draghandler = new DragEventHandler(Window1_DragOver2);
                DragScope.PreviewDragOver += draghandler;
                DragScope.PreviewMouseLeftButtonUp += aktuellupdate;

                // Drag Leave is optional, but write up explains why I like it .. 
                //DragEventHandler dragleavehandler = new DragEventHandler(DragScope_DragLeave);
                //DragScope.DragLeave += dragleavehandler;

                // QueryContinue Drag goes with drag leave... 
                //QueryContinueDragEventHandler queryhandler = new QueryContinueDragEventHandler(DragScope_QueryContinueDrag);
                //DragScope.QueryContinueDrag += queryhandler;
                walzenarea.Children.Remove(rotor);
                Rectangle dummy = new Rectangle();

                dummy.Width = 260;
                dummy.Height = 900;
                dummy.Opacity = 0.0;
                dummy.Stroke = Brushes.Green;
                dummy.StrokeThickness = 5;
                dummy.Fill = Brushes.LawnGreen;
                //dummy.AllowDrop = true;
                walzenarea.AllowDrop = true;
                dropBoxCanvasWalze.AllowDrop = true;

                Canvas.SetLeft(dummy, Canvas.GetLeft(rotor));

                rotor.PreviewMouseLeftButtonDown -= List_PreviewMouseLeftButtonDown;
                walzenarea.Children.Add(dummy);

                dummy.Drop += List_Drop4;
                rotor.helpNextAnimation -= helptoanimate4;
                walze = null;

                dummyrec[3] = dummy;
                    
                //steckerbrett.Children.Insert(Int32.Parse(button.Uid), button2);

                //Here we create our adorner.. 
                _adorner = new DragAdorner(DragScope, (UIElement)rotor.iAm, true, 1, this.ActualWidth, this.ActualHeight);

                _layer = AdornerLayer.GetAdornerLayer(DragScope as Visual);
                _layer.Add(_adorner);

                suc = false;
                IsDragging = true;

                //_dragHasLeftScope = false;
                //Finally lets drag drop 
                //if (!button.Uid.Equals(button.Content.ToString()))
                //  switchbuttons(Int32.Parse(button.Content.ToString()), Int32.Parse(button.Uid));


                DataObject data = new DataObject("myFormat", rotor.typ + "");
                DragDropEffects de = DragDrop.DoDragDrop(rotorarea, data, DragDropEffects.Move);
                Debug.Text += "k";



                if (!suc)
                {
                    dummyrec[3] = null;
                    dummy.Drop -= List_Drop4;
                    walzenarea.Children.Remove(dummy);
                    walzenarea.Children.Add(rotor);
                    dropBoxCanvas.AllowDrop = false;
                    walze = rotor;
                }

                // Clean up our mess :) 
                //DragScope.AllowDrop = previousDrop;
                if (_adorner != null)
                    AdornerLayer.GetAdornerLayer(DragScope).Remove(_adorner);

                _adorner = null;

                //           DragSource.GiveFeedback -= feedbackhandler;
                //         DragScope.DragLeave -= dragleavehandler;
                //       DragScope.QueryContinueDrag -= queryhandler;
                DragScope.PreviewDragOver -= draghandler;

                //IsDragging = false;


                // Initialize the drag & drop operation
                //DataObject dragData = new DataObject("myFormat", button.Uid);

                //DragDrop.DoDragDrop(button, dragData , DragDropEffects.Move);

            }
        }

        private void Walze_MouseMove1(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;
            Image rotor = sender as Image;


            if (e.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(diff.X) - 4 > SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) - 4 > SystemParameters.MinimumVerticalDragDistance)
            {
                // Get the dragged ListViewItem

                everythingblack();
                //lList[Int32.Parse(button.Uid)].X2 = mousePos.X;
                // Find the data behind the ListViewItem

                // Let's define our DragScope .. In this case it is every thing inside our main window .. 
                DragScope = mainCanvas as FrameworkElement;
                System.Diagnostics.Debug.Assert(DragScope != null);

                // We enable Drag & Drop in our scope ...  We are not implementing Drop, so it is OK, but this allows us to get DragOver 
                bool previousDrop = DragScope.AllowDrop;
                walzenarea.AllowDrop = true;
                dropBoxCanvasWalze.AllowDrop = true;
                //DragScope.AllowDrop = true;

                // Let's wire our usual events.. 
                // GiveFeedback just tells it to use no standard cursors..  

                //GiveFeedbackEventHandler feedbackhandler = new GiveFeedbackEventHandler(DragSource_GiveFeedback);
                //this.DragSource.GiveFeedback += feedbackhandler;

                // The DragOver event ... 
                DragEventHandler draghandler = new DragEventHandler(Window1_DragOver2);
                DragScope.PreviewDragOver += draghandler;
                DragScope.PreviewMouseLeftButtonUp += aktuellupdate;

                // Drag Leave is optional, but write up explains why I like it .. 
                //DragEventHandler dragleavehandler = new DragEventHandler(DragScope_DragLeave);
                //DragScope.DragLeave += dragleavehandler;

                // QueryContinue Drag goes with drag leave... 
                //QueryContinueDragEventHandler queryhandler = new QueryContinueDragEventHandler(DragScope_QueryContinueDrag);
                //DragScope.QueryContinueDrag += queryhandler;
                dropBoxCanvasWalze.Children.Remove(rotor);
                
                //steckerbrett.Children.Insert(Int32.Parse(button.Uid), button2);

                //Here we create our adorner.. 
                _adorner = new DragAdorner(DragScope, (UIElement)rotor, true, 1, this.ActualWidth, this.ActualHeight);

                _layer = AdornerLayer.GetAdornerLayer(DragScope as Visual);
                _layer.Add(_adorner);


                suc = false;
                IsDragging = true;
                //_dragHasLeftScope = false;
                //Finally lets drag drop 
                //if (!button.Uid.Equals(button.Content.ToString()))
                //  switchbuttons(Int32.Parse(button.Content.ToString()), Int32.Parse(button.Uid));



                DataObject data = new DataObject("myFormat", rotor.Uid);
                DragDropEffects de = DragDrop.DoDragDrop(mainmainmain, data, DragDropEffects.Move);

                if (!suc)
                {
                    dropBoxCanvasWalze.Children.Add(rotor);
                    rotorarea.AllowDrop = false;
                    dropBoxCanvasWalze.AllowDrop = false;
                }

                // Clean up our mess :) 
                //DragScope.AllowDrop = previousDrop;
                if (_adorner != null)
                    AdornerLayer.GetAdornerLayer(DragScope).Remove(_adorner);

                _adorner = null;

                //           DragSource.GiveFeedback -= feedbackhandler;
                //         DragScope.DragLeave -= dragleavehandler;
                //       DragScope.QueryContinueDrag -= queryhandler;
                DragScope.PreviewDragOver -= draghandler;

                IsDragging = false;


                // Initialize the drag & drop operation
                //DataObject dragData = new DataObject("myFormat", button.Uid);

                //DragDrop.DoDragDrop(button, dragData , DragDropEffects.Move);

            }
        }

        private void Rotor_MouseMove1(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;
            Image rotor = sender as Image;
            

            if (e.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(diff.X) - 4 > SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) - 4 > SystemParameters.MinimumVerticalDragDistance)
            {
                // Get the dragged ListViewItem

                everythingblack();
                //lList[Int32.Parse(button.Uid)].X2 = mousePos.X;
                // Find the data behind the ListViewItem

                // Let's define our DragScope .. In this case it is every thing inside our main window .. 
                //DragScope = rotorarea as FrameworkElement;
                DragScope = mainCanvas as FrameworkElement;
                
                System.Diagnostics.Debug.Assert(DragScope != null);

                // We enable Drag & Drop in our scope ...  We are not implementing Drop, so it is OK, but this allows us to get DragOver 
                bool previousDrop = DragScope.AllowDrop;
                rotorarea.AllowDrop = true;
                dropBoxCanvas.AllowDrop = true;
                //DragScope.AllowDrop = true;
                
                // Let's wire our usual events.. 
                // GiveFeedback just tells it to use no standard cursors..  

                //GiveFeedbackEventHandler feedbackhandler = new GiveFeedbackEventHandler(DragSource_GiveFeedback);
                //this.DragSource.GiveFeedback += feedbackhandler;

                // The DragOver event ... 
                //DragEventHandler draghandler = new DragEventHandler(Window1_DragOver);
                //DragScope.PreviewDragOver += draghandler;

                DragEventHandler draghandler = new DragEventHandler(Window1_DragOver2);
                DragScope.PreviewDragOver += draghandler;
                DragScope.PreviewMouseLeftButtonUp += aktuellupdate;

                // Drag Leave is optional, but write up explains why I like it .. 
                //DragEventHandler dragleavehandler = new DragEventHandler(DragScope_DragLeave);
                //DragScope.DragLeave += dragleavehandler;

                // QueryContinue Drag goes with drag leave... 
                //QueryContinueDragEventHandler queryhandler = new QueryContinueDragEventHandler(DragScope_QueryContinueDrag);
                //DragScope.QueryContinueDrag += queryhandler;
                dropBoxCanvas.Children.Remove(rotor);
                
                //steckerbrett.Children.Insert(Int32.Parse(button.Uid), button2);

                //Here we create our adorner.. 
                _adorner = new DragAdorner(DragScope, (UIElement)rotor, true, 1, this.ActualWidth, this.ActualHeight);

                _layer = AdornerLayer.GetAdornerLayer(DragScope as Visual);
                _layer.Add(_adorner);

                //maingrid.AllowDrop = true;
               
                suc = false;
                IsDragging = true;
                //_dragHasLeftScope = false;
                //Finally lets drag drop 
                //if (!button.Uid.Equals(button.Content.ToString()))
                //  switchbuttons(Int32.Parse(button.Content.ToString()), Int32.Parse(button.Uid));



                DataObject data = new DataObject("myFormat", rotor.Uid );
                DragDropEffects de = DragDrop.DoDragDrop(mainmainmain, data, DragDropEffects.Move);

                if (!suc) 
                
                {
                    dropBoxCanvas.Children.Add(rotor);
                    rotorarea.AllowDrop = false;
                    dropBoxCanvas.AllowDrop = false;
                }

                // Clean up our mess :) 
                DragScope.AllowDrop = previousDrop;
                if (_adorner != null)
                    AdornerLayer.GetAdornerLayer(DragScope).Remove(_adorner);

                _adorner = null;

                //           DragSource.GiveFeedback -= feedbackhandler;
                //         DragScope.DragLeave -= dragleavehandler;
                //       DragScope.QueryContinueDrag -= queryhandler;
                DragScope.PreviewDragOver -= draghandler;

                //IsDragging = false;


                // Initialize the drag & drop operation
                //DataObject dragData = new DataObject("myFormat", button.Uid);

                //DragDrop.DoDragDrop(button, dragData , DragDropEffects.Move);

            }
        }

        Boolean b = true;
        Boolean suc = false;


        private void List_Drop(object sender, DragEventArgs e)
        {
            
                //Contact contact = e.Data.GetData("myFormat") as Contact;
                //ListView listView = sender as ListView;
                //listView.Items.Add(contact);

                maingrid.AllowDrop = false;

                aktuell = -1;
                Button dummy = new Button();

                String uID = e.Data.GetData("myFormat") as String;
                Button button = sender as Button;
                int myInteger1 = Int32.Parse(uID);
                int myInteger2 = Int32.Parse(button.Uid);

                Debug.Text += " " + myInteger1 + "" + myInteger2;
                Debug.Text = "hossa";
                if (b && (button.Content.ToString().Equals(button.Uid) || Int32.Parse(button.Content.ToString()).Equals(myInteger1)))
                {
                    switchbuttons(myInteger1, myInteger2);
                }

                else if (b && !button.Content.ToString().Equals(button.Uid))
                {
                    switchbuttons(Int32.Parse(button.Content.ToString()), myInteger2);
                    switchbuttons(myInteger1, myInteger2);
                }


                else
                    b = true;
            

        }
        
        private void List_Drop2(object sender, DragEventArgs e)
        {
                suc = true;   
                String uID = e.Data.GetData("myFormat") as String;
                Debug.Text = "hello" + uID;

                dropBoxCanvas.AllowDrop = false;

                int urint = Int32.Parse(uID);
            /*
                TextBlock tebo = new TextBlock();
                tebo.Background = Brushes.LightSkyBlue;
                tebo.Height = 100;
                tebo.Width = 50;
                tebo.TextAlignment = TextAlignment.Center;
                tebo.FontSize = 50;
                switch (urint)
                {
                    case 1: tebo.Uid = "1";
                        tebo.Text = "I";
                        break;
                    case 2: tebo.Uid = "2";
                        tebo.Text = "II";
                        break;
                    case 3: tebo.Uid = "3";
                        tebo.Text = "III";
                        break;
                    case 4: tebo.Uid = "4";
                        tebo.Text = "IV";
                        break;
                    case 5: tebo.Uid = "5";
                        tebo.Text = "V";
                        break;

                }
                Canvas.SetLeft(tebo, 50 * urint);
                dropBoxCanvas.Children.Add(tebo);
                tebo.Cursor = Cursors.Hand;
                tebo.PreviewMouseMove += Rotor_MouseMove1;*/

                Image img = new Image();
                img.Height = 100;
                img.Width = 50;
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                String path = "";
                switch (urint)
                {
                    case 1: img.Uid = "1";
                        path = "Images/rotor"+ urint +".jpg";
                        rotorimgs[urint-1] = img;
                        break;
                    case 2: img.Uid = "2";
                        path = "Images/rotor" + urint + ".jpg";
                        rotorimgs[urint-1] = img;
                        break;
                    case 3: img.Uid = "3";
                        path = "Images/rotor" + urint + ".jpg";
                        rotorimgs[urint-1] = img;
                        break;
                    case 4: img.Uid = "4";
                        path = "Images/rotor" + urint + ".jpg";
                        rotorimgs[urint-1] = img;
                        break;
                    case 5: img.Uid = "5";
                        path = "Images/rotor" + urint + ".jpg";
                        rotorimgs[urint-1] = img;
                        break;

                }

                bi.UriSource = new Uri(path, UriKind.Relative);
                bi.EndInit();
                img.Source = bi;
                dropBoxCanvas.Children.Add(img);
                Canvas.SetLeft(img, 50 * urint);
                
                img.Cursor = Cursors.Hand;
                img.PreviewMouseMove += Rotor_MouseMove1;
                
        }

        private void List_Drop21(object sender, DragEventArgs e)
        {
            suc = true;
            String uID = e.Data.GetData("myFormat") as String;
            Debug.Text = "hello" + uID;

            dropBoxCanvasWalze.AllowDrop = false;
            walzenarea.AllowDrop = false;
            
            int urint = Int32.Parse(uID);
            /*
            TextBlock tebo = new TextBlock();
            tebo.Background = Brushes.LightSkyBlue;
            tebo.Height = 100;
            tebo.Width = 50;
            tebo.TextAlignment = TextAlignment.Center;
            tebo.FontSize = 50;
            

            switch(urint)
            {
                case 1: tebo.Uid = "1";
                    tebo.Text = "A";
                    break;
                case 2: tebo.Uid = "2";
                    tebo.Text = "B";
                    break;
                case 3: tebo.Uid = "3";
                    tebo.Text = "C";
                    break;
                
                
            }
            Canvas.SetLeft(tebo, 50 * urint);
            dropBoxCanvasWalze.Children.Add(tebo);
            tebo.Cursor = Cursors.Hand;
            tebo.PreviewMouseMove += Walze_MouseMove1;
            */
            Image img = new Image();
                img.Height = 100;
                img.Width = 50;
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                String path = "";
                switch (urint)
                {
                    case 1: img.Uid = "1";
                        path = "Images/rotor" + urint + ".jpg";
                        walzeimgs[urint] = img;        
                        break;
                    case 2: img.Uid = "2";
                        path = "Images/rotor" + urint + ".jpg";
                        walzeimgs[urint] = img;
                        break;
                    case 3: img.Uid = "3";
                        path = "Images/rotor" + urint + ".jpg";
                        walzeimgs[urint] = img;
                        break;
                }
                
                bi.UriSource = new Uri(path, UriKind.Relative);
                bi.EndInit();
                img.Source = bi;
                Canvas.SetLeft(img, 50 * urint);
                dropBoxCanvasWalze.Children.Add(img);
                img.Cursor = Cursors.Hand;
                img.PreviewMouseMove += Walze_MouseMove1;
        }


            
        private void List_Drop31(object sender, DragEventArgs e)
        {
            suc = true;   
            rotorarea.AllowDrop = false;

            dropBoxCanvas.AllowDrop = false;
            String uID = e.Data.GetData("myFormat") as String;
            Debug.Text = "hello" + uID;

            int urint = Int32.Parse(uID);
            Rotor2 rotor2 = new Rotor2(urint, this.ActualWidth, this.ActualHeight);
            Canvas.SetLeft(rotor2, 230);
            rotorarea.Children.Add(rotor2);
            rotor2.PreviewMouseMove += new MouseEventHandler(Rotor_MouseMove);
            rotorarray[1] = rotor2;
            rotor2.helpNextAnimation += helptoanimate3;
            rotor2.helpNextAnimation2 += helptoanimate7;
            rotorarea.Children.Remove(dummyrec[1]);
            rotor2.fast = speed * 80;
            rotor2.Cursor = Cursors.Hand;
            rotor2.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
        }

        private void List_Drop32(object sender, DragEventArgs e)
        {
            suc = true;   
            rotorarea.AllowDrop = false;
            dropBoxCanvas.AllowDrop = false;
            String uID = e.Data.GetData("myFormat") as String;
            Debug.Text = "hello" + uID;
     
            int urint = Int32.Parse(uID);
            Rotor2 rotor2 = new Rotor2(urint, this.ActualWidth, this.ActualHeight);
            Canvas.SetLeft(rotor2, 460);
            rotorarea.Children.Add(rotor2);
            rotor2.PreviewMouseMove += new MouseEventHandler(Rotor_MouseMove);
            rotorarray[2] = rotor2;
            rotor2.helpNextAnimation += helptoanimate2;
            rotor2.helpNextAnimation2 += helptoanimate8;
            rotorarea.Children.Remove(dummyrec[2]);
            rotor2.fast = speed * 80;
            rotor2.Cursor = Cursors.Hand;
            rotor2.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
        }

        private void List_Drop33(object sender, DragEventArgs e)
        {
            suc = true;
            rotorarea.AllowDrop = false;
            dropBoxCanvas.AllowDrop = false;
            String uID = e.Data.GetData("myFormat") as String;
            Debug.Text = "hello" + uID;

            int urint = Int32.Parse(uID);
            Rotor2 rotor2 = new Rotor2(urint, this.ActualWidth, this.ActualHeight);
            Canvas.SetLeft(rotor2, 690);
            rotorarea.Children.Add(rotor2);
            rotor2.PreviewMouseMove += new MouseEventHandler(Rotor_MouseMove);
            rotorarray[3] = rotor2;
            rotor2.helpNextAnimation += helptoanimatem4;
            rotor2.helpNextAnimation2 += helptoanimatem42;
            rotorarea.Children.Remove(dummyrec[3]);
            rotor2.fast = speed * 80;
            rotor2.Cursor = Cursors.Hand;
            rotor2.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
        }

        private void List_Drop30(object sender, DragEventArgs e)
        {
            suc = true;   
            rotorarea.AllowDrop = false;
            dropBoxCanvas.AllowDrop = false;
            String uID = e.Data.GetData("myFormat") as String;
            Debug.Text = "hello" + uID;

            int urint = Int32.Parse(uID);
            Rotor2 rotor2 = new Rotor2(urint, this.ActualWidth, this.ActualHeight);
            Canvas.SetLeft(rotor2, 0);
            rotorarea.Children.Add(rotor2);
            rotor2.PreviewMouseMove += new MouseEventHandler(Rotor_MouseMove);
            rotorarray[0] = rotor2;

            rotor2.helpNextAnimation += helptoanimate4;
            rotor2.helpNextAnimation2 += helptoanimate6;
            rotorarea.Children.Remove(dummyrec[0]);
            rotor2.fast = speed * 80;
            rotor2.Cursor = Cursors.Hand;
            rotor2.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;


        }
        
        private void List_Drop4(object sender, DragEventArgs e)
        {
            suc = true;
            walzenarea.AllowDrop = false;
            dropBoxCanvasWalze.AllowDrop = false;
            String uID = e.Data.GetData("myFormat") as String;
            Debug.Text = "hello" + uID;

            int urint = Int32.Parse(uID);
            Walze walzetmp = new Walze(urint, this.Width, this.Height);
            Canvas.SetLeft(walzetmp, 0);
            walzenarea.Children.Add(walzetmp);
            walzetmp.PreviewMouseMove += new MouseEventHandler(Walze_MouseMove);
            Canvas.SetTop(walzetmp, 60);
            walze = walzetmp;
            walzetmp.helpNextAnimation += helptoanimate5;
            walzenarea.Children.Remove(dummyrec[3]);
            walzetmp.fast = speed * 80;
            walzetmp.Cursor = Cursors.Hand;
            walzetmp.PreviewMouseLeftButtonDown += List_PreviewMouseLeftButtonDown;
            

        }
       

        private void Help(object sender, EventArgs e)
        {
            
        }

       
       

        private void switchbuttons(int button1, int button2) 
        {
            
            Button dummy = new Button();
            double dummyl ;

           // Debug.Text += lList[button1].X1 + "" + lList[button2].X1;
            /*Storyboard stb = new Storyboard();
            Storyboard stb1 = new Storyboard();
            
            AnimationClock myclock =fadeIn.CreateClock();
            



            Storyboard.SetTargetName(fadeIn, lList[Int32.Parse(bList[button1].Content.ToString())].Name);
            Storyboard.SetTargetProperty(fadeIn,new PropertyPath(Line.OpacityProperty));
            
            Storyboard.SetTargetName(fadeOut, lList[Int32.Parse(bList[button1].Content.ToString())].Name);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath(Line.OpacityProperty));
            
              
            
            stb.Children.Add(fadeOut);
            
            
            
            stb.Begin(this, true);
            stb.Children.Add(fadeIn);
            
            stb.BeginTime = new TimeSpan(0, 0, 0, 5);
            stb.Begin(this, true);*/
            //fadeOut.Completed += new EventHandler(Help);
            lList[Int32.Parse(bList[button1].Content.ToString())].BeginAnimation(OpacityProperty,fadeOut);
            lList[Int32.Parse(bList[button2].Content.ToString())].BeginAnimation(OpacityProperty, fadeOut);
            bList[Int32.Parse(bList[button2].Content.ToString())].BeginAnimation(OpacityProperty, fadeOut);
            bList[Int32.Parse(bList[button1].Content.ToString())].BeginAnimation(OpacityProperty, fadeOut);
            //Debug.Text="";


            int help = switchlist[button1];
            switchlist[button1] = switchlist[button2];
            switchlist[button2] = help;

            for (int i = 0; i < switchlist.Length; i++)
            {
             //   Debug.Text += i + "" + switchlist[i] + " ";
            }

            dummyl = lList[Int32.Parse(bList[button1].Content.ToString())].X1;
            dummy.Content = bList[button1].Content;
            //dummy.Uid = bList[button1].Uid;
            //dummy = bList[button1];


            //bList[button1] = bList[button2];
            lList[Int32.Parse(bList[button1].Content.ToString())].X1 = lList[Int32.Parse(bList[button2].Content.ToString())].X1;
            lList[Int32.Parse(bList[button1].Content.ToString())].X2 =15+Int32.Parse(bList[button1].Content.ToString()) * 30;
            lList[Int32.Parse(bList[button1].Content.ToString())].Y2 = 200;
            bList[button1].Content = bList[button2].Content;
            //bList[button1].Uid = bList[button2].Uid;


            lList[Int32.Parse(bList[button2].Content.ToString())].X1 = dummyl;
            lList[Int32.Parse(bList[button2].Content.ToString())].X2 = 15 + Int32.Parse(bList[button2].Content.ToString()) * 30;
            lList[Int32.Parse(bList[button2].Content.ToString())].Y2 = 200;
            bList[button2].Content = dummy.Content;
            //bList[button2].Uid = dummy.Uid;

            fadeIn.Completed += new EventHandler(Help);
            lList[Int32.Parse(bList[button1].Content.ToString())].BeginAnimation(OpacityProperty, fadeIn);
            lList[Int32.Parse(bList[button2].Content.ToString())].BeginAnimation(OpacityProperty, fadeIn);

            bList[Int32.Parse(bList[button1].Content.ToString())].BeginAnimation(OpacityProperty, fadeIn);
            bList[Int32.Parse(bList[button2].Content.ToString())].BeginAnimation(OpacityProperty, fadeIn);

           // yield return; enumerators move next 

            b = true;
            //bList[button2] = dummy;


            //bList[button2].Content = "test";
            
            }

        private void List_DragEnter(object sender, DragEventArgs e)
        {

            if (!e.Data.GetDataPresent("String") || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
                
            }

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

        }

        //löschen ab hier


        private Point _startPoint;
        private bool _isDragging;
        FrameworkElement _dragScope;
        DragAdorner _adorner = null;
        AdornerLayer _layer;

        public FrameworkElement DragScope
        {
            get { return _dragScope; }
            set { _dragScope = value; }
        }

        public bool IsDragging
        {
            get { return _isDragging; }
            set { _isDragging = value; }
        }

        void Window1_DragOver(object sender, DragEventArgs args)
        {
            
            if (_adorner != null)
            {
                if (aktuell != -1)
                {
                    lList[aktuell].X2 = args.GetPosition(DragScope).X * 2200 / this.ActualWidth - 1700  /* - _startPoint.X */ ;
                    lList[aktuell].Y2 = args.GetPosition(DragScope).Y * 1250 / this.ActualHeight - 380*1250 / this.ActualHeight /* - _startPoint.Y */ ;
                }
                _adorner.LeftOffset = args.GetPosition(DragScope).X /* - _startPoint.X */ ;
                _adorner.TopOffset = args.GetPosition(DragScope).Y /* - _startPoint.Y */ ;
                
            }
        }

        void Window1_DragOver2(object sender, DragEventArgs args)
        {

            if (_adorner != null)
            {
                
                _adorner.LeftOffset = args.GetPosition(DragScope).X /* - _startPoint.X */ ;
                _adorner.TopOffset = args.GetPosition(DragScope).Y /* - _startPoint.Y */ ;

            }
        }

        void aktuellupdate(object sender, MouseButtonEventArgs args)
        {
            aktuell = -1;
        }

        void DragSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        void DragSource_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point position0 = e.GetPosition(null);
            lList[0].X2 = position0.X;
            lList[0].Y2 = position0.Y;
            if (e.LeftButton == MouseButtonState.Pressed && !IsDragging)
            {
                Point position = e.GetPosition(null);

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    // StartDrag(e);
                    //  StartDragCustomCursor(e);
                    // StartDragWindow(e);
                    StartDragInProcAdorner(e);

                }
            }
        }

        private void StartDragInProcAdorner(MouseEventArgs e)
        {

            
        }





      

    }
}