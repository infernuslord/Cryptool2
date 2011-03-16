using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Cryptool.P2PEditor.GUI
{
    class ParticipateAdorner : Adorner
    {
        public ParticipateAdorner(UIElement adornedElement) : base(adornedElement)
        {
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var text = new FormattedText(Properties.Resources.Participating_, Thread.CurrentThread.CurrentUICulture,
                FlowDirection.LeftToRight, new Typeface("Arial"), 26.0, Brushes.Black);
            var startPoint = new Point((DesiredSize.Width / 2 - text.Width / 2), (DesiredSize.Height / 2 - text.Height / 2));
            drawingContext.DrawText(text, startPoint);

            drawingContext.DrawRectangle((Brush)new BrushConverter().ConvertFromString("#64D4D4D4"),
                                         new Pen(Brushes.Gray, 1), new Rect(new Point(0, 0), DesiredSize));
            base.OnRender(drawingContext);
        }
    }
}
