#define _DEBUG_

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Cryptool.Plugins.Keccak
{
    public class Keccak_f
    {
        #region variables

        private const int X = 5;
        private const int Y = 5;

        private int z, l;             // 2^l = z = lane size
        private int rounds;


        private byte[][][] columns;
        private byte[][][] rows;
        private byte[][][] lanes;


        /* translation vectors for rho */
        private int[][] translationVectors = new int[][] 
        { 
            new int[] { 0, 1, 190, 28, 91 }, 
            new int[] { 36, 300, 6, 55, 276 }, 
            new int[] { 3, 10, 171, 153, 231 }, 
            new int[] { 105, 45, 15, 21, 136 }, 
            new int[] { 210, 66, 253, 120, 78 }
        };

        /* round constants for iota */
        private byte[][] roundConstants = new byte[][]
        {
            new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x82, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x8a, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x00, 0x80, 0x00, 0x80, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x8b, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x01, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x81, 0x80, 0x00, 0x80, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x09, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x8a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x88, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x09, 0x80, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x0a, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x8b, 0x80, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x8b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x89, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x03, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x02, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x0a, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x0a, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x81, 0x80, 0x00, 0x80, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x80, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 },
            new byte[] { 0x01, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x08, 0x80, 0x00, 0x80, 0x00, 0x00, 0x00, 0x80 },
        };

        #endregion

        public Keccak_f(int stateSize, ref byte[] state)
        {
            Debug.Assert(stateSize % 25 == 0);
            z = stateSize / 25;                     // length of a lane
            l = (int)Math.Log((double)z, 2);        // parameter l

            Debug.Assert((int)Math.Pow(2, l) == z);
            rounds = 12 + 2 * l;
        }

        public void Permute(ref byte[] state)
        {
            /* the order of steps is taken from the pseudo-code description at http://keccak.noekeon.org/specs_summary.html (accessed on 2013-02-01) */

#if _DEBUG_
            Console.WriteLine("#Keccak-f: start Keccak-f[{0}] with {1} rounds", z * 25, rounds);
            Console.WriteLine("#Keccak-f: state before permutation:");
            KeccakHashFunction.PrintBits(state, z);
            KeccakHashFunction.PrintBytes(state, z);
#endif

            for (int i = 0; i < rounds; i++)
            {
#if _VERBOSE_
                Console.WriteLine("\nRound {0}", i + 1);
                Console.WriteLine("State before Keccak-f[{0}]", z * 25);
                KeccakHashFunction.PrintBits(state, z);
                KeccakHashFunction.PrintBytes(state, z);
#endif

                Theta(ref state);
                Rho(ref state);
                Pi(ref state);
                Chi(ref state);
                Iota(ref state, i);
            }

#if _DEBUG_
            Console.WriteLine("\n#Keccak-f: state after permutation");
            KeccakHashFunction.PrintBits(state, z);
            KeccakHashFunction.PrintBytes(state, z);
#endif

        }

        #region step functions

        public void Theta(ref byte[] state)
        {
            byte parity1, parity2, parityResult;
            byte[][][] tmpColumns;

            GetColumnsFromState(ref state);

            // clone `columns` in `tmpColumns`
            tmpColumns = new byte[z][][];
            for (int i = 0; i < z; i++)
            {
                tmpColumns[i] = new byte[X][];
                for (int j = 0; j < X; j++)
                {
                    tmpColumns[i][j] = new byte[Y];
                    for (int k = 0; k < Y; k++)
                    {
                        tmpColumns[i][j][k] = columns[i][j][k];
                    }
                }
            }


            for (int i = 0; i < z; i++)          // iterate over slices
            {
                for (int j = 0; j < X; j++)      // iterate over columns of a slice
                {
                    /* xor the parities of two certain nearby columns */
                    parity1 = Parity(columns[i][(X + j - 1) % X]);              // add X because if j = 0 the reult would be negative
                    parity2 = Parity(columns[(z + i - 1) % z][(j + 1) % X]);    // same here with z
                    parityResult = (byte)(parity1 ^ parity2);

                    for (int k = 0; k < Y; k++)  // iterate over bits of a column
                    {
                        tmpColumns[i][j][k] ^= parityResult;
                    }
                }
            }

            columns = tmpColumns;

            SetColumnsToState(ref state);
        }

        public void Chi(ref byte[] state)
        {
            byte inv;
            byte[] row = new byte[X];

            GetRowsFromState(ref state);

            for (int i = 0; i < z; i++)             // iterate over slices
            {
                for (int j = 0; j < Y; j++)         // iterate over rows of a slice
                {
                    for (int k = 0; k < X; k++)     // save old value of row
                    {
                        row[k] = rows[i][j][k];
                    }
                    for (int k = 0; k < X; k++)     // iterate over bits of a row
                    {
                        /* the inverting has to be calculated manually. Since a byte represents a bit, the "~"-operator would lead to false results here. */
                        inv = row[(k + 1) % X];
                        inv = (byte)(inv == 0x00 ? 0x01 : 0x00);

                        rows[i][j][k] = (byte)(row[k] ^ (inv & row[(k + 2) % X]));
                    }
                }
            }

            /* write back to state */
            SetRowsToState(ref state);
        }

        public void Pi(ref byte[] state)
        {
            byte[][][] tmpLanes;

            // init `tmpLanes`
            tmpLanes = new byte[Y][][];
            for (int i = 0; i < Y; i++)
            {
                tmpLanes[i] = new byte[X][];
                for (int j = 0; j < X; j++)
                {
                    tmpLanes[i][j] = new byte[z];
                }
            }

            GetLanesFromState(ref state);

            /* rearrange lanes in a certain pattern */
            for (int i = 0; i < Y; i++)         // iterate over planes
            {
                for (int j = 0; j < X; j++)     // iterate over lanes of a plane
                {
                    tmpLanes[(2 * j + 3 * i) % X][i] = lanes[i][j];
                }
            }

            lanes = tmpLanes;
            SetLanesToState(ref state);
        }

        public void Rho(ref byte[] state)
        {
            /* do nothing when lane size is 1 */
            if (z == 1) return;

            GetLanesFromState(ref state);

            /* rotate lanes by a certain value */
            for (int i = 0; i < Y; i++)         // iterate over planes
            {
                for (int j = 0; j < X; j++)     // iterate over lanes of a plane
                {
                    lanes[i][j] = RotateByteArray(lanes[i][j], translationVectors[i][j] % z);
                }
            }

            SetLanesToState(ref state);
        }

        public void Iota(ref byte[] state, int round)
        {
            /* map round constant bits to bytes */
            byte[] constant = KeccakHashFunction.ByteArrayToBitArray(roundConstants[round]);

            /* truncate constant to the size of z */
            byte[] truncatedConstant = KeccakHashFunction.SubArray(constant, 0, z);

            byte[] firstLane = GetFirstLaneFromState(ref state);

            /* xor round constant */
            for (int i = 0; i < z; i++)
            {
                firstLane[i] ^= truncatedConstant[i];
            }

            SetFirstLaneToState(ref state, firstLane);
        }

        #endregion

        #region helper methods

        /* returns the first lane from state (needed by iota step) */
        public byte[] GetFirstLaneFromState(ref byte[] state)
        {
            return KeccakHashFunction.SubArray(state, 0, z);
        }

        /* sets the first lane of the state (needed by iota step) */
        public void SetFirstLaneToState(ref byte[] state, byte[] firstLane)
        {
            Array.Copy(firstLane, 0, state, 0, z);
        }

        /**
        * transforms the state to a lane-wise representation of the state 
        * */
        public void GetLanesFromState(ref byte[] state)
        {
            lanes = new byte[Y][][];
            for (int i = 0; i < Y; i++)         // iterate over y coordinate
            {
                lanes[i] = new byte[X][];
                for (int j = 0; j < X; j++)     // iterate over x coordinate
                {
                    lanes[i][j] = new byte[z];
                    lanes[i][j] = KeccakHashFunction.SubArray(state, (i * 5 + j) * z, z);
                }
            }
        }

        /**
            * sets the state from the lane-wise representation of the state
            * */
        public void SetLanesToState(ref byte[] state)
        {
            for (int i = 0; i < Y; i++)         // iterate over y coordinate
            {
                for (int j = 0; j < X; j++)     // iterate over x coordinate
                {
                    Array.Copy(lanes[i][j], 0, state, (i * 5 + j) * z, z);
                }
            }
        }

        /**
            * transforms the state to a column-wise representation of the state 
            * */
        public void GetColumnsFromState(ref byte[] state)
        {
            columns = new byte[z][][];
            for (int i = 0; i < z; i++)         // iterate over z coordinate
            {
                columns[i] = new byte[X][];
                for (int j = 0; j < X; j++)     // iterate over x coordinate
                {
                    columns[i][j] = new byte[Y];
                    for (int k = 0; k < Y; k++) // iterate over y coordinate
                    {
                        columns[i][j][k] = state[(j * z) + k * (X * z) + i];
                    }
                }
            }
        }

        /**
            * sets the state from the column-wise representation of the state
            * */
        public void SetColumnsToState(ref byte[] state)
        {
            for (int i = 0; i < z; i++)         // iterate over z coordinate
            {
                for (int j = 0; j < X; j++)     // iterate over x coordinate
                {
                    for (int k = 0; k < Y; k++) // iterate over y coordinate
                    {
                        state[(j * z) + k * (X * z) + i] = columns[i][j][k];
                    }
                }
            }
        }

        /**
            * transforms the state to a row-wise representation of the state 
            * */
        public void GetRowsFromState(ref byte[] state)
        {
            rows = new byte[z][][];
            for (int i = 0; i < z; i++)         // iterate over z coordinate
            {
                rows[i] = new byte[Y][];
                for (int j = 0; j < Y; j++)     // iterate over y coordinate
                {
                    rows[i][j] = new byte[X];
                    for (int k = 0; k < X; k++) // iterate over x coordinate
                    {
                        rows[i][j][k] = state[(k * z) + j * (X * z) + i];
                    }
                }
            }
        }

        /**
            * sets the state from the row-wise representation of the state
            * */
        public void SetRowsToState(ref byte[] state)
        {
            for (int i = 0; i < z; i++)         // iterate over z coordinate
            {
                for (int j = 0; j < Y; j++)     // iterate over y coordinate
                {
                    for (int k = 0; k < X; k++) // iterate over x coordinate
                    {
                        state[(k * z) + j * (X * z) + i] = rows[i][j][k];
                    }
                }
            }
        }

        /**
            * rotates an array of bytes by a given value (used by Rho)
            * */
        public byte[] RotateByteArray(byte[] lane, int value)
        {
            byte[] tmpLane = new byte[z];

            Array.Copy(lane, 0, tmpLane, value, lane.Length - value);
            Array.Copy(lane, lane.Length - value, tmpLane, 0, value);

            return tmpLane;
        }


        /**
            * computes the parity bit of a byte array
            * */
        public byte Parity(byte[] column)
        {
            byte parity = 0x00;

            for (int i = 0; i < Y; i++)
            {
                parity ^= column[i];
            }

            return parity;
        }

        #endregion
    }
}
