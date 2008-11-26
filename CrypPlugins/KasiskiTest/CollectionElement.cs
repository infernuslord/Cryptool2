using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.KasiskiTest
{
    public class CollectionElement
    {


       
        private int m_amount;
        private int m_factor;



        public CollectionElement(int factor, int amount)
        {
            m_amount = amount;
            m_factor = factor;
           
        }

                        
        public int Factor
        {
            get { return m_factor; }
            set
            {
                m_factor = value;
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
