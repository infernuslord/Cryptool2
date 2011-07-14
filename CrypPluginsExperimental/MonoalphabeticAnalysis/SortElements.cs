using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Cryptool.MonoalphabeticAnalysis
{

    //public enum SortElemetsBy { byString, byFrequency };

    public class SortElements : System.Collections.IComparer
    {

        public enum SortElemetsBy { byString, byFrequency };
        //Private value holding the Elements sort type
        private SortElemetsBy sortType;

        /// <summary>
        /// Creates a new instance of the SortElements class. Use this class to specify sorting
        /// methods for arraylists filled with only elements.(class CollectionElement)
        /// </summary>
        /// <param name="sortingType"></param>
        public SortElements(SortElemetsBy sortingType)
        {
            this.sortType = sortingType;
        }

        /// <summary>
        /// Gets or sets the SortElementsBy type.
        /// </summary>
        public SortElemetsBy SortType
        {
            get
            {
                return this.sortType;
            }
            set
            {
                this.sortType = value;
            }
        }

        /// <summary>
        /// Implementation of the Compare method, required for the IComparer class
        /// </summary>
        /// <param name="x">First object</param>
        /// <param name="y">Second object</param>
        /// <returns>An int, which is the value obtained when the two objects
        /// have been compared.</returns>
        public int Compare(object x, object y)
        {
            //Check whether x and y are both CollectionElement classes.
            if ((x is CollectionElement) && (y is CollectionElement))
            {
                //Some little typecasting
                CollectionElement b = (CollectionElement)y;
                CollectionElement a = (CollectionElement)x;

                //Check the sorting type
                if (this.sortType == SortElemetsBy.byString)
                {
                    //Use the native string.CompareTo method
                    return (a.Caption.CompareTo(b.Caption));
                }
                else
                {
                    //Use the native int.CompareTo method
                    return (a.Frequency.CompareTo(b.Frequency));
                }
            }
            else
            {
                //return 0 if x or y are of wrong type
                return 0;
            }
        }

    }
}
