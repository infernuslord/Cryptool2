using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Cryptool.Sigaba;
using Sigaba.Properties;



namespace Sigaba
{
    /// <summary>
    /// Interaction logic for SigabaPresentation.xaml
    /// </summary>
    public partial class SigabaPresentation : UserControl
    {
        #region rotors
        
        private PresentationRotor pr0;
        private PresentationRotor pr1;
        private PresentationRotor pr2;
        private PresentationRotor pr3;
        private PresentationRotor pr4;

        private PresentationRotor pr10;
        private PresentationRotor pr11;
        private PresentationRotor pr12;
        private PresentationRotor pr13;
        private PresentationRotor pr14;

        private PresentationIndex pri0;
        private PresentationIndex pri1;
        private PresentationIndex pri2;
        private PresentationIndex pri3;
        private PresentationIndex pri4;

        #endregion

        public Boolean Callback;

        public SigabaPresentation(Sigaba facade, SigabaSettings settings)
        {
            InitializeComponent(); 
            
            SizeChanged += sizeChanged; 
            AddRotors(facade,settings);
            clearPresentation();
        }

        void st_Completed(object sender, EventArgs e)
        {
            Callback = false;
            clearPresentation();
        }

        private void sizeChanged(Object sender, EventArgs eventArgs)
        {
            if(this.ActualWidth<this.ActualHeight)
                this.MainCanvas.RenderTransform = new ScaleTransform(this.ActualWidth / 600, this.ActualWidth / 600);
            else
                this.MainCanvas.RenderTransform = new ScaleTransform(this.ActualHeight / 600, this.ActualHeight / 600);
        }

        public void clearPresentation()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
                                                                                       {
                                                                                           textBlock6.Text = "" ;
                                                                                           textBlock1.Text = "" ;
                                                                                           textBlock3.Text = "" ;
                                                                                           textBlock4.Text = "" ;
                                                                                           
                                                                                           textBlock5.Text = "" ;
                                                                                           textBlock7.Text = "" ;
                                                                                           textBlock56.Text = "" ;
                                                                                           textBlock57.Text = "" ;
                                                                                           textBlock58.Text = "" ;
                                                                                           textBlock59.Text = "" ;

                                                                                           textBlock52.Text = "" ;
                                                                                           textBlock53.Text = "" ;
                                                                                           textBlock54.Text = "" ;
                                                                                           textBlock55.Text = "" ;

                                                                                           textBlock48.Text = "" ;
                                                                                           textBlock49.Text = "" ;
                                                                                           textBlock50.Text = "" ;
                                                                                           textBlock51.Text = "" ;
                                                                                           
                                                                                           textBlock44.Text = "" ;
                                                                                           textBlock45.Text = "" ;
                                                                                           textBlock46.Text = "" ;
                                                                                           textBlock47.Text = "" ;
                                                                                           
                                                                                           textBlock40.Text = "" ;
                                                                                           textBlock41.Text = "" ;
                                                                                           textBlock42.Text = "" ;
                                                                                           textBlock43.Text = "" ;
                                                                                           
                                                                                           textBlock36.Text = "" ;
                                                                                           textBlock37.Text = "" ;
                                                                                           textBlock38.Text = "" ;
                                                                                           textBlock39.Text = "" ;
                                                                                           
                                                                                           textBlock32.Text = "" ;
                                                                                           textBlock33.Text = "" ;
                                                                                           textBlock34.Text = "" ;
                                                                                           textBlock35.Text = "" ;
                                                                                           
                                                                                           textBlock8.Text = "" ;
                                                                                           textBlock9.Text = "" ;
                                                                                           textBlock10.Text = "";
                                                                                           textBlock11.Text = "" ;
                                                                                           textBlock12.Text = "" ;
                                                                                           textBlock13.Text = "" ;
                                                                                           textBlock14.Text = "" ;
                                                                                           textBlock15.Text = "" ;
                                                                                           textBlock16.Text = "" ;
                                                                                           textBlock17.Text = "" ;
                                                                                           textBlock18.Text = "" ;
                                                                                           textBlock19.Text = "" ;
                                                                                           textBlock20.Text = "" ;
                                                                                           textBlock21.Text = "" ;
                                                                                           textBlock22.Text = "" ;
                                                                                           textBlock23.Text = "" ;
                                                                                           textBlock24.Text = "" ;
                                                                                           textBlock25.Text = "" ;
                                                                                           textBlock26.Text = "" ;
                                                                                           textBlock27.Text = "" ;
                                                                                           
                                                                                           textBlock28.Text = "" ;
                                                                                           textBlock29.Text = "" ;
                                                                                           textBlock30.Text = "" ;
                                                                                           textBlock31.Text = "" ;
                                                                                       }, null);
        }

        public Storyboard st = new Storyboard();
        private double time = 0;
        public double SpeedRatio = 40;

        private void FillStoryBoard(char c, TextBlock t)
        {
            StringAnimationUsingKeyFrames stas = new StringAnimationUsingKeyFrames();

            DiscreteStringKeyFrame desc = new DiscreteStringKeyFrame("", TimeSpan.FromMilliseconds(time));
            DiscreteStringKeyFrame desc2 = new DiscreteStringKeyFrame(c + "", TimeSpan.FromMilliseconds(time + 300));
            DiscreteStringKeyFrame desc3 = new DiscreteStringKeyFrame("", TimeSpan.FromMilliseconds(400 * 60));

            stas.KeyFrames.Add(desc);
            stas.KeyFrames.Add(desc2);
            stas.KeyFrames.Add(desc3);

            Storyboard.SetTarget(stas, t);
            Storyboard.SetTargetProperty(stas, new PropertyPath("(Text)"));
            time += 400;
            st.Children.Add(stas);
        }

        private void FillStoryBoard(int c, TextBlock t)
        {
            StringAnimationUsingKeyFrames stas = new StringAnimationUsingKeyFrames();



            DiscreteStringKeyFrame desc = new DiscreteStringKeyFrame("", TimeSpan.FromMilliseconds(time));
            DiscreteStringKeyFrame desc2 = new DiscreteStringKeyFrame(c + "", TimeSpan.FromMilliseconds(time + 300));
            DiscreteStringKeyFrame desc3 = new DiscreteStringKeyFrame("", TimeSpan.FromMilliseconds(400*60));

            stas.KeyFrames.Add(desc);
            stas.KeyFrames.Add(desc2);
            stas.KeyFrames.Add(desc3);

            Storyboard.SetTarget(stas, t);
            Storyboard.SetTargetProperty(stas, new PropertyPath("(Text)"));
            time += 400;
            st.Children.Add(stas);
        }

        public void fillPresentation(int[,] s)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
                                                                                       {
                                                                                           
                                                                                           time = 0;
                                                                                           st = new Storyboard();

                                                                                           st.SpeedRatio = SpeedRatio;

                                                                                           st.Completed += new EventHandler(st_Completed);
                                                                                           /*
                                                                                           textBlock6.Text = "" +(char)(s[4, 0] + 65);
                                                                                           textBlock1.Text = "" +(char)(s[4, 1] + 65);
                                                                                           textBlock3.Text = "" +(char)(s[4, 2] + 65);
                                                                                           textBlock4.Text = "" + (char) (s[4, 3] + 65);
                                                                                           textBlock5.Text = "" + (char) (s[4, 4] + 65);
                                                                                           textBlock7.Text = "" + (char) (s[4, 5] + 65);
                                                                                           */

                                                                                           FillStoryBoard((char)(s[4, 5] + 65), textBlock7);
                                                                                           FillStoryBoard((char)(s[4, 4] + 65), textBlock5);
                                                                                           FillStoryBoard((char)(s[4, 3] + 65), textBlock4);
                                                                                           FillStoryBoard((char)(s[4, 2] + 65), textBlock3);
                                                                                           FillStoryBoard((char)(s[4, 1] + 65), textBlock1);
                                                                                           FillStoryBoard((char)(s[4, 0] + 65), textBlock6);
                                                                                           
                                                                                           FillStoryBoard((char)(s[0, 0] + 65), textBlock52);
                                                                                           FillStoryBoard((char)(s[1, 0] + 65), textBlock53);
                                                                                           FillStoryBoard((char)(s[2, 0] + 65), textBlock54);
                                                                                           FillStoryBoard((char)(s[3, 0] + 65), textBlock55);
                                                                                           
                                                                                           /*
                                                                                           textBlock52.Text = "" + (char) (s[0, 0] + 65);
                                                                                           textBlock53.Text = "" + (char) (s[1, 0] + 65);
                                                                                           textBlock54.Text = "" + (char) (s[2, 0] + 65);
                                                                                           textBlock55.Text = "" + (char) (s[3, 0] + 65);
                                                                                           
                                                                                           textBlock48.Text = "" + (char) (s[0, 1] + 65);
                                                                                           textBlock49.Text = "" + (char) (s[1, 1] + 65);
                                                                                           textBlock50.Text = "" + (char) (s[2, 1] + 65);
                                                                                           textBlock51.Text = "" + (char) (s[3, 1] + 65);
                                                                                            */

                                                                                           FillStoryBoard((char)(s[0, 1] + 65), textBlock48);
                                                                                           FillStoryBoard((char)(s[1, 1] + 65), textBlock49);
                                                                                           FillStoryBoard((char)(s[2, 1] + 65), textBlock50);
                                                                                           FillStoryBoard((char)(s[3, 1] + 65), textBlock51);
                                                                                           
                                                                                           /*
                                                                                           textBlock44.Text = "" + (char) (s[0, 2] + 65);
                                                                                           textBlock45.Text = "" + (char) (s[1, 2] + 65);
                                                                                           textBlock46.Text = "" + (char) (s[2, 2] + 65);
                                                                                           textBlock47.Text = "" + (char) (s[3, 2] + 65);
                                                                                           */

                                                                                           FillStoryBoard((char)(s[0, 2] + 65), textBlock44);
                                                                                           FillStoryBoard((char)(s[1, 2] + 65), textBlock45);
                                                                                           FillStoryBoard((char)(s[2, 2] + 65), textBlock46);
                                                                                           FillStoryBoard((char)(s[3, 2] + 65), textBlock47);
                                                                                           
                                                                                           /*
                                                                                           textBlock40.Text = "" + (char) (s[0, 3] + 65);
                                                                                           textBlock41.Text = "" + (char) (s[1, 3] + 65);
                                                                                           textBlock42.Text = "" + (char) (s[2, 3] + 65);
                                                                                           textBlock43.Text = "" + (char) (s[3, 3] + 65);
                                                                                           */

                                                                                           FillStoryBoard((char)(s[0, 3] + 65), textBlock40);
                                                                                           FillStoryBoard((char)(s[1, 3] + 65), textBlock41);
                                                                                           FillStoryBoard((char)(s[2, 3] + 65), textBlock42);
                                                                                           FillStoryBoard((char)(s[3, 3] + 65), textBlock43);
                                                                                           
                                                                                           /*
                                                                                           textBlock36.Text = "" + (char) (s[0, 4] + 65);
                                                                                           textBlock37.Text = "" + (char) (s[1, 4] + 65);
                                                                                           textBlock38.Text = "" + (char) (s[2, 4] + 65);
                                                                                           textBlock39.Text = "" + (char) (s[3, 4] + 65);
                                                                                           
                                                                                           textBlock32.Text = "" + (char) (s[0, 5] + 65);
                                                                                           textBlock33.Text = "" + (char) (s[1, 5] + 65);
                                                                                           textBlock34.Text = "" + (char) (s[2, 5] + 65);
                                                                                           textBlock35.Text = "" + (char) (s[3, 5] + 65);
                                                                                           */

                                                                                           FillStoryBoard((char)(s[0, 4] + 65), textBlock36);
                                                                                           FillStoryBoard((char)(s[1, 4] + 65), textBlock37);
                                                                                           FillStoryBoard((char)(s[2, 4] + 65), textBlock38);
                                                                                           FillStoryBoard((char)(s[3, 4] + 65), textBlock39);
                                                                                           FillStoryBoard((char)(s[0, 5] + 65), textBlock32);
                                                                                           FillStoryBoard((char)(s[1, 5] + 65), textBlock33);
                                                                                           FillStoryBoard((char)(s[2, 5] + 65), textBlock34);
                                                                                           FillStoryBoard((char)(s[3, 5] + 65), textBlock35);
                                                                                           
                                                                                           /*
                                                                                           textBlock8.Text = "" + s[0, 7];
                                                                                           textBlock9.Text = "" + s[1, 7];
                                                                                           textBlock10.Text = "" + s[2, 7];
                                                                                           textBlock11.Text = "" + s[3, 7];
                                                                                           textBlock12.Text = "" + s[0, 8];
                                                                                           textBlock13.Text = "" + s[1, 8];
                                                                                           textBlock14.Text = "" + s[2, 8];
                                                                                           textBlock15.Text = "" + s[3, 8];
                                                                                           textBlock16.Text = "" + s[0, 9];
                                                                                           textBlock17.Text = "" + s[1, 9];
                                                                                           textBlock18.Text = "" + s[2, 9];
                                                                                           textBlock19.Text = "" + s[3, 9];*/
                                                                                           
                                                                                           FillStoryBoard(s[0, 7], textBlock8);
                                                                                           FillStoryBoard(s[1, 7], textBlock9);
                                                                                           FillStoryBoard(s[2, 7], textBlock10);
                                                                                           FillStoryBoard(s[3, 7], textBlock11);
                                                                                           FillStoryBoard(s[0, 8], textBlock12);
                                                                                           FillStoryBoard(s[1, 8], textBlock13);
                                                                                           FillStoryBoard(s[2, 8], textBlock14);
                                                                                           FillStoryBoard(s[3, 8], textBlock15);
                                                                                           FillStoryBoard(s[0, 9], textBlock16);
                                                                                           FillStoryBoard(s[1, 9], textBlock17);
                                                                                           FillStoryBoard(s[2, 9], textBlock18);
                                                                                           FillStoryBoard(s[3, 9], textBlock19);
                                                                                           
                                                                                           /*
                                                                                           textBlock20.Text = "" + s[0, 10];
                                                                                           textBlock21.Text = "" + s[1, 10];
                                                                                           textBlock22.Text = "" + s[2, 10];
                                                                                           textBlock23.Text = "" + s[3, 10];
                                                                                           textBlock24.Text = "" + s[0, 11];
                                                                                           textBlock25.Text = "" + s[1, 11];
                                                                                           textBlock26.Text = "" + s[2, 11];
                                                                                           textBlock27.Text = "" + s[3, 11];
                                                                                           textBlock28.Text = "" + s[0, 12];
                                                                                           textBlock29.Text = "" + s[1, 12];
                                                                                           textBlock30.Text = "" + s[2, 12];
                                                                                           textBlock31.Text = "" + s[3, 12];
                                                                                           */
                                                                                           FillStoryBoard(s[0, 7], textBlock20);
                                                                                           FillStoryBoard(s[1, 7], textBlock21);
                                                                                           FillStoryBoard(s[2, 7], textBlock22);
                                                                                           FillStoryBoard(s[3, 7], textBlock23);
                                                                                           FillStoryBoard(s[0, 8], textBlock24);
                                                                                           FillStoryBoard(s[1, 8], textBlock25);
                                                                                           FillStoryBoard(s[2, 8], textBlock26);
                                                                                           FillStoryBoard(s[3, 8], textBlock27);
                                                                                           FillStoryBoard(s[0, 9], textBlock28);
                                                                                           FillStoryBoard(s[1, 9], textBlock29);
                                                                                           FillStoryBoard(s[2, 9], textBlock30);
                                                                                           FillStoryBoard(s[3, 9], textBlock31);

                                                                                           /*
                                                                                           textBlock56.Text = "" + (s[0, 14] + 1);
                                                                                           textBlock57.Text = "" + (s[1, 14] + 1);
                                                                                           textBlock58.Text = "" + (s[2, 14] + 1);
                                                                                           textBlock59.Text = "" + (s[3, 14] + 1);
                                                                                           */

                                                                                           FillStoryBoard(s[0, 14], textBlock56);
                                                                                           FillStoryBoard(s[1, 14], textBlock57);
                                                                                           FillStoryBoard(s[2, 14], textBlock58);
                                                                                           FillStoryBoard(s[3, 14], textBlock59);
                                                                                            
                                                                                           st.Begin();

                                                                                       }, null);
        }

        public void setRotors()
        {

        }

        public void settings_OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            SigabaSettings settings = sender as SigabaSettings;

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
                                                                                       {

                                                                                           pri0.SetType(settings.IndexRotor1);
                                                                                           pri1.SetType(settings.IndexRotor2);
                                                                                           pri2.SetType(settings.IndexRotor3);
                                                                                           pri3.SetType(settings.IndexRotor4);
                                                                                           pri4.SetType(settings.IndexRotor5);

                                                                                           pr0.Index = settings.CipherRotor5;
                                                                                           pr1.Index = settings.CipherRotor4;
                                                                                           pr2.Index = settings.CipherRotor3;
                                                                                           pr3.Index = settings.CipherRotor2;
                                                                                           pr4.Index = settings.CipherRotor1;

                                                                                           pr10.Index = settings.ControlRotor5;
                                                                                           pr11.Index = settings.ControlRotor4;
                                                                                           pr12.Index = settings.ControlRotor3;
                                                                                           pr13.Index = settings.ControlRotor2;
                                                                                           pr14.Index = settings.ControlRotor1;

                                                                                           pr0.Reverse(settings.CipherRotor5Reverse);
                                                                                           pr1.Reverse(settings.CipherRotor4Reverse);
                                                                                           pr2.Reverse(settings.CipherRotor3Reverse);
                                                                                           pr3.Reverse(settings.CipherRotor2Reverse);
                                                                                           pr4.Reverse(settings.CipherRotor1Reverse);

                                                                                           pr10.Reverse(settings.ControlRotor5Reverse);
                                                                                           pr11.Reverse(settings.ControlRotor4Reverse);
                                                                                           pr12.Reverse(settings.ControlRotor3Reverse);
                                                                                           pr13.Reverse(settings.ControlRotor2Reverse);
                                                                                           pr14.Reverse(settings.ControlRotor1Reverse);

                                                                                           pri0.Reverse(settings.IndexRotor1Reverse);
                                                                                           pri1.Reverse(settings.IndexRotor2Reverse);
                                                                                           pri2.Reverse(settings.IndexRotor3Reverse);
                                                                                           pri3.Reverse(settings.IndexRotor4Reverse);
                                                                                           pri4.Reverse(settings.IndexRotor5Reverse);

                                                                                           pr0.SetPosition(
                                                                                               settings.CipherKey[0]);

                                                                                           pr1.SetPosition(
                                                                                               settings.CipherKey[1]);
                                                                                           pr2.SetPosition(
                                                                                               settings.CipherKey[2]);
                                                                                           pr3.SetPosition(
                                                                                               settings.CipherKey[3]);
                                                                                           pr4.SetPosition(
                                                                                               settings.CipherKey[4]);

                                                                                           pr10.SetPosition(
                                                                                               settings.ControlKey[0]);
                                                                                           pr11.SetPosition(
                                                                                               settings.ControlKey[1]);
                                                                                           pr12.SetPosition(
                                                                                               settings.ControlKey[2]);
                                                                                           pr13.SetPosition(
                                                                                               settings.ControlKey[3]);
                                                                                           pr14.SetPosition(
                                                                                               settings.ControlKey[4]);

                                                                                           pri0.SetPosition(
                                                                                               settings.IndexKey[0] - 48);
                                                                                           pri1.SetPosition(
                                                                                               settings.IndexKey[1] - 48);
                                                                                           pri2.SetPosition(
                                                                                               settings.IndexKey[2] - 48);
                                                                                           pri3.SetPosition(
                                                                                               settings.IndexKey[3] - 48);
                                                                                           pri4.SetPosition(
                                                                                               settings.IndexKey[4] - 48);
                                                                                       }, null);


        }

        private void SetSettings(SigabaSettings settings)
        {

            pri0.SetType(settings.IndexRotor1);
            pri1.SetType(settings.IndexRotor2);
            pri2.SetType(settings.IndexRotor3);
            pri3.SetType(settings.IndexRotor4);
            pri4.SetType(settings.IndexRotor5);

            pr0.Index = settings.CipherRotor5;
            pr1.Index = settings.CipherRotor4;
            pr2.Index = settings.CipherRotor3;
            pr3.Index = settings.CipherRotor2;
            pr4.Index = settings.CipherRotor1;
            pr10.Index = settings.ControlRotor5;
            pr11.Index = settings.ControlRotor4;
            pr12.Index = settings.ControlRotor3;
            pr13.Index = settings.ControlRotor2;
            pr14.Index = settings.ControlRotor1;


            pr0.Reverse(settings.CipherRotor5Reverse);
            pr1.Reverse(settings.CipherRotor4Reverse);
            pr2.Reverse(settings.CipherRotor3Reverse);
            pr3.Reverse(settings.CipherRotor2Reverse);
            pr4.Reverse(settings.CipherRotor1Reverse);

            pr10.Reverse(settings.ControlRotor1Reverse);
            pr11.Reverse(settings.ControlRotor2Reverse);
            pr12.Reverse(settings.ControlRotor3Reverse);
            pr13.Reverse(settings.ControlRotor4Reverse);
            pr14.Reverse(settings.ControlRotor5Reverse);

            pri0.Reverse(settings.IndexRotor1Reverse);
            pri1.Reverse(settings.IndexRotor2Reverse);
            pri2.Reverse(settings.IndexRotor3Reverse);
            pri3.Reverse(settings.IndexRotor4Reverse);
            pri4.Reverse(settings.IndexRotor5Reverse);

            pr0.SetPosition(
                settings.CipherKey[0]);

            pr1.SetPosition(
                settings.CipherKey[1]);
            pr2.SetPosition(
                settings.CipherKey[2]);
            pr3.SetPosition(
                settings.CipherKey[3]);
            pr4.SetPosition(
                settings.CipherKey[4]);

            pr10.SetPosition(
                settings.ControlKey[0]);
            pr11.SetPosition(
                settings.ControlKey[1]);
            pr12.SetPosition(
                settings.ControlKey[2]);
            pr13.SetPosition(
                settings.ControlKey[3]);
            pr14.SetPosition(
                settings.ControlKey[4]);

            pri0.SetPosition(
                settings.IndexKey[0] - 48);
            pri1.SetPosition(
                settings.IndexKey[1] - 48);
            pri2.SetPosition(
                settings.IndexKey[2] - 48);
            pri3.SetPosition(
                settings.IndexKey[3] - 48);
            pri4.SetPosition(
                settings.IndexKey[4] - 48);
        }

        private void AddRotors(Sigaba facade, SigabaSettings _settings)
        {
            int distance = 50;
            pr0 = new PresentationRotor();
            pr1 = new PresentationRotor();
            pr2 = new PresentationRotor();
            pr3 = new PresentationRotor();
            pr4 = new PresentationRotor();

            canvas1.MouseLeftButtonDown += new MouseButtonEventHandler(pr0_MouseLeftButtonDown);
            canvas2.MouseLeftButtonDown += new MouseButtonEventHandler(pr0_MouseLeftButtonDown);
            canvas3.MouseLeftButtonDown += new MouseButtonEventHandler(pr0_MouseLeftButtonDown);
            canvas4.MouseLeftButtonDown += new MouseButtonEventHandler(pr0_MouseLeftButtonDown);
            canvas5.MouseLeftButtonDown += new MouseButtonEventHandler(pr0_MouseLeftButtonDown);
            

            canvas1.Children.Add(pr0);
            canvas2.Children.Add(pr1);
            canvas3.Children.Add(pr2);
            canvas4.Children.Add(pr3);
            canvas5.Children.Add(pr4);
            
            Canvas.SetLeft(pr0, 7);
            Canvas.SetLeft(pr1, 7);
            Canvas.SetLeft(pr2, 7);
            Canvas.SetLeft(pr3, 7);
            Canvas.SetLeft(pr4, 7);

            pr0.RenderTransform = new ScaleTransform(1.2, 1.2);
            pr1.RenderTransform = new ScaleTransform(1.2, 1.2);
            pr2.RenderTransform = new ScaleTransform(1.2, 1.2);
            pr3.RenderTransform = new ScaleTransform(1.2, 1.2);
            pr4.RenderTransform = new ScaleTransform(1.2, 1.2);

            pri0 = new PresentationIndex(1);
            pri1 = new PresentationIndex(2);
            pri2 = new PresentationIndex(3);
            pri3 = new PresentationIndex(4);
            pri4 = new PresentationIndex(5);

            canvas7.MouseLeftButtonDown += new MouseButtonEventHandler(pri0_MouseLeftButtonDown);
            canvas8.MouseLeftButtonDown += new MouseButtonEventHandler(pri0_MouseLeftButtonDown);
            canvas9.MouseLeftButtonDown += new MouseButtonEventHandler(pri0_MouseLeftButtonDown);
            canvas10.MouseLeftButtonDown += new MouseButtonEventHandler(pri0_MouseLeftButtonDown);
            canvas11.MouseLeftButtonDown += new MouseButtonEventHandler(pri0_MouseLeftButtonDown);
            
            Canvas.SetLeft(pri0, 6);
            Canvas.SetLeft(pri1, 6);
            Canvas.SetLeft(pri2, 6);
            Canvas.SetLeft(pri3, 6);
            Canvas.SetLeft(pri4, 6);

            pri0.RenderTransform = new ScaleTransform(1.2, 1.2);
            pri1.RenderTransform = new ScaleTransform(1.2, 1.2);
            pri2.RenderTransform = new ScaleTransform(1.2, 1.2);
            pri3.RenderTransform = new ScaleTransform(1.2, 1.2);
            pri4.RenderTransform = new ScaleTransform(1.2, 1.2);

            canvas7.Children.Add(pri0);
            canvas8.Children.Add(pri1);
            canvas9.Children.Add(pri2);
            canvas10.Children.Add(pri3);
            canvas11.Children.Add(pri4);
            
            pr10 = new PresentationRotor();
            pr11 = new PresentationRotor();
            pr12 = new PresentationRotor();
            pr13 = new PresentationRotor();
            pr14 = new PresentationRotor();

            canvas6.MouseLeftButtonDown += new MouseButtonEventHandler(pr0_MouseLeftButtonDown);
            canvas12.MouseLeftButtonDown += new MouseButtonEventHandler(pr0_MouseLeftButtonDown);
            canvas13.MouseLeftButtonDown += new MouseButtonEventHandler(pr0_MouseLeftButtonDown);
            canvas14.MouseLeftButtonDown += new MouseButtonEventHandler(pr0_MouseLeftButtonDown);
            canvas15.MouseLeftButtonDown += new MouseButtonEventHandler(pr0_MouseLeftButtonDown);

            canvas6.Children.Add(pr10);
            canvas12.Children.Add(pr11);
            canvas13.Children.Add(pr12);
            canvas14.Children.Add(pr13);
            canvas15.Children.Add(pr14);

            pr10.RenderTransform = new ScaleTransform(1.2, 1.2);
            Canvas.SetLeft(pr10, 7);
            pr11.RenderTransform = new ScaleTransform(1.2, 1.2);
            Canvas.SetLeft(pr11, 7);
            pr12.RenderTransform = new ScaleTransform(1.2, 1.2);
            Canvas.SetLeft(pr12, 7);
            pr13.RenderTransform = new ScaleTransform(1.2, 1.2);
            Canvas.SetLeft(pr13, 7);
            pr14.RenderTransform = new ScaleTransform(1.2, 1.2);
            Canvas.SetLeft(pr14, 7);

            SetSettings(_settings);
        }

        private Rotor3 toro;
        private Rotor3Index toro2;

        void pr0_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Canvas canv = sender as Canvas;

            PresentationRotor pr = new PresentationRotor();
            foreach (UIElement ui in canv.Children)
            {
                if(ui is PresentationRotor)
                {
                    pr = ui as PresentationRotor;
                }
            }
            
            
            Console.WriteLine(pr.Index+"---------------------------------------------------------------------------");
            
            if(!MainCanvas.Children.Contains(toro))
            {
                toro = new Rotor3(new Rotor(SigabaConstants.ControlCipherRotors[pr.Index], pr.Position - 65, pr.Reversed),true);
                toro.RenderTransform = new ScaleTransform(0.6, 0.6);
                MainCanvas.Children.Add(toro);
            }

            else
            {
                MainCanvas.Children.Remove(toro);
                toro = new Rotor3(new Rotor(SigabaConstants.ControlCipherRotors[pr.Index], pr.Position - 65, pr.Reversed), true);
                toro.RenderTransform = new ScaleTransform(0.6, 0.6);
                MainCanvas.Children.Add(toro);
            }
            
            Canvas.SetRight(toro,0);
            Canvas.SetTop(toro, 20);

            if (MainCanvas.Children.Contains(toro2))
            {
                MainCanvas.Children.Remove(toro2);
            }

            toro.MouseLeftButtonUp += new MouseButtonEventHandler(toro_MouseLeftButtonDown);
        }

        void toro_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainCanvas.Children.Remove(toro);
           
        }

        void toro2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainCanvas.Children.Remove(toro2);
        }

        void pri0_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Canvas canv = sender as Canvas;

            PresentationIndex pr = new PresentationIndex(0);
            foreach (UIElement ui in canv.Children)
            {
                if (ui is PresentationIndex)
                {
                    pr = ui as PresentationIndex;
                }
            }

            Console.WriteLine(pr.Index + "---------------------------------------------------------------------------");

            if (!MainCanvas.Children.Contains(toro2))
            {
                toro2 = new Rotor3Index(new Rotor(SigabaConstants.IndexRotors[pr.Index], pr.Position, pr.Reversed));
                toro2.RenderTransform = new ScaleTransform(0.6, 0.6);
                MainCanvas.Children.Add(toro2);
            }

            else
            {
                MainCanvas.Children.Remove(toro2);
                toro2 = new Rotor3Index(new Rotor(SigabaConstants.IndexRotors[pr.Index], pr.Position , pr.Reversed));
                toro2.RenderTransform = new ScaleTransform(0.6, 0.6);
                MainCanvas.Children.Add(toro2);
            }

            if (MainCanvas.Children.Contains(toro))
            {
                MainCanvas.Children.Remove(toro);
            }

            Canvas.SetRight(toro2, 0);
            Canvas.SetTop(toro2, 20);

            toro2.MouseLeftButtonUp += new MouseButtonEventHandler(toro2_MouseLeftButtonDown);
        }

        public void stop()
        {
            st.Stop();
            st.Freeze();
            Callback = false;
        }


    }
}