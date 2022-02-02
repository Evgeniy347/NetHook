using NetHook.Cores.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NetHook.Cores.NetSocket
{
    public enum TypeMessage
    {
        Request = 0,
        Responce = 1
    }

    public class MessageSocket
    {
        public MessageSocket(Socket socket, string method, int id, TypeMessage type)
        {
            Socket = socket;
            MethodName = method;
            TypeMessage = type;
            ID = id;
        }

        public MessageSocket(Socket socket, string value)
        {
            Socket = socket;
            ParceRaw(value);
        }

        public Socket Socket { get; }

        public TypeMessage TypeMessage { get; private set; }

        public string MethodName { get; private set; }

        public int Size => Body?.Length ?? 0;

        public string Body { get; private set; }

        public int ID { get; private set; }

        public string RawData
        {
            get => $"{MethodName}|{Size}|{ID}|{(int)TypeMessage}|{Body}";
        }

        private void ParceRaw(string value)
        {
            try
            {
                int methodIndex = value.IndexOf('|');
                int sizeIndex = value.IndexOf('|', methodIndex + 1);
                int idIndex = value.IndexOf('|', sizeIndex + 1);
                int typeIndex = value.IndexOf('|', idIndex + 1);

                MethodName = value.Substring(0, methodIndex);
                string sizeStr = value.Substring(methodIndex + 1, sizeIndex - methodIndex - 1);
                int size = int.Parse(sizeStr);

                string idStr = value.Substring(sizeIndex + 1, idIndex - sizeIndex - 1);
                ID = int.Parse(idStr);

                string typeStr = value.Substring(idIndex + 1, typeIndex - idIndex - 1);

                TypeMessage = (TypeMessage)int.Parse(typeStr);

                Body = value.Substring(typeIndex + 1);

                if (size != Size)
                    throw new Exception("Check Size Exception");
            }
            catch (Exception ex)
            {
                throw new Exception(value, ex);
            }
        }

        public T GetObject<T>()
        {
            if (Body == string.Empty || Body == null)
                return default;
            else if (typeof(string) == typeof(T))
                return (T)(object)Body;
            else
                return Body.DeserializeJSON<T>();
        }

        public void SetObject(object obj)
        {
            if (obj == null)
                Body = string.Empty;
            else if (typeof(string) == obj.GetType())
                Body = (string)obj;
            else
            {
                Body = obj.SerializerJSON();
            }
        }
    }
}
