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

        public string DomainInfo { get; private set; } = $"{AppDomain.CurrentDomain.Id} {AppDomain.CurrentDomain.FriendlyName}";

        public string RawData => $"{MethodName}|{Size}|{ID}|{(int)TypeMessage}|{DomainInfo}|{Body}";

        private void ParceRaw(string value)
        {
            try
            {
                string[] parts = value.Split('|');

                MethodName = parts[0];
                int size = int.Parse(parts[1]);
                ID = int.Parse(parts[2]);
                TypeMessage = (TypeMessage)int.Parse(parts[3]);
                DomainInfo = parts[4];
                Body = parts.Skip(5).JoinString("|");

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

        public override string ToString()
        {
            return $"{MethodName} ID:{ID} Size:{Size} TypeMessage:{TypeMessage} DomainInfo:{DomainInfo} ";
        }
    }
}
