using NetHook.Cores.Socket;
using SocketLibrary;
using System;
using System.Collections.Generic;
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
            string message = connectedSocket.FullReceive();

            if (string.IsNullOrEmpty(message))
                return new MessageSocket();

            MessageSocket result = new MessageSocket(message);

            if (method != null && result.MethodName != method)
                throw new Exception($"Check Method '{method}' {Environment.NewLine}{message}");

            Console.WriteLine($"AcceptMessage:{result.MethodName} {result.Size} {connectedSocket.UnderlyingSocket.LocalEndPoint}");
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

        public static void SendMessage<TMessage>(this ConnectedSocket connectedSocket, TMessage message)
        {
            string messageJson = message.SerializerJSON();
            lock (connectedSocket)
                connectedSocket.Send(messageJson);
        }

        public static void SendMessage<TMessage>(this ConnectedSocket connectedSocket, string methodName, TMessage message)
        {
            MessageSocket messageSocket = new MessageSocket()
            {
                MethodName = methodName,
                Body = message.SerializerJSON()
            };

            Console.WriteLine($"SendMessage:{messageSocket.MethodName} {messageSocket.Size} {connectedSocket.UnderlyingSocket.LocalEndPoint}");
            connectedSocket.Send(messageSocket.RawData);
        }

        public static void SendMessage(this ConnectedSocket connectedSocket, MessageSocket message)
        {
            connectedSocket.Send(message.RawData);
        }

        public static void SendMessage(this ConnectedSocket connectedSocket, string method, string body)
        {
            MessageSocket message = new MessageSocket()
            {
                MethodName = method,
                Body = body
            };

            Console.WriteLine($"Send:{message.MethodName} {message.Size} {connectedSocket.UnderlyingSocket.LocalEndPoint}");

            connectedSocket.Send(message.RawData);
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
            var buffer = new List<byte>();

            while (connectedSocket.UnderlyingSocket.Available == 0)
                Thread.Sleep(100);

            while (connectedSocket.UnderlyingSocket.Available > 0)
            {
                var currByte = new Byte[1];
                var byteCounter = connectedSocket.UnderlyingSocket.Receive(currByte, currByte.Length, SocketFlags.None);
                if (byteCounter.Equals(1))
                    buffer.Add(currByte[0]);
                else
                    break;
            }
            return buffer.ToArray();
        }
    }
}
