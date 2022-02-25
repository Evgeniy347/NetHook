using NetHook.Cores.Extensions;
using NetHook.Cores.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetHook.Cores.NetSocket
{
    public abstract class DuplexSocket : IDisposable
    {
        protected Socket _connectedSocketListener;
        private Thread _connectedSocketListenerThread;
        protected Socket _connectedSocketSender;
        private readonly AutoResetEvent _autoResetRequest = new AutoResetEvent(true);
        private readonly AutoResetEvent _autoResetWait = new AutoResetEvent(true);

        private bool _disposed;
        private int _index = 1;
        private readonly ConcurrentDictionary<int, MessageSocket> _messages = new ConcurrentDictionary<int, MessageSocket>();
        private readonly ConcurrentQueue<Action> _processRequest = new ConcurrentQueue<Action>();

        public DuplexSocket()
        { }

        public Dictionary<string, Func<MessageSocket, object>> HandlerRequest { get; set; } = new Dictionary<string, Func<MessageSocket, object>>();

        protected void RunListenThread()
        {
            _connectedSocketListenerThread = ThreadHelper.RunWhileLogic(() =>
            {
                if (_disposed || !_connectedSocketListener.IsSocketConnected())
                    return false;

                MessageSocket message = _connectedSocketListener.AcceptMessage();
                if (message.TypeMessage == TypeMessage.Request)
                {
                    _processRequest.Enqueue(() => RunProcessHandler(message));
                    _autoResetRequest.Set();
                }
                else
                {
                    _messages[message.ID] = message;
                    _autoResetWait.Set();
                }


                return true;
            }, $"ListenThread {_connectedSocketListener.RemoteEndPoint}");
        }

        private void RunProcessHandler(MessageSocket message)
        {
            MessageSocket messageSocket = new MessageSocket(_connectedSocketSender, message.MethodName, message.ID, TypeMessage.Responce);

            if (HandlerRequest != null &&
                HandlerRequest.TryGetValue(message.MethodName, out Func<MessageSocket, object> func))
            {
                try
                {
                    var result = func.Invoke(message);
                    messageSocket.SetObject(result);
                }
                catch (Exception ex)
                {
                    messageSocket.ErrorText = ex.ToString();
                }
            }

            _connectedSocketSender.SendMessage(messageSocket);
        }

        protected void RunProcessQueue()
        {
            ThreadHelper.RunWhileLogic(() =>
            {
                if (_disposed)
                    return false;

                _autoResetRequest.WaitOne(500);

                while (_processRequest.TryDequeue(out Action action))
                    action.Invoke();

                return true;
            }, $"ProcessQueue {_connectedSocketListener.RemoteEndPoint}");
        }

        public int SendMessage<T>(string method, T body)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DuplexSocket));

            MessageSocket messageSocket = new MessageSocket(_connectedSocketSender, method, _index++, TypeMessage.Request);

            messageSocket.SetObject(body);
            _connectedSocketSender.SendMessage(messageSocket);

            return messageSocket.ID;
        }

        public T Receve<T>(int indexMessage)
        {
            if (indexMessage <= 0)
                throw new ArgumentException(nameof(indexMessage));

            while (true)
            {
                if (_messages.TryRemove(indexMessage, out MessageSocket value))
                    return value.GetObject<T>();
                if (_disposed)
                    throw new ObjectDisposedException(nameof(DuplexSocket));
                _autoResetWait.WaitOne(500);
            }
        }

        public TRespoce SendWithReceve<TRequest, TRespoce>(string method, TRequest body)
        {
            int id = SendMessage(method, body);
            TRespoce respoce = Receve<TRespoce>(id);

            return respoce;
        }

        public bool IsSocketConnected()
        {
            return _connectedSocketListenerThread.IsAlive && _connectedSocketSender.IsSocketConnected();
        }

        protected Socket ConnectSoccet(string host, int port)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(host, port);

            return socket;
        }

        public void Dispose()
        {
            _disposed = true;

            try
            {
                _connectedSocketListener?.DisposeSocket();
            }
            catch { }
            try
            {
                _connectedSocketSender?.DisposeSocket();
            }
            catch { }

            _connectedSocketSender = null;
            _connectedSocketListener = null;
            _autoResetRequest.Set();
            _autoResetWait.Set();
        }
    }
}
