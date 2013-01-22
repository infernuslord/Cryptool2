using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Primes.WpfControls.NumberTheory.PowerMod
{
    public class PowerModCommands
    {
        public static RoutedUICommand ReOrderPointsCommand = new RoutedUICommand
          ("ReOrderPointsCommand", "ReOrderPointsCommand", typeof(PowerModControl));
    }
}