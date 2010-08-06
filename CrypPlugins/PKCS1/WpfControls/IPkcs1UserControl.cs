using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PKCS1.WpfControls
{
    interface IPkcs1UserControl
    {
        void Dispose();
        void Init();
        void SetTab(int i);
    }
}
