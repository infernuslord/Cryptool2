using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.PluginBase.Control
{
  public interface IControl
  {
  }

  //public static class ControlInterfaceList
  //{
  //  /// <summary>
  //  /// This is a static list with all the known control interfaces. Every time
  //  /// an control interface is added to PluginBase, it's name should be added here, too.
  //  /// The list is e.g. accessed by the attribute "ControlSpecificAttribute" to see if an
  //  /// given interface is valid or by the extension method "GetControlInterfaces".
  //  /// </summary>
  //  public static List<string> InterfaceNameList = new List<string>(
  //    new string[] 
  //    {
  //      typeof(Cryptool.PluginBase.Control.IControlEncryption).FullName
  //    }
  //  );

  //}
}
