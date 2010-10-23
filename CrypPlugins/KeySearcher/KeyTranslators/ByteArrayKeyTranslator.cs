/*                              
   Copyright 2010 Sven Rech (svenrech at googlemail dot com), Uni Duisburg-Essen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Control;
using KeySearcher.KeyPattern;

namespace KeySearcher.KeyTranslators
{
    /// <summary>
    /// Implements a simple translator for bytearray keys that are represented by the pattern "[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-...".
    /// Is used by AES and DES.
    /// </summary>
    public class ByteArrayKeyTranslator : KeyTranslator
    {
        private int progress = 0;
        private KeyPattern.KeyPattern pattern;
        private int[] movementStatus;
        private byte[] keya;
        private int[] movementPointers;
        private KeyMovement[] keyMovements;

        #region KeyTranslator Members

        public void SetKeys(object keys)
        {
            if (!(keys is KeyPattern.KeyPattern))
                throw new Exception("Something went horribly wrong!");

            pattern = (KeyPattern.KeyPattern)keys;

            keyMovements = pattern.getKeyMovements();
            movementStatus = pattern.getWildcardProgress();
            if (movementStatus.Length != keyMovements.Length)
                throw new Exception("Movements and Wildcards do not fit together!");
            
            movementPointers = new int[movementStatus.Length];
            int mpc = 0;

            byte[] keya2 = new byte[256];
            int kc = 0;
            string wildcardKey = pattern.WildcardKey;
            int i = 0;

            bool first = true;

            while (i < wildcardKey.Length)
            {
                if (wildcardKey[i] == '[')
                {
                    i++;
                    while (wildcardKey[i++] != ']') ;
                    i--;

                    movementPointers[mpc++] = kc++;

                    first = false;
                }
                else if (wildcardKey[i] == '-')
                {
                    first = true;
                }
                else
                {
                    byte val = Convert.ToByte(""+wildcardKey[i], 16);
                    if (first)
                        keya2[kc/2] = (byte)((int)val << 4);
                    else
                        keya2[kc/2] |= val;

                    kc++;

                    first = false;
                }

                i++;
            }

            keya = new byte[kc/2];
            for (int c = 0; c < (kc/2); c++)
                keya[c] = keya2[c];

            for (int x = 0; x < movementStatus.Length - 1; x++)
                setWildcard(x);
        }

        public byte[] GetKey()
        {
            return keya;
        }

        public bool NextKey()
        {
            int i = movementStatus.Length - 1;

            movementStatus[i]++;
            
            while (i >= 0 && !wildcardInRange(i))
            {                
                movementStatus[i] = 0;
                setWildcard(i);

                i--;
                if (i >= 0)
                    movementStatus[i]++;          
            }

            if (i >= 0)
                setWildcard(i);

            progress++;

            return i >= 0;
        }

        public string GetKeyRepresentation()
        {
            return pattern.getKey(progress);
        }

        /// <summary>
        /// Sets wildcard i in keya to the current progress
        /// </summary>
        /// <param name="i">the wildcard index</param>
        private void setWildcard(int i)
        {
            int index = movementPointers[i] / 2;
            byte mask;
            byte shift;
            if (movementPointers[i] % 2 == 0)
            {
                mask = 1+2+4+8;
                shift = 4;
            }
            else
            {
                mask = 16+32+64+128;
                shift = 0;
            }

            keya[index] = (byte)((keya[index] & mask) | (calcWildcard(i) << shift));
        }

        private int calcWildcard(int i)
        {
            KeyMovement mov = keyMovements[i];
            if (mov is LinearKeyMovement)
            {
                return movementStatus[i] * (mov as LinearKeyMovement).A + (mov as LinearKeyMovement).B;
            }
            else if (mov is IntervalKeyMovement)
            {
                return (mov as IntervalKeyMovement).IntervalList[movementStatus[i]];
            }

            throw new Exception("Movement not implemented!");
        }

        private bool wildcardInRange(int i)
        {
            KeyMovement mov = keyMovements[i];
            if (mov is LinearKeyMovement)
            {
                return movementStatus[i] < (mov as LinearKeyMovement).UpperBound;
            }
            else if (mov is IntervalKeyMovement)
            {
                return movementStatus[i] < (mov as IntervalKeyMovement).IntervalList.Count;
            }

            return false;            
        }

        public int GetProgress()
        {
            int result = progress;
            pattern.addKey(progress);

            progress = 0;
            return result;
        }

        #endregion

    }
}
