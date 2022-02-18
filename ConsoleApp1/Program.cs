using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {


        static public void Main(string[] args)
        {
            Console.WriteLine("start");
            try
            {
                while (true)
                {
                    Console.ReadLine();

                    string arg2 = "sdf";
                    string arg = "sdf";
                    Method1(arg);
                    Method1(arg);
                    Method2(arg);
                    Method3(arg2);
                    Method4(arg2);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine("end");
            Console.ReadLine();
        }

        static public void Method1(string arg)
        {
            Console.WriteLine(arg);
        }

        static public void Method2(string arg)
        {
            Console.WriteLine(arg);
        }

        static public void Method3(string arg)
        {
            Console.WriteLine(arg);
        }
        static public void Method4(string arg)
        {
            Console.WriteLine(arg);
        }
    }
}
