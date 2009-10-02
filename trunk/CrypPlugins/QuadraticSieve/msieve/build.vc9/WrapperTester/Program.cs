using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

namespace WrapperTester
{
    class Program
    {
        private static Queue yieldqueue;
        private static bool running;

        static void prepareSieving(IntPtr conf, int update, IntPtr core_sieve_fcn)
        {
            Console.WriteLine("Update: " + update);

            yieldqueue = Queue.Synchronized(new Queue());
            
            //Create a thread:
            IntPtr clone = Msieve.msieve.cloneSieveConf(conf);
            WaitCallback worker = new WaitCallback(MSieveJob);
            running = true;
            ThreadPool.QueueUserWorkItem(worker, new object[] { clone, update, core_sieve_fcn, yieldqueue });
        }

        public static void MSieveJob(object param)
        {
            
            object[] parameters = (object[])param;
            IntPtr clone = (IntPtr)parameters[0];
            int update = (int)parameters[1];
            IntPtr core_sieve_fcn = (IntPtr)parameters[2];
            Queue yieldqueue = (Queue)parameters[3];

            while (running)
            {
                    Msieve.msieve.collectRelations(clone, update, core_sieve_fcn);                    
                    IntPtr yield = Msieve.msieve.getYield(clone);
                    yieldqueue.Enqueue(yield);
            }
        }

        static void Main(string[] args)
        {            
            Msieve.callback_struct callbacks = new Msieve.callback_struct();
            callbacks.showProgress = delegate(IntPtr conf, int num_relations, int max_relations)
            {                
                System.Console.WriteLine("" + num_relations + " of " + max_relations + " relations!");
                if (num_relations != -1)
                    while (yieldqueue != null && yieldqueue.Count != 0)
                    {
                        Msieve.msieve.saveYield(conf, (IntPtr)yieldqueue.Dequeue());
                        Console.WriteLine("Get yield from queue.");
                    }
                else
                    running = false;                
            };
            callbacks.prepareSieving = prepareSieving;
            
            Msieve.msieve.initMsieve(callbacks);

            //ArrayList factors = Msieve.msieve.factorize("8490874917243147254909119 * 6760598565031862090687387", null);
            ArrayList factors = Msieve.msieve.factorize("(2^300-1)/2", null);
            //ArrayList factors = Msieve.msieve.factorize("(2^200 - 1) / 2", null);            
            foreach (String str in factors)
                Console.WriteLine(str);
            Console.ReadLine();
        }
    }
}
