using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PKCS1.Library
{
    public delegate void Navigate(NavigationCommandType type);
    public delegate void ParamChanged(ParameterChangeType type);
    public delegate void SigGenerated(SignatureType type);
    public delegate void VoidDelegate();
}
