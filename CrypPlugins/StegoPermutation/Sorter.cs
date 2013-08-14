/*
   Copyright 2011 CrypTool 2 Team

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

﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Numerics;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.StegoPermutation
{
    public class Sorter<T> where T: class, IComparable
    {
		private Collection<T> source;

        public int Capacity
        {
			get
			{
                int bits;

                if (source.Count <= 100)
                {
                    // explicitly calculate the factorial
                    BigInteger capacity = ((BigInteger)source.Count).Factorial();
                    bits = capacity.BitCount();
                }
                else
                {
                    // approximate factorial with the Stirling formula
                    double n = source.Count;
                    double ldStirling = 0.5 * Math.Log(2 * Math.PI * n, 2) + n * Math.Log(n / Math.E, 2);
                    bits = (int)ldStirling;
                }

                return bits/8;
			}
        }
			
		public Sorter(Collection<T> source)
        {
			this.source = source;
		}

        public T[] sortItems(string alphabet)
        {
            T[] sortedItems = new T[source.Count];
            source.CopyTo(sortedItems, 0);

            if (alphabet == null || alphabet.Length == 0)
            {
                StringComparer comparer = new StringComparer("");
                Array.Sort(sortedItems, comparer);
            }
            else
            {
                StringComparer comparer = new StringComparer(alphabet);
                Array.Sort(sortedItems, comparer);
            }

            return sortedItems;
        }

        public Collection<T> Encode(Stream messageStream, string alphabet, StegoPermutationPresentation presentation, StegoPermutation stego)
        {
            Collection<T> result = new Collection<T>();

            stego.ProgressChanged(10, 100);

            T[] sortedItems = sortItems(alphabet);

            stego.ProgressChanged(20, 100);

            // initialize message
            messageStream.Position = 0;
            byte[] buffer = new byte[messageStream.Length];
            messageStream.Read(buffer, 0, buffer.Length);
            BigInteger message = 0;
            for (int i = 0; i < buffer.Length; i++)
                message = message * 256 + buffer[buffer.Length-1-i];
            
            // initialize carrier
            result.Clear();
            for (int n = 0; n < source.Count; n++)
                result.Add(null);

            // update presentation control
            SendOrPostCallback updatePresentationResultListDelegate = (SendOrPostCallback)delegate
            {
                presentation.UpdateResultList(result);
            };

            if (presentation.IsVisible)
            {
                presentation.Dispatcher.Invoke(DispatcherPriority.Normal, updatePresentationResultListDelegate, null);
            }

            try
            {
                LRArray lr = new LRArray((ulong)source.Count);
                lr.set_all();

                int skip = 0;
                for (int indexSource = 0; indexSource < source.Count; indexSource++)
                {
                    int cnt = source.Count - indexSource;
                    skip = (int)(message % cnt);
                    message = message / cnt;
                    int resultIndex = (int)lr.get_set_idx_chg((ulong)skip);
                    result[resultIndex] = sortedItems[indexSource];

                    stego.ProgressChanged(indexSource+1, source.Count);

                    if (presentation.IsVisible)
                    {
                        presentation.Dispatcher.Invoke(DispatcherPriority.Normal, updatePresentationResultListDelegate, null);
                        Thread.Sleep(500);
                    }
                }
            }
            catch (Exception ex)
            {
            }
			
			return result;
        }

        public void Decode(Stream messageStream, string alphabet, StegoPermutationPresentation presentation, StegoPermutation stego)
        {
            Dictionary<T, ulong> string2index = new Dictionary<T, ulong>();
            ulong[] numList = new ulong[source.Count];
            ulong[] numListInv = new ulong[source.Count];
            ulong[] values = new ulong[source.Count];

            stego.ProgressChanged(10, 100);

            T[] sortedItems = sortItems(alphabet);

            for (int i = 0; i < sortedItems.Length; i++) string2index[sortedItems[i]] = (ulong)i;
            for (int i = 0; i < numList.Length; i++) numList[i] = string2index[source[i]];
            for (int i = 0; i < numList.Length; i++) numListInv[numList[i]] = (ulong)i;

            stego.ProgressChanged(20, 100);

            BigInteger message = new BigInteger(0);
            BigIntegerClass messageWrapper = new BigIntegerClass(message);

            // update presentation control
            SendOrPostCallback updatePresentationResultNumberDelegate = (SendOrPostCallback)delegate
            {
                presentation.UpdateResultNumber(messageWrapper);
            };

            if (presentation.IsVisible)
            {
                presentation.Dispatcher.Invoke(DispatcherPriority.Normal, updatePresentationResultNumberDelegate, null);
            }

            LRArray lr = new LRArray((ulong)source.Count);
            lr.set_all();

            try
            {
                for (int carrierIndex = source.Count - 1; carrierIndex >= 0; carrierIndex--)
                {
                    ulong v = lr.num_SRE(numList[carrierIndex]);
                    values[carrierIndex] = lr.num_SRE(numList[carrierIndex]);
                    lr.get_set_idx_chg((ulong)carrierIndex - v);
                }

                stego.ProgressChanged(30, 100);

                message = 0;
                for (int i = 1; i <= source.Count; i++)
                {
                    message = i * message + values[numListInv[source.Count - i]];

                    stego.ProgressChanged(i, source.Count+1);

                    if (presentation.IsVisible)
                    {
                        messageWrapper.BigIntegerStruct = message;
                        presentation.Dispatcher.Invoke(DispatcherPriority.Normal, updatePresentationResultNumberDelegate, null);
                        Thread.Sleep(500);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            // convert message to stream
            byte[] messageBytes = message.ToByteArray();
            int cnt = messageBytes.Length;
            if (messageBytes[cnt - 1] == 0) cnt--;
            messageStream.Write(messageBytes, 0, cnt);
            messageStream.Position = 0;
        }
    }
}
