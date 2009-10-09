using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.FrequencyTest
{
    public class CollectionElement
    {
        private string m_caption;
        private double m_percent;
        private int m_amount;

        public CollectionElement(int amount, double percent, string caption)
        {
            m_amount = amount;
            m_caption = caption;
            m_percent = percent;
        }


        public string Caption
        {
            get { return m_caption; }
            set
            {
                m_caption = value;
            }
        }


        public double Percent
        {
            get { return m_percent; }
            set
            {
                m_percent = value;
            }
        }


        public int Amount
        {
            get { return m_amount; }
            set
            {
                m_amount = value;
            }
        }
    }
}
