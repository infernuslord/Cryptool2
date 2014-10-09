using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Transcriptor
{
    class SignComparer : IComparer<Sign>
    {
        bool mode;

        public SignComparer(bool mode)
        {
            this.mode = mode;
        }

        public int Compare(Sign a, Sign b)
        {
            if (mode == true)
            {
                if (a.X < b.X)
                {
                    return -1;
                }
                else
                {
                    if (a.X == b.X)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }
            else
            {
                if (a.Y < b.Y)
                {
                    return -1;
                }
                else
                {
                    if (a.X == b.X)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }

                }
            }
        }
    }
}
