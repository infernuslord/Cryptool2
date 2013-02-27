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

                if (!pres.runToEnd)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelRound.Content = (i + 1).ToString() + "/" + rounds;
                        pres.labelCurrentStep.Content = "Theta";
                    }, null);
                }

                Theta(ref state);

                if (!pres.runToEnd)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelCurrentStep.Content = "Rho";
                    }, null);
                }

                Rho(ref state);

                if (!pres.runToEnd)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelCurrentStep.Content = "Pi";
                    }, null);
                }

                Pi(ref state);

                if (!pres.runToEnd)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelCurrentStep.Content = "Chi";
                    }, null);
                }

                Chi(ref state);

                if (!pres.runToEnd)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelCurrentStep.Content = "Iota";
                    }, null);
                }

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

                    ThetaPres(columns[i][j], tmpColumns[i][j], columns[i][(X + j - 1) % X], columns[(z + i - 1) % z][(j + 1) % X], parity1, parity2, i, j);
                }
            }

            columns = tmpColumns;

            SetColumnsToState(ref state);

            pres.autostep = false;
            pres.skip = false;
        }

        public void Rho(ref byte[] state)
        {
            /* do nothing when lane size is 1 */
            if (z == 1) return;

            byte[] oldLane;
            int[][] translationVectorsPres = new int[5][];

            #region presentation translation vectors table

            if (!pres.runToEnd)
            {
                /* initialize translation vectors for presentation */
                for (int i = 0; i < 5; i++)
                {
                    translationVectorsPres[i] = new int[5];
                    for (int j = 0; j < 5; j++)
                    {
                        translationVectorsPres[i][j] = translationVectors[i][j] % z;
                    }
                }
                
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.label139.Content = translationVectorsPres[0][0].ToString();
                    pres.label140.Content = translationVectorsPres[0][1].ToString();
                    pres.label141.Content = translationVectorsPres[0][2].ToString();
                    pres.label142.Content = translationVectorsPres[0][3].ToString();
                    pres.label143.Content = translationVectorsPres[0][4].ToString();

                    pres.label144.Content = translationVectorsPres[1][0].ToString();
                    pres.label145.Content = translationVectorsPres[1][1].ToString();
                    pres.label146.Content = translationVectorsPres[1][2].ToString();
                    pres.label147.Content = translationVectorsPres[1][3].ToString();
                    pres.label148.Content = translationVectorsPres[1][4].ToString();

                    pres.label149.Content = translationVectorsPres[2][0].ToString();
                    pres.label150.Content = translationVectorsPres[2][1].ToString();
                    pres.label151.Content = translationVectorsPres[2][2].ToString();
                    pres.label152.Content = translationVectorsPres[2][3].ToString();
                    pres.label153.Content = translationVectorsPres[2][4].ToString();

                    pres.label154.Content = translationVectorsPres[3][0].ToString();
                    pres.label155.Content = translationVectorsPres[3][1].ToString();
                    pres.label156.Content = translationVectorsPres[3][2].ToString();
                    pres.label157.Content = translationVectorsPres[3][3].ToString();
                    pres.label158.Content = translationVectorsPres[3][4].ToString();

                    pres.label159.Content = translationVectorsPres[4][0].ToString();
                    pres.label160.Content = translationVectorsPres[4][1].ToString();
                    pres.label161.Content = translationVectorsPres[4][2].ToString();
                    pres.label162.Content = translationVectorsPres[4][3].ToString();
                    pres.label163.Content = translationVectorsPres[4][4].ToString();
                }, null);
            }
            #endregion

            GetLanesFromState(ref state);

            /* rotate lanes by a certain value */
            for (int i = 0; i < Y; i++)         // iterate over planes
            {
                for (int j = 0; j < X; j++)     // iterate over lanes of a plane
                {
                    oldLane = lanes[i][j];
                    lanes[i][j] = RotateByteArray(lanes[i][j], translationVectors[i][j] % z);
                    RhoPres(oldLane, lanes[i][j], i, j, translationVectors[i][j] % z);
                }
            }

            SetLanesToState(ref state);

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

            /* rearrange lanes in a certain pattern */
            for (int i = 0; i < Y; i++)         // iterate over planes
            {
                for (int j = 0; j < X; j++)     // iterate over lanes of a plane
                {
                    tmpLanes[(2 * j + 3 * i) % X][i] = lanes[i][j];
                }
            }

            PiPres(tmpLanes);

            lanes = tmpLanes;
            SetLanesToState(ref state);

            pres.autostep = false;
            pres.skip = false;
        }

        public void Chi(ref byte[] state)
        {
            byte inv;
            byte[] oldRow = new byte[X];

            GetRowsFromState(ref state);

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
            
            pres.autostep = false;
            pres.skip = false;
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
                //IotaPres(firstLane, truncatedConstant);
            }

            SetFirstLaneToState(ref state, firstLane);
        }

        #endregion

        #region presentation methods

        public void CubePres(Visibility vis)
        {
            switch (z)
            {
                case 4:
                    pres.imgCube4z.Visibility = vis;
                    break;

                case 8:
                case 16:
                case 32:
                case 64:
                    pres.imgCubeDefault.Visibility = vis;
                    break;

                default:
                    break;
            }
        }

        public void ThetaPres(byte[] columnOld, byte[] columnNew, byte[] columnLeft, byte[] columnRight, int parity1, int parity2, int slice, int column)
        {
            if (!pres.runToEnd && !pres.skip)
            {
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    /* show theta canvas in first iteration */
                    if (slice == 0 && column == 0)
                    {
                        pres.canvasStepDetailTheta.Visibility = Visibility.Visible;
                        pres.canvasCubeTheta.Visibility = Visibility.Visible;
                    }

                    /* show slice and column indexes */
                    pres.labelOuterPartCaption.Content = "Slice";
                    pres.labelOuterPart.Content = (slice + 1).ToString();
                    pres.labelInnerPartCaption.Content = "Column";
                    pres.labelInnerPart.Content = (column + 1).ToString();

                    #region pres cube

                    switch (z)
                    {
                        case 1:
                            /* TODO */
                            break;
                        case 2:
                            /* TODO */
                            break;
                        case 4:      
                            /* TODO */
                            break;

                        #region lane size greater than 4

                        case 8:
                        case 16:
                        case 32:
                        case 64:
                            
                            /* show cube */
                            if (slice == 0)
                            {
                                /* show default cube */
                                pres.imgCubeDefault.Visibility = Visibility.Visible;
                                pres.imgCubeDefaultInner.Visibility = Visibility.Hidden;
                            }
                            else if (slice == 2)
                            {
                                /* show inner cube */
                                pres.imgCubeDefault.Visibility = Visibility.Hidden;
                                pres.imgCubeDefaultInner.Visibility = Visibility.Visible;
                            }
                            else if (slice == z - 2)
                            {
                                /* show bottom cube */
                                pres.imgCubeDefaultInner.Visibility = Visibility.Hidden;
                                pres.imgCubeDefaultBottom.Visibility = Visibility.Visible;
                            }

                            /* move modified row */
                            if (slice == 0)
                            {
                                pres.imgThetaModifiedRowFront.SetValue(Canvas.LeftProperty, 7.0 + column * 26);
                                pres.imgThetaModifiedRowFront.Visibility = Visibility.Visible;
                                pres.imgThetaModifiedRowTop.SetValue(Canvas.LeftProperty, 8.0 + column * 26);
                                pres.imgThetaModifiedRowTop.SetValue(Canvas.TopProperty, 50.0);
                                pres.imgThetaModifiedRowTop.Visibility = Visibility.Visible;
                                if (column == 4)
                                {
                                    pres.imgThetaModifiedRowSide.SetValue(Canvas.LeftProperty, 137.0);
                                    pres.imgThetaModifiedRowSide.SetValue(Canvas.TopProperty, 51.0);
                                    pres.imgThetaModifiedRowSide.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    pres.imgThetaModifiedRowSide.Visibility = Visibility.Hidden;
                                }
                            }
                            else if (slice == 1)
                            {
                                pres.imgThetaModifiedRowFront.Visibility = Visibility.Hidden;
                                pres.imgThetaModifiedRowTop.SetValue(Canvas.LeftProperty, 21.0 + column * 26);
                                pres.imgThetaModifiedRowTop.SetValue(Canvas.TopProperty, 37.0);
                                if (column == 4)
                                {
                                    pres.imgThetaModifiedRowSide.SetValue(Canvas.LeftProperty, 150.0);
                                    pres.imgThetaModifiedRowSide.SetValue(Canvas.TopProperty, 38.0);
                                    pres.imgThetaModifiedRowSide.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    pres.imgThetaModifiedRowSide.Visibility = Visibility.Hidden;
                                }
                            }
                            else if (slice >= 2 && slice < z - 2)
                            {
                                pres.imgThetaModifiedRowTop.SetValue(Canvas.LeftProperty, 34.0 + column * 26);
                                pres.imgThetaModifiedRowTop.SetValue(Canvas.TopProperty, 24.0);
                                if (column == 4)
                                {
                                    pres.imgThetaModifiedRowSide.SetValue(Canvas.LeftProperty, 163.0);
                                    pres.imgThetaModifiedRowSide.SetValue(Canvas.TopProperty, 25.0);
                                    pres.imgThetaModifiedRowSide.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    pres.imgThetaModifiedRowSide.Visibility = Visibility.Hidden;
                                }
                            }
                            else // slice >= z - 2, last two slices
                            {
                                pres.imgThetaModifiedRowTop.SetValue(Canvas.LeftProperty, 34.0 + column * 26 + (slice - (z - 2)) * 13);
                                pres.imgThetaModifiedRowTop.SetValue(Canvas.TopProperty, 24.0 - (slice - (z - 2)) * 13);

                                if (column == 4)
                                {
                                    pres.imgThetaModifiedRowSide.SetValue(Canvas.LeftProperty, 163.0 + (slice - (z - 2)) * 13);
                                    pres.imgThetaModifiedRowSide.SetValue(Canvas.TopProperty, 25.0 - (slice - (z - 2)) * 13);
                                    pres.imgThetaModifiedRowSide.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    pres.imgThetaModifiedRowSide.Visibility = Visibility.Hidden;
                                }
                            }

                            /* move left row */
                            if (slice == 0)
                            {
                                if (column == 0)
                                {
                                    pres.imgThetaLeftRowFront.SetValue(Canvas.LeftProperty, 111.0);
                                    pres.imgThetaLeftRowFront.Visibility = Visibility.Visible;
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.LeftProperty, 112.0);
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.TopProperty, 50.0);
                                    pres.imgThetaLeftRowTop.Visibility = Visibility.Visible;
                                    pres.imgThetaLeftRowSide.SetValue(Canvas.LeftProperty, 137.0);
                                    pres.imgThetaLeftRowSide.SetValue(Canvas.TopProperty, 51.0);
                                    pres.imgThetaLeftRowSide.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    pres.imgThetaLeftRowSide.Visibility = Visibility.Hidden;
                                    pres.imgThetaLeftRowFront.SetValue(Canvas.LeftProperty, 7.0 + (column - 1) * 26);
                                    pres.imgThetaLeftRowFront.Visibility = Visibility.Visible;
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.LeftProperty, 8.0 + (column - 1) * 26);
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.TopProperty, 50.0);
                                    pres.imgThetaLeftRowTop.Visibility = Visibility.Visible;
                                }
                            }
                            else if (slice == 1)
                            {
                                pres.imgThetaLeftRowFront.Visibility = Visibility.Hidden;
                                if (column == 0)
                                {
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.LeftProperty, 125.0);
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.TopProperty, 37.0);
                                    pres.imgThetaLeftRowTop.Visibility = Visibility.Visible;
                                    pres.imgThetaLeftRowSide.SetValue(Canvas.LeftProperty, 150.0);
                                    pres.imgThetaLeftRowSide.SetValue(Canvas.TopProperty, 38.0);
                                    pres.imgThetaLeftRowSide.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    pres.imgThetaLeftRowSide.Visibility = Visibility.Hidden;
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.LeftProperty, 21.0 + (column - 1) * 26);
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.TopProperty, 37.0);
                                }
                            }
                            else if (slice >= 2 && slice < z - 2)
                            {
                                if (column == 0)
                                {
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.LeftProperty, 138.0);
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.TopProperty, 24.0);
                                    pres.imgThetaLeftRowTop.Visibility = Visibility.Visible;
                                    pres.imgThetaLeftRowSide.SetValue(Canvas.LeftProperty, 163.0);
                                    pres.imgThetaLeftRowSide.SetValue(Canvas.TopProperty, 25.0);
                                    pres.imgThetaLeftRowSide.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    pres.imgThetaLeftRowSide.Visibility = Visibility.Hidden;
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.LeftProperty, 34.0 + (column - 1) * 26);
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.TopProperty, 24.0);
                                }
                            }
                            else // slice >= z - 2, last two slices
                            {
                                if (column == 0)
                                {
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.LeftProperty, 138.0 + (slice - (z - 2)) * 13);
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.TopProperty, 24.0 - (slice - (z - 2)) * 13);
                                    pres.imgThetaLeftRowTop.Visibility = Visibility.Visible;
                                    pres.imgThetaLeftRowSide.SetValue(Canvas.LeftProperty, 163.0 + (slice - (z - 2)) * 13);
                                    pres.imgThetaLeftRowSide.SetValue(Canvas.TopProperty, 25.0 - (slice - (z - 2)) * 13);
                                    pres.imgThetaLeftRowSide.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    pres.imgThetaLeftRowSide.Visibility = Visibility.Hidden;
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.LeftProperty, 34.0 + (column - 1) * 26 + (slice - (z - 2)) * 13);
                                    pres.imgThetaLeftRowTop.SetValue(Canvas.TopProperty, 24.0 - (slice - (z - 2)) * 13);
                                }
                            }

                            /* move right row */
                            if (slice == 0)
                            {
                                pres.imgThetaRightRowSide.Visibility = Visibility.Hidden;
                                pres.imgThetaRightRowFront.Visibility = Visibility.Hidden;
                                pres.imgThetaRightRowTop.Visibility = Visibility.Hidden;
                                if (column == 4)
                                {
                                    pres.imgThetaRightRowTopFading.SetValue(Canvas.LeftProperty, 46.0);
                                    pres.imgThetaRightRowTopFading.SetValue(Canvas.TopProperty, 11.0);
                                    pres.imgThetaRightRowTopFading.Visibility = Visibility.Visible;
                                    pres.imgThetaRightRowSideFading.Visibility = Visibility.Hidden;
                                }
                                else
                                {
                                    pres.imgThetaRightRowTopFading.SetValue(Canvas.LeftProperty, 72.0 + column * 26);
                                    pres.imgThetaRightRowTopFading.SetValue(Canvas.TopProperty, 11.0);
                                    pres.imgThetaRightRowTopFading.Visibility = Visibility.Visible;
                                    if (column == 3)
                                    {
                                        pres.imgThetaRightRowSideFading.SetValue(Canvas.LeftProperty, 176.0);
                                        pres.imgThetaRightRowSideFading.SetValue(Canvas.TopProperty, 12.0);
                                        pres.imgThetaRightRowSideFading.Visibility = Visibility.Visible;
                                    }
                                    else
                                    {
                                        pres.imgThetaRightRowSideFading.Visibility = Visibility.Hidden;
                                    }
                                }
                            }
                            else if (slice == 1)
                            {
                                pres.imgThetaRightRowSideFading.Visibility = Visibility.Hidden;
                                pres.imgThetaRightRowTopFading.Visibility = Visibility.Hidden;
                                if (column == 4)
                                {
                                    pres.imgThetaRightRowFront.SetValue(Canvas.LeftProperty, 7.0);
                                    pres.imgThetaRightRowTop.SetValue(Canvas.LeftProperty, 8.0);
                                    pres.imgThetaRightRowTop.SetValue(Canvas.TopProperty, 50.0);
                                    pres.imgThetaRightRowTop.Visibility = Visibility.Visible;
                                    pres.imgThetaRightRowSide.Visibility = Visibility.Hidden;
                                }
                                else
                                {
                                    pres.imgThetaRightRowFront.SetValue(Canvas.LeftProperty, 33.0 + column * 26);
                                    pres.imgThetaRightRowTop.SetValue(Canvas.LeftProperty, 34.0 + column * 26);
                                    pres.imgThetaRightRowTop.SetValue(Canvas.TopProperty, 50.0);
                                    pres.imgThetaRightRowTop.Visibility = Visibility.Visible;

                                    if (column == 3)
                                    {
                                        pres.imgThetaRightRowSide.SetValue(Canvas.LeftProperty, 137.0);
                                        pres.imgThetaRightRowSide.SetValue(Canvas.TopProperty, 51.0);
                                        pres.imgThetaRightRowSide.Visibility = Visibility.Visible;
                                    }
                                    else
                                    {
                                        pres.imgThetaRightRowSide.Visibility = Visibility.Hidden;
                                    }
                                }

                                pres.imgThetaRightRowFront.Visibility = Visibility.Visible;
                            }
                            else if (slice >= 2 && slice < z - 2)
                            {
                                pres.imgThetaRightRowFront.Visibility = Visibility.Hidden;
                                if (column == 4)
                                {
                                    pres.imgThetaRightRowSide.Visibility = Visibility.Hidden;
                                    pres.imgThetaRightRowTop.SetValue(Canvas.LeftProperty, 21.0);
                                    pres.imgThetaRightRowTop.SetValue(Canvas.TopProperty, 37.0);
                                    pres.imgThetaRightRowTop.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    pres.imgThetaRightRowTop.SetValue(Canvas.LeftProperty, 47.0 + column * 26);
                                    pres.imgThetaRightRowTop.SetValue(Canvas.TopProperty, 37.0);
                                    pres.imgThetaRightRowTop.Visibility = Visibility.Visible;
                                    if (column == 3)
                                    {
                                        pres.imgThetaRightRowSide.SetValue(Canvas.LeftProperty, 150.0);
                                        pres.imgThetaRightRowSide.SetValue(Canvas.TopProperty, 38.0);
                                        pres.imgThetaRightRowSide.Visibility = Visibility.Visible;
                                    }
                                    else
                                    {
                                        pres.imgThetaRightRowSide.Visibility = Visibility.Hidden;
                                    }
                                }
                            }
                            else // slice >= z - 2, last two slices
                            {
                                if (column == 4)
                                {
                                    pres.imgThetaRightRowSide.Visibility = Visibility.Hidden;
                                    pres.imgThetaRightRowTop.SetValue(Canvas.LeftProperty, 21.0 + (slice - (z - 2)) * 13);
                                    pres.imgThetaRightRowTop.SetValue(Canvas.TopProperty, 37.0 - (slice - (z - 2)) * 13);
                                    pres.imgThetaRightRowTop.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    pres.imgThetaRightRowTop.SetValue(Canvas.LeftProperty, 47.0 + column * 26 + (slice - (z - 2)) * 13);
                                    pres.imgThetaRightRowTop.SetValue(Canvas.TopProperty, 37.0 - (slice - (z - 2)) * 13);
                                    pres.imgThetaRightRowTop.Visibility = Visibility.Visible;
                                    if (column == 3)
                                    {
                                        pres.imgThetaRightRowSide.SetValue(Canvas.LeftProperty, 150.0 + (slice - (z - 2)) * 13);
                                        pres.imgThetaRightRowSide.SetValue(Canvas.TopProperty, 38.0 - (slice - (z - 2)) * 13);
                                        pres.imgThetaRightRowSide.Visibility = Visibility.Visible;
                                    }
                                    else
                                    {
                                        pres.imgThetaRightRowSide.Visibility = Visibility.Hidden;
                                    }
                                }
                            }                           
                            
                            break;

                        #endregion

                        default:
                            break;
                    }

                    #endregion

                    #region pres detailed

                    /* left and right column */
                    pres.label164.Content = columnLeft[0].ToString();
                    pres.label165.Content = columnLeft[1].ToString();
                    pres.label166.Content = columnLeft[2].ToString();
                    pres.label167.Content = columnLeft[3].ToString();
                    pres.label168.Content = columnLeft[4].ToString();

                    pres.label169.Content = columnRight[0].ToString();
                    pres.label170.Content = columnRight[1].ToString();
                    pres.label171.Content = columnRight[2].ToString();
                    pres.label172.Content = columnRight[3].ToString();
                    pres.label173.Content = columnRight[4].ToString();

                    /* parity bits */
                    pres.label174.Content = parity1.ToString();
                    pres.label175.Content = parity2.ToString();

                    /* old and new column */
                    pres.label176.Content = columnOld[0].ToString();
                    pres.label177.Content = columnOld[1].ToString();
                    pres.label178.Content = columnOld[2].ToString();
                    pres.label179.Content = columnOld[3].ToString();
                    pres.label180.Content = columnOld[4].ToString();

                    pres.label181.Content = columnNew[0].ToString();
                    pres.label182.Content = columnNew[1].ToString();
                    pres.label183.Content = columnNew[2].ToString();
                    pres.label184.Content = columnNew[3].ToString();
                    pres.label185.Content = columnNew[4].ToString();

                    #endregion

                }, null);

                /* wait for button clicks */
                if (!pres.autostep ||(slice == (z - 1) && column == (X - 1)))
                {
                    pres.autostep = false;
                    AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }
                /* sleep between steps, if autostep was clicked */
                else
                {
                    System.Threading.Thread.Sleep(pres.autostepSpeed);       // value adjustable by a slider
                }                
            }

            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                /* hide theta canvas after last iteration */
                if (slice == (z - 1) && column == (X - 1))
                {
                    pres.canvasStepDetailTheta.Visibility = Visibility.Hidden;
                    pres.canvasCubeTheta.Visibility = Visibility.Hidden;
                    pres.imgCubeDefaultInner.Visibility = Visibility.Hidden;
                    pres.imgCubeDefaultBottom.Visibility = Visibility.Hidden;
                }
            }, null);
        }

        public void RhoPres(byte[] oldLane, byte[] newLane, int plane, int lane, int rotationOffset)
        {
            if (!pres.runToEnd && !pres.skip)
            {
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    /* show rho canvas in first iteration */
                    if (plane == 0 && lane == 0)
                    {
                        pres.canvasStepDetailRho.Visibility = Visibility.Visible;
                        pres.canvasCubeRho.Visibility = Visibility.Visible;
                        pres.imgCubeDefault.Visibility = Visibility.Visible;
                    }

                    pres.labelOuterPartCaption.Content = "Plane";
                    pres.labelOuterPart.Content = (plane + 1).ToString();
                    pres.labelInnerPartCaption.Content = "Lane";
                    pres.labelInnerPart.Content = (lane + 1).ToString();

                    #region pres cube

                    /* move modified lane */
                    pres.imgRhoModifiedLane.SetValue(Canvas.TopProperty, 167.0 - plane * 26);
                    pres.imgRhoModifiedLane.SetValue(Canvas.LeftProperty, 7.0 + lane * 26);


                    pres.imgRhoModifiedSideLane.Visibility = Visibility.Hidden;
                    pres.imgRhoModifiedTopLane.Visibility = Visibility.Hidden;

                    if (lane == 4)
                    {
                        pres.imgRhoModifiedSideLane.Visibility = Visibility.Visible;
                        pres.imgRhoModifiedSideLane.SetValue(Canvas.TopProperty, 116.0 - plane * 26);
                    } 
                    if (plane == 4)
                    {
                        pres.imgRhoModifiedTopLane.SetValue(Canvas.LeftProperty, 8.0 + lane * 26);
                        pres.imgRhoModifiedTopLane.Visibility = Visibility.Visible;
                    }
                    
                    #endregion

                    #region pres detailed

                    /* move table marker */
                    pres.imgRhoTableMarker.SetValue(Canvas.TopProperty, 73.0 + plane * 19);
                    pres.imgRhoTableMarker.SetValue(Canvas.LeftProperty, 277.0 + lane * 22);

                    pres.labelRotationOffset.Content = rotationOffset.ToString();

                    #region fill lane labels for different lane sizes

                    switch (z)
                    {
                        case 1:
                            /* TODO */
                            break;
                        case 2:
                            /* TODO */
                            break;
                        case 4:
                            /* TODO */
                            break;
                        case 8:
                            /* TODO */
                            break;
                        case 16:
                            /* TODO */
                            break;
                        case 32:
                            /* TODO */
                            break;
                        case 64:

                            #region fill labels of old lane

                            pres.label11.Content = oldLane[0].ToString();
                            pres.label12.Content = oldLane[1].ToString();
                            pres.label13.Content = oldLane[2].ToString();
                            pres.label14.Content = oldLane[3].ToString();
                            pres.label15.Content = oldLane[4].ToString();
                            pres.label16.Content = oldLane[5].ToString();
                            pres.label17.Content = oldLane[6].ToString();
                            pres.label18.Content = oldLane[7].ToString();
                            pres.label19.Content = oldLane[8].ToString();
                            pres.label20.Content = oldLane[9].ToString();
                            pres.label21.Content = oldLane[10].ToString();
                            pres.label22.Content = oldLane[11].ToString();
                            pres.label23.Content = oldLane[12].ToString();
                            pres.label24.Content = oldLane[13].ToString();
                            pres.label25.Content = oldLane[14].ToString();
                            pres.label26.Content = oldLane[15].ToString();

                            pres.label27.Content = oldLane[16].ToString();
                            pres.label28.Content = oldLane[17].ToString();
                            pres.label29.Content = oldLane[18].ToString();
                            pres.label30.Content = oldLane[19].ToString();
                            pres.label31.Content = oldLane[20].ToString();
                            pres.label32.Content = oldLane[21].ToString();
                            pres.label33.Content = oldLane[22].ToString();
                            pres.label34.Content = oldLane[23].ToString();
                            pres.label35.Content = oldLane[24].ToString();
                            pres.label36.Content = oldLane[25].ToString();
                            pres.label37.Content = oldLane[26].ToString();
                            pres.label38.Content = oldLane[27].ToString();
                            pres.label39.Content = oldLane[28].ToString();
                            pres.label40.Content = oldLane[29].ToString();
                            pres.label41.Content = oldLane[30].ToString();
                            pres.label42.Content = oldLane[31].ToString();

                            pres.label43.Content = oldLane[32].ToString();
                            pres.label44.Content = oldLane[33].ToString();
                            pres.label45.Content = oldLane[34].ToString();
                            pres.label46.Content = oldLane[35].ToString();
                            pres.label47.Content = oldLane[36].ToString();
                            pres.label48.Content = oldLane[37].ToString();
                            pres.label49.Content = oldLane[38].ToString();
                            pres.label50.Content = oldLane[39].ToString();
                            pres.label51.Content = oldLane[40].ToString();
                            pres.label52.Content = oldLane[41].ToString();
                            pres.label53.Content = oldLane[42].ToString();
                            pres.label54.Content = oldLane[43].ToString();
                            pres.label55.Content = oldLane[44].ToString();
                            pres.label56.Content = oldLane[45].ToString();
                            pres.label57.Content = oldLane[46].ToString();
                            pres.label58.Content = oldLane[47].ToString();

                            pres.label59.Content = oldLane[48].ToString();
                            pres.label60.Content = oldLane[49].ToString();
                            pres.label61.Content = oldLane[50].ToString();
                            pres.label62.Content = oldLane[51].ToString();
                            pres.label63.Content = oldLane[52].ToString();
                            pres.label64.Content = oldLane[53].ToString();
                            pres.label65.Content = oldLane[54].ToString();
                            pres.label66.Content = oldLane[55].ToString();
                            pres.label67.Content = oldLane[56].ToString();
                            pres.label68.Content = oldLane[57].ToString();
                            pres.label69.Content = oldLane[58].ToString();
                            pres.label70.Content = oldLane[59].ToString();
                            pres.label71.Content = oldLane[60].ToString();
                            pres.label72.Content = oldLane[61].ToString();
                            pres.label73.Content = oldLane[62].ToString();
                            pres.label74.Content = oldLane[63].ToString();

                            #endregion

                            #region fill labels of old lane

                            pres.label75.Content = newLane[0].ToString();
                            pres.label76.Content = newLane[1].ToString();
                            pres.label77.Content = newLane[2].ToString();
                            pres.label78.Content = newLane[3].ToString();
                            pres.label79.Content = newLane[4].ToString();
                            pres.label80.Content = newLane[5].ToString();
                            pres.label81.Content = newLane[6].ToString();
                            pres.label82.Content = newLane[7].ToString();
                            pres.label83.Content = newLane[8].ToString();
                            pres.label84.Content = newLane[9].ToString();
                            pres.label85.Content = newLane[10].ToString();
                            pres.label86.Content = newLane[11].ToString();
                            pres.label87.Content = newLane[12].ToString();
                            pres.label88.Content = newLane[13].ToString();
                            pres.label89.Content = newLane[14].ToString();
                            pres.label90.Content = newLane[15].ToString();

                            pres.label91.Content = newLane[16].ToString();
                            pres.label92.Content = newLane[17].ToString();
                            pres.label93.Content = newLane[18].ToString();
                            pres.label94.Content = newLane[19].ToString();
                            pres.label95.Content = newLane[20].ToString();
                            pres.label96.Content = newLane[21].ToString();
                            pres.label97.Content = newLane[22].ToString();
                            pres.label98.Content = newLane[23].ToString();
                            pres.label99.Content = newLane[24].ToString();
                            pres.label100.Content = newLane[25].ToString();
                            pres.label101.Content = newLane[26].ToString();
                            pres.label102.Content = newLane[27].ToString();
                            pres.label103.Content = newLane[28].ToString();
                            pres.label104.Content = newLane[29].ToString();
                            pres.label105.Content = newLane[30].ToString();
                            pres.label106.Content = newLane[31].ToString();

                            pres.label107.Content = newLane[32].ToString();
                            pres.label108.Content = newLane[33].ToString();
                            pres.label109.Content = newLane[34].ToString();
                            pres.label110.Content = newLane[35].ToString();
                            pres.label111.Content = newLane[36].ToString();
                            pres.label112.Content = newLane[37].ToString();
                            pres.label113.Content = newLane[38].ToString();
                            pres.label114.Content = newLane[39].ToString();
                            pres.label115.Content = newLane[40].ToString();
                            pres.label116.Content = newLane[41].ToString();
                            pres.label117.Content = newLane[42].ToString();
                            pres.label118.Content = newLane[43].ToString();
                            pres.label119.Content = newLane[44].ToString();
                            pres.label120.Content = newLane[45].ToString();
                            pres.label121.Content = newLane[46].ToString();
                            pres.label122.Content = newLane[47].ToString();

                            pres.label123.Content = newLane[48].ToString();
                            pres.label124.Content = newLane[49].ToString();
                            pres.label125.Content = newLane[50].ToString();
                            pres.label126.Content = newLane[51].ToString();
                            pres.label127.Content = newLane[52].ToString();
                            pres.label128.Content = newLane[53].ToString();
                            pres.label129.Content = newLane[54].ToString();
                            pres.label130.Content = newLane[55].ToString();
                            pres.label131.Content = newLane[56].ToString();
                            pres.label132.Content = newLane[57].ToString();
                            pres.label133.Content = newLane[58].ToString();
                            pres.label134.Content = newLane[59].ToString();
                            pres.label135.Content = newLane[60].ToString();
                            pres.label136.Content = newLane[61].ToString();
                            pres.label137.Content = newLane[62].ToString();
                            pres.label138.Content = newLane[63].ToString();

                            #endregion

                            break;

                        default:
                            break;
                    }
                    #endregion

                    #endregion

                }, null);

                /* wait for button clicks */
                if (!pres.autostep || (plane == Y - 1 && lane == X - 1))
                {
                    pres.autostep = false;
                    AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }
                /* sleep between steps, if autostep was clicked */
                else
                {
                    System.Threading.Thread.Sleep(pres.autostepSpeed * 3);       // value adjustable by a slider (slower for rho, since it performs less steps than theta and chi)
                }
            }

            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                /* hide rho canvas after last iteration */
                if (plane == (Y - 1) && lane == (X - 1))
                {
                    pres.canvasStepDetailRho.Visibility = Visibility.Hidden;
                    pres.canvasCubeRho.Visibility = Visibility.Hidden;
                    pres.imgCubeDefault.Visibility = Visibility.Hidden;
                }
            }, null);
        }

        public void PiPres(byte[][][] tmpLanes)
        {
            /* presentation is fixed to six rounds and performed after the step mapping of pi */
            for (int i = 0; i < 6; i++)
            {
                if (!pres.runToEnd && !pres.skip)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelOuterPartCaption.Content = "Step";
                        pres.labelOuterPart.Content = (i + 1).ToString() + "/6";
                        pres.labelInnerPartCaption.Content = "";
                        pres.labelInnerPart.Content = "";

                        #region rounds

                        switch (i)
                        {
                            case 0:
                                pres.canvasCubePi_1.Visibility = Visibility.Visible;
                                pres.imgPiCube_1.Visibility = Visibility.Visible;
                                pres.canvasStepDetailPi_1.Visibility = Visibility.Visible;
                                pres.imgPiDetailed_1.Visibility = Visibility.Visible;
                                break;

                            case 1:
                                pres.canvasCubePi_1.Visibility = Visibility.Hidden;
                                pres.canvasStepDetailPi_1.Visibility = Visibility.Hidden;
                                pres.canvasCubePi_2.Visibility = Visibility.Visible;
                                pres.canvasStepDetailPi_2.Visibility = Visibility.Visible;
                                break;

                            case 2:
                                pres.canvasCubePi_2.Visibility = Visibility.Hidden;
                                pres.canvasStepDetailPi_2.Visibility = Visibility.Hidden;
                                pres.canvasCubePi_3.Visibility = Visibility.Visible;
                                pres.canvasStepDetailPi_3.Visibility = Visibility.Visible;
                                break;

                            case 3:
                                pres.canvasCubePi_3.Visibility = Visibility.Hidden;
                                pres.canvasStepDetailPi_3.Visibility = Visibility.Hidden;
                                pres.canvasCubePi_4.Visibility = Visibility.Visible;
                                pres.canvasStepDetailPi_4.Visibility = Visibility.Visible;
                                break;

                            case 4:
                                pres.canvasCubePi_4.Visibility = Visibility.Hidden;
                                pres.canvasStepDetailPi_4.Visibility = Visibility.Hidden;
                                pres.canvasCubePi_5.Visibility = Visibility.Visible;
                                pres.canvasStepDetailPi_5.Visibility = Visibility.Visible;
                                break;

                            case 5:
                                pres.canvasCubePi_5.Visibility = Visibility.Hidden;
                                pres.canvasStepDetailPi_5.Visibility = Visibility.Hidden;
                                pres.canvasCubePi_6.Visibility = Visibility.Visible;
                                pres.canvasStepDetailPi_6.Visibility = Visibility.Visible;
                                break;

                            default:
                                break;
                        }

                        #endregion

                    }, null);

                    /* wait for button clicks */
                    if (!pres.autostep || i == 5)
                    {
                        pres.autostep = false;
                        AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
                        buttonNextClickedEvent.WaitOne();
                    }
                    /* sleep between steps, if autostep was clicked */
                    else
                    {
                        System.Threading.Thread.Sleep(pres.autostepSpeed * 8);       // value adjustable by a slider (slower for pi, since it performs only 6 steps)
                    }
                }
            }

            /* hide pi canvas after last iteration */
            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.canvasCubePi_1.Visibility = Visibility.Hidden;
                pres.canvasStepDetailPi_1.Visibility = Visibility.Hidden;
                pres.canvasCubePi_2.Visibility = Visibility.Hidden;
                pres.canvasStepDetailPi_2.Visibility = Visibility.Hidden;
                pres.canvasCubePi_3.Visibility = Visibility.Hidden;
                pres.canvasStepDetailPi_3.Visibility = Visibility.Hidden;
                pres.canvasCubePi_4.Visibility = Visibility.Hidden;
                pres.canvasStepDetailPi_4.Visibility = Visibility.Hidden;
                pres.canvasCubePi_5.Visibility = Visibility.Hidden;
                pres.canvasStepDetailPi_5.Visibility = Visibility.Hidden;
                pres.canvasCubePi_6.Visibility = Visibility.Hidden;
                pres.canvasStepDetailPi_6.Visibility = Visibility.Hidden;
            }, null);
        }

        public void ChiPres(byte[] oldRow, int slice, int row)
        {
            if (!pres.runToEnd && !pres.skip)
            {
                /* show slice and row indexes */
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.labelOuterPartCaption.Content = "Slice";
                    pres.labelOuterPart.Content = (slice + 1).ToString();
                    pres.labelInnerPartCaption.Content = "Row";
                    pres.labelInnerPart.Content = (row + 1).ToString();

                    /* show chi canvas in first iteration */
                    if (slice == 0 && row == 0)
                    {
                        pres.canvasStepDetailChi.Visibility = Visibility.Visible;
                        pres.canvasCubeChi.Visibility = Visibility.Visible;
                    }

                }, null);

                #region pres cube

                switch (z)
                {
                    case 1:
                        /* TODO */
                        break;
                    case 2:
                        /* TODO */
                        break;
                    #region lane size 4

                    case 4:

                        /* show slice and row indexes */
                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            /* move modified row */
                            double modifiedRowTop = 155 - row * 26 - slice * 13;
                            double modifiedRowLeft = 137 + slice * 13;
                            pres.imgModifiedRow.SetValue(Canvas.TopProperty, modifiedRowTop);
                            pres.imgModifiedRow.SetValue(Canvas.LeftProperty, modifiedRowLeft);

                            /* move first row and toggle visibility*/
                            if (slice == 0)
                            {
                                double modifiedFirstRowTop = 167 - row * 26;
                                pres.imgModifiedFirstRow.Visibility = Visibility.Visible;
                                pres.imgModifiedFirstRow.SetValue(Canvas.TopProperty, modifiedFirstRowTop);
                            }
                            else
                            {
                                pres.imgModifiedFirstRow.Visibility = Visibility.Hidden;
                            }

                            /* toggle visibility top row */
                            if (row == 4)
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

                        pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            /* show cube */
                            if (slice == 0)
                            {
                                /* show default cube */
                                pres.imgCubeDefault.Visibility = Visibility.Visible;
                                pres.imgCubeDefaultInner.Visibility = Visibility.Hidden;
                            }
                            else if (slice == 2)
                            {
                                /* show inner cube */
                                pres.imgCubeDefault.Visibility = Visibility.Hidden;
                                pres.imgCubeDefaultInner.Visibility = Visibility.Visible;
                            }
                            else if (slice == z - 3)
                            {
                                /* change to bottom cube */
                                pres.imgCubeDefaultInner.Visibility = Visibility.Hidden;
                                pres.imgCubeDefaultBottom.Visibility = Visibility.Visible;
                            }

                            /* move first row and toggle visibility*/
                            if (slice == 0)
                            {
                                pres.imgModifiedFirstRow.SetValue(Canvas.TopProperty, 167.0 - row * 26);
                                pres.imgModifiedRow.SetValue(Canvas.TopProperty, 155.0 - row * 26);
                                pres.imgModifiedRow.SetValue(Canvas.LeftProperty, 137.0);
                                pres.imgModifiedFirstRow.Visibility = Visibility.Visible;
                            }

                            /* move modified row only */
                            else if (slice > 0 && slice < z - 3)
                            {
                                pres.imgModifiedFirstRow.Visibility = Visibility.Hidden;

                                pres.imgModifiedRow.SetValue(Canvas.TopProperty, 142.0 - row * 26);
                                pres.imgModifiedRow.SetValue(Canvas.LeftProperty, 150.0);
                            }
                            else // slice >= z - 3, last three slices
                            {           
                                pres.imgModifiedRow.SetValue(Canvas.TopProperty, 142.0 - row * 26 - (slice - (z - 3)) * 13);
                                pres.imgModifiedRow.SetValue(Canvas.LeftProperty, 150.0 + (slice - (z - 3)) * 13);
                            }

                            /* toggle visibility top row */
                            if (row == 4)
                            {
                                if (slice == 0)
                                {
                                    pres.imgModifiedTopRow.SetValue(Canvas.TopProperty, 50.0);
                                    pres.imgModifiedTopRow.SetValue(Canvas.LeftProperty, 8.0);
                                }
                                else if (slice < z - 3)
                                {
                                    pres.imgModifiedTopRow.SetValue(Canvas.TopProperty, 37.0);
                                    pres.imgModifiedTopRow.SetValue(Canvas.LeftProperty, 21.0);
                                }
                                else /* last two slices */
                                {
                                    pres.imgModifiedTopRow.SetValue(Canvas.TopProperty, 37.0 - (slice - (z - 3)) * 13);
                                    pres.imgModifiedTopRow.SetValue(Canvas.LeftProperty, 21.0 + (slice - (z - 3)) * 13);
                                }

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

                #region pres detailed

                /* presentation detailed step */
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.label1.Content = oldRow[0].ToString();
                    pres.label2.Content = oldRow[1].ToString();
                    pres.label3.Content = oldRow[2].ToString();
                    pres.label4.Content = oldRow[3].ToString();
                    pres.label5.Content = oldRow[4].ToString();

                    pres.label6.Content = rows[slice][row][0].ToString();
                    pres.label7.Content = rows[slice][row][1].ToString();
                    pres.label8.Content = rows[slice][row][2].ToString();
                    pres.label9.Content = rows[slice][row][3].ToString();
                    pres.label10.Content = rows[slice][row][4].ToString();
                }, null);

                #endregion

                /* wait for button clicks */
                if (!pres.autostep || (slice == (z - 1) && row == (Y - 1)))
                {
                    pres.autostep = false;
                    AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }
                /* sleep between steps, if autostep was clicked */
                else
                {
                    System.Threading.Thread.Sleep(pres.autostepSpeed);       // value adjustable by a slider
                }
            }

            /* hide chi canvas after last iteration */
            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (slice == (z - 1) && row == (Y - 1))
                {
                    pres.canvasStepDetailChi.Visibility = Visibility.Hidden;
                    pres.canvasCubeChi.Visibility = Visibility.Hidden;
                    pres.imgCubeDefaultBottom.Visibility = Visibility.Hidden;
                }
            }, null);
        }

        public void IotaPres(byte[] firstLane, byte[] truncatedConstant)
        {
            if (!pres.runToEnd && !pres.skip)
            {
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    /* show iota canvas in first iteration */
                    pres.canvasStepDetailRho.Visibility = Visibility.Visible;
                    pres.canvasCubeRho.Visibility = Visibility.Visible;
                    pres.imgCubeDefault.Visibility = Visibility.Visible;

                    #region pres cube

                    #endregion

                    #region pres detailed
                    
                    #endregion

                }, null);

                /* wait for button clicks */
                if (!pres.autostep)
                {
                    pres.autostep = false;
                    AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }
                /* sleep between steps, if autostep was clicked */
                else
                {
                    System.Threading.Thread.Sleep(pres.autostepSpeed * 3);       // value adjustable by a slider (slower for rho, since it performs less steps than theta and chi)
                }
            }

            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                /* hide iota canvas after last iteration */              
            }, null);

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
