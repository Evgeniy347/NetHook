using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetHook.Core;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NetHook.MSTest
{
    [TestClass]
    public class UnitTestLocalHook
    {
        [TestMethod]
        public void TestInstanceHook()
        {
            TestInstance instance1 = new TestInstance() { Instance = "1" };
            TestInstance instance2 = new TestInstance() { Instance = "2" };

            MethodInfo methodInstance = typeof(TestInstance).GetMethod("TestInstanceMethod", BindingFlags.Instance | BindingFlags.Public);

            Action testAction = () =>
            {
                Assert.AreEqual($"TestInstanceMethod 1 test", instance1.TestInstanceMethod("test"));
                Assert.AreEqual($"TestInstanceMethod 2 test", instance2.TestInstanceMethod("test"));
            };

            Helpers.TestInvoke<TestInstance, string, string>(testAction, 2, methodInstance);
        }

        [TestMethod]
        public void TestPrivateInstanceHook()
        {
            TestPrivateInstance instance1 = new TestPrivateInstance() { Instance = "1" };
            TestPrivateInstance instance2 = new TestPrivateInstance() { Instance = "2" };

            MethodInfo method = typeof(TestPrivateInstance).GetMethod("TestInstanceMethod", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo methodStatic = typeof(TestPrivateInstance).GetMethod("TestStaticMethod");

            Action testAction = () =>
            {
                Assert.AreEqual($"TestInstanceMethod 1 test", instance1.TestInstanceMethod("test"));
                Assert.AreEqual($"TestInstanceMethod 2 test", instance2.TestInstanceMethod("test"));
                Assert.AreEqual($"TestStaticMethod test", TestPrivateInstance.TestStaticMethod("test"));
            };

            Helpers.TestInvoke<TestPrivateInstance, string, string>(testAction, 3, method, methodStatic);
        }


        [TestMethod]
        public void TestStaticHook()
        {
            MethodInfo method = typeof(TestInstance).GetMethod("TestStaticMethod");

            Action testAction = () =>
            {
                Assert.AreEqual($"TestStaticMethod test", TestInstance.TestStaticMethod("test"));
            };

            Helpers.TestInvoke<TestInstance, string, string>(testAction, 1, method);
        }


        [TestMethod]
        public void TestPrivateArgumentHook()
        {
            TestPrivateArgumentInstance instance1 = new TestPrivateArgumentInstance() { Instance = "1" };
            TestPrivateArgumentInstance instance2 = new TestPrivateArgumentInstance() { Instance = "2" };

            MethodInfo method = typeof(TestPrivateArgumentInstance).GetMethod("TestInstanceMethod", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo methodStatic = typeof(TestPrivateArgumentInstance).GetMethod("TestStaticMethod");

            Action testAction = () =>
            {
                Assert.AreEqual($"TestInstanceMethod 1 3", instance1.TestInstanceMethod(new TestPrivateInstance() { Instance = "3" }));
                Assert.AreEqual($"TestInstanceMethod 2 4", instance2.TestInstanceMethod(new TestPrivateInstance() { Instance = "4" }));
                Assert.AreEqual($"TestStaticMethod 5", TestPrivateArgumentInstance.TestStaticMethod(new TestPrivateInstance() { Instance = "5" }));
            };

            Helpers.TestInvoke<TestPrivateArgumentInstance, string, TestPrivateInstance>(testAction, 3, method, methodStatic);
        }

        [TestMethod]
        public void TestStructArgumentHook()
        {
            MethodInfo method = typeof(TestStructInstance).GetMethod("TestArgumentMethod");

            Action testAction = () =>
            {
                DateTime dateTime = DateTime.Now;
                Assert.AreEqual($"TestArgumentMethod {dateTime}", TestStructInstance.TestArgumentMethod(dateTime));
            };

            try
            {
                Helpers.TestInvoke<TestStructInstance, string, DateTime>(testAction, 1, method);
            }
            catch (NotImplementedException ex)
            {
            }
        }

        [TestMethod]
        public void TestStructReturnHook()
        {
            MethodInfo method = typeof(TestStructInstance).GetMethod("TestRetMethod");

            Action testAction = () =>
            {
                string dateTime = DateTime.Now.ToString();
                Assert.AreEqual(dateTime, TestStructInstance.TestRetMethod(dateTime).ToString());
            };

            try
            {
                Helpers.TestInvoke<TestStructInstance, DateTime, string>(testAction, 1, method);
            }
            catch (NotImplementedException ex)
            {
            }
        }

        [TestMethod]
        public void TestArgsHook()
        {
            TestInstance instance1 = new TestInstance() { Instance = "1" };
            TestInstance instance2 = new TestInstance() { Instance = "2" };

            MethodInfo methodInstance = typeof(TestInstance).GetMethod("TestInstanceMethodArgs", BindingFlags.Instance | BindingFlags.Public);

            Action testAction = () =>
            {
                string[] args = new string[10];
                for (int i = 0; i < args.Length; i++)
                    args[i] = $"arg_{i}";

                Assert.AreEqual($"TestInstanceMethodArgs 1 {string.Join(" ", args)}", instance1.TestInstanceMethodArgs(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]));
                Assert.AreEqual($"TestInstanceMethodArgs 2 {string.Join(" ", args)}", instance2.TestInstanceMethodArgs(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]));
            };

            Helpers.TestInvoke<TestInstance, string, string>(testAction, 2, 10, methodInstance);
        }

        public class TestStructInstance
        {
            public static string TestArgumentMethod(DateTime date)
            {
                return $"{nameof(TestArgumentMethod)} {date}";
            }

            public static DateTime TestRetMethod(string dateStr)
            {
                return DateTime.Parse(dateStr);
            }
        }

        public class TestInstance
        {
            public string Instance;
            public string TestInstanceMethod(string value)
            {
                return $"{nameof(TestInstanceMethod)} {Instance} {value}";
            }

            public string TestInstanceMethodArgs(string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8, string arg9, string arg10)
            {
                return $"{nameof(TestInstanceMethodArgs)} {Instance} {arg1} {arg2} {arg3} {arg4} {arg5} {arg6} {arg7} {arg8} {arg9} {arg10}";
            }

            public static string TestStaticMethod(string value)
            {
                return $"{nameof(TestStaticMethod)} {value}";
            }
        }

        private class TestPrivateInstance
        {
            public string Instance;
            public string TestInstanceMethod(string value)
            {
                return $"{nameof(TestInstanceMethod)} {Instance} {value}";
            }

            public static string TestStaticMethod(string value)
            {
                return $"{nameof(TestStaticMethod)} {value}";
            }
        }

        private class TestPrivateArgumentInstance
        {
            public string Instance;
            public string TestInstanceMethod(TestPrivateInstance value)
            {
                return $"{nameof(TestInstanceMethod)} {Instance} {value?.Instance}";
            }

            public static string TestStaticMethod(TestPrivateInstance value)
            {
                return $"{nameof(TestStaticMethod)} {value?.Instance}";
            }
        }

    }
}
