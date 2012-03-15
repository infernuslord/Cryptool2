using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeyTextBox
{
    public interface IKeyManager
    {
        event KeyChangedHandler OnKeyChanged;

        void SetKey(string key);
        string GetKey();
        string GetFormat();
    }

    public delegate void KeyChangedHandler(string key);
}
