using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetHook.Core;
using NetHook.Cores.Handlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetHook.MSTest
{
    public static class Helpers
    {
        public static void TestInvoke<TInstance, TResult, TArgument>(Action testAction, int iterInvoke, params MethodInfo[] methods)
        {
            HashSet<object> beforeObj = new HashSet<object>();
            HashSet<object> afterObj = new HashSet<object>();

            int countBefore = 0;
            int countAfter = 0;

            testAction();

            string methodName = new StackTrace().GetFrames().Skip(1).First().GetMethod().Name;

            using (LocalHookAdapter adapter = LocalHookAdapter.CreateInstance())
            {
                adapter.AddHook(methods);

                string fileValue = adapter.CodeDoomProvider.Save();
                string fileName = $@"..\GenerateSourceCode\{methodName}.cs";
                Console.WriteLine(fileName);
                File.WriteAllText(fileName, fileValue);

                StringBuilder stringBuilder = new StringBuilder();
                adapter.Install(stringBuilder);
                Console.WriteLine(stringBuilder);

                ActionHandler handler = new ActionHandler();

                handler.BeforeEvent += (x, y, z) =>
                {
                    if (y != null)
                    {
                        Assert.IsInstanceOfType(y, typeof(TInstance));
                        Assert.AreEqual(beforeObj.Add(y), true);
                    }

                    Assert.AreEqual(z.Length, 1);
                    Assert.IsInstanceOfType(z[0], typeof(TArgument));

                    countBefore++;
                };

                handler.AfterEvent += (x, y, z, e) =>
                {
                    if (y != null)
                    {
                        Assert.IsInstanceOfType(y, typeof(TInstance));
                        Assert.AreEqual(afterObj.Add(y), true);
                    }

                    Assert.IsInstanceOfType(z, typeof(TResult));

                    countAfter++;
                    Assert.AreEqual(countBefore, countAfter);
                };

                adapter.RegisterHandler(handler);

                testAction();
                Assert.AreEqual(countBefore, countAfter);
            }

            testAction();

            Assert.AreEqual(countBefore, iterInvoke);
            Assert.AreEqual(countAfter, iterInvoke);
        }

    }
}
