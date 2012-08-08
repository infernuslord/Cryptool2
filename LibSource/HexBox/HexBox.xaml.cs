using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace HexBox
{
    /// <summary>
    /// Interaction logic for HexBox.xaml
    /// </summary>
    public partial class HexBox : UserControl
    {
        #region Private variables

        private FileStream fs;
        private DynamicFileByteProvider dyfipro;
        private double lastUpdate;
        private long[] mark;
        private Boolean markedBackwards = false;
        private TextBlock Info = new TextBlock();
        private int cell;
        private int cellText;
        private Point falloff = new Point(0, 0);


        #endregion

        #region Properties

        public Boolean inReadOnlyMode = false;
        public string Pfad = string.Empty;
        
        #endregion

        #region Constructor

        private StretchText st;
        private HexText ht;

        public HexBox()
        {
            InitializeComponent();

            st = new StretchText();
            ht = new HexText();

            canvas1.MouseDown +=new MouseButtonEventHandler(ht_MouseDown);
            canvas1.MouseUp += new MouseButtonEventHandler(ht_MouseUp);
            canvas1.MouseMove += ht_MouseMove;
            canvas2.MouseDown += new MouseButtonEventHandler(st_MouseDown);
            canvas2.MouseMove += st_MouseMove;
            canvas2.MouseUp += new MouseButtonEventHandler(st_MouseUp);

            canvas1.Cursor = Cursors.IBeam;
            
            canvas2.Cursor = Cursors.IBeam;

            st.FontFamily = new FontFamily("Consolas");

            st.Width = 100;
            //ht.Width = 100;
            //ht.Text = "foo bar";

            Binding myBinding = new Binding("ByteContent");
            myBinding.Source = st;
            //myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            myBinding.Mode = BindingMode.TwoWay;
            ht.SetBinding(HexText.ByteProperty, myBinding);



            canvas2.Children.Add(st);
            canvas1.Children.Add(ht);

            this.MouseWheel += new MouseWheelEventHandler(MainWindow_MouseWheel);
            
            mark = new long[2];

            mark[0] = -1;

            mark[1] = -1;

            cursor2.Focus();
            for (int j = 0; j < 16; j++)
            {
                TextBlock id = new TextBlock();
                id.TextAlignment = TextAlignment.Right;
                id.VerticalAlignment = VerticalAlignment.Center;
                id.FontFamily = new FontFamily("Consolas");

                id.Height = 20;
                id.Text = "00000000";
                id.FontSize = 13;
                Grid.SetRow(id, j);
                gridid.Children.Add(id);
                for (int i = 0; i < 16; i++)
                {
                    TextBlock tb = new TextBlock();
                    tb.Cursor = Cursors.IBeam;
                    tb.Text = "  ";
                    tb.FontSize = 13;
                    tb.Width = 20;
                    tb.Height = 20;
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    //tb.MouseLeftButtonDown += new MouseButtonEventHandler(tb_MouseDown);
                    //tb.MouseLeftButtonUp += new MouseButtonEventHandler(tb_MouseUp);

                    tb.Visibility = Visibility.Hidden;


                    tb.MouseMove += tb_MouseMove;

                    Grid.SetColumn(tb, i);
                    Grid.SetRow(tb, j);
                    tb.TextAlignment = TextAlignment.Center;
                    tb.Background = Brushes.Transparent;
                    grid1.Children.Add(tb);

                    tb.FontFamily = new FontFamily("Consolas");

                    TextBlock tb2 = new TextBlock();

                    tb2.FontFamily = new FontFamily("Consolas");
                    tb2.Cursor = Cursors.IBeam;
                    tb2.Width = 20;
                    tb2.Height = 20;
                    tb2.Text = "  ";
                    tb2.FontSize = 13;
                    tb2.Background = Brushes.Transparent;
                    tb2.VerticalAlignment = VerticalAlignment.Stretch;
                    tb2.HorizontalAlignment = HorizontalAlignment.Center;
                    //tb2.MouseLeftButtonDown += new MouseButtonEventHandler(tb2_MouseDown);
                    //tb2.MouseLeftButtonUp += new MouseButtonEventHandler(tb2_MouseUp);
                    tb2.MouseMove += tb_MouseMove;

                    tb2.Visibility = Visibility.Hidden;

                    Grid.SetColumn(tb2, i);
                    Grid.SetRow(tb2, j);
                    tb2.TextAlignment = TextAlignment.Center;

                    tb2.Background = Brushes.Transparent;
                    grid2.Children.Add(tb2);
                }
            }

            Storyboard sb = new Storyboard();
            
            canvas1.MouseDown += new MouseButtonEventHandler(canvas1_MouseDown);
            canvas2.MouseDown += new MouseButtonEventHandler(canvas2_MouseDown);

            cursor.PreviewKeyDown += KeyInputHexField;
            cursor2.PreviewKeyDown += KeyInputASCIIField;

            cursor2.TextInput += ASCIIField_TextInput;
            cursor.TextInput += HexBox_TextInput;

            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            Stream s = new MemoryStream();

            dyfipro = new DynamicFileByteProvider(s);

            dyfipro.LengthChanged += new EventHandler(dyfipro_LengthChanged);

        }

        #endregion

        #region Mouse interaction and events

        private void setcursor(Point p) 
        {
            p.X = cell / 2 % 16 * (ht.CharWidth * 3);
            p.Y = cell / 2 / 16 * 20;

            Canvas.SetLeft(cursor, p.X);

            Canvas.SetTop(cursor, p.Y);

        }

        private void setcuror2(Point p)
        {
            p.X = cellText % 16 * (st.CharWidth);
            p.Y = cellText / 16 * 20;

            Canvas.SetLeft(cursor2, p.X);

            Canvas.SetTop(cursor2, p.Y);
        }

        private void st_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(canvas2);
            
            falloff = p;
            
            cellText = (int)(((int)Math.Round((p.Y - 10) / 20.0)) * 16 + (int)Math.Round((p.X) / (st.CharWidth)));

            if (cellText + (long)fileSlider.Value * 16 > dyfipro.Length)
            {
                cellText = (int)(dyfipro.Length - (long)fileSlider.Maximum * 16);
            }

            st.mark[0] = cellText;
            ht.mark[0] = st.mark[0] * 2;

            mark[0] = cellText + (long)fileSlider.Value * 16;

            setcuror2(p);

            cursor2.Focus();
            
            st.removemarks = true;
            ht.removemarks = true;

            updateUI((long)fileSlider.Value);
        }

        private void ht_MouseDown(object sender, MouseButtonEventArgs e)
        {

            Point p = e.GetPosition(canvas1);
            falloff = p;
            cell = (int)(((int)Math.Round((p.Y - 10) / 20.0)) * 16 + (int)Math.Round((p.X) / (ht.CharWidth * 3))) * 2;

            if (cell / 2 + (long)fileSlider.Value * 16 > dyfipro.Length)
            {
                cell = (int)(dyfipro.Length - (long)fileSlider.Maximum * 16) * 2;
            }

            ht.mark[0] = cell;

            ht.removemarks = true;
            

            mark[0] = cell / 2 + (long)fileSlider.Value * 16;


            setcursor(p);

            cursor.Focus();
            updateUI((long)fileSlider.Value);
        }

        private void st_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                
                if (10 < Math.Abs(e.GetPosition(canvas2).X - falloff.X) || 10 < Math.Abs(e.GetPosition(canvas2).Y - falloff.Y))
                {
                    st.removemarks = false;
                }



                Point p = e.GetPosition(canvas2);

                cellText = (int)(((int)Math.Round((p.Y - 10) / 20.0)) * 16 + (int)Math.Round((p.X) / (st.CharWidth)));



                if (mark[0] > cellText + (long)fileSlider.Value * 16)
                {
                    mark[1] = cellText + (long)fileSlider.Value * 16;
                    markedBackwards = true;
                    st.mark[1] = (int)(mark[1] - fileSlider.Value * 16);
                    //ht.mark[1] = (int)(mark[1] - fileSlider.Value * 16)*2;
                }

                else
                {
                    mark[1] = cellText + (long)fileSlider.Value * 16;
                    st.mark[1] = (int)(mark[1] - fileSlider.Value * 16);
                    //ht.mark[1] = (int)(mark[1] - fileSlider.Value * 16)*2;
                }

                

                updateUI((long)fileSlider.Value);
            }
        }

        private void ht_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (10 < Math.Abs(e.GetPosition(canvas1).X - falloff.X) || 10 < Math.Abs(e.GetPosition(canvas1).Y - falloff.Y))
                { 
                    ht.removemarks = false; 
                }

                Point p = e.GetPosition(canvas1);

                cell = (int)(((int)Math.Round((p.Y - 10) / 20.0)) * 16 + (int)Math.Round((p.X) / (ht.CharWidth * 3))) * 2;

                if (cell / 2 + (long)fileSlider.Value * 16 > dyfipro.Length)
                {
                    cell = (int)(dyfipro.Length - (long)fileSlider.Maximum) * 2 - 2;
                }

                if (mark[0] > cell /2 + (long)fileSlider.Value * 16)
                {
                    mark[1] = cell/2 + (long)fileSlider.Value * 16;
                    markedBackwards = true;
                    ht.mark[1] = (int) (cell);
                }

                else
                {
                    mark[1] = cell/2 + (long)fileSlider.Value * 16;
                    ht.mark[1] = (int)(cell);
                }

                

                updateUI((long)fileSlider.Value);
            }
        }


        private void st_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(canvas2);

            cellText = (int)(((int)Math.Round((p.Y - 10) / 20.0)) * 16 + (int)Math.Round((p.X) / (st.CharWidth)));

            if (cellText + (long)fileSlider.Value * 16 > dyfipro.Length)
            {
                cellText = (int)(dyfipro.Length - (long)fileSlider.Maximum * 16) ;
            }

            setcuror2(p);

            if (mark[1] < mark[0])
            {
                markedBackwards = true;
                long help = mark[1];
                mark[1] = mark[0];
                mark[0] = help;

            }


        }

        private void ht_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(canvas1);
            cell = (int)(((int)Math.Round((p.Y - 10) / 20.0)) * 16 + (int)Math.Round((p.X) / (ht.CharWidth * 3))) * 2;

            if (cell/2  + (long)fileSlider.Value * 16 > dyfipro.Length)
            {
                cell = (int)(dyfipro.Length - (long)fileSlider.Maximum * 16  )*2 ;
            }

            setcursor(p);

            if (mark[1] < mark[0])
            {
                markedBackwards = true;
                long help = mark[1];
                mark[1] = mark[0];
                mark[0] = help;

            }

        }

       


        private void canvas1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            cursor.Focus();

            //e.Handled = true;
        }

        private void canvas2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            cursor2.Focus();

            //e.Handled = true;
        }

        #endregion

        #region Keyinput


        private void setPositionText(int cellText)
        {
            if (cellText  + (long)fileSlider.Value * 16 > dyfipro.Length)
            {
                cellText = (int)(dyfipro.Length - (long)fileSlider.Value * 16)  ;
            }
            Point p = new Point();

            p.X = cellText % 16 * st.CharWidth ;
            p.Y = cellText / 16 * 20;

            

            Canvas.SetLeft(cursor2, p.X);

            Canvas.SetTop(cursor2, p.Y);

            cursor2.Focus();
        }

        private void setPosition(int cell)
        {
            if(cell / 2 + (long)fileSlider.Value * 16 >dyfipro.Length)
            {
                cell = (int)(dyfipro.Length - (long)fileSlider.Value * 16)*2-2;
            }

            
            
            

            Point p = new Point();

            p.X = cell % 32 * (ht.CharWidth * 3) / 2;
            p.Y = cell / 32 * 20;

            if(cell%2==1)
            {
                p.X = p.X - ht.CharWidth/2;
            }

            Canvas.SetLeft(cursor, p.X);

            Canvas.SetTop(cursor, p.Y);

            cursor.Focus();
        }

      

        private void KeyInputHexField(object sender, KeyEventArgs e)
        {
            if (Pfad != "" && Pfad != " ")
            {
                //debug.Text = e.Key.ToString();    
                Key k = e.Key;
                
                Boolean releasemark = true;


                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                string s = k.ToString();

                if (e.Key == Key.Right)
                {
                    
                    if (cell / 2 + (long)fileSlider.Value * 16 < dyfipro.Length)
                    {
                        if(cell<32*16)
                        {    this.cell++;}
                        else
                        {
                            if (fileSlider.Value < fileSlider.Maximum)
                            {
                                fileSlider.Value += 1;

                                this.cell = 32*16-32;
                            }
                        }

                    }
                    
                    e.Handled = true;
                }

                else if (e.Key == Key.Left)
                {
                    if (cell >0)
                    {
                        this.cell--;
                    }
                    else
                    {
                        if (fileSlider.Value > 0)
                        {
                            fileSlider.Value -= 1;

                            this.cell = 32;
                        }
                    }
                
                    e.Handled = true;
                }

                else if (e.Key == Key.Down)
                {
                 
                    if (cell / 2 + (long)fileSlider.Value * 16 + 15 < dyfipro.Length)
                    {
                        if(cell<32*16-32)
                        {
                            this.cell +=  32;
                        }
                        else
                        {
                            if(fileSlider.Value<fileSlider.Maximum)
                            {
                                fileSlider.Value += 1;
                            }
                        }

                 
                    }
                    e.Handled = true;
                }

                else if (e.Key == Key.Up)
                {
                    if(cell>32)
                    {cell -= 32;}
                    else
                    {
                        if (fileSlider.Value >0)
                        {
                            fileSlider.Value -= 1;
                        }
                    }
                    e.Handled = true;
                }

                else if (e.Key == Key.Back)
                {
                    if (mark[1] - mark[0] == 0)
                    {

                        
                        if (cell > 0)
                        {
                            dyfipro.DeleteBytes(cell / 2 + (long)fileSlider.Value * 16 - 1, 1);
                        

                            if (cell%2 == 1)
                            {
                                cell--;
                            }
                            cell--;
                            cell--;
                        }
                        else
                        {
                            if (fileSlider.Value > 0)
                            {
                                fileSlider.Value -= 1;

                                this.cell = 30;
                            }
                        }
                    }

                    

                    e.Handled = true;
                }

                 if (e.Key == Key.PageDown)
                {
                    fileSlider.Value += 16;
                     e.Handled = true;
                }

                else if (e.Key == Key.PageUp)
                {
                    fileSlider.Value -= 16;
                    e.Handled = true;
                }

                else if (e.Key == Key.End)
                {
                    cell = ((int)(cell / 32) + 1) * 32-2;
                    e.Handled = true;
                }


                else if (e.Key == Key.Home)
                {
                    cell = ((int)(cell / 32) ) * 32 ;
                    e.Handled = true;

                }


                else if (e.Key == Key.Return)
                {
                    e.Handled = true;
                }
                
                setPosition(cell);
                
                

                if (e.Key == Key.Tab)
                {
                    cursor2.Focus();
                }



                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {   
                    releasemark = false;
                    if (mark[0] == -1 || mark[1] == -1)
                    {
                        mark[0] = cell/2  + (long)fileSlider.Value * 16;
                        mark[1] = cell/2  + (long)fileSlider.Value * 16;
                    }

                    if (cell / 2 + (long)fileSlider.Value * 16 < mark[0])
                    {
                        markedBackwards = true;
                    }

                    if (cell / 2 + (long)fileSlider.Value * 16 > mark[1])
                    {
                        markedBackwards = false;
                    }

                    if (cell / 2 + (long)fileSlider.Value * 16 <= mark[1] && cell / 2 + (long)fileSlider.Value * 16 >= mark[0] &&
                        !markedBackwards)
                    {
                        mark[1] = cell / 2 + (long)fileSlider.Value * 16;
                    }

                    if (cell / 2 + (long)fileSlider.Value * 16 <= mark[1] && cell / 2 + (long)fileSlider.Value * 16 >= mark[0] &&
                        markedBackwards)
                    {
                        mark[0] = cell / 2 + (long)fileSlider.Value * 16;
                    }


                    if (cell / 2 + (long)fileSlider.Value * 16 < mark[0])
                    {
                        mark[0] = cell / 2 + (long)fileSlider.Value * 16;
                    }

                    if (cell / 2 + (long)fileSlider.Value * 16 > mark[1])
                    {
                        mark[1] = cell / 2 + (long)fileSlider.Value * 16;
                    }

                    ht.removemarks = false;
                }
                else
                {
                    
                }

                if (!Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    if (releasemark)
                    {
                        mark[0] = -1;
                        mark[1] = -1;
                        ht.mark[0] = -1;
                        ht.mark[1] = -1;
                        ht.removemarks = true;
                    }
                }

                if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.A)
                {

                }



                if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.C)
                {

                    Copy_HexBoxField();


                }

                if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.V)
                {

                    HexBox_TextInput_Help(Clipboard.GetText());
                    
                }

                if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.X)
                {

                    Cut_HexBoxField();
                    e.Handled = true;
                }

                if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.S)
                {

                    dyfipro.ApplyChanges();
                    e.Handled = true;
                }


                int cell2 = (int)(Canvas.GetTop(cursor) / 20 * 32 + Canvas.GetLeft(cursor) / 10);
                Column.Text = (cell2 / 2) % 16 + "";
                Line.Text = (int)(Canvas.GetTop(cursor) / 20) + (long)fileSlider.Value + "";
            }
            else
            {
                e.Handled = true;
            }

            
            updateUI((long)fileSlider.Value);
        }

        private void KeyInputASCIIField(object sender, KeyEventArgs e)
        {
            if (Pfad != "" && Pfad != " ")
            {
                Key k = e.Key;
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                string s = k.ToString();
                if (e.Key == Key.Right)
                {
                    if (cellText + (long)fileSlider.Value * 16 < dyfipro.Length)
                    {
                        if (cellText < 256)
                        {
                            this.cellText++;
                        }
                        else
                        {
                            if (fileSlider.Value < fileSlider.Maximum)
                            {
                                fileSlider.Value += 1;

                                this.cellText = 16 * 15 ;
                            }
                        }

                    }
                    
                    e.Handled = true;
                }

                else if (e.Key == Key.Left)
                {

                    if (cellText > 0)
                    {
                        this.cellText--;
                    }
                    else
                    {
                        if (fileSlider.Value > 0)
                        {
                            fileSlider.Value -= 1;

                            this.cellText = 16;
                        }
                    }

                    
                    e.Handled = true;
                }

                else if (e.Key == Key.Down)
                {
                    if (cellText + (long)fileSlider.Value * 16 + 15 < dyfipro.Length)
                    {
                        if (cellText < 16 * 15)
                        {
                            this.cellText += 16;
                        }
                        else
                        {
                            if (fileSlider.Value < fileSlider.Maximum)
                            {
                                fileSlider.Value += 1;
                            }
                        }


                    }
                    e.Handled = true;
                }

                else if (e.Key == Key.Up)
                {
                    if (cellText > 16)
                    { cellText -= 16; }
                    else
                    {
                        if (fileSlider.Value > 0)
                        {
                            fileSlider.Value -= 1;
                        }
                    }
                    e.Handled = true;

                    
                }

                else if (e.Key == Key.Back)
                {


                    if (mark[1] - mark[0] == 0)
                    {
                        dyfipro.DeleteBytes(cellText + (long)fileSlider.Value * 16 - 1, 1);

                        if (cellText > 0)
                        {

                            cellText--;

                        }
                        else
                        {
                            if (fileSlider.Value > 0)
                            {
                                fileSlider.Value -= 1;

                                this.cellText = 30;
                            }
                        }
                    }

                    else 
                    { 
                    
                    }

                    e.Handled = true;
                }

                

                if (e.Key == Key.PageDown)
                {
                    fileSlider.Value += 16;

                    e.Handled = true;
                }

                else if (e.Key == Key.PageUp)
                {
                    fileSlider.Value -= 16;
                    e.Handled = true;
                }

                else if (e.Key == Key.End)
                {


                    cellText = ((int)(cellText / 16) + 1) * 16 - 1;
                    e.Handled = true;
                }


                else if (e.Key == Key.Home)
                {
                    cellText = ((int)(cellText / 16)) * 16;
                    e.Handled = true;

                }


                else if (e.Key == Key.Return)
                {
                    e.Handled = true;
                }

                else if (e.Key == Key.Tab)
                {

                    cursor.Focus();
                    e.Handled = true;
                }
                /*
                e.Handled = true;*/

                setPositionText(cellText);

                Boolean releasemark = true;

   
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        releasemark = false;

                        if (mark[0] == -1 || mark[1] == -1)
                        {
                            mark[0] = cellText + (long)fileSlider.Value * 16;
                            mark[1] = cellText + (long)fileSlider.Value * 16;
                            
                        }

                        if (cellText + (long)fileSlider.Value * 16 < mark[0])
                        {
                            markedBackwards = true;
                        }

                        if (cellText + (long)fileSlider.Value * 16 > mark[1])
                        {
                            markedBackwards = false;
                        }

                        if (cellText + (long)fileSlider.Value * 16 <= mark[1] && cellText + (long)fileSlider.Value * 16 >= mark[0] &&
                            !markedBackwards)
                        {
                            mark[1] = cellText + (long)fileSlider.Value * 16;
                        }

                        if (cellText + (long)fileSlider.Value * 16 <= mark[1] && cellText + (long)fileSlider.Value * 16 >= mark[0] &&
                            markedBackwards)
                        {
                            mark[0] = cellText + (long)fileSlider.Value * 16;
                        }


                        if (cellText + (long)fileSlider.Value * 16 < mark[0])
                        {
                            mark[0] = cellText + (long)fileSlider.Value * 16;
                        }

                        if (cellText + (long)fileSlider.Value * 16 > mark[1])
                        {
                            mark[1] = cellText + (long)fileSlider.Value * 16;
                        }
                        
                        //updateUI((long)fileSlider.Value);
                        st.removemarks = false;
                    }
                if (!Keyboard.IsKeyDown(Key.LeftCtrl))
                {

                    if (releasemark)
                    {
                        mark[0] = -1;
                        mark[1] = -1;
                        st.removemarks = true;
                        //updateUI((long)fileSlider.Value);
                    }

                }


                if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.C)
                {

                    Copy_ASCIIFild();


                }

                if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.V)
                {
                    ASCIIField_TextInput_Help((String) Clipboard.GetData(DataFormats.Text));
                }

                if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.X)
                {
                    Cut_ASCIIFild();
                }

                int cell2 = (int)(Canvas.GetTop(cursor) / 20 * 32 + Canvas.GetLeft(cursor) / 10);
                Column.Text = (cell2 / 2) % 16 + "";
                Line.Text = (int)(Canvas.GetTop(cursor) / 20) + (long)fileSlider.Value + "";
            }
            else
            {
                e.Handled = true;
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                e.Handled = true;
            }

            updateUI((long)fileSlider.Value);

        }

        private void HelpExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
           
        }

        private void nextHexBoxField()
        {
            Boolean b = true;

            if (Canvas.GetLeft(cursor) < 328.7)
            {
                Canvas.SetLeft(cursor, Canvas.GetLeft(cursor) + ht.CharWidth);
                if (Canvas.GetLeft(cursor) % (ht.CharWidth * 3) > ht.CharWidth * 3 / 2)
                {
                    Canvas.SetLeft(cursor, Canvas.GetLeft(cursor) + ht.CharWidth);
                }
                b = false;
                cell++;
            }
            else if (Canvas.GetTop(cursor) < 300)
            {
                Canvas.SetTop(cursor, Canvas.GetTop(cursor) + 20);
                Canvas.SetLeft(cursor, 0);
                cell++;
            }

            else if (fileSlider.Value != fileSlider.Maximum && Canvas.GetLeft(cursor) > 320.7 && Canvas.GetTop(cursor) > 290 &&
                b)
            {
                Canvas.SetLeft(cursor, 0);
                Canvas.SetLeft(cursor2, 0);
                cell=480;
                fileSlider.Value += 1;
            }
             //working on endfile

        }

        private void nextASCIIField()
        {
            Boolean b = true;

            //if (Canvas.GetLeft(cursor) < 300)
            //{
                
            //    Canvas.SetLeft(cursor, Canvas.GetLeft(cursor) + ht.CharWidth);
            //    if (Canvas.GetLeft(cursor) % (ht.CharWidth * 3) > ht.CharWidth * 3 / 2)
            //    {
            //        Canvas.SetLeft(cursor, Canvas.GetLeft(cursor) + ht.CharWidth);
            //    }
                
            //    b = false;
            //}
            //else if (Canvas.GetTop(cursor) < 300)
            //{
            //    Canvas.SetTop(cursor, Canvas.GetTop(cursor) + 20);
            //    Canvas.SetLeft(cursor, 0);
            //}

            

            if (Canvas.GetLeft(cursor2) < 107)
            {   
                Canvas.SetLeft(cursor2, Canvas.GetLeft(cursor2) + ht.CharWidth);
                cellText++;
            }

            else if (Canvas.GetTop(cursor2) < 300)
            {
                Canvas.SetTop(cursor2, Canvas.GetTop(cursor2) + 20);
                Canvas.SetLeft(cursor2, 0);
                cellText++;
            }


            else if (fileSlider.Value != fileSlider.Maximum && Canvas.GetLeft(cursor2) >= 107 &&
                Canvas.GetTop(cursor2) >= 300)
            {
                Canvas.SetLeft(cursor, 0);
                Canvas.SetLeft(cursor2, 0);
                cellText = 240;
                fileSlider.Value += 1;

            }

            

        }

        private void backHexBoxField()
        {
            Boolean b = true;

            if (Canvas.GetLeft(cursor) > 10)
            {
                Canvas.SetLeft(cursor, Canvas.GetLeft(cursor) - ht.CharWidth*3);
                b = false;
            }
            else if (Canvas.GetTop(cursor) > 10)
            {
                Canvas.SetTop(cursor, Canvas.GetTop(cursor) - 20);
                Canvas.SetLeft(cursor, 328.7 - ht.CharWidth);
            }



            if (Canvas.GetLeft(cursor2) > 0)
                Canvas.SetLeft(cursor2, Canvas.GetLeft(cursor2) - st.CharWidth);

            else if (Canvas.GetTop(cursor2) > 0)
            {
                Canvas.SetTop(cursor2, Canvas.GetTop(cursor2) - 20);
                Canvas.SetLeft(cursor2, 150);
            }



            if (fileSlider.Value != fileSlider.Minimum && Canvas.GetLeft(cursor2) == 0 &&
                Canvas.GetTop(cursor2) == 0 && b)
            {
                Canvas.SetLeft(cursor, 328.7 - ht.CharWidth );
                Canvas.SetLeft(cursor2, 107);
                fileSlider.Value -= 1;
            }

            cell--;
            cell--;

        }

        private void backASCIIField()
        {
            Boolean b = true;

            if (Canvas.GetLeft(cursor) > 10)
            {
                Canvas.SetLeft(cursor, Canvas.GetLeft(cursor) - ht.CharWidth);
                b = false;
            }
            else if (Canvas.GetTop(cursor) > 10)
            {
                Canvas.SetTop(cursor, Canvas.GetTop(cursor) - 20);
                Canvas.SetLeft(cursor, 328.7);
            }



            if (Canvas.GetLeft(cursor2) > 0)
                Canvas.SetLeft(cursor2, Canvas.GetLeft(cursor2) - st.CharWidth);

            else if (Canvas.GetTop(cursor2) > 0)
            {
                Canvas.SetTop(cursor2, Canvas.GetTop(cursor2) - 20);
                Canvas.SetLeft(cursor2, 107);
            }



            if (fileSlider.Value != fileSlider.Minimum && Canvas.GetLeft(cursor2) == 0 &&
                Canvas.GetTop(cursor2) == 0 && b)
            {
                Canvas.SetLeft(cursor, 328.7);
                Canvas.SetLeft(cursor2, 107);
                fileSlider.Value -= 1;
            }

            cellText--;
        }

        private void ASCIIField_TextInput(object sender, TextCompositionEventArgs e) 
        {
            ASCIIField_TextInput_Help(e.Text);
            e.Handled = true;
        }
        private void ASCIIField_TextInput_Help(String e)
        {
            for (int ix = 0; ix < e.Length; ix++)
            {
                if (insertCheck.IsChecked == false)
                {
                    if (cellText + (long)fileSlider.Value * 16 < dyfipro.Length)
                    {
                        dyfipro.WriteByte(cellText + (long)fileSlider.Value * 16,
                                          Encoding.GetEncoding(1252).GetBytes((char)e[ix]+"")[0]);
                    }
                    else
                    {
                        Byte[] dummyArray = { Encoding.GetEncoding(1252).GetBytes((char)e[ix] + "")[0] };
                        dyfipro.InsertBytes(cellText + (long)fileSlider.Value * 16,
                                          dummyArray);
                    }
                }
                else
                {
                    Byte[] dummyArray = { Encoding.GetEncoding(1252).GetBytes((char)e[ix] + "")[0] };


                    dyfipro.InsertBytes(cellText + (long)fileSlider.Value * 16, dummyArray);
                }

                //nextASCIIField();

                cellText++;
                
                if (cellText == 16 * 16)
                {
                    if (fileSlider.Value < fileSlider.Maximum)
                    {
                        fileSlider.Value += 1;
                        this.cellText = 16 * 15;
                    }
                }
                
            }
            
            setPositionText(cellText);
            updateUI((long)fileSlider.Value);
           
        }

        private void HexBox_TextInput(object sender, TextCompositionEventArgs e)
        {
            HexBox_TextInput_Help(e.Text);
            e.Handled = true;
        }
        private void HexBox_TextInput_Help(String e)
        {

            String test = e.ToLower();
            test = test.Replace("a", "");
            test = test.Replace("b", "");
            test = test.Replace("c", "");
            test = test.Replace("d", "");
            test = test.Replace("e", "");
            test = test.Replace("f", "");
            test = test.Replace("1", "");
            test = test.Replace("2", "");
            test = test.Replace("3", "");
            test = test.Replace("4", "");
            test = test.Replace("5", "");
            test = test.Replace("6", "");
            test = test.Replace("7", "");
            test = test.Replace("8", "");
            test = test.Replace("9", "");
            test = test.Replace("0", "");

            if (test.Length == 0)
            {
                for (int ix = 0; ix < e.Length; ix++)
                {

                    Byte[] dummyArray = { Encoding.GetEncoding(1252).GetBytes(e)[ix] };

                    byte b = new byte();

                    string s = "  ";

                    char c = e[ix];

                    if (cell / 2 + (long)fileSlider.Value * 16 < dyfipro.Length)
                    { s = String.Format("{0:X2}", dyfipro.ReadByte(cell / 2 + (long)fileSlider.Value * 16)); }

                    if (insertCheck.IsChecked == false)
                    {
                        if (cell % 2 == 0)
                        {
                            int i = e[ix];

                            if (e[0] > 96 && e[0] < 103 || e[0] > 47 && e[0] < 58)
                            {

                                s = c + "" + (char)s[1];
                            }
                            if (cell / 2 + (long)fileSlider.Value * 16 < dyfipro.Length)
                            {
                                dyfipro.WriteByte(cell / 2 + (long)fileSlider.Value * 16,
                                                  (byte)Convert.ToInt32(s, 16));
                            }
                            else
                            {

                                s = (char)s[0] + "" + c;
                                dummyArray[0] = (byte)Convert.ToInt32(s, 16);
                                dyfipro.InsertBytes(cell / 2 + (long)fileSlider.Value * 16, dummyArray);
                            }

                        }
                        if (cell % 2 == 1)
                        {

                            if (e[0] > 96 && e[0] < 103 || e[0] > 47 && e[0] < 58)
                            {
                                s = (char)s[0] + "" + c;

                            }
                            if (cell / 2 + (long)fileSlider.Value * 16 < dyfipro.Length)
                            {
                                dyfipro.WriteByte(cell / 2 + (long)fileSlider.Value * 16,
                                                  (byte)Convert.ToInt32(s, 16));
                            }
                        }
                    }
                    else
                    {

                        if (cell % 2 == 0)
                        {
                            int i = e[0];


                            if (e[0] > 96 && e[0] < 103 || e[0] > 47 && e[0] < 58)
                            {
                                s = c + "0";
                            }
                            dummyArray[0] = (byte)Convert.ToInt32(s, 16);
                            dyfipro.InsertBytes(cell / 2 + (long)fileSlider.Value * 16, dummyArray);

                        }
                        if (cell % 2 == 1)
                        {

                            if (e[0] > 96 && e[0] < 103 || e[0] > 47 && e[0] < 58)
                            {
                                s = (char)s[0] + ""+ c ;

                            }
                            if (cell / 2 + (long)fileSlider.Value * 16 < dyfipro.Length)
                            {
                                dyfipro.WriteByte(cell / 2 + (long)fileSlider.Value * 16, (byte)Convert.ToInt32(s, 16));
                            }
                        }
                    }


                    cell++;
                    if (cell == 32 * 16)
                    {
                        if (fileSlider.Value < fileSlider.Maximum)
                        {
                            fileSlider.Value += 1;
                            this.cell = 32 * 15;
                        }
                    }


                    

                    
                }
            }
            setPosition(cell);
            updateUI((long)fileSlider.Value);
            
            
        }

        #endregion

        #region Public Methods

        public AsyncCallback callback()
        {
            return null;
        }

        public void dispose() //Disposes File See IDisposable for further information
        {
            dyfipro.Dispose();
        }

        private void updateUI(long position) // Updates UI
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            long end = dyfipro.Length - position * 16;
            int max = 256;
            
            st.Text = "";

            Byte[] help2;
            if(end<256)
            {
                help2 = new byte[end];
            }
            else
            {
                help2 = new byte[256];
            }

            

            for (int i = 0; i < help2.Count(); i++)
            {
                if (i <= max)
                {
                    if (i<end)    
                        help2[i] = dyfipro.ReadByte(i + position * 16);   
                }    
            }

            ht.ByteContent = help2;

            
            for (int j = 0; j < 16; j++)
            {
                TextBlock id = gridid.Children[j] as TextBlock;
                //id.Text = (position + j) * 16 + "";
                long s = (position + j) * 16;
                id.Text = "";
                for (int x = 8 - s.ToString("X").Length; x > 0; x--)
                {
                    id.Text += "0";
                }
                id.Text += s.ToString("X");

            }


            

            ht.mark[0] = (int)(mark[0] - position * 16) * 2;            
            ht.mark[1] = (int)(mark[1] - position * 16) * 2;

            st.mark[0] = (int)(mark[0] - position * 16);
            st.mark[1] = (int)(mark[1] - position * 16);
            

            lastUpdate = position;
        }

        private void dyfipro_LengthChanged(object sender, EventArgs e) // occures when length of file changed 
        {

            
            double old = fileSlider.Maximum;

            fileSlider.Maximum = (dyfipro.Length - 256)/16 + 1;

            
            if ((long) old > (long) fileSlider.Maximum && fileSlider.Value == fileSlider.Maximum)
            {
                if (Canvas.GetLeft(cursor2) > 10)
                {
                    Canvas.SetTop(cursor, Canvas.GetTop(cursor) + 20);
                    Canvas.SetTop(cursor2, Canvas.GetTop(cursor2) + 20);
                }
                else
                {
                    Canvas.SetLeft(cursor, 320);
                    Canvas.SetLeft(cursor2, 160);

                }
            }

            if ((long) old < (long) fileSlider.Maximum && fileSlider.Value == fileSlider.Maximum)
            {
                if (Canvas.GetLeft(cursor2) > 140)
                {
                    Canvas.SetTop(cursor, Canvas.GetTop(cursor) - 20);
                    Canvas.SetTop(cursor2, Canvas.GetTop(cursor2) - 20);
                }
                else
                {
                    Canvas.SetLeft(cursor, 0);
                    Canvas.SetLeft(cursor2, 0);

                }
            }



        }

        public void openFile(String fileName,Boolean canRead) // opens file 
        {
            
            dyfipro.Dispose();

            if (fileName != "" && fileName != " "&& File.Exists(fileName))
                {
                    FileName.Text = fileName;
                    Pfad = fileName;
                    try
                    {
                        dyfipro = new DynamicFileByteProvider(Pfad, false);
                        makeUnAccesable(true);
                    }
                    catch (IOException ioe)
                    {
                        dyfipro = new DynamicFileByteProvider(Pfad, true);
                        makeUnAccesable(false);
                    }


                    dyfipro.LengthChanged += new EventHandler(dyfipro_LengthChanged);

                    fileSlider.Minimum = 0;
                    fileSlider.Maximum = (dyfipro.Length - 256)/16 + 1;
                    fileSlider.ViewportSize = 16;

                    Info.Text = dyfipro.Length/256 + "";

                    fileSlider.ValueChanged += MyManipulationCompleteEvent;
                    fileSlider.SmallChange = 1;
                    fileSlider.LargeChange = 1;

                    updateUI(0);
                
            }
            
        }

        public void closeFile(Boolean clear) // closes file
        {
            dyfipro.Dispose();
            if(clear)
            {fillempty();}
        }

        public Boolean saveData(Boolean ask,Boolean saveas ) // saves changed data to file
        {
            try
            {
                if (dyfipro.Length != 0)
                    if (dyfipro.HasChanges() || saveas)
                    {
                        MessageBoxResult result;
                        if (ask)
                        {
                            string messageBoxText = "Do you want to save changes in a new File? (If you click no, changes will saved permenantly)";
                            string caption = "FileInput";
                            MessageBoxButton button = MessageBoxButton.YesNoCancel;
                            
                            MessageBoxImage icon = MessageBoxImage.Warning;



                            result = MessageBox.Show(messageBoxText, caption, button, icon);
                        }
                        else
                        {
                            result = MessageBoxResult.Yes;
                        }
                        // Process message box results
                        switch (result)
                        {
                            case MessageBoxResult.Yes:
                                // User pressed Yes button
                                // ...

                                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                                saveFileDialog1.Title = "Save Data";
                                saveFileDialog1.FileName = Pfad;
                                saveFileDialog1.ShowDialog();



                                // If the file name is not an empty string open it for saving.
                                if (saveFileDialog1.FileName != "")
                                {
                                    // Saves the Image via a FileStream created by the OpenFile method.

                                    if (saveFileDialog1.FileName != Pfad)
                                    {


                                        System.IO.FileStream fs = (System.IO.FileStream) saveFileDialog1.OpenFile();


                                        for (long i = 0; i < dyfipro.Length; i++)
                                        {
                                            fs.WriteByte(dyfipro.ReadByte(i));
                                        }
                                        FileName.Text = saveFileDialog1.FileName;
                                        Pfad = saveFileDialog1.FileName;
                                        fs.Close();

                                    }
                                    else
                                    {
                                        dyfipro.ApplyChanges();

                                    }
                                }
                                OnFileChanged(this, EventArgs.Empty);
                                break;
                            case MessageBoxResult.No:
                                dyfipro.ApplyChanges();
                                break;
                            case MessageBoxResult.Cancel:
                                // User pressed Cancel button
                                // ...
                                break;
                        }

                    }
            }
            catch(Exception e)
            {

            }

            return true;
        }

        public void fillempty() // clears data in HexBox
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            
            int max = 256;

            
            for (int j = 0; j < 16; j++)
            {
                TextBlock id = gridid.Children[j] as TextBlock;
                //id.Text = (position + j) * 16 + "";
                long s = j * 16;
                id.Text = s.ToString("X");

            }

            for (int i = 0; i < 256; i++)
            {
                TextBlock tb = grid1.Children[i] as TextBlock;
                
                tb.Text = String.Format("{0:X2}", "");
                
                

                tb.Background = Brushes.Transparent;
                
            }

            for (int i = 0; i < 256; i++)
            {
                TextBlock tb = grid2.Children[i] as TextBlock;
                
                    tb.Text = "";

            }
        }

        public void collapseControl(Boolean b) // changes visibility of user controls, when HexBox is nor visible
        {
            grid1.IsEnabled = b;
            grid2.IsEnabled = b;
            
            if (b)
            {
                cursor.Visibility = Visibility.Visible;
                cursor2.Visibility = Visibility.Visible;
                saveAs.Visibility = Visibility.Visible;
                save.Visibility = Visibility.Visible;
                newFile.Visibility = Visibility.Visible;
                openFileButton.Visibility = Visibility.Visible;
            }
            else
            {
                cursor.Visibility = Visibility.Collapsed;
                cursor2.Visibility = Visibility.Collapsed;
                saveAs.Visibility = Visibility.Collapsed;
                save.Visibility = Visibility.Collapsed;
                newFile.Visibility = Visibility.Collapsed;
                openFileButton.Visibility = Visibility.Collapsed;

            }

        }

        public void makeUnAccesable(Boolean b) // allows or doesn't allows manipulation of data
        {
            
            grid1.IsEnabled = b;
            grid2.IsEnabled = b;
            saveAs.IsEnabled = b;
            save.IsEnabled = b;
            if (b)
            {
                cursor.Visibility = Visibility.Visible;
                cursor2.Visibility = Visibility.Visible;
            }
            else
            {
                cursor.Visibility = Visibility.Collapsed;
                cursor2.Visibility = Visibility.Collapsed;
            }

        }

        #endregion

        #region Buttons

        private void Open_Button_Click(object sender, RoutedEventArgs e)
        {
            dyfipro.Dispose();


            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                //openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog().Value)
                {
                    Pfad = openFileDialog.FileName;
                }
                if (Pfad != "" && File.Exists(Pfad))
                {
                    FileName.Text = Pfad;
                    try
                    {
                        dyfipro = new DynamicFileByteProvider(Pfad, false);
                        makeUnAccesable(true);

                    }
                    catch (IOException ioe)
                    {
                        dyfipro = new DynamicFileByteProvider(Pfad, true);
                        makeUnAccesable(false);
                    }

                    dyfipro.LengthChanged += new EventHandler(dyfipro_LengthChanged);



                    fileSlider.Minimum = 0;
                    fileSlider.Maximum = (dyfipro.Length - 256) / 16 + 1;
                    fileSlider.ViewportSize = 16;



                    Info.Text = dyfipro.Length / 256 + "";


                    fileSlider.ValueChanged += MyManipulationCompleteEvent;
                    fileSlider.SmallChange = 1;
                    fileSlider.LargeChange = 1;





                    updateUI(0);

                    OnFileChanged(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                //TODO: GuiLogMessage here
            }
            
            
        }

        private void Paste_Click_HexBoxField(object sender, RoutedEventArgs e)
        {
            //Question: what could be pasted into the data Block
        }

        private void Copy_Click_HexBoxField(object sender, RoutedEventArgs e)
        {
            Copy_HexBoxField();
        }

        private void help_copy_Hexbox() 
        {
            StringBuilder clipBoardString = new StringBuilder();


            for (long i = mark[0]; i <= mark[1]; i++)
            {
                clipBoardString.Append(String.Format("{0:X2}", dyfipro.ReadByte(i)));   
            }
            

            try
            {
                Clipboard.SetText(clipBoardString.ToString());
            }
            catch (Exception exp)
            {

            }
        
        }

        private void Copy_HexBoxField()
        {
            help_copy_Hexbox();

            mark[1] = -1;
            mark[0] = -1;

            ht.removemarks = true;
            updateUI((long)fileSlider.Value);
        }

        private void Cut_Click_HexBoxField(object sender, RoutedEventArgs e)
        {
            Cut_HexBoxField();
        }

        private void Cut_HexBoxField()
        {
            help_copy_Hexbox();

            int celltemp = cell;

            if (celltemp / 2 + (long)fileSlider.Value * 16 - 2 < dyfipro.Length)
            {
                TextBlock tb = grid1.Children[celltemp / 2] as TextBlock;
                TextBlock tb2 = grid2.Children[celltemp / 2] as TextBlock;

                if (mark[1] - mark[0] == 0)
                {
                    if (celltemp / 2 + (int)fileSlider.Value * 16 - 1 > -1)
                    {

                        dyfipro.DeleteBytes(celltemp / 2 + (long)fileSlider.Value * 16 , 1);
                        
                    }
                }
                else
                {
                    if (markedBackwards)
                    {
                        // if (cell / 2 + (int)fileSlider.Value * 16 - 1 > -1)
                        dyfipro.DeleteBytes(mark[0] , mark[1] - mark[0]+1);

                        if (fileSlider.Value == fileSlider.Maximum)
                        {
                            Canvas.SetTop(cursor2, (mark[0] / 16 - fileSlider.Value) * 20);
                            Canvas.SetTop(cursor, (mark[0] / 16 - fileSlider.Value) * 20);
                        }
                    }
                    else
                    {
                        // if (cell / 2 + (int)fileSlider.Value * 16 - 1 > -1)
                        dyfipro.DeleteBytes(mark[0], mark[1] - mark[0]+1);

                        if (mark[1] - mark[0] > celltemp)
                        {
                            fileSlider.Value = mark[0] / 16;

                            Canvas.SetTop(cursor2, 0);
                            Canvas.SetTop(cursor, 0);

                        }

                        else
                        {

                            Canvas.SetTop(cursor2, (mark[0] / 16 - fileSlider.Value) * 20);
                            Canvas.SetTop(cursor, (mark[0] / 16 - fileSlider.Value) * 20);

                        }

                        Canvas.SetLeft(cursor2, mark[0] % 16 * 10);
                        Canvas.SetLeft(cursor, mark[0] % 16 * 20);

                    }
                }

                mark[1] = -1;
                mark[0] = -1;

                ht.removemarks = true;

                updateUI((long)fileSlider.Value);
            }

            
        }

        private void Copy_Click_ASCIIFild(object sender, RoutedEventArgs e)
        {
            Copy_ASCIIFild();
        }

        private void Copy_ASCIIFild()
        {
            StringBuilder clipBoardString = new StringBuilder();
            for (long i = mark[0]; i < mark[1]; i++)
            {
                if (dyfipro.ReadByte(i) > 34 && dyfipro.ReadByte(i) < 128)
                {
                    clipBoardString.Append((char)dyfipro.ReadByte(i));
                }
                else
                {
                    clipBoardString.Append('.');
                }
            }

            try
            {
                Clipboard.SetText(clipBoardString.ToString());
            } 
            catch(Exception exp)
            {

            }
            mark[1] = -1;
            mark[0] = -1;

            updateUI((long)fileSlider.Value);

        }

        private void Paste_Click_ASCIIFild(object sender, RoutedEventArgs e)
        {
            Paste_ASCIIFild();
        }

        private void Paste_ASCIIFild()
        {
            

            if (cellText + (int) fileSlider.Value*16  < dyfipro.Length)
            {
                TextBlock tb = grid1.Children[cellText] as TextBlock;
                TextBlock tb2 = grid2.Children[cellText] as TextBlock;



                if (mark[1] - mark[0] == 0)
                {


                }
                else
                {
                    if (markedBackwards)
                    {
                        // if (cell / 2 + (int)fileSlider.Value * 16 - 1 > -1)
                        dyfipro.DeleteBytes(mark[0] + 1, mark[1] - mark[0]);
                    }
                    else
                    {
                        // if (cell / 2 + (int)fileSlider.Value * 16 - 1 > -1)
                        dyfipro.DeleteBytes(mark[0], mark[1] - mark[0]);
                        if (mark[1] - mark[0] > cell)
                        {
                            fileSlider.Value = mark[0]/16;

                            Canvas.SetTop(cursor2, 0);
                            Canvas.SetTop(cursor, 0);

                        }

                        else
                        {

                            Canvas.SetTop(cursor2, (mark[0]/16 - fileSlider.Value)*20);
                            Canvas.SetTop(cursor, (mark[0]/16 - fileSlider.Value)*20);

                        }

                        Canvas.SetLeft(cursor2, mark[0]%16*10);
                        Canvas.SetLeft(cursor, mark[0]%16*20);

                    }
                }


                mark[1] = -1;
                mark[0] = -1;
                updateUI((long) fileSlider.Value);
            }

            if (markedBackwards)
                cellText = (int) (Canvas.GetTop(cursor)/20*16 + Canvas.GetLeft(cursor)/10 + 2);
            else
                cellText = (int) (Canvas.GetTop(cursor)/20*16 + Canvas.GetLeft(cursor)/10);

            String text = (String) Clipboard.GetData(DataFormats.Text);
            
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            dyfipro.InsertBytes(cellText + (int) fileSlider.Value*16, enc.GetBytes(text));
            
            //mark[0] = cell / 2 + (int)fileSlider.Value * 16 ;
            //mark[1] = cell/2 + (int) fileSlider.Value*16 + text.Length;
            //markedBackwards = false;

            //if (mark[1] > fileSlider.Value * 16)
            //{
            //    
            //}
            
            //Canvas.SetLeft(cursor2, mark[1] % 16 * 10);
            //Canvas.SetLeft(cursor, mark[1] % 16 * 20);

            //Canvas.SetTop(cursor2, mark[1] % 16*10);
            //Canvas.SetTop(cursor, mark[1] % 16 * 10);

            //fileSlider.Value = mark[1] / 16;
            //mark[1] = -1;

            //mark[1] = -1;

            updateUI((long)fileSlider.Value);

        }

        private void Cut_Click_ASCIIFild(object sender, RoutedEventArgs e)
        {
            Cut_ASCIIFild();
        }

        private void Cut_ASCIIFild()
        {
            StringBuilder clipBoardString = new StringBuilder();
            for (long i = mark[0]; i < mark[1]; i++)
            {
                if (dyfipro.ReadByte(i) > 34 && dyfipro.ReadByte(i) < 128)
                {
                    clipBoardString.Append((char)dyfipro.ReadByte(i));
                }
                else 
                {
                    clipBoardString.Append('.');
                }
            }

            try
            {
                Clipboard.SetText(clipBoardString.ToString());
            }
            catch (Exception exp)
            {

            }

            if (cellText + (int)fileSlider.Value * 16 - 2 < dyfipro.Length)
            {
                if (mark[1] - mark[0] == 0)
                {
                    if (cellText + (int)fileSlider.Value * 16 - 1 > -1)
                    {
                        dyfipro.DeleteBytes(cellText + (long)fileSlider.Value * 16 - 1, 1);
                        backASCIIField();
                    }

                }
                else
                {
                    if (markedBackwards)
                    {
                        // if (cell / 2 + (int)fileSlider.Value * 16 - 1 > -1)
                        dyfipro.DeleteBytes(mark[0] + 1, mark[1] - mark[0]);
                    }
                    else
                    {
                        // if (cell / 2 + (int)fileSlider.Value * 16 - 1 > -1)
                        dyfipro.DeleteBytes(mark[0], mark[1] - mark[0]);
                        if (mark[1] - mark[0] > cellText)
                        {
                            fileSlider.Value = mark[0] / 16;

                            Canvas.SetTop(cursor2, 0);
                            Canvas.SetTop(cursor, 0);

                        }

                        else
                        {

                            Canvas.SetTop(cursor2, (mark[0] / 16 - fileSlider.Value) * 20);
                            Canvas.SetTop(cursor, (mark[0] / 16 - fileSlider.Value) * 20);

                        }

                        Canvas.SetLeft(cursor2, mark[0] % 16 * 10);
                        Canvas.SetLeft(cursor, mark[0] % 16 * 20);

                    }
                }


                mark[1] = -1;
                mark[0] = -1;
                updateUI((long)fileSlider.Value);
            }
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            dyfipro.ApplyChanges();
        }

        private void Save_As_Button_Click(object sender, RoutedEventArgs e)
        {
            saveData(false,true);
        }

        public event EventHandler OnFileChanged;

        private void New_Button_Click(object sender, RoutedEventArgs e)
        {
            
            dyfipro.Dispose();
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Title = "Save Data";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method.
                System.IO.FileStream fs = (System.IO.FileStream) saveFileDialog1.OpenFile();

                fs.Dispose();
                fs.Close();

                Pfad = saveFileDialog1.FileName;


                FileName.Text = Pfad;

                dyfipro = new DynamicFileByteProvider(Pfad, false);


                dyfipro.LengthChanged += new EventHandler(dyfipro_LengthChanged);



                fileSlider.Minimum = 0;
                fileSlider.Maximum = (dyfipro.Length - 256)/16 + 1;
                fileSlider.ViewportSize = 16;



                Info.Text = dyfipro.Length/256 + "";


                fileSlider.ValueChanged += MyManipulationCompleteEvent;
                fileSlider.SmallChange = 1;
                fileSlider.LargeChange = 1;



                OnFileChanged(this, EventArgs.Empty);

                updateUI(0);
            }

        }

        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {

            fileSlider.Value -=  e.Delta/10;


        }

        private void MyManipulationCompleteEvent(object sender, EventArgs e)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            if (lastUpdate != fileSlider.Value)
            {
                updateUI((long)fileSlider.Value);
            }

            Info.Text = (long)fileSlider.Value + "" + Math.Round(fileSlider.Value * 16, 0) + fileSlider.Value;

        }

        #endregion

        #region not needed anymore
        private void tb_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                TextBlock tb = sender as TextBlock;
                int cell = (int)(Grid.GetRow(tb) * 16 + Grid.GetColumn(tb));

                if (mark[0] > cell + (long)fileSlider.Value * 16)
                {
                    mark[1] = cell + (long)fileSlider.Value * 16;
                    markedBackwards = true;
                }

                else
                {
                    mark[1] = cell + (long)fileSlider.Value * 16;
                }

                updateUI((long)fileSlider.Value);
            }
        }

        private void tb_MouseDown(object sender, MouseButtonEventArgs e)
        {


            TextBlock tb = sender as TextBlock;


            Canvas.SetLeft(cursor, Grid.GetColumn(tb) * 20);
            Canvas.SetLeft(cursor2, Grid.GetColumn(tb) * 10);


            Canvas.SetTop(cursor, Grid.GetRow(tb) * 20);
            Canvas.SetTop(cursor2, Grid.GetRow(tb) * 20);

            int cell = (int)(Grid.GetRow(tb) * 16 + Grid.GetColumn(tb));

            mark[0] = cell + (long)fileSlider.Value * 16;


            Column.Text = Grid.GetColumn(tb) + "";
            Line.Text = Grid.GetRow(tb) + "";

            cursor.Focus();
        }

        private void tb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock tb = sender as TextBlock;

            Canvas.SetLeft(cursor, Grid.GetColumn(tb) * 20);
            Canvas.SetLeft(cursor2, Grid.GetColumn(tb) * 10);


            Canvas.SetTop(cursor, Grid.GetRow(tb) * 20);
            Canvas.SetTop(cursor2, Grid.GetRow(tb) * 20);

            int cell = (int)(Grid.GetRow(tb) * 16 + Grid.GetColumn(tb));

            mark[1] = cell + (long)fileSlider.Value * 16;

            markedBackwards = false;

            if (mark[1] < mark[0])
            {
                long help = mark[0];
                mark[0] = mark[1];
                mark[1] = help;

                markedBackwards = true;

            }

            Column.Text = Grid.GetColumn(tb) + "";
            Line.Text = Grid.GetRow(tb) + "";

            updateUI((long)fileSlider.Value);

            if (mark[0] > dyfipro.Length)
            {

                Canvas.SetLeft(cursor, Grid.GetColumn(grid2.Children[(int)(cell / 2 - (cell / 2 + (long)fileSlider.Value * 16 - dyfipro.Length))]) * 20);
                Canvas.SetLeft(cursor2, Grid.GetColumn(grid2.Children[(int)(cell / 2 - (cell / 2 + (long)fileSlider.Value * 16 - dyfipro.Length))]) * 10);


                Canvas.SetTop(cursor, Grid.GetRow(grid2.Children[(int)(cell / 2 - (cell / 2 + (long)fileSlider.Value * 16 - dyfipro.Length))]) * 20);
                Canvas.SetTop(cursor2, Grid.GetRow(grid2.Children[(int)(cell / 2 - (cell / 2 + (long)fileSlider.Value * 16 - dyfipro.Length))]) * 20);
            }

        }

        private void tb2_MouseDown(object sender, MouseButtonEventArgs e)
        {

            TextBlock tb = sender as TextBlock;

            Canvas.SetLeft(cursor, Grid.GetColumn(tb) * 20);
            Canvas.SetLeft(cursor2, Grid.GetColumn(tb) * 10);


            Canvas.SetTop(cursor, Grid.GetRow(tb) * 20);
            Canvas.SetTop(cursor2, Grid.GetRow(tb) * 20);

            int cell = (int)(Grid.GetRow(tb) * 16 + Grid.GetColumn(tb));

            mark[0] = cell + (long)fileSlider.Value * 16;

            Column.Text = Grid.GetColumn(tb) + "";
            Line.Text = Grid.GetRow(tb) + "";

            cursor2.Focus();


        }

        private void tb2_MouseUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock tb = sender as TextBlock;

            Canvas.SetLeft(cursor, Grid.GetColumn(tb) * 20);
            Canvas.SetLeft(cursor2, Grid.GetColumn(tb) * 10);


            Canvas.SetTop(cursor, Grid.GetRow(tb) * 20);
            Canvas.SetTop(cursor2, Grid.GetRow(tb) * 20);

            int cell = (int)(Grid.GetRow(tb) * 16 + Grid.GetColumn(tb));

            mark[1] = cell + (long)fileSlider.Value * 16;

            markedBackwards = false;

            if (mark[1] < mark[0])
            {
                long help = mark[0];
                mark[0] = mark[1];
                mark[1] = help;

                markedBackwards = true;

            }

            Column.Text = Grid.GetColumn(tb) + "";
            Line.Text = Grid.GetRow(tb) + "";

            updateUI((long)fileSlider.Value);

            if (mark[0] > dyfipro.Length)
            {

                Canvas.SetLeft(cursor, Grid.GetColumn(grid2.Children[(int)(cell / 2 - (cell / 2 + (long)fileSlider.Value * 16 - dyfipro.Length))]) * 20);
                Canvas.SetLeft(cursor2, Grid.GetColumn(grid2.Children[(int)(cell / 2 - (cell / 2 + (long)fileSlider.Value * 16 - dyfipro.Length))]) * 10);


                Canvas.SetTop(cursor, Grid.GetRow(grid2.Children[(int)(cell / 2 - (cell / 2 + (long)fileSlider.Value * 16 - dyfipro.Length))]) * 20);
                Canvas.SetTop(cursor2, Grid.GetRow(grid2.Children[(int)(cell / 2 - (cell / 2 + (long)fileSlider.Value * 16 - dyfipro.Length))]) * 20);
            }

        }
        #endregion
    }


}

 




