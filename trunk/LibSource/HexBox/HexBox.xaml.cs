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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class HexBox : UserControl
    {
        private FileStream fs;
        private DynamicFileByteProvider dyfipro;
        
        private TextBox tb3 = new TextBox();
        public string Pfad = string.Empty;

        private long[] mark;
        private Boolean markedBackwards = false;

        

        public HexBox()
        {
            InitializeComponent();

            

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



            cursor.PreviewKeyDown += Tastedruecken;
            cursor2.PreviewKeyDown += Tastedruecken2;

            cursor2.TextInput += MyControl_TextInput;

         


          
            //Byte[] buffer = new byte[256];

            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            Stream s = new MemoryStream();




            dyfipro = new DynamicFileByteProvider(s);



            dyfipro.LengthChanged += new EventHandler(dyfipro_LengthChanged);




            /*OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog().Value)
            {
                Pfad = openFileDialog.FileName;
            }
            //fs = new FileStream(@"C:\Users\Julian\Videos\72.Stunden.The.Next.Three.Days.German.PROPER.AC3D.720p.Bluray.x264-Vetax/vetax-72h720.mkv", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            fs = new FileStream(Pfad, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

            
            dyfipro = new DynamicFileByteProvider(Pfad,true);

            

            dyfipro.LengthChanged +=new EventHandler(dyfipro_LengthChanged);
            

            fileSlider.Minimum = 0;
            //fileSlider.Maximum = fs.Length/256;
            //fileSlider.Maximum = (dyfipro.Length-256)/16+1;
            fileSlider.ViewportSize = 16;
            

            //tb3.Text = fs.Length/256 +"";
            tb3.Text = dyfipro.Length / 256 + "";

            //fileSlider.ManipulationCompleted += MyManipulationCompleteEvent;
            fileSlider.ValueChanged += MyManipulationCompleteEvent;
            fileSlider.SmallChange = 1;
            fileSlider.LargeChange = 1;
            


            //fs.Read(buffer, 0, 256);

            //buffer = dyfipro.ReadByte(0);
            
            //System.Console.WriteLine(String.Format("{0:X2}", dyfipro.ReadByte(2)));


            

            fill(0);

            */
            //output.Text = enc.GetString(buffer);
            //output.Text = Convert.ToString(buffer[0]); 

            //fs.Flush(); 
            //fs.Close(); //<-- Breakpoint here
        }

        private void cursor_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            Line.Text = "Hallo";
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

            fill((long) fileSlider.Value);

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

            fill((long) fileSlider.Value);

        }

        private void canvas1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            cursor.Focus();

            e.Handled = true;
        }

        private void canvas2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            cursor2.Focus();

            e.Handled = true;
        }

        public AsyncCallback callback()
        {
            return null;
        }

        public void dispose()
        {
            dyfipro.Dispose();
        }

        public void fill(long position)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            long end = dyfipro.Length - position*16;
            int max = 256;

            if (end < 256)
            {
                max = (int) end - 1;
            }

            for (int j = 0; j < 16; j++)
            {
                TextBlock id = gridid.Children[j] as TextBlock;
                //id.Text = (position + j) * 16 + "";
                long s = (position + j)*16;
                id.Text = s.ToString("X");

            }

            for (int i = 0; i < 256; i++)
            {
                TextBlock tb = grid1.Children[i] as TextBlock;
                //tb.Text = BitConverter.ToString((byte[])buffer).Replace("-", "").Trim()[i * 2] + "" + BitConverter.ToString((byte[])buffer).Replace("-", "").Trim()[i * 2 + 1];
                if (i <= max)
                {
                    tb.Text = String.Format("{0:X2}", dyfipro.ReadByte(i + position*16));
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
                        if (i + position*16 > mark[0] && i + position*16 <= mark[1])
                        {
                            tb.Background = Brushes.SkyBlue;
                        }
                    }
                    else
                    {
                        if (i + position*16 >= mark[0] && i + position*16 < mark[1])
                        {
                            tb.Background = Brushes.SkyBlue;
                        }
                    }
                }

                //rtb.AppendText(enc.GetString(buffer));
            }

            for (int i = 0; i < 256; i++)
            {
                TextBlock tb = grid2.Children[i] as TextBlock;
                if (i <= max)
                {
                    tb.Text = (char) dyfipro.ReadByte(i + position*16) + "";



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
                        if (i + position*16 > mark[0] && i + position*16 <= mark[1])
                        {
                            tb.Background = Brushes.SkyBlue;
                        }
                    }
                    else
                    {
                        if (i + position*16 >= mark[0] && i + position*16 < mark[1])
                        {
                            tb.Background = Brushes.SkyBlue;
                        }
                    }
                }
            }
        }



        public void keyweiter()
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

            if (Canvas.GetLeft(cursor)/10%2 == 0)
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

        public void keyweiter2()
        {
            Boolean b = true;

            if (Canvas.GetLeft(cursor) < 300)
            {
                if (Canvas.GetLeft(cursor)/10%2 == 0)
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

        private void keyback()
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

        private void keyback2()
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


        public void Tastedruecken3(object sender, EventArgs e)
        {
            //System.Console.WriteLine("hallo welt");
        }

        private void HelpExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            //System.Console.WriteLine("deine mutter");
        }

        public void Tastedruecken(object sender, KeyEventArgs e)
        {
            //debug.Text = e.Key.ToString();    
            Key k = e.Key;

            Boolean releasemark = true;



            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            string s = k.ToString();
            if (e.Key == Key.Right)
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


                if (Canvas.GetLeft(cursor)/10%2 == 0)
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



                if (Canvas.GetLeft(cursor)/10%2 == 1)
                {

                    if (Canvas.GetLeft(cursor2) > 0)
                        Canvas.SetLeft(cursor2, Canvas.GetLeft(cursor2) - 10);

                    else if (Canvas.GetTop(cursor2) > 0)
                    {
                        Canvas.SetTop(cursor2, Canvas.GetTop(cursor2) - 20);
                        Canvas.SetLeft(cursor2, 150);
                    }
                }

                if (fileSlider.Value != fileSlider.Minimum && Canvas.GetLeft(cursor) == 0 && Canvas.GetTop(cursor) == 0 &&
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
                if (Canvas.GetTop(cursor2) > 290)
                    fileSlider.Value += 1;
                if (Canvas.GetTop(cursor) < 300)
                    Canvas.SetTop(cursor, Canvas.GetTop(cursor) + 20);
                if (Canvas.GetTop(cursor2) < 300)
                    Canvas.SetTop(cursor2, Canvas.GetTop(cursor2) + 20);
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


                int cell = (int) (Canvas.GetTop(cursor)/20*32 + Canvas.GetLeft(cursor)/10);
                if (cell/2 + (long) fileSlider.Value*16 - 2 < dyfipro.Length)
                {
                    TextBlock tb = grid1.Children[cell/2] as TextBlock;
                    TextBlock tb2 = grid2.Children[cell/2] as TextBlock;

                    if (mark[1] - mark[0] == 0)
                    {
                        if (cell/2 + (int) fileSlider.Value*16 - 1 > -1)
                        {
                            dyfipro.DeleteBytes(cell/2 + (long) fileSlider.Value*16 - 1, 1);
                            keyback();
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
                                Canvas.SetTop(cursor2, (mark[0]/16 - fileSlider.Value)*20);
                                Canvas.SetTop(cursor, (mark[0]/16 - fileSlider.Value)*20);
                            }
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



                    fill((long) fileSlider.Value);
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


            else if (e.Key == Key.A || e.Key == Key.B || e.Key == Key.C || e.Key == Key.D || e.Key == Key.E ||
                     e.Key == Key.F)
            {
                int cell = (int) (Canvas.GetTop(cursor)/20*32 + Canvas.GetLeft(cursor)/10);
                if (cell/2 + (long) fileSlider.Value*16 < dyfipro.Length)
                {
                    TextBlock tb = grid1.Children[cell/2] as TextBlock;



                    if (cell%2 == 0)
                    {
                        tb.Text = k.ToString().ToUpper() + tb.Text[1];

                    }
                    if (cell%2 == 1)
                    {
                        tb.Text = tb.Text[0] + k.ToString().ToUpper();

                    }


                    TextBlock tb2 = grid2.Children[cell/2] as TextBlock;
                    tb2.Text = (char) Convert.ToInt32(tb.Text, 16) + "";


                    dyfipro.WriteByte(cell/2 + (long) fileSlider.Value*16, (byte) Convert.ToInt32(tb.Text, 16));


                    keyweiter();
                }

                else if ((long) (cell/2 + (long) fileSlider.Value*16) == dyfipro.Length)
                {
                    TextBlock tb = grid1.Children[cell/2] as TextBlock;

                    if (tb.Text == "")
                    {
                        tb.Text = "00";
                    }

                    if (cell%2 == 0)
                    {
                        tb.Text = k.ToString().ToUpper() + tb.Text[1];

                    }
                    if (cell%2 == 1)
                    {
                        tb.Text = tb.Text[0] + k.ToString().ToUpper();

                    }


                    TextBlock tb2 = grid2.Children[cell/2] as TextBlock;
                    tb2.Text = (char) Convert.ToInt32(tb.Text, 16) + "";
                    Byte[] bytes = new Byte[1];
                    bytes[0] = (byte) Convert.ToInt32(tb.Text, 16);

                    dyfipro.InsertBytes(cell/2 + (long) fileSlider.Value*16, bytes);




                    keyweiter();
                }

                e.Handled = true;
            }

            else if (e.Key == Key.D0 || e.Key == Key.D1 || e.Key == Key.D2 || e.Key == Key.D3 || e.Key == Key.D4 ||
                     e.Key == Key.D5 || e.Key == Key.D6 || e.Key == Key.D7 || e.Key == Key.D8 || e.Key == Key.D9)
            {
                int cell = (int) (Canvas.GetTop(cursor)/20*32 + Canvas.GetLeft(cursor)/10);
                if (cell/2 + (long) fileSlider.Value*16 < dyfipro.Length)
                {
                    TextBlock tb = grid1.Children[cell/2] as TextBlock;
                    TextBlock tb2 = grid2.Children[cell/2] as TextBlock;

                    Byte[] stringBytes = enc.GetBytes("");
                    if (cell%2 == 0)
                    {
                        tb.Text = k.ToString().Remove(0, 1) + tb.Text[1];

                    }
                    if (cell%2 == 1)
                    {
                        tb.Text = tb.Text[0] + k.ToString().Remove(0, 1);

                    }

                    tb2.Text = (char) Convert.ToInt32(tb.Text, 16) + "";


                    dyfipro.WriteByte(cell/2 + (long) fileSlider.Value*16, (byte) Convert.ToInt32(tb.Text, 16));



                    keyweiter();
                }

                else if ((long) (cell/2 + (long) fileSlider.Value*16) == dyfipro.Length)
                {
                    TextBlock tb = grid1.Children[cell/2] as TextBlock;
                    TextBlock tb2 = grid2.Children[cell/2] as TextBlock;

                    Byte[] stringBytes = enc.GetBytes("");

                    if (tb.Text == "")
                    {
                        tb.Text = "00";
                    }

                    if (cell%2 == 0)
                    {
                        tb.Text = k.ToString().Remove(0, 1) + tb.Text[1];

                    }
                    if (cell%2 == 1)
                    {
                        tb.Text = tb.Text[0] + k.ToString().Remove(0, 1);

                    }

                    tb2.Text = (char) Convert.ToInt32(tb.Text, 16) + "";
                    Byte[] bytes = new Byte[1];
                    bytes[0] = (Byte) Convert.ToInt32(tb.Text, 16);

                    dyfipro.InsertBytes(cell/2 + (long) fileSlider.Value*16, bytes);



                    keyweiter();

                }

                e.Handled = true;
            }
            e.Handled = true;
            //int x = (int)s[0] - 65;
            if (e.Key == Key.Tab)
            {

                cursor2.Focus();
            }


            //if (e.Key == Key.Right||e.Key == Key.Up||e.Key == Key.Down||e.Key == Key.Left)            
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {

                int cell = (int) (Canvas.GetTop(cursor)/20*32 + Canvas.GetLeft(cursor)/10);
                if (mark[0] == -1 || mark[1] == -1)
                {
                    mark[0] = cell/2 + (long) fileSlider.Value*16;
                    mark[1] = cell/2 + (long) fileSlider.Value*16;
                }



                if (cell/2 + (long) fileSlider.Value*16 < mark[0])
                {
                    markedBackwards = true;
                }

                if (cell/2 + (long) fileSlider.Value*16 > mark[1])
                {
                    markedBackwards = false;
                }

                if (cell/2 + (long) fileSlider.Value*16 <= mark[1] && cell/2 + (long) fileSlider.Value*16 >= mark[0] &&
                    !markedBackwards)
                {
                    mark[1] = cell/2 + (long) fileSlider.Value*16;
                }

                if (cell/2 + (long) fileSlider.Value*16 <= mark[1] && cell/2 + (long) fileSlider.Value*16 >= mark[0] &&
                    markedBackwards)
                {
                    mark[0] = cell/2 + (long) fileSlider.Value*16;
                }


                if (cell/2 + (long) fileSlider.Value*16 < mark[0])
                {
                    mark[0] = cell/2 + (long) fileSlider.Value*16;
                }

                if (cell/2 + (long) fileSlider.Value*16 > mark[1])
                {
                    mark[1] = cell/2 + (long) fileSlider.Value*16;
                }

                fill((long) fileSlider.Value);
                releasemark = false;
            }

            if (!Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (releasemark)
                {
                    mark[0] = -1;
                    mark[1] = -1;
                    fill((long) fileSlider.Value);
                }
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.A)
            {
                mark[0] = 0;
                mark[1] = dyfipro.Length;

                fill((long) fileSlider.Value);
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.C)
            {

                StringBuilder clipBoardString = new StringBuilder();
                for (long i = mark[0]; i < mark[1]; i++)
                {
                    clipBoardString.Append(String.Format("{0:X2}", dyfipro.ReadByte(i)));

                }

                System.Console.WriteLine(mark[0] + "     " + mark[1] + " " + clipBoardString.Length);
                System.Console.WriteLine(clipBoardString.ToString());
                Clipboard.SetData(DataFormats.UnicodeText, clipBoardString.ToString());

            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.S)
            {

                dyfipro.ApplyChanges();

            }


            int cell2 = (int) (Canvas.GetTop(cursor)/20*32 + Canvas.GetLeft(cursor)/10);
            Column.Text = (cell2/2)%16 + "";
            Line.Text = (int) (Canvas.GetTop(cursor)/20) + (long) fileSlider.Value + "";
        }


        public void Tastedruecken2(object sender, KeyEventArgs e)
        {
            //System.Console.WriteLine("wuppdiwupp");

            //debug.Text = e.Key.ToString();    
            Key k = e.Key;



            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            string s = k.ToString();
            if (e.Key == Key.Right)
            {
                Boolean b = true;
                if (Canvas.GetLeft(cursor) < 300)
                {
                    Canvas.SetLeft(cursor, Canvas.GetLeft(cursor2)*2 + 20);

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
                e.Handled = true;
            }

            else if (e.Key == Key.Left)
            {
                Boolean b = true;
                if (Canvas.GetLeft(cursor) > 0)
                {
                    Canvas.SetLeft(cursor, Canvas.GetLeft(cursor2)*2 - 20);

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
                if (Canvas.GetTop(cursor2) > 290)
                    fileSlider.Value += 1;
                if (Canvas.GetTop(cursor) < 300)
                    Canvas.SetTop(cursor, Canvas.GetTop(cursor) + 20);
                if (Canvas.GetTop(cursor2) < 300)
                    Canvas.SetTop(cursor2, Canvas.GetTop(cursor2) + 20);
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
/*
            else if (e.Key == Key.Space)
            {
                int cell = (int)(Canvas.GetTop(cursor) / 20 * 32 + Canvas.GetLeft(cursor) / 10);
                TextBlock tb = grid1.Children[cell / 2] as TextBlock;
                TextBlock tb2 = grid2.Children[cell / 2] as TextBlock;

                Byte[] stringBytes = enc.GetBytes(" ");
                StringBuilder sbBytes = new StringBuilder(stringBytes.Length * 2);
                foreach (byte b in stringBytes)
                {
                    sbBytes.AppendFormat("{0:X2}", b);
                }

                tb.Text = sbBytes.ToString() + "";


                tb2.Text = " ";




                dyfipro.WriteByte(cell / 2 + (int)fileSlider.Value * 16, stringBytes[0]);

                keyweiter2();

            }


            else if(e.Key != Key.Tab)
            {
                int cell = (int)(Canvas.GetTop(cursor) / 20 * 32 + Canvas.GetLeft(cursor) / 10);
                TextBlock tb = grid1.Children[cell / 2] as TextBlock;
                TextBlock tb2 = grid2.Children[cell / 2] as TextBlock;

                Byte[] stringBytes = enc.GetBytes(k.ToString());
                if (stringBytes.Count() < 2)
                {
                    StringBuilder sbBytes = new StringBuilder(stringBytes.Length*2);
                    foreach (byte b in stringBytes)
                    {
                        sbBytes.AppendFormat("{0:X2}", b);
                    }

                    tb.Text = sbBytes.ToString() + "";

                    
                    tb2.Text = k.ToString() + "";




                    dyfipro.WriteByte(cell/2 + (int) fileSlider.Value*16, stringBytes[0]);

                    keyweiter2();
                }
                else
                {
                    
                }

            }

            */


            else if (e.Key == Key.Back)
            {

                int cell = (int) (Canvas.GetTop(cursor)/20*32 + Canvas.GetLeft(cursor)/10);
                if (cell/2 + (int) fileSlider.Value*16 - 2 < dyfipro.Length)
                {
                    TextBlock tb = grid1.Children[cell/2] as TextBlock;
                    TextBlock tb2 = grid2.Children[cell/2] as TextBlock;



                    if (mark[1] - mark[0] == 0)
                    {
                        if (cell/2 + (int) fileSlider.Value*16 - 1 > -1)
                        {
                            dyfipro.DeleteBytes(cell/2 + (long) fileSlider.Value*16 - 1, 1);
                            keyback2();
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



                    fill((long) fileSlider.Value);
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

            if (e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left ||
                e.Key == Key.RightShift || e.Key == Key.LeftShift)
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    int cell = (int) (Canvas.GetTop(cursor)/20*32 + Canvas.GetLeft(cursor)/10);

                    if (mark[0] == -1 || mark[1] == -1)
                    {
                        if (cell/2 + (long) fileSlider.Value*16 - 1 != -1)
                        {
                            mark[0] = cell/2 + (long) fileSlider.Value*16;
                        }
                        else
                        {
                            mark[0] = 0;
                        }
                        mark[1] = cell/2 + (long) fileSlider.Value*16;
                        System.Console.WriteLine(mark[0]);
                    }

                    if (cell/2 + (long) fileSlider.Value*16 < mark[0])
                    {
                        markedBackwards = true;
                    }

                    if (cell/2 + (long) fileSlider.Value*16 > mark[1])
                    {
                        markedBackwards = false;
                    }

                    if (cell/2 + (long) fileSlider.Value*16 <= mark[1] && cell/2 + (long) fileSlider.Value*16 >= mark[0] &&
                        !markedBackwards)
                    {
                        mark[1] = cell/2 + (long) fileSlider.Value*16;
                    }

                    if (cell/2 + (long) fileSlider.Value*16 <= mark[1] && cell/2 + (long) fileSlider.Value*16 >= mark[0] &&
                        markedBackwards)
                    {
                        mark[0] = cell/2 + (long) fileSlider.Value*16;
                    }


                    if (cell/2 + (long) fileSlider.Value*16 <= mark[0])
                    {
                        mark[0] = cell/2 + (long) fileSlider.Value*16;
                    }

                    if (cell/2 + (long) fileSlider.Value*16 >= mark[1])
                    {
                        mark[1] = cell/2 + (long) fileSlider.Value*16;
                    }

                    fill((long) fileSlider.Value);
                    releasemark = false;
                }
            if (!Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (releasemark)
                {
                    mark[0] = -1;
                    mark[1] = -1;
                    fill((long) fileSlider.Value);
                }
            }


            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.C)
            {

                StringBuilder clipBoardString = new StringBuilder();
                for (long i = mark[0]; i < mark[1]; i++)
                {
                    clipBoardString.Append((char) dyfipro.ReadByte(i));

                }

                System.Console.WriteLine(mark[0] + "     " + mark[1] + " " + clipBoardString.Length);
                System.Console.WriteLine(clipBoardString.ToString());
                Clipboard.SetData(DataFormats.OemText, clipBoardString);

            }

            int cell2 = (int) (Canvas.GetTop(cursor)/20*32 + Canvas.GetLeft(cursor)/10);
            Column.Text = (cell2/2)%16 + "";
            Line.Text = (int) (Canvas.GetTop(cursor)/20) + (long) fileSlider.Value + "";
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




                    dyfipro.WriteByte(cell/2 + (long) fileSlider.Value*16,
                                      Encoding.GetEncoding(1252).GetBytes(e.Text)[0]);

                    keyweiter2();
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




                    dyfipro.InsertBytes(cell/2 + (long) fileSlider.Value*16, Encoding.GetEncoding(1252).GetBytes(e.Text));

                    keyweiter2();

                }
            }


            //System.Console.WriteLine( e.Text);
            e.Handled = true;
        }

        private void MyManipulationCompleteEvent(object sender, EventArgs e)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            //Byte[] buffer = new byte[256];
            //fs = new FileStream(@"C:\Users\Julian\Videos\72.Stunden.The.Next.Three.Days.German.PROPER.AC3D.720p.Bluray.x264-Vetax/vetax-72h720.mkv", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            //fs = new FileStream(@"C:\Bar.txt", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

            //fs.Seek( (long)fileSlider.Value*16 ,SeekOrigin.Begin);
            //fs.Read(buffer, 0 , 256);
            fill((long) fileSlider.Value);

            tb3.Text = (long) fileSlider.Value + "" + Math.Round(fileSlider.Value*16, 0) + fileSlider.Value;

            //System.Console.WriteLine(fileSlider.Value);

            //output.Text = enc.GetString(buffer);
            //output.Text = Convert.ToString(buffer[0]);
            //output.Text = (int) fileSlider.Value+"";

        }

        public void dyfipro_LengthChanged(object sender, EventArgs e)
        {

            //System.Console.WriteLine(e.ToString());
            double old = fileSlider.Maximum;

            fileSlider.Maximum = (dyfipro.Length - 256)/16 + 1;

            System.Console.WriteLine(old + "old");
            System.Console.WriteLine(fileSlider.Maximum + "maximum");
            System.Console.WriteLine(dyfipro.Length + "dyfirprolength");
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

        public void openFile(String fileName,Boolean canRead)
        {
            
            dyfipro.Dispose();
            try
            {
                if (fileName != "")
                {
                    FileName.Text = fileName;

                    dyfipro = new DynamicFileByteProvider(fileName, canRead);


                    dyfipro.LengthChanged += new EventHandler(dyfipro_LengthChanged);



                    fileSlider.Minimum = 0;
                    fileSlider.Maximum = (dyfipro.Length - 256)/16 + 1;
                    fileSlider.ViewportSize = 16;



                    tb3.Text = dyfipro.Length/256 + "";


                    fileSlider.ValueChanged += MyManipulationCompleteEvent;
                    fileSlider.SmallChange = 1;
                    fileSlider.LargeChange = 1;





                    fill(0);
                }
            }
            finally
            {
            }
        }

        public void closeFile()
        {
            dyfipro.Dispose();
            //fillempty();
        }

        public void fillempty()
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

        private void Button_Click(object sender, RoutedEventArgs e)
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

                    dyfipro = new DynamicFileByteProvider(Pfad, false);


                    dyfipro.LengthChanged += new EventHandler(dyfipro_LengthChanged);



                    fileSlider.Minimum = 0;
                    fileSlider.Maximum = (dyfipro.Length - 256)/16 + 1;
                    fileSlider.ViewportSize = 16;



                    tb3.Text = dyfipro.Length/256 + "";


                    fileSlider.ValueChanged += MyManipulationCompleteEvent;
                    fileSlider.SmallChange = 1;
                    fileSlider.LargeChange = 1;





                    fill(0);

                    OnFileChanged(this, EventArgs.Empty);
                }
            }
            finally
            {
                
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            dyfipro.ApplyChanges();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Title = "Save Data";
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
                    fs.Close();
                }
                else
                {
                    dyfipro.ApplyChanges();
                }
            }
        }

        public event EventHandler OnFileChanged;

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
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



                tb3.Text = dyfipro.Length/256 + "";


                fileSlider.ValueChanged += MyManipulationCompleteEvent;
                fileSlider.SmallChange = 1;
                fileSlider.LargeChange = 1;





                fill(0);
            }

        }
    }
}

 




