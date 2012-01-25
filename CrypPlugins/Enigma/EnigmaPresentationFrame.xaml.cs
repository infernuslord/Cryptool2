using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace Cryptool.Enigma
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class EnigmaPresentationFrame : UserControl
    {
        private Enigma facade;
        public EnigmaPresentation EnigmaPresentation;
        public EnigmaPresentationFrame(Enigma facade)
        {
            this.facade = facade;
            InitializeComponent();

            EnigmaPresentation =  new EnigmaPresentation(facade);
            dockPanel1.Children.Add(EnigmaPresentation);


            Binding disableBoolBinding = new Binding("DisabledBoolProperty");
            disableBoolBinding.Mode = BindingMode.TwoWay;
            disableBoolBinding.Source = EnigmaPresentation.PresentationDisabled;
            visbileCheckbox.SetBinding(CheckBox.IsCheckedProperty, disableBoolBinding);

            Binding myBinding2 = new Binding("IsChecked");
            myBinding2.Source = visbileCheckbox;
            myBinding2.Mode = BindingMode.TwoWay;
            BooleanToVisibilityConverter booleanToHidden = new BooleanToVisibilityConverter();
            myBinding2.Converter = booleanToHidden;
            EnigmaPresentation.SetBinding(TextBox.VisibilityProperty, myBinding2);

            this.IsVisibleChanged += new DependencyPropertyChangedEventHandler(EnigmaPresentationFrame_IsVisibleChanged);




        }

        public class BooleanToVisibilityConverter : IValueConverter
        {

            public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
            {
                if (targetType == typeof(Visibility))
                {
                    var visible = System.Convert.ToBoolean(value, culture);
                    if (InvertVisibility)
                        visible = !visible;
                    return visible ? Visibility.Visible : Visibility.Hidden;
                }
                throw new InvalidOperationException("Converter can only convert to value of type Visibility.");
            }

            public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
            {
                throw new InvalidOperationException("Converter cannot convert back.");
            }

            public Boolean InvertVisibility { get; set; }

        }

        public void ChangeStatus(Boolean isrunning, Boolean isvisible)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
                                                                                  {
                                                                                        EnigmaPresentation.disablePresentation(isrunning,isvisible);
                                                                                      if(isrunning && isvisible)
                                                                                      {
                                                                                          enigmaStatus.Text = "Präsentation aktiv";
                                                                                          enigmaStatus.Background = Brushes.LawnGreen;
                                                                                      }
                                                                                      else if (!isrunning && isvisible)
                                                                                      {
                                                                                          enigmaStatus.Text = "Präsentation bereit";
                                                                                          enigmaStatus.Background = Brushes.LawnGreen;
                                                                                      }
                                                                                      else if (isrunning && !isvisible)
                                                                                      {
                                                                                          enigmaStatus.Text = "Workspace aktiv, Präsentation abgeschaltet";
                                                                                          visbileCheckbox.IsChecked = false;
                                                                                          enigmaStatus.Background = Brushes.Tomato;
                                                                                      }
                                                                                      else
                                                                                      {
                                                                                          enigmaStatus.Text = "Präsentation abgeschaltet";
                                                                                          enigmaStatus.Background = Brushes.Tomato;
                                                                                      }
                                                                                  }, null);
        }

        private void EnigmaPresentationFrame_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //if(!this.IsVisible)
            //EnigmaPresentation.giveFeedbackAndDie();
        }

        private void visbileCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            
            enigmaStatus.Text = "Präsentation abgeschaltet";
            enigmaStatus.Background = Brushes.Tomato;

            EnigmaPresentation.giveFeedbackAndDie();
        }

        private void visbileCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (facade.isrunning)
            {
                enigmaStatus.Text = "Präsentation bereit, bitte starten Sie den Workspace neu";
            }
            else
            enigmaStatus.Text = "Präsentation bereit";
            enigmaStatus.Background = Brushes.LawnGreen;
        }

    }
}
