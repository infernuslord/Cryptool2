using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Cryptool.FrequencyTest
{
    public class DataSource
    {

        private ObservableCollection<CollectionElement> valueCollection;

        public ObservableCollection<CollectionElement> ValueCollection
        {
            get { return valueCollection; }
            set { valueCollection = value; }
        }

        //public void AddtoCollection(int i, double d, string s)
        // {
        //    CollectionElement z = new CollectionElement(i, d, s);
        //    valueCollection.Add(z);
        // }

        public DataSource()
        {
            // CollectionElement z= new CollectionElement(30,30.5,"qqq");
            valueCollection = new ObservableCollection<CollectionElement>();
            // valueCollection.Add(z);
            // CollectionElement y = new CollectionElement(30, 30.5, "qqq");
           // CollectionElement s = new CollectionElement(30, 30.5, "qqq");
            // valueCollection.Add(s);
            // valueCollection.Add(y);
            // CollectionElement q = new CollectionElement(30, 30.5, "qqq");
           //  valueCollection.Add(q);
        }
    }
}
