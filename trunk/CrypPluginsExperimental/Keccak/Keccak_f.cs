#define _DEBUG_

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;
using System.Windows;

namespace Cryptool.Plugins.Keccak
{
    public class Keccak_f
    {
        #region variables

        private const int X = 5;
        private const int Y = 5;

        private int z, l;             // 2^l = z = lane size
        private int rounds;

        private KeccakPres pres;

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

        public Keccak_f(int stateSize, ref byte[] state, ref KeccakPres pres)
        {
            Debug.Assert(stateSize % 25 == 0);
            z = stateSize / 25;                     // length of a lane
            l = (int)Math.Log((double)z, 2);        // parameter l

            Debug.Assert((int)Math.Pow(2, l) == z);
            rounds = 12 + 2 * l;
            this.pres = pres;
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

                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.labelRound.Content = (i+1).ToString() + "/" + rounds;
                }, null);

                Theta(ref state);
                Rho(ref state);
                Pi(ref state);

                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.labelCurrentStep.Content = "Chi";
                }, null);

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

            //pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            //{
            //    pres.label1.Content = "Theta";
            //}, null);

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
            byte[] oldRow = new byte[X];

            GetRowsFromState(ref state);

            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                #region switch cube presentation
                switch (z)
                {
                    case 4:
                        pres.imgCube4z.Visibility = Visibility.Visible;
                        pres.imgCube4z.Opacity = 1;
                        break;

                    case 8:
                    case 16:
                    case 32:
                    case 64:
                        pres.imgCubeDefault.Visibility = Visibility.Visible;
                        pres.imgCubeDefault.Opacity = 1;
                        break;

                    default:
                        break;
                }

                #endregion
                
                pres.canvasCube.Visibility = Visibility.Visible;
                pres.canvasStepDetailChi.Visibility = Visibility.Visible;
            }, null);

            for (int i = 0; i < z; i++)             // iterate over slices
            {
                for (int j = 0; j < Y; j++)         // iterate over rows of a slice
                {
                    for (int k = 0; k < X; k++)     // save old value of row
                    {
                        oldRow[k] = rows[i][j][k];
                    }
                    for (int k = 0; k < X; k++)     // iterate over bits of a row
                    {
                        /* the inverting has to be calculated manually. Since a byte represents a bit, the "~"-operator would lead to false results here. */
                        inv = oldRow[(k + 1) % X];
                        inv = (byte)(inv == 0x00 ? 0x01 : 0x00);

                        rows[i][j][k] = (byte)(oldRow[k] ^ (inv & oldRow[(k + 2) % X]));
                    }                   

                    ChiPres(oldRow, i, j);
                }
            }

            /* write back to state */
            SetRowsToState(ref state);

            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                #region switch cube presentation
                switch (z)
                {
                    case 4:
                        pres.imgCube4z.Visibility = Visibility.Hidden;
                        break;

                    case 8:
                    case 16:
                    case 32:
                    case 64:
                        pres.imgCubeDefault.Visibility = Visibility.Hidden;
                        break;

                    default:
                        break;
                }

                #endregion

                pres.canvasCube.Visibility = Visibility.Hidden;
                pres.canvasStepDetailChi.Visibility = Visibility.Hidden;
            }, null);



            pres.autostep = false;
            pres.skip = false;
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

            //pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            //{
            //    pres.label1.Content = "Pi";
            //}, null);

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

            //pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            //{
            //    pres.label1.Content = "Rho";
            //}, null);

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

            //pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            //{
            //    pres.label1.Content = "Iota";
            //}, null);

            /* xor round constant */
            for (int i = 0; i < z; i++)
            {
                firstLane[i] ^= truncatedConstant[i];
            }

            SetFirstLaneToState(ref state, firstLane);
        }

        #endregion

        #region presentation methods

        public void ChiPres(byte[] oldRow, int i, int j)
        {
            if (!pres.runToEnd && !pres.skip)
            {
                #region switch different lane sizes

                switch (z)
                {
                    #region lane size 4

                    case 4:
                        
                        /* show slice and row indexes */
                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.labelSlice.Content = (i + 1).ToString();
                            pres.labelRow.Content = (j + 1).ToString();

                            /* move modified row */
                            double modifiedRowTop = 155 - j * 26 - i * 13;
                            double modifiedRowLeft = 138 + i * 13;
                            pres.imgModifiedRow.SetValue(Canvas.TopProperty, modifiedRowTop);
                            pres.imgModifiedRow.SetValue(Canvas.LeftProperty, modifiedRowLeft);

                            /* move first row and toggle visibility*/
                            if (i == 0)
                            {
                                double modifiedFirstRowTop = 167 - j * 26;
                                pres.imgModifiedFirstRow.Visibility = Visibility.Visible;
                                pres.imgModifiedFirstRow.SetValue(Canvas.TopProperty, modifiedFirstRowTop);
                            }
                            else
                            {
                                pres.imgModifiedFirstRow.Visibility = Visibility.Hidden;
                            }

                            /* toggle visibility top row */
                            if (j == 4)
                            {
                                pres.imgModifiedTopRow.SetValue(Canvas.TopProperty, modifiedRowTop - 1);
                                pres.imgModifiedTopRow.SetValue(Canvas.LeftProperty, modifiedRowLeft - 130);
                                pres.imgModifiedTopRow.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                pres.imgModifiedTopRow.Visibility = Visibility.Hidden;
                            }

                        }, null);

                        break;

                    #endregion

                    #region lane size greater than 4

                    case 8:
                    case 16:
                    case 32:
                    case 64:

                        /* show slice and row indexes */
                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            pres.labelSlice.Content = (i + 1).ToString();
                            pres.labelRow.Content = (j + 1).ToString();

                            double modifiedRowTop = 0;
                            double modifiedRowLeft = 0;

                            /* move first row and toggle visibility*/
                            if (i == 0)
                            {
                                /* show cube */
                                pres.imgCubeDefault.Visibility = Visibility.Visible;
                                pres.imgCubeDefaultInner.Visibility = Visibility.Hidden;

                                double modifiedFirstRowTop = 167 - j * 26;
                                modifiedRowTop = 155 - j * 26;
                                modifiedRowLeft = 138;
                                pres.imgModifiedFirstRow.SetValue(Canvas.TopProperty, modifiedFirstRowTop);
                                pres.imgModifiedRow.SetValue(Canvas.TopProperty, modifiedRowTop);
                                pres.imgModifiedRow.SetValue(Canvas.LeftProperty, modifiedRowLeft);
                                pres.imgModifiedFirstRow.Visibility = Visibility.Visible;
                            }

                            /* move modified row only */
                            else if (i > 0)
                            {
                                pres.imgModifiedFirstRow.Visibility = Visibility.Hidden;

                                modifiedRowTop = 142 - j * 26;
                                modifiedRowLeft = 151;
                                pres.imgModifiedRow.SetValue(Canvas.TopProperty, modifiedRowTop);
                                pres.imgModifiedRow.SetValue(Canvas.LeftProperty, modifiedRowLeft);

                                /* change to fading cube */
                                if (i == 2)
                                {
                                    pres.imgCubeDefault.Visibility = Visibility.Hidden;
                                    pres.imgCubeDefaultInner.Visibility = Visibility.Visible;
                                }
                            }

                            /* toggle visibility top row */
                            if (j == 4)
                            {
                                pres.imgModifiedTopRow.SetValue(Canvas.TopProperty, modifiedRowTop - 1);
                                pres.imgModifiedTopRow.SetValue(Canvas.LeftProperty, modifiedRowLeft - 130);
                                pres.imgModifiedTopRow.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                pres.imgModifiedTopRow.Visibility = Visibility.Hidden;
                            }

                        }, null);

                        break;  
                                  
                    #endregion
                    
                    default:
                        break;
                }

                

                #endregion

                /* presentation detailed step */
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.label1.Content = oldRow[0].ToString();
                    pres.label2.Content = oldRow[1].ToString();
                    pres.label3.Content = oldRow[2].ToString();
                    pres.label4.Content = oldRow[3].ToString();
                    pres.label5.Content = oldRow[4].ToString();

                    pres.label6.Content = rows[i][j][0].ToString();
                    pres.label7.Content = rows[i][j][1].ToString();
                    pres.label8.Content = rows[i][j][2].ToString();
                    pres.label9.Content = rows[i][j][3].ToString();
                    pres.label10.Content = rows[i][j][4].ToString();
                }, null);

                /* wait for button clicks */
                if (!pres.autostep)
                {
                    AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }
                /* sleep between steps, if autostep was clicked */
                else
                {
                    System.Threading.Thread.Sleep(300);       // value adjustable by a slider
                }
            }

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
