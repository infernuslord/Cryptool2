using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Tool;
using Cryptool.PluginBase;
using Primes.WpfVisualization;

namespace Primes
{
  [Author("Timo Eckhardt", "T-Eckhardt@gmx.de", "Uni Siegen", "http://www.uni-siegen.de")]
  [PluginInfo("Primes.Properties.Resources", false, "Primes", "Primes", "", "Primes/icon.png")] 
  public class PrimesPlugin:ITool
  {
    #region IPlugin Members
    private PrimesControl m_PrimesPlugin = null;
    public event Cryptool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;

    public event Cryptool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

    public event Cryptool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;

    public Cryptool.PluginBase.ISettings Settings
    {
      get { return null; }
    }

    public System.Windows.Controls.UserControl Presentation
    {
      get
      {
        if (m_PrimesPlugin == null) m_PrimesPlugin = new PrimesControl();
        return m_PrimesPlugin;
      }
    }

    public System.Windows.Controls.UserControl QuickWatchPresentation
    {
      get { throw new NotImplementedException(); }
    }

    public void PreExecution()
    {
    }

    public void Execute()
    {
    }

    public void PostExecution()
    {
    }

    public void Pause()
    {
    }

    public void Stop()
    {
    }

    public void Initialize()
    {
    }

    public void Dispose()
    {
      if (m_PrimesPlugin == null) m_PrimesPlugin.Dispose();

    }

    #endregion

    #region INotifyPropertyChanged Members

    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
