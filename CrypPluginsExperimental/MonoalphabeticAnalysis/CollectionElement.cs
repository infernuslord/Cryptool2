using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.MonoalphabeticAnalysis
{
    public class CollectionElement
    {

        private string m_mapping;
        private string m_caption;
        private double m_frequency;
       // private int m_amount;



        public CollectionElement(string caption, double frequency,string mapping)
        {
            m_mapping = mapping;
            m_caption = caption;
            m_frequency = frequency;
        }






        public string Caption
        {
            get { return m_caption; }
            set
            {
                m_caption = value;
            }
        }


        public double Frequency
        {
            get { return m_frequency; }
            set
            {
                m_frequency = value;
            }
        }
        public string Mapping
        {
            get { return m_mapping; }
            set
            {
                m_mapping = value;
            }
        }

    }
}
