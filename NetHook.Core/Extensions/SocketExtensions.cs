using NetHook.Cores.NetSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetHook.Cores.Extensions
{
    public static class SocketExtensions
    {

        public static void SetDefaultProperty(this Socket socket)
        {
            socket.ReceiveTimeout = 1000;
            socket.ReceiveBufferSize = int.MaxValue;
            socket.SendBufferSize = int.MaxValue;
        }

        public static string GetKey(this Socket socket)
        {
            var epl = socket.LocalEndPoint as IPEndPoint;
            var epr = socket.RemoteEndPoint as IPEndPoint;

            return $"({epl.Port}/{epr.Port})";
        }

        public static MessageSocket AcceptMessage(this Socket socket, string method = null)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            string message = socket.FullReceive();

            MessageSocket result = new MessageSocket(socket, message);

            if (method != null && result.MethodName != method)
                throw new Exception($"Check Method '{method}' {Environment.NewLine}{message}");

            Console.WriteLine($"Port:{socket.GetKey()} AcceptMessage:{result} SocketElapsed {stopwatch.Elapsed}");

            if (result.ID <= 0)
            {
                Console.WriteLine(message);
                throw new ArgumentException(nameof(result.ID));
            }
            return result;
        }

        public static TMessage AcceptMessage<TMessage>(this Socket socket)
        {
            string messageJson = socket.FullReceive();

            if (string.IsNullOrEmpty(messageJson))
                return default;

            try
            {
                TMessage message = messageJson.DeserializeJSON<TMessage>();
                return message;
            }
            catch (Exception ex)
            {
                throw new Exception(messageJson, ex);
            }
        }



        public static void SendMessage(this Socket socket, string method, string body, int id, TypeMessage type)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            MessageSocket message = new MessageSocket(socket, method, id, type);

            message.SetObject(body);
            socket.SendMessage(message);
        }

        public static bool IsSocketConnected(this Socket socket)
        {
            lock (socket)
            {
                try
                {
                    return !((socket.Poll(1000, SelectMode.SelectRead) && (socket.Available == 0)) || !socket.Connected);
                }
                catch
                {
                    return false;
                }
            }
        }


        public static string FullReceive(this Socket socket)
        {
            byte[] raw = socket.ReceiveAll();

            string @string = Encoding.UTF8.GetString(raw);
            char[] trimChars = new char[1];
            return @string.TrimEnd(trimChars);
        }

        public static void SendMessage(this Socket socket, MessageSocket message)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            byte[] rawData = Encoding.UTF8.GetBytes(message.RawData);
            byte[] bytesSize = BitConverter.GetBytes(rawData.Length);

            try
            {
                if (message.ID <= 0)
                    throw new ArgumentException(nameof(message.ID));

                lock (socket)
                {
                    socket.Send(bytesSize);
                    socket.Send(rawData);
                }
            }
            finally
            {
                Console.WriteLine($"Port:{socket.GetKey()} SendMessage:{message} SocketElapsed {stopwatch.Elapsed}");
            }
        }

        public static byte[] ReceiveAll(this Socket socket)
        {
            lock (socket)
            {
                while (socket.Available < 4)
                    Thread.Sleep(50);

                byte[] sizeBytes = new byte[4];
                var byteCounter = socket.Receive(sizeBytes, sizeBytes.Length, SocketFlags.None);

                int size = BitConverter.ToInt32(sizeBytes, 0);

                while (socket.Available != size)
                    Thread.Sleep(50);

                var result = new byte[size];
                socket.Receive(result, result.Length, SocketFlags.None);

                return result;
            }
        }


        public static void DisposeSocket(this Socket socket)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Dispose();
        }
    }
}
