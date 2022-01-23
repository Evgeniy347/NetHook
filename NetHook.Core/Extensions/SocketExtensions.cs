using NetHook.Cores.Socket;
using SocketLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetHook.Cores.Extensions
{
    public static class SocketExtensions
    {
        public static MessageSocket AcceptMessage(this ConnectedSocket connectedSocket, string method = null)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            string message = connectedSocket.FullReceive();

            if (string.IsNullOrEmpty(message))
                return new MessageSocket();
            MessageSocket result = new MessageSocket(message);

            if (method != null && result.MethodName != method)
                throw new Exception($"Check Method '{method}' {Environment.NewLine}{message}");

            Console.WriteLine($"AcceptMessage:{result.MethodName} {result.Size} {connectedSocket.UnderlyingSocket.LocalEndPoint} SocketElapsed {stopwatch.Elapsed}");
            return result;
        }

        public static TMessage AcceptMessage<TMessage>(this ConnectedSocket connectedSocket)
        {
            string messageJson = connectedSocket.FullReceive();

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

        public static void SendMessage<TMessage>(this ConnectedSocket connectedSocket, string methodName, TMessage message)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            MessageSocket messageSocket = new MessageSocket()
            {
                MethodName = methodName,
                Body = message.SerializerJSON()
            };

            connectedSocket.SendMessage(messageSocket);
        }

        public static void SendMessage(this ConnectedSocket connectedSocket, MessageSocket message)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            byte[] rawData = Encoding.UTF8.GetBytes(message.RawData);
            byte[] bytesSize = BitConverter.GetBytes(rawData.Length);

            connectedSocket.UnderlyingSocket.Send(bytesSize);
            connectedSocket.UnderlyingSocket.Send(rawData);

            Console.WriteLine($"SendMessage:{message.MethodName} {message.Size} {connectedSocket.UnderlyingSocket.LocalEndPoint} SocketElapsed {stopwatch.Elapsed}");
        }

        public static void SendMessage(this ConnectedSocket connectedSocket, string method, string body)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            MessageSocket message = new MessageSocket()
            {
                MethodName = method,
                Body = body
            };

            connectedSocket.SendMessage(message);
        }

        public static bool IsSocketConnected(this ConnectedSocket connectedSocket)
        {
            lock (connectedSocket)
            {
                try
                {
                    var soket = connectedSocket.UnderlyingSocket;
                    return !((soket.Poll(1000, SelectMode.SelectRead) && (soket.Available == 0)) || !soket.Connected);
                }
                catch
                {
                    return false;
                }
            }
        }


        public static string FullReceive(this ConnectedSocket connectedSocket)
        {
            byte[] raw = connectedSocket.ReceiveAll();

            string @string = Encoding.UTF8.GetString(raw);
            char[] trimChars = new char[1];
            return @string.TrimEnd(trimChars);
        }

        public static byte[] ReceiveAll(this ConnectedSocket connectedSocket)
        {
            while (connectedSocket.UnderlyingSocket.Available < 4)
                Thread.Sleep(50);

            byte[] sizeBytes = new byte[4];
            var byteCounter = connectedSocket.UnderlyingSocket.Receive(sizeBytes, sizeBytes.Length, SocketFlags.None);

            int size = BitConverter.ToInt32(sizeBytes, 0);

            while (connectedSocket.UnderlyingSocket.Available != size)
                Thread.Sleep(50);

            var result = new byte[size];
            connectedSocket.UnderlyingSocket.Receive(result, result.Length, SocketFlags.None);

            return result;
        }
    }
}
