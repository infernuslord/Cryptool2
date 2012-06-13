﻿using System;
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

        #endregion

        #region Properties

        public Boolean inReadOnlyMode = false;
        public string Pfad = string.Empty;
        
        #endregion

        #region Constructor

        public HexBox()
        {
            InitializeComponent();

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
                    tb.MouseLeftButtonDown += new MouseButtonEventHandler(tb_MouseDown);
                    tb.MouseLeftButtonUp += new MouseButtonEventHandler(tb_MouseUp);

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
                    tb2.MouseLeftButtonDown += new MouseButtonEventHandler(tb2_MouseDown);
                    tb2.MouseLeftButtonUp += new MouseButtonEventHandler(tb2_MouseUp);
                    tb2.MouseMove += tb_MouseMove;

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

            cursor2.TextInput += MyControl_TextInput;

            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            Stream s = new MemoryStream();

            dyfipro = new DynamicFileByteProvider(s);

            dyfipro.LengthChanged += new EventHandler(dyfipro_LengthChanged);

        }

        #endregion

        #region Mouse interaction and events

        private void tb_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                TextBlock tb = sender as TextBlock;
                int cell = (int) (Grid.GetRow(tb)*16 + Grid.GetColumn(tb));

                if (mark[0] > cell + (long) fileSlider.Value*16)
                    {
                        mark[1] = cell + (long) fileSlider.Value*16;
                        markedBackwards = true;
                    }
            
                else
                    {
                        mark[1] = cell + (long) fileSlider.Value*16;
                    }
              
                updateUI((long)fileSlider.Value);
            }
        }

        private void tb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock tb = sender as TextBlock;

            Canvas.SetLeft(cursor, Grid.GetColumn(tb)*20);
            Canvas.SetLeft(cursor2, Grid.GetColumn(tb)*10);


            Canvas.SetTop(cursor, Grid.GetRow(tb)*20);
            Canvas.SetTop(cursor2, Grid.GetRow(tb)*20);

            int cell = (int) (Grid.GetRow(tb)*16 + Grid.GetColumn(tb));

            mark[0] = cell + (long) fileSlider.Value*16;


            Column.Text = Grid.GetColumn(tb) + "";
            Line.Text = Grid.GetRow(tb) + "";

            cursor.Focus();
        }

        private void tb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock tb = sender as TextBlock;

            Canvas.SetLeft(cursor, Grid.GetColumn(tb)*20);
            Canvas.SetLeft(cursor2, Grid.GetColumn(tb)*10);


            Canvas.SetTop(cursor, Grid.GetRow(tb)*20);
            Canvas.SetTop(cursor2, Grid.GetRow(tb)*20);

            int cell = (int) (Grid.GetRow(tb)*16 + Grid.GetColumn(tb));

            mark[1] = cell + (long) fileSlider.Value*16;

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

            updateUI((long) fileSlider.Value);
            
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

            Canvas.SetLeft(cursor, Grid.GetColumn(tb)*20);
            Canvas.SetLeft(cursor2, Grid.GetColumn(tb)*10);


            Canvas.SetTop(cursor, Grid.GetRow(tb)*20);
            Canvas.SetTop(cursor2, Grid.GetRow(tb)*20);

            int cell = (int) (Grid.GetRow(tb)*16 + Grid.GetColumn(tb));

            mark[0] = cell + (long) fileSlider.Value*16;

            Column.Text = Grid.GetColumn(tb) + "";
            Line.Text = Grid.GetRow(tb) + "";

            cursor2.Focus();


        }

        private void tb2_MouseUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock tb = sender as TextBlock;

            Canvas.SetLeft(cursor, Grid.GetColumn(tb)*20);
            Canvas.SetLeft(cursor2, Grid.GetColumn(tb)*10);


            Canvas.SetTop(cursor, Grid.GetRow(tb)*20);
            Canvas.SetTop(cursor2, Grid.GetRow(tb)*20);

            int cell = (int) (Grid.GetRow(tb)*16 + Grid.GetColumn(tb));

            mark[1] = cell + (long) fileSlider.Value*16;

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

            updateUI((long) fileSlider.Value);

            if (mark[0] > dyfipro.Length)
            {

                Canvas.SetLeft(cursor, Grid.GetColumn(grid2.Children[(int)(cell / 2 - (cell / 2 + (long)fileSlider.Value * 16 - dyfipro.Length))]) * 20);
                Canvas.SetLeft(cursor2, Grid.GetColumn(grid2.Children[(int)(cell / 2 - (cell / 2 + (long)fileSlider.Value * 16 - dyfipro.Length))]) * 10);


                Canvas.SetTop(cursor, Grid.GetRow(grid2.Children[(int)(cell / 2 - (cell / 2 + (long)fileSlider.Value * 16 - dyfipro.Length))]) * 20);
                Canvas.SetTop(cursor2, Grid.GetRow(grid2.Children[(int)(cell / 2 - (cell / 2 + (long)fileSlider.Value * 16 - dyfipro.Length))]) * 20);
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
                    int cell = (int)(Canvas.GetTop(cursor) / 20 * 32 + Canvas.GetLeft(cursor) / 10);
                    if (cell / 2 + (long)fileSlider.Value * 16 < dyfipro.Length)
                    {
                        Boolean b = true;
                        if (Canvas.GetLeft(cursor) < 310)
                        {
                            Canvas.SetLeft(cursor, Canvas.GetLeft(cursor) + 10);
                            b = false;
                        }
                        else if (Canvas.GetTop(cursor) < 300)
                        {
                            Canvas.SetTop(cursor, Canvas.GetTop(cursor) + 20);
                            Canvas.SetLeft(cursor, 0);

                        }


                        if (Canvas.GetLeft(cursor) / 10 % 2 == 0)
                        {

                            if (Canvas.GetLeft(cursor2) < 150)
                                Canvas.SetLeft(cursor2, Canvas.GetLeft(cursor2) + 10);

                            else if (Canvas.GetTop(cursor2) < 300)
                            {
                                Canvas.SetTop(cursor2, Canvas.GetTop(cursor2) + 20);
                                Canvas.SetLeft(cursor2, 0);
                            }
                        }

                        if (fileSlider.Value != fileSlider.Maximum && Canvas.GetLeft(cursor) == 310 &&
                            Canvas.GetTop(cursor) == 300 && b)
                        {
                            Canvas.SetLeft(cursor, 0);
                            Canvas.SetLeft(cursor2, 0);
                            fileSlider.Value += 1;
                        }
                    }
                    e.Handled = true;
                }

                else if (e.Key == Key.Left)
                {
                    Boolean b = true;
                    if (Canvas.GetLeft(cursor) > 0)
                    {
                        Canvas.SetLeft(cursor, Canvas.GetLeft(cursor) - 10);
                        b = false;
                    }
                    else if (Canvas.GetTop(cursor) > 0)
                    {
                        Canvas.SetTop(cursor, Canvas.GetTop(cursor) - 20);
                        Canvas.SetLeft(cursor, 310);
                    }



                    if (Canvas.GetLeft(cursor) / 10 % 2 == 1)
                    {

                        if (Canvas.GetLeft(cursor2) > 0)
                            Canvas.SetLeft(cursor2, Canvas.GetLeft(cursor2) - 10);

                        else if (Canvas.GetTop(cursor2) > 0)
                        {
                            Canvas.SetTop(cursor2, Canvas.GetTop(cursor2) - 20);
                            Canvas.SetLeft(cursor2, 150);
                        }
                    }

                    if (fileSlider.Value != fileSlider.Minimum && Canvas.GetLeft(cursor) == 0 &&
                        Canvas.GetTop(cursor) == 0 &&
                        b)
                    {
                        Canvas.SetLeft(cursor, 310);
                        Canvas.SetLeft(cursor2, 150);
                        fileSlider.Value -= 1;
                    }
                    e.Handled = true;
                }

                else if (e.Key == Key.Down)
                {
                    int cell = (int)(Canvas.GetTop(cursor) / 20 * 32 + Canvas.GetLeft(cursor) / 10);
                    if (cell / 2 + (long)fileSlider.Value * 16 + 15 < dyfipro.Length)
                    {
                        if (Canvas.GetTop(cursor2) > 290)
                            fileSlider.Value += 1;
                        if (Canvas.GetTop(cursor) < 300)
                            Canvas.SetTop(cursor, Canvas.GetTop(cursor) + 20);
                        if (Canvas.GetTop(cursor2) < 300)
                            Canvas.SetTop(cursor2, Canvas.GetTop(cursor2) + 20);
                    }
                    e.Handled = true;
                }

                else if (e.Key == Key.Up)
                {
                    if (Canvas.GetTop(cursor2) == 0)
                        fileSlider.Value -= 1;
                    if (Canvas.GetTop(cursor) > 0)
                        Canvas.SetTop(cursor, Canvas.GetTop(cursor) - 20);
                    if (Canvas.GetTop(cursor2) > 0)
                        Canvas.SetTop(cursor2, Canvas.GetTop(cursor2) - 20);
                    e.Handled = true;
                }

                else if (e.Key == Key.Back)
                {


                    int cell = (int)(Canvas.GetTop(cursor) / 20 * 32 + Canvas.GetLeft(cursor) / 10);
                    if (cell / 2 + (long)fileSlider.Value * 16 - 2 < dyfipro.Length)
                    {
                        TextBlock tb = grid1.Children[cell / 2] as TextBlock;
                        TextBlock tb2 = grid2.Children[cell / 2] as TextBlock;

                        if (mark[1] - mark[0] == 0)
                        {
                            if (cell / 2 + (int)fileSlider.Value * 16 - 1 > -1)
                            {
                                dyfipro.DeleteBytes(cell / 2 + (long)fileSlider.Value * 16 - 1, 1);
                                backHexBoxField();
                            }
                        }
                        else
                        {
                            if (markedBackwards)
                            {
                                // if (cell / 2 + (int)fileSlider.Value * 16 - 1 > -1)
                                dyfipro.DeleteBytes(mark[0] + 1, mark[1] - mark[0]);

                                if (fileSlider.Value == fileSlider.Maximum)
                                {
                                    Canvas.SetTop(cursor2, (mark[0] / 16 - fileSlider.Value) * 20);
                                    Canvas.SetTop(cursor, (mark[0] / 16 - fileSlider.Value) * 20);
                                }
                            }
                            else
                            {
                                // if (cell / 2 + (int)fileSlider.Value * 16 - 1 > -1)
                                dyfipro.DeleteBytes(mark[0], mark[1] - mark[0]);

                                if (mark[1] - mark[0] > cell)
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



                        //fill((long) fileSlider.Value);
                    }
                    e.Handled = true;

                }

                else if (e.Key == Key.PageDown)
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
                    Canvas.SetLeft(cursor2, 150);
                    Canvas.SetLeft(cursor, 300);
                    e.Handled = true;
                }

                else if (e.Key == Key.Home)
                {
                    Canvas.SetLeft(cursor2, 0);
                    Canvas.SetLeft(cursor, 0);
                    e.Handled = true;

                }


                else if (e.Key == Key.Return)
                {
                    e.Handled = true;
                }


                else if (e.Key == Key.A || e.Key == Key.B || e.Key == Key.C && !Keyboard.IsKeyDown(Key.LeftCtrl) ||
                         e.Key == Key.D || e.Key == Key.E ||
                         e.Key == Key.F)
                {
                    int cell = (int)(Canvas.GetTop(cursor) / 20 * 32 + Canvas.GetLeft(cursor) / 10);
                    if (cell / 2 + (long)fileSlider.Value * 16 < dyfipro.Length)
                    {
                        TextBlock tb = grid1.Children[cell / 2] as TextBlock;



                        if (cell % 2 == 0)
                        {
                            if (insertCheck.IsChecked == true)
                            {
                                tb.Text = k.ToString().ToUpper() + tb.Text[1];
                            }
                            else
                            {
                                tb.Text = k.ToString().ToUpper() + "0";
                            }

                        }
                        if (cell % 2 == 1)
                        {
                            tb.Text = tb.Text[0] + k.ToString().ToUpper();

                        }


                        TextBlock tb2 = grid2.Children[cell / 2] as TextBlock;
                        tb2.Text = (char)Convert.ToInt32(tb.Text, 16) + "";

                        if (insertCheck.IsChecked == true)
                        {
                            dyfipro.WriteByte(cell / 2 + (long)fileSlider.Value * 16, (byte)Convert.ToInt32(tb.Text, 16));
                        }

                        else
                        {
                            if (cell % 2 == 1)
                            {
                                dyfipro.WriteByte(cell / 2 + (long)fileSlider.Value * 16,
                                                  (byte)Convert.ToInt32(tb.Text, 16));
                            }


                            else
                            {
                                Byte[] dummyArray = { (byte)Convert.ToInt32(tb.Text, 16) };


                                dyfipro.InsertBytes(cell / 2 + (long)fileSlider.Value * 16, dummyArray);
                            }
                        }
                        nextHexBoxField();
                    }

                    else if ((long)(cell / 2 + (long)fileSlider.Value * 16) == dyfipro.Length)
                    {
                        TextBlock tb = grid1.Children[cell / 2] as TextBlock;

                        if (tb.Text == "")
                        {
                            tb.Text = "00";
                        }

                        if (cell % 2 == 0)
                        {
                            tb.Text = k.ToString().ToUpper() + tb.Text[1];

                        }
                        if (cell % 2 == 1)
                        {
                            tb.Text = tb.Text[0] + k.ToString().ToUpper();

                        }


                        TextBlock tb2 = grid2.Children[cell / 2] as TextBlock;
                        tb2.Text = (char)Convert.ToInt32(tb.Text, 16) + "";
                        Byte[] bytes = new Byte[1];
                        bytes[0] = (byte)Convert.ToInt32(tb.Text, 16);

                        if (cell % 2 == 1)
                        {
                            dyfipro.WriteByte(cell / 2 + (long)fileSlider.Value * 16, (byte)Convert.ToInt32(tb.Text, 16));
                        }


                        else
                        {
                            Byte[] dummyArray = { (byte)Convert.ToInt32(tb.Text, 16) };


                            dyfipro.InsertBytes(cell / 2 + (long)fileSlider.Value * 16, dummyArray);
                        }




                        nextHexBoxField();
                    }

                    e.Handled = true;
                }

                else if (e.Key == Key.D0 || e.Key == Key.D1 || e.Key == Key.D2 || e.Key == Key.D3 || e.Key == Key.D4 ||
                         e.Key == Key.D5 || e.Key == Key.D6 || e.Key == Key.D7 || e.Key == Key.D8 || e.Key == Key.D9)
                {
                    int cell = (int)(Canvas.GetTop(cursor) / 20 * 32 + Canvas.GetLeft(cursor) / 10);
                    if (cell / 2 + (long)fileSlider.Value * 16 < dyfipro.Length)
                    {
                        TextBlock tb = grid1.Children[cell / 2] as TextBlock;
                        TextBlock tb2 = grid2.Children[cell / 2] as TextBlock;

                        Byte[] stringBytes = enc.GetBytes("");
                        if (cell % 2 == 0)
                        {
                            if (insertCheck.IsChecked == false)
                            {
                                tb.Text = k.ToString().Remove(0, 1) + tb.Text[1];
                            }
                            else
                            {
                                tb.Text = k.ToString().Remove(0, 1) + "0";
                            }

                        }
                        if (cell % 2 == 1)
                        {
                            tb.Text = tb.Text[0] + k.ToString().Remove(0, 1);

                        }

                        tb2.Text = (char)Convert.ToInt32(tb.Text, 16) + "";


                        if (insertCheck.IsChecked == false)
                        {
                            dyfipro.WriteByte(cell / 2 + (long)fileSlider.Value * 16, (byte)Convert.ToInt32(tb.Text, 16));
                        }

                        else
                        {

                            if (cell % 2 == 1)
                            {
                                dyfipro.WriteByte(cell / 2 + (long)fileSlider.Value * 16,
                                                  (byte)Convert.ToInt32(tb.Text, 16));
                            }


                            else
                            {
                                Byte[] dummyArray = { (byte)Convert.ToInt32(tb.Text, 16) };


                                dyfipro.InsertBytes(cell / 2 + (long)fileSlider.Value * 16, dummyArray);
                            }

                        }
                        nextHexBoxField();
                    }

                    else if ((long)(cell / 2 + (long)fileSlider.Value * 16) == dyfipro.Length)
                    {
                        TextBlock tb = grid1.Children[cell / 2] as TextBlock;
                        TextBlock tb2 = grid2.Children[cell / 2] as TextBlock;

                        Byte[] stringBytes = enc.GetBytes("");



                        if (cell % 2 == 0)
                        {
                            if (insertCheck.IsChecked == false)
                            {
                                tb.Text = k.ToString().Remove(0, 1) + tb.Text[1];
                            }
                            else
                            {
                                tb.Text = k.ToString().Remove(0, 1) + "0";
                            }

                        }
                        if (cell % 2 == 1)
                        {
                            tb.Text = tb.Text[0] + k.ToString().Remove(0, 1);

                        }

                        tb2.Text = (char)Convert.ToInt32(tb.Text, 16) + "";
                        Byte[] bytes = new Byte[1];
                        bytes[0] = (Byte)Convert.ToInt32(tb.Text, 16);

                        if (insertCheck.IsChecked == false)
                        {
                            dyfipro.WriteByte(cell / 2 + (long)fileSlider.Value * 16, (byte)Convert.ToInt32(tb.Text, 16));
                        }

                        else
                        {


                            if (cell % 2 == 1)
                            {
                                dyfipro.WriteByte(cell / 2 + (long)fileSlider.Value * 16,
                                                  (byte)Convert.ToInt32(tb.Text, 16));
                            }


                            else
                            {
                                Byte[] dummyArray = { (byte)Convert.ToInt32(tb.Text, 16) };


                                dyfipro.InsertBytes(cell / 2 + (long)fileSlider.Value * 16, dummyArray);
                            }

                        }

                        nextHexBoxField();

                    }

                    e.Handled = true;
                }
                e.Handled = true;

                if (e.Key == Key.Tab)
                {

                    cursor2.Focus();
                }



                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {

                    int cell = (int)(Canvas.GetTop(cursor) / 20 * 32 + Canvas.GetLeft(cursor) / 10);
                    if (mark[0] == -1 || mark[1] == -1)
                    {
                        mark[0] = cell / 2 + (long)fileSlider.Value * 16;
                        mark[1] = cell / 2 + (long)fileSlider.Value * 16;
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

                    releasemark = false;
                }

                if (!Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    if (releasemark)
                    {
                        mark[0] = -1;
                        mark[1] = -1;
                        
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
                    int cell = (int)(Canvas.GetTop(cursor) / 20 * 32 + Canvas.GetLeft(cursor) / 10);
                    if (cell / 2 + (long)fileSlider.Value * 16 < dyfipro.Length)
                    {
                        Boolean b = true;
                        if (Canvas.GetLeft(cursor) < 300)
                        {
                            Canvas.SetLeft(cursor, Canvas.GetLeft(cursor2) * 2 + 20);

                        }
                        else if (Canvas.GetTop(cursor) < 300)
                        {
                            Canvas.SetTop(cursor, Canvas.GetTop(cursor2) + 20);
                            Canvas.SetLeft(cursor, 0);

                        }


                        if (Canvas.GetLeft(cursor2) < 150)
                        {
                            Canvas.SetLeft(cursor2, Canvas.GetLeft(cursor2) + 10);
                            b = false;
                        }


                        else if (Canvas.GetTop(cursor2) < 300)
                        {
                            Canvas.SetTop(cursor2, Canvas.GetTop(cursor2) + 20);
                            Canvas.SetLeft(cursor2, 0);
                        }


                        if (fileSlider.Value != fileSlider.Maximum && Canvas.GetLeft(cursor2) == 150 &&
                            Canvas.GetTop(cursor2) == 300 && b)
                        {
                            Canvas.SetLeft(cursor, 0);
                            Canvas.SetLeft(cursor2, 0);
                            fileSlider.Value += 1;
                        }
                    }
                    e.Handled = true;
                }

                else if (e.Key == Key.Left)
                {
                    Boolean b = true;
                    if (Canvas.GetLeft(cursor) > 0)
                    {
                        Canvas.SetLeft(cursor, Canvas.GetLeft(cursor2) * 2 - 20);

                    }
                    else if (Canvas.GetTop(cursor) > 0)
                    {
                        Canvas.SetTop(cursor, Canvas.GetTop(cursor) - 20);
                        Canvas.SetLeft(cursor, 310);
                    }


                    if (Canvas.GetLeft(cursor2) > 0)
                    {
                        Canvas.SetLeft(cursor2, Canvas.GetLeft(cursor2) - 10);
                        b = false;
                    }
                    else if (Canvas.GetTop(cursor2) > 0)
                    {
                        Canvas.SetTop(cursor2, Canvas.GetTop(cursor2) - 20);
                        Canvas.SetLeft(cursor2, 150);
                    }


                    if (fileSlider.Value != fileSlider.Minimum && Canvas.GetLeft(cursor2) == 0 &&
                        Canvas.GetTop(cursor2) == 0 && b)
                    {
                        Canvas.SetLeft(cursor, 310);
                        Canvas.SetLeft(cursor2, 150);
                        fileSlider.Value -= 1;
                    }
                    e.Handled = true;
                }

                else if (e.Key == Key.Down)
                {
                    int cell = (int)(Canvas.GetTop(cursor) / 20 * 32 - 1 + Canvas.GetLeft(cursor) / 10);
                    if (cell / 2 + (long)fileSlider.Value * 16 + 15 < dyfipro.Length)
                    {
                        if (Canvas.GetTop(cursor2) > 290)
                            fileSlider.Value += 1;
                        if (Canvas.GetTop(cursor) < 300)
                            Canvas.SetTop(cursor, Canvas.GetTop(cursor) + 20);
                        if (Canvas.GetTop(cursor2) < 300)
                            Canvas.SetTop(cursor2, Canvas.GetTop(cursor2) + 20);
                    }
                    e.Handled = true;
                }

                else if (e.Key == Key.Up)
                {
                    if (Canvas.GetTop(cursor2) == 0)
                        fileSlider.Value -= 1;
                    if (Canvas.GetTop(cursor) > 0)
                        Canvas.SetTop(cursor, Canvas.GetTop(cursor) - 20);
                    if (Canvas.GetTop(cursor2) > 0)
                        Canvas.SetTop(cursor2, Canvas.GetTop(cursor2) - 20);
                    e.Handled = true;
                }
                

                else if (e.Key == Key.Back)
                {

                    int cell = (int)(Canvas.GetTop(cursor) / 20 * 32 + Canvas.GetLeft(cursor) / 10);
                    if (cell / 2 + (int)fileSlider.Value * 16 - 2 < dyfipro.Length)
                    {
                        TextBlock tb = grid1.Children[cell / 2] as TextBlock;
                        TextBlock tb2 = grid2.Children[cell / 2] as TextBlock;



                        if (mark[1] - mark[0] == 0)
                        {
                            if (cell / 2 + (int)fileSlider.Value * 16 - 1 > -1)
                            {
                                dyfipro.DeleteBytes(cell / 2 + (long)fileSlider.Value * 16 - 1, 1);
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
                                if (mark[1] - mark[0] > cell)
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



                        //updateUI((long)fileSlider.Value);    
                    }

                    e.Handled = true;
                }

                else if (e.Key == Key.PageDown)
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
                    Canvas.SetLeft(cursor2, 150);
                    Canvas.SetLeft(cursor, 300);
                    e.Handled = true;
                }

                else if (e.Key == Key.Home)
                {
                    Canvas.SetLeft(cursor2, 0);
                    Canvas.SetLeft(cursor, 0);
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

                Boolean releasemark = true;

                if (e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.PageDown || e.Key == Key.PageUp ||
                    e.Key == Key.End || e.Key == Key.Home || e.Key == Key.RightShift || e.Key == Key.LeftShift)
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        int cell = (int)(Canvas.GetTop(cursor) / 20 * 32 + Canvas.GetLeft(cursor) / 10);

                        if (mark[0] == -1 || mark[1] == -1)
                        {
                            if (cell / 2 + (long)fileSlider.Value * 16 - 1 != -1)
                            {
                                mark[0] = cell / 2 + (long)fileSlider.Value * 16;
                            }
                            else
                            {
                                mark[0] = 0;
                            }
                            mark[1] = cell / 2 + (long)fileSlider.Value * 16;
                            System.Console.WriteLine(mark[0]);
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


                        if (cell / 2 + (long)fileSlider.Value * 16 <= mark[0])
                        {
                            mark[0] = cell / 2 + (long)fileSlider.Value * 16;
                        }

                        if (cell / 2 + (long)fileSlider.Value * 16 >= mark[1])
                        {
                            mark[1] = cell / 2 + (long)fileSlider.Value * 16;
                        }

                        //updateUI((long)fileSlider.Value);
                        releasemark = false;
                    }
                if (!Keyboard.IsKeyDown(Key.LeftCtrl))
                {

                    if (releasemark)
                    {
                        mark[0] = -1;
                        mark[1] = -1;
                        //updateUI((long)fileSlider.Value);
                    }

                }


                if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.C)
                {

                    Copy_ASCIIFild();


                }

                if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.V)
                {
                    Paste_ASCIIFild();
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
            //System.Console.WriteLine("deine mutter");
        }

        private void nextHexBoxField()
        {
            Boolean b = true;

            if (Canvas.GetLeft(cursor) < 310)
            {
                Canvas.SetLeft(cursor, Canvas.GetLeft(cursor) + 10);
                b = false;
            }
            else if (Canvas.GetTop(cursor) < 300)
            {
                Canvas.SetTop(cursor, Canvas.GetTop(cursor) + 20);
                Canvas.SetLeft(cursor, 0);
            }

            if (Canvas.GetLeft(cursor) / 10 % 2 == 0)
            {

                if (Canvas.GetLeft(cursor2) < 150)
                    Canvas.SetLeft(cursor2, Canvas.GetLeft(cursor2) + 10);

                else if (Canvas.GetTop(cursor2) < 300)
                {
                    Canvas.SetTop(cursor2, Canvas.GetTop(cursor2) + 20);
                    Canvas.SetLeft(cursor2, 0);
                }
            }

            if (fileSlider.Value != fileSlider.Maximum && Canvas.GetLeft(cursor) == 310 && Canvas.GetTop(cursor) == 300 &&
                b)
            {
                Canvas.SetLeft(cursor, 0);
                Canvas.SetLeft(cursor2, 0);
                fileSlider.Value += 1;
            }

        }

        private void nextASCIIField()
        {
            Boolean b = true;

            if (Canvas.GetLeft(cursor) < 300)
            {
                if (Canvas.GetLeft(cursor) / 10 % 2 == 0)
                {
                    Canvas.SetLeft(cursor, Canvas.GetLeft(cursor) + 20);
                }
                else
                {
                    Canvas.SetLeft(cursor, Canvas.GetLeft(cursor) + 10);
                }
                b = false;
            }
            else if (Canvas.GetTop(cursor) < 300)
            {
                Canvas.SetTop(cursor, Canvas.GetTop(cursor) + 20);
                Canvas.SetLeft(cursor, 0);
            }



            if (Canvas.GetLeft(cursor2) < 150)
                Canvas.SetLeft(cursor2, Canvas.GetLeft(cursor2) + 10);

            else if (Canvas.GetTop(cursor2) < 300)
            {
                Canvas.SetTop(cursor2, Canvas.GetTop(cursor2) + 20);
                Canvas.SetLeft(cursor2, 0);
            }


            if (fileSlider.Value != fileSlider.Maximum && Canvas.GetLeft(cursor2) == 150 &&
                Canvas.GetTop(cursor2) == 300 && b)
            {
                Canvas.SetLeft(cursor, 0);
                Canvas.SetLeft(cursor2, 0);
                fileSlider.Value += 1;
            }



        }

        private void backHexBoxField()
        {
            Boolean b = true;

            if (Canvas.GetLeft(cursor) > 10)
            {
                Canvas.SetLeft(cursor, Canvas.GetLeft(cursor) - 20);
                b = false;
            }
            else if (Canvas.GetTop(cursor) > 10)
            {
                Canvas.SetTop(cursor, Canvas.GetTop(cursor) - 20);
                Canvas.SetLeft(cursor, 300);
            }



            if (Canvas.GetLeft(cursor2) > 0)
                Canvas.SetLeft(cursor2, Canvas.GetLeft(cursor2) - 10);

            else if (Canvas.GetTop(cursor2) > 0)
            {
                Canvas.SetTop(cursor2, Canvas.GetTop(cursor2) - 20);
                Canvas.SetLeft(cursor2, 150);
            }



            if (fileSlider.Value != fileSlider.Minimum && Canvas.GetLeft(cursor2) == 0 &&
                Canvas.GetTop(cursor2) == 0 && b)
            {
                Canvas.SetLeft(cursor, 300);
                Canvas.SetLeft(cursor2, 150);
                fileSlider.Value -= 1;
            }


        }

        private void backASCIIField()
        {
            Boolean b = true;

            if (Canvas.GetLeft(cursor) > 10)
            {
                Canvas.SetLeft(cursor, Canvas.GetLeft(cursor) - 20);
                b = false;
            }
            else if (Canvas.GetTop(cursor) > 10)
            {
                Canvas.SetTop(cursor, Canvas.GetTop(cursor) - 20);
                Canvas.SetLeft(cursor, 300);
            }



            if (Canvas.GetLeft(cursor2) > 0)
                Canvas.SetLeft(cursor2, Canvas.GetLeft(cursor2) - 10);

            else if (Canvas.GetTop(cursor2) > 0)
            {
                Canvas.SetTop(cursor2, Canvas.GetTop(cursor2) - 20);
                Canvas.SetLeft(cursor2, 150);
            }



            if (fileSlider.Value != fileSlider.Minimum && Canvas.GetLeft(cursor2) == 0 &&
                Canvas.GetTop(cursor2) == 0 && b)
            {
                Canvas.SetLeft(cursor, 300);
                Canvas.SetLeft(cursor2, 150);
                fileSlider.Value -= 1;
            }


        }

        private void MyControl_TextInput(object sender, TextCompositionEventArgs e)
        {

            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            int cell = (int) (Canvas.GetTop(cursor)/20*32 + Canvas.GetLeft(cursor)/10);
            if (cell/2 + (long) fileSlider.Value*16 < dyfipro.Length)
            {

                TextBlock tb = grid1.Children[cell/2] as TextBlock;
                TextBlock tb2 = grid2.Children[cell/2] as TextBlock;

                Byte[] stringBytes = enc.GetBytes(e.Text);


                if (stringBytes.Count() < 2 && stringBytes.Count() > 0)
                {
                    tb.Text = Encoding.GetEncoding(1252).GetBytes(e.Text)[0].ToString("X2");
                    tb2.Text = e.Text + "";



                    if (insertCheck.IsChecked == false)
                    {
                        dyfipro.WriteByte(cell / 2 + (long)fileSlider.Value * 16,
                                          Encoding.GetEncoding(1252).GetBytes(e.Text)[0]);
                    }
                    else
                    {
                        Byte[] dummyArray = {Encoding.GetEncoding(1252).GetBytes(e.Text)[0]};


                        dyfipro.InsertBytes(cell/2 + (long) fileSlider.Value*16, dummyArray);
                    }
                    

                    nextASCIIField();
                }
            }
            else if ((long) (cell/2 + (long) fileSlider.Value*16) == dyfipro.Length)
            {
                TextBlock tb = grid1.Children[cell/2] as TextBlock;
                TextBlock tb2 = grid2.Children[cell/2] as TextBlock;

                Byte[] stringBytes = enc.GetBytes(e.Text);



                if (stringBytes.Count() < 2 && stringBytes.Count() > 0)
                {
                    tb.Text = Encoding.GetEncoding(1252).GetBytes(e.Text)[0].ToString("X2");



                    tb2.Text = e.Text + "";




                    if (insertCheck.IsChecked == false)
                    {
                        dyfipro.WriteByte(cell / 2 + (long)fileSlider.Value * 16,
                                          Encoding.GetEncoding(1252).GetBytes(e.Text)[0]);
                    }
                    else
                    {
                        Byte[] dummyArray = { Encoding.GetEncoding(1252).GetBytes(e.Text)[0] };


                        dyfipro.InsertBytes(cell / 2 + (long)fileSlider.Value * 16, dummyArray);
                    }

                    nextASCIIField();

                }
            }

            updateUI((long)fileSlider.Value);
            //System.Console.WriteLine( e.Text);
            e.Handled = true;
            
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

            if (end < 256)
            {
                max = (int)end - 1;
            }

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

            for (int i = 0; i < 256; i++)
            {
                TextBlock tb = grid1.Children[i] as TextBlock;
                Byte help = dyfipro.ReadByte(i + position * 16);

                //tb.Text = BitConverter.ToString((byte[])buffer).Replace("-", "").Trim()[i * 2] + "" + BitConverter.ToString((byte[])buffer).Replace("-", "").Trim()[i * 2 + 1];
                if (i <= max)
                {
                    tb.Text = String.Format("{0:X2}", help);
                }
                else
                {
                    tb.Text = "";
                }

                tb.Background = Brushes.Transparent;
                if (mark[0] != -1)
                {
                    if (markedBackwards)
                    {
                        if (i + position * 16 > mark[0] && i + position * 16 <= mark[1])
                        {
                            tb.Background = Brushes.SkyBlue;
                        }
                    }
                    else
                    {
                        if (i + position * 16 >= mark[0] && i + position * 16 < mark[1])
                        {
                            tb.Background = Brushes.SkyBlue;
                        }
                    }
                }

                TextBlock tb2 = grid2.Children[i] as TextBlock;
                if (i <= max)
                {
                    if (31 < help && 128 > help)
                        tb2.Text = (char)help + "";
                    else
                        tb2.Text = ".";



                }
                else
                {
                    tb2.Text = "";
                }

                tb2.Background = Brushes.Transparent;
                if (mark[0] != -1)
                {
                    if (markedBackwards)
                    {
                        if (i + position * 16 > mark[0] && i + position * 16 <= mark[1])
                        {
                            tb2.Background = Brushes.SkyBlue;
                        }
                    }
                    else
                    {
                        if (i + position * 16 >= mark[0] && i + position * 16 < mark[1])
                        {
                            tb2.Background = Brushes.SkyBlue;
                        }
                    }
                }

                //rtb.AppendText(enc.GetString(buffer));*/
            }

            for (int i = 0; i < 256; i++)
            {

            }



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

        private void Copy_HexBoxField()
        {
            StringBuilder clipBoardString = new StringBuilder();


            for (long i = mark[0]; i < mark[1]; i++)
            {
                if (markedBackwards)
                    clipBoardString.Append(String.Format("{0:X2}", dyfipro.ReadByte(i + 1)));
                else
                    clipBoardString.Append(String.Format("{0:X2}", dyfipro.ReadByte(i)));

            }

            System.Console.WriteLine(mark[0] + "     " + mark[1] + " " + clipBoardString.Length);
            System.Console.WriteLine(clipBoardString.ToString());
            try
            {
                Clipboard.SetText(clipBoardString.ToString());
            }
            catch (Exception exp)
            {

            }
            mark[1] = -1;
            mark[0] = -1;

            updateUI((long)fileSlider.Value);
        }

        private void Cut_Click_HexBoxField(object sender, RoutedEventArgs e)
        {
            Cut_HexBoxField();
        }

        private void Cut_HexBoxField()
        {
            StringBuilder clipBoardString = new StringBuilder();


            for (long i = mark[0]; i < mark[1]; i++)
            {
                if(markedBackwards)
                    clipBoardString.Append(String.Format("{0:X2}", dyfipro.ReadByte(i+1)));
                else
                    clipBoardString.Append(String.Format("{0:X2}", dyfipro.ReadByte(i)));

            }

            System.Console.WriteLine(mark[0] + "     " + mark[1] + " " + clipBoardString.Length);
            System.Console.WriteLine(clipBoardString.ToString());
            try
            {
                Clipboard.SetText(clipBoardString.ToString());
            }
            catch (Exception exp)
            {

            }

            int cell = (int)(Canvas.GetTop(cursor) / 20 * 32 + Canvas.GetLeft(cursor) / 10);
            if (cell / 2 + (long)fileSlider.Value * 16 - 2 < dyfipro.Length)
            {
                TextBlock tb = grid1.Children[cell / 2] as TextBlock;
                TextBlock tb2 = grid2.Children[cell / 2] as TextBlock;

                if (mark[1] - mark[0] == 0)
                {
                    if (cell / 2 + (int)fileSlider.Value * 16 - 1 > -1)
                    {
                        dyfipro.DeleteBytes(cell / 2 + (long)fileSlider.Value * 16 - 1, 1);
                        backHexBoxField();
                    }
                }
                else
                {
                    if (markedBackwards)
                    {
                        // if (cell / 2 + (int)fileSlider.Value * 16 - 1 > -1)
                        dyfipro.DeleteBytes(mark[0] + 1, mark[1] - mark[0]);

                        if (fileSlider.Value == fileSlider.Maximum)
                        {
                            Canvas.SetTop(cursor2, (mark[0] / 16 - fileSlider.Value) * 20);
                            Canvas.SetTop(cursor, (mark[0] / 16 - fileSlider.Value) * 20);
                        }
                    }
                    else
                    {
                        // if (cell / 2 + (int)fileSlider.Value * 16 - 1 > -1)
                        dyfipro.DeleteBytes(mark[0], mark[1] - mark[0]);

                        if (mark[1] - mark[0] > cell)
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

        private void Copy_Click_ASCIIFild(object sender, RoutedEventArgs e)
        {
            Copy_ASCIIFild();
        }

        private void Copy_ASCIIFild()
        {
            StringBuilder clipBoardString = new StringBuilder();
            for (long i = mark[0]; i < mark[1]; i++)
            {
                if(markedBackwards)
                    clipBoardString.Append((char)dyfipro.ReadByte(i+1));
                else
                    clipBoardString.Append((char)dyfipro.ReadByte(i));

            }

            System.Console.WriteLine(mark[0] + "     " + mark[1] + " " + clipBoardString.Length);
            System.Console.WriteLine(clipBoardString.ToString());
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
            int cell = (int) (Canvas.GetTop(cursor)/20*32 + Canvas.GetLeft(cursor)/10);

            if (cell/2 + (int) fileSlider.Value*16 - 2 < dyfipro.Length)
            {
                TextBlock tb = grid1.Children[cell/2] as TextBlock;
                TextBlock tb2 = grid2.Children[cell/2] as TextBlock;



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
                cell = (int) (Canvas.GetTop(cursor)/20*32 + Canvas.GetLeft(cursor)/10 + 2);
            else
                cell = (int) (Canvas.GetTop(cursor)/20*32 + Canvas.GetLeft(cursor)/10);

            String text = (String) Clipboard.GetData(DataFormats.Text);
            Console.WriteLine(text);
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            dyfipro.InsertBytes(cell/2 + (int) fileSlider.Value*16, enc.GetBytes(text));
            
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
                clipBoardString.Append((char)dyfipro.ReadByte(i));

            }

            System.Console.WriteLine(mark[0] + "     " + mark[1] + " " + clipBoardString.Length);
            System.Console.WriteLine(clipBoardString.ToString());
            try
            {
                Clipboard.SetText(clipBoardString.ToString());
            }
            catch (Exception exp)
            {

            }



            int cell = (int)(Canvas.GetTop(cursor) / 20 * 32 + Canvas.GetLeft(cursor) / 10);
            if (cell / 2 + (int)fileSlider.Value * 16 - 2 < dyfipro.Length)
            {
                TextBlock tb = grid1.Children[cell / 2] as TextBlock;
                TextBlock tb2 = grid2.Children[cell / 2] as TextBlock;



                if (mark[1] - mark[0] == 0)
                {
                    if (cell / 2 + (int)fileSlider.Value * 16 - 1 > -1)
                    {
                        dyfipro.DeleteBytes(cell / 2 + (long)fileSlider.Value * 16 - 1, 1);
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
                        if (mark[1] - mark[0] > cell)
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

    }
}

 




