using System;
using System.Net.Sockets;
using System.Net;
using SocketServer.Common;

namespace SocketServer.Network
{
	public class SocketListener
	{

        private SocketAsyncEventArgs? _acceptArgs = null;
        private Socket? _listenSocket = null;

        private bool _bThread = true;
        public delegate void AcceptClientHandler(ref Socket client, ref Client token);
        private AcceptClientHandler? _callback_AcceptClient = null;

        private AutoResetEvent? _flowEvent;

        public async void Start(AcceptClientHandler onConnectcallback)
		{
            _callback_AcceptClient = onConnectcallback;

            string host = Define.BINDING_IP;
            int port = Define.BINDING_PORT;

            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.NoDelay = true;

            IPAddress ipAddr = host == "*" ? IPAddress.Any : IPAddress.Parse(host);
            IPEndPoint endPoint = new IPEndPoint(ipAddr, port);

            try
            {
                _listenSocket.Bind(endPoint);
                _listenSocket.Listen(128);

                Console.WriteLine("Listen Start...");

                _acceptArgs = new SocketAsyncEventArgs();
                _acceptArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptComplected);

                Thread listenThread = new Thread(ListenThread);
                await Task.Delay(1000);
                listenThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: msg={0}, trace={1}", ex.Message, ex.StackTrace);
            }
        }

        private void ListenThread()
        {
            _flowEvent = new AutoResetEvent(false);

            while (_bThread)
            {
                _acceptArgs.AcceptSocket = null;
                bool pending = true;

                try
                {
                    pending = _listenSocket.AcceptAsync(_acceptArgs);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error Accept. message=" + ex.Message);
                    continue;
                }

                if (!pending)
                {
                    OnAcceptComplected(null, _acceptArgs);
                }

                _flowEvent.WaitOne();
            }
        }

        public void OnAcceptComplected(object? sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Console.WriteLine($"[{DateTime.Now}] TID : {Thread.CurrentThread.ManagedThreadId}] Accept Success!");

                Socket socket = e.AcceptSocket;

                _flowEvent.Set();

                Client client = e.UserToken as Client;
                _callback_AcceptClient?.Invoke(ref socket, ref client);

                return;
            }
            else
            {
                Console.WriteLine("Failed to accept client.");
            }

            _flowEvent.Set();
        }

    }
}

