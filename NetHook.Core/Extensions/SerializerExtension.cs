using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace NetHook.Cores.Extensions
{
    public static class SerializerExtension
    {

        public static string SerializerJSON(this object obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
                serializer.WriteObject(memoryStream, obj);
                string json = Encoding.UTF8.GetString(memoryStream.ToArray());

                return json;
            }
        }

        public static TType DeserializeJSON<TType>(this string json)
        {
            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                try
                {
                    DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(TType));
                    return (TType)deserializer.ReadObject(memoryStream);

                }
                catch (Exception ex)
                {
                    throw new Exception($"{json}{Environment.NewLine}{ex}");
                }
            }
        }
    }
}
