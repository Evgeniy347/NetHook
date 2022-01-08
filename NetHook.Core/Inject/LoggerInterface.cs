using System;
using System.ComponentModel;

namespace NetHook.Cores.Inject
{
    public class LoggerInterface : MarshalByRefObject
    {
        public void OnCreateFile(Int32 InClientPID, String[] InFileNames)
        {
            if (InFileNames == null)
            {
                Console.WriteLine("null");
                return;
            }

            for (int i = 0; i < InFileNames.Length; i++)
            {
                Console.WriteLine(InFileNames[i]);
            }
        }
    }
}
