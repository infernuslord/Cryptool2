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
using System.Threading;
using System.Windows.Automation.Peers;
using System.Windows.Threading;

namespace Cryptool.LFSR
{
    /// <summary>
    /// Interaction logic for LFSRPresentation.xaml
    /// </summary>

    public partial class LFSRPresentation : UserControl
    {
        public LFSRPresentation()
        {
            InitializeComponent();
            /*
            char[] tapSequence = { '1', '1', '0', '0' };
            //char[] myState = { '0'};
            //char[] myState = { '0', '1'};
            //char[] myState = { '0', '1', '0'};
            char[] myState = { '0', '1', '0', '0' };
            char output = '1';

            DrawLFSR(myState, tapSequence);
            FillBoxes(myState, tapSequence, output);
            //DeleteAll(100);*/
        }

        public void DrawLFSR(char[] state, char[] tapSequence)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                // hide initial textbox
                infoText.Visibility = Visibility.Hidden;

                // add lines and triangles
                Line HoriLine1 = new Line();
                HoriLine1.X1 = 5;
                HoriLine1.Y1 = 18;
                HoriLine1.X2 = 180;
                HoriLine1.Y2 = 18;
                HoriLine1.Stroke = Brushes.Black;
                HoriLine1.StrokeThickness = 1;
                myGrid.Children.Add(HoriLine1);

                Line HoriLine2 = new Line();
                HoriLine2.X1 = 5;
                HoriLine2.Y1 = 47;
                HoriLine2.X2 = 150;
                HoriLine2.Y2 = 47;
                HoriLine2.Stroke = Brushes.Black;
                HoriLine1.StrokeThickness = 1;
                myGrid.Children.Add(HoriLine2);

                Line VertLine1 = new Line();
                VertLine1.X1 = 5;
                VertLine1.Y1 = 18;
                VertLine1.X2 = 5;
                VertLine1.Y2 = 48;
                VertLine1.Stroke = Brushes.Black;
                VertLine1.StrokeThickness = 1;
                myGrid.Children.Add(VertLine1);

                Line VertLine2 = new Line();
                VertLine2.X1 = 150;
                VertLine2.Y1 = 18;
                VertLine2.X2 = 150;
                VertLine2.Y2 = 48;
                VertLine2.Stroke = Brushes.Black;
                VertLine2.StrokeThickness = 1;
                myGrid.Children.Add(VertLine2);

                // adjust lines
                HoriLine1.X2 = 60 + state.Length * 30;
                HoriLine2.X2 = 30 + state.Length * 30;
                VertLine2.X1 = 30 + state.Length * 30;
                VertLine2.X2 = 30 + state.Length * 30;

                // add left triangle ////////////////////
                // Create a path to draw a geometry with.
                Path leftTriangle = new Path();
                leftTriangle.Stroke = Brushes.Black;
                leftTriangle.StrokeThickness = 1;
                leftTriangle.Fill = Brushes.Black;

                // Create a StreamGeometry to use to specify myPath.
                StreamGeometry geometryLT = new StreamGeometry();
                geometryLT.FillRule = FillRule.EvenOdd;

                // Open a StreamGeometryContext that can be used to describe this StreamGeometry 
                // object's contents.
                using (StreamGeometryContext ctx = geometryLT.Open())
                {

                    // Begin the triangle at the point specified. Notice that the shape is set to 
                    // be closed so only two lines need to be specified below to make the triangle.
                    ctx.BeginFigure(new Point(13, 14), true /* is filled */, true /* is closed */);

                    // Draw a line to the next specified point.
                    ctx.LineTo(new Point(13, 22), true /* is stroked */, false /* is smooth join */);

                    // Draw another line to the next specified point.
                    ctx.LineTo(new Point(20, 18), true /* is stroked */, false /* is smooth join */);
                }

                // Freeze the geometry (make it unmodifiable)
                // for additional performance benefits.
                geometryLT.Freeze();

                // Specify the shape (triangle) of the Path using the StreamGeometry.
                leftTriangle.Data = geometryLT;

                myGrid.Children.Add(leftTriangle);

                // add right triangle ///////////////////
                // Create a path to draw a geometry with.
                Path rightTriangle = new Path();
                rightTriangle.Stroke = Brushes.Black;
                rightTriangle.StrokeThickness = 1;
                rightTriangle.Fill = Brushes.Black;

                // Create a StreamGeometry to use to specify myPath.
                StreamGeometry geometryRT = new StreamGeometry();
                geometryRT.FillRule = FillRule.EvenOdd;

                // Open a StreamGeometryContext that can be used to describe this StreamGeometry 
                // object's contents.
                using (StreamGeometryContext ctx = geometryRT.Open())
                {

                    // Begin the triangle at the point specified. Notice that the shape is set to 
                    // be closed so only two lines need to be specified below to make the triangle.
                    ctx.BeginFigure(new Point(60 + state.Length * 30, 14), true /* is filled */, true /* is closed */);

                    // Draw a line to the next specified point.
                    ctx.LineTo(new Point(60 + state.Length * 30, 22), true /* is stroked */, false /* is smooth join */);

                    // Draw another line to the next specified point.
                    ctx.LineTo(new Point(67 + state.Length * 30, 18), true /* is stroked */, false /* is smooth join */);
                }

                // Freeze the geometry (make it unmodifiable)
                // for additional performance benefits.
                geometryRT.Freeze();

                // Specify the shape (triangle) of the Path using the StreamGeometry.
                rightTriangle.Data = geometryRT;

                // Data="M180,14 L180,22 L187,18 Z"
                myGrid.Children.Add(rightTriangle);



                TextBox[] myTextBoxes = new TextBox[state.Length];
                Grid[] myGrids = new Grid[state.Length];
                Ellipse[] myEllipses = new Ellipse[state.Length];
                Line[] myLines = new Line[state.Length];

                // add TextBoxes
                int i;
                double left;
                for (i = 0; i < state.Length; i++)
                {
                    // add textboxes
                    left = (double)i * 30 + 20;
                    myTextBoxes[i] = new TextBox();
                    myTextBoxes[i].Margin = new Thickness(left, 3, 0, 0);
                    myTextBoxes[i].Width = 30;
                    myTextBoxes[i].Height = 30;
                    myTextBoxes[i].HorizontalAlignment = HorizontalAlignment.Left;
                    myTextBoxes[i].VerticalAlignment = VerticalAlignment.Top;
                    myTextBoxes[i].Name = "textBoxBit" + i;
                    myTextBoxes[i].Visibility = Visibility.Visible;
                    myTextBoxes[i].BorderThickness = new Thickness(1);
                    myTextBoxes[i].IsReadOnly = true;
                    myTextBoxes[i].TextAlignment = TextAlignment.Center;
                    myTextBoxes[i].VerticalContentAlignment = VerticalAlignment.Center;
                    myTextBoxes[i].BorderBrush = Brushes.Black;

                    myGrid.Children.Add(myTextBoxes[i]);

                    // add XORs
                    myGrids[i] = new Grid();
                    myGrids[i].Name = "XORGrid" + i;
                    myGrids[i].Height = 30;
                    myGrids[i].Width = 30;
                    myGrids[i].HorizontalAlignment = HorizontalAlignment.Left;
                    myGrids[i].VerticalAlignment = VerticalAlignment.Top;
                    myGrids[i].Margin = new Thickness(left, 32, 0, 0);

                    myGrid.Children.Add(myGrids[i]);

                    if (tapSequence[i] == '0') myGrids[i].Visibility = Visibility.Hidden;
                    else
                    {
                        myEllipses[i] = new Ellipse();
                        myEllipses[i].Name = "ellipseXOR" + i;
                        myEllipses[i].Stroke = Brushes.Black;
                        myEllipses[i].Margin = new Thickness(7, 7, 7, 7);

                        myLines[i] = new Line();
                        myLines[i].Name = "VertLineXOR" + i;
                        myLines[i].Stroke = Brushes.Black;
                        myLines[i].StrokeThickness = 1;
                        myLines[i].X1 = 15;
                        myLines[i].Y1 = 0;
                        myLines[i].X2 = 15;
                        myLines[i].Y2 = 22;

                        myGrids[i].Children.Add(myEllipses[i]);
                        myGrids[i].Children.Add(myLines[i]);
                    }
                }
                // disable last XOR
                myGrids[i-1].Visibility = Visibility.Hidden;

                // add output bit label
                Label outPutLabel = new Label();
                left = (double)i * 30 + 65;
                outPutLabel.Margin = new Thickness(left, 3, 0, 0);
                outPutLabel.Width = 30;
                outPutLabel.Height = 30;
                outPutLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
                outPutLabel.VerticalContentAlignment = VerticalAlignment.Center;
                outPutLabel.HorizontalAlignment = HorizontalAlignment.Left;
                outPutLabel.VerticalAlignment = VerticalAlignment.Top;
                myGrid.Children.Add(outPutLabel);

            }, null);

        }

        public void FillBoxes(char[] state, char[] tapSequence, char output)
        {
            // fill the boxes with current state
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                // get the textboxes as children of myGrid. textboxes are 7, 9, 11, ...
                Visual childVisual;
                int i;
                for (i = 0; i < state.Length; i++)
                {
                    childVisual = (Visual)VisualTreeHelper.GetChild(myGrid, 7 + i * 2);
                    childVisual.SetValue(TextBox.TextProperty, state[i].ToString());
                }

                // update output label
                childVisual = (Visual)VisualTreeHelper.GetChild(myGrid, 9 + (i-1) * 2);
                childVisual.SetValue(Label.ContentProperty, output);
            }, null);
        }

        public void DeleteAll(int end)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                // remove all elements except infoText beginning with the second child
                myGrid.Children.RemoveRange(1, end);

                // show initial infoText again
                infoText.Visibility = Visibility.Visible;
            }, null);
        }
    }
}
