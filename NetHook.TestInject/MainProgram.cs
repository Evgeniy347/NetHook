using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace NetHook.TestInject
{
    public class MainProgram
    {
        public static void Main(string[] args)
        {
            string jStr = "xxx";
            string value = new MainProgram().TestMethodReqursive(jStr);
            value = new MainProgram().TestMethod_1(jStr, jStr, jStr, jStr, jStr, jStr, jStr);
            value = new MainProgram().TestMethod(jStr);

            Main1();
        }

        public static void Main1()
        {
            while (true)
            {
                try
                {
                    string jStr = "xxx";
                    Console.WriteLine("TestMethodReqursive");
                    Console.ReadLine();
                    string value = new MainProgram().TestMethodReqursive(jStr);
                    Console.WriteLine(value);

                    Console.WriteLine("TestMethod_1");
                    Console.ReadLine();
                    value = new MainProgram().TestMethod_1(jStr, jStr, jStr, jStr, jStr, jStr, jStr);
                    Console.WriteLine(value);

                    Console.WriteLine("TestMethod");
                    Console.ReadLine();
                    value = new MainProgram().TestMethod(jStr);
                    Console.WriteLine(value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public static void Main2()
        {
            for (int i = 0; i < 4; i++)
            {
                Thread thread = new Thread((o) =>
                {
                    string indexStr = o.ToString();
                    int j = 0;
                    string value = null;
                    while (true)
                    {
                        try
                        {
                            Console.ReadLine();
                            j++;
                            string jStr = j.ToString();
                            switch (indexStr)
                            {
                                case "0":
                                    Console.WriteLine("TestMethodReqursive");
                                    Console.ReadLine();
                                    value = new MainProgram().TestMethodReqursive(jStr);
                                    break;
                                case "1":
                                    Console.WriteLine("TestMethod_1");
                                    Console.ReadLine();
                                    value = new MainProgram().TestMethod_1(jStr, jStr, jStr, jStr, jStr, jStr, jStr);
                                    break;
                                default:
                                    Console.WriteLine("TestMethod");
                                    Console.ReadLine();
                                    value = new MainProgram().TestMethod(jStr);
                                    break;
                            }
                            Console.WriteLine(value);
                            Thread.Sleep(i * 100 + 1000);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                });

                thread.IsBackground = true;
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start(i);
            }

            Console.ReadLine();
        }

        public string TestMethod(string value)
        {
            var t = this.GetHashCode();
            return value + " " + t + " " + Thread.CurrentThread.ManagedThreadId + " " + string.Join(" => ", new StackTrace().GetFrames().Select(x => x.GetMethod().Name).Reverse().ToArray());
        }

        public string TestMethod_1(string value, string value2, string value3, string value4, string value5, string value6, string value7)
        {
            return value + " " + Thread.CurrentThread.ManagedThreadId + " " + string.Join(" => ", new StackTrace().GetFrames().Select(x => x.GetMethod().Name).Reverse().ToArray());
        }

        static int index = 0;

        public string TestMethodReqursive(string value)
        {
            if (index == 0)
                index = 5;
            else
            {
                index--;
                return TestMethodReqursive(value);
            }

            return value + " " + AppDomain.CurrentDomain.Id + " " + Thread.CurrentThread.ManagedThreadId + " " + string.Join(" => ", new StackTrace().GetFrames().Select(x => x.GetMethod().Name).Reverse().ToArray());
        }
    }
}
