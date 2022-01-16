using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetHook.Cores.Socket
{
    public class MessageSocket
    {
        public MessageSocket() { }

        public MessageSocket(string value) { ParceRaw(value); }

        public string MethodName { get; set; }

        public int Size => Body.Length;

        public string Body { get; set; }

        public string RawData { get => $"{MethodName}|{Size}|{Body}"; set => ParceRaw(value); }

        private void ParceRaw(string value)
        {
            try
            {
                int methodIndex = value.IndexOf('|');
                int sizeIndex = value.IndexOf('|', methodIndex + 1);

                MethodName = value.Substring(0, methodIndex);
                string sizeStr = value.Substring(methodIndex + 1, sizeIndex - methodIndex - 1);

                int size = int.Parse(sizeStr);
                Body = value.Substring(sizeIndex + 1);

                if (size != Size)
                    throw new Exception("Check Size Exception");
            }
            catch (Exception ex)
            {
                throw new Exception(value, ex);
            }
        }
    }
}
