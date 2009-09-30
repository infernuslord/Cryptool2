using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;

namespace WrapperTester
{
    class Program
    {
        static void prepareSieving(IntPtr conf, int update, IntPtr core_sieve_fcn)
        {
            Console.WriteLine("Update: " + update);
            
            /*IntPtr clone = Msieve.msieve.cloneSieveConf(conf);
            for (int i = 0; i < 200; i++)
            {
                Msieve.msieve.collectRelations(clone, update, core_sieve_fcn);
                Msieve.msieve.saveYield(conf, Msieve.msieve.getYield(clone));
            }*/
        }

        static void Main(string[] args)
        {
            Msieve.callback_struct callbacks = new Msieve.callback_struct();
            callbacks.showProgress = delegate(int num_relations, int max_relations)
            {
                System.Console.WriteLine("" + num_relations + " von " + max_relations);
                //Msieve.msieve.stop();
            };
            callbacks.prepareSieving = prepareSieving;
            
            Msieve.msieve.initMsieve(callbacks);

            ArrayList factors = Msieve.msieve.factorize("(10^110-1)/9", null);            
            foreach (String str in factors)
                Console.WriteLine(str);
            Console.ReadLine();
        }
    }
}
