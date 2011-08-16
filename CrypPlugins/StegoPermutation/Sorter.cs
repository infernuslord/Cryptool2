/* HOWTO: Set year, author name and organization.
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
                BigInteger capacity = ((BigInteger)source.Count).Factorial();
                int byteCapacity = (capacity.BitCount() / 8) - 1;
            	return byteCapacity;
			}
        }
			
		public Sorter(Collection<T> source)
        {
			this.source = source;
		}

        public Collection<T> Encode(Stream messageStream, string alphabet)
        {
            Collection<T> result = new Collection<T>();
			T[] sortedItems = new T[source.Count];
            source.CopyTo(sortedItems, 0);
            
			if(alphabet == null || alphabet.Length == 0) {
				Array.Sort(sortedItems);
			} else {
				StringComparer comparer = new StringComparer(alphabet);
				Array.Sort(sortedItems, comparer);
			}
            
            // initialize message
            messageStream.Position = 0;
            byte[] buffer = new byte[messageStream.Length];
            messageStream.Read(buffer, 0, buffer.Length);
            BigInteger message = new BigInteger(buffer);
            
            // initialize carrier
            Collection<int> freeIndexes = new Collection<int>();
            result.Clear();
            for (int n = 0; n < source.Count; n++)
            {
                freeIndexes.Add(n);
                result.Add(null);
			}

            int skip = 0;
            for (int indexSource = 0; indexSource < source.Count; indexSource++)
            {
                skip = (int)(message % freeIndexes.Count);
                message = message / freeIndexes.Count;
                int resultIndex = freeIndexes[skip];
                result[resultIndex] = sortedItems[indexSource];
                freeIndexes.RemoveAt(skip);
            }
			
			return result;
        }

        public void Decode(Stream messageStream, string alphabet)
        {
            T[] sortedItems = new T[source.Count];
            source.CopyTo(sortedItems, 0);

			StringComparer comparer = null;
			if(alphabet == null || alphabet.Length == 0) {
				Array.Sort(sortedItems);
			} else {
				comparer = new StringComparer(alphabet);
				Array.Sort(sortedItems, comparer);
			}
			
			/* // TEST
			foreach(T s in sortedItems){
				Console.WriteLine(s.ToString());
			}*/
			
            BigInteger message = new BigInteger(0);

            for (int carrierIndex = 0; carrierIndex < source.Count; carrierIndex++)
            {
                int skip = 0;
                for (int countIndex = 0; countIndex < carrierIndex; countIndex++)
                {
					
					int compResult = 0; 
					if(comparer == null){
						compResult = source[countIndex].CompareTo(source[carrierIndex]);
					}else{
						compResult = comparer.DoCompare(source[countIndex], source[carrierIndex]);
					}
					
                    if (compResult > 0)
                    {   // There is a bigger item to the left. It's place
                        // must have been skipped by the current item.
                        skip++;
                    }
                }

                // Revert the division that resulted in this skip value
                int itemOrdinal = Array.IndexOf(sortedItems, source[carrierIndex])+1;
                BigInteger value = new BigInteger(skip);
                for (int countIndex = 1; countIndex < itemOrdinal; countIndex++)
                {
                    value *= (source.Count - countIndex + 1);
                }
                message += value;
            }

            byte[] messageBytes = message.ToByteArray();
            messageStream.Write(messageBytes, 0, messageBytes.Length);
            messageStream.Position = 0;
        }
    }
}
