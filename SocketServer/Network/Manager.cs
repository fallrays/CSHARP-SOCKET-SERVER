using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.DataProtection;
using SocketServer.Buffer;
using SocketServer.Common;

namespace SocketServer.Network
{
	public class NetworkManager
	{
        private static NetworkManager? _instance = null;

        public static NetworkManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NetworkManager();
                return _instance;
            }
        }

        public NetworkManager()
		{
		}

        private SocketListener _socketListener = new SocketListener();
        private SocketAsyncEventArgsPool? _receiveEventArgsPool = null;
        private SocketAsyncEventArgsPool? _sendEventArgsPool = null;

        private BufferManager? _bufferManager = null;
        private int _bufferSize;

        private Queue<byte[]> sendQueue = new Queue<byte[]>();

        public void Init()
        {
            _bufferSize = Define.BUFFER_SIZE * 4;
            _bufferManager = new BufferManager(_bufferSize);

            _receiveEventArgsPool = new SocketAsyncEventArgsPool(Define.MAX_CONNECTION);
            _sendEventArgsPool = new SocketAsyncEventArgsPool(Define.MAX_CONNECTION);

            SocketAsyncEventArgs arg;

            for (int i = 0; i < Define.MAX_CONNECTION; i++)
            {
                Client info = new Client();

                {
                    arg = new SocketAsyncEventArgs();
                    arg.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);
                    arg.UserToken = info;
                    _bufferManager.SetBuffer(ref arg);
                    _receiveEventArgsPool.Push(ref arg);
                }
                {
                    arg = new SocketAsyncEventArgs();
                    arg.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
                    arg.UserToken = info;
                    _bufferManager.SetBuffer(ref arg);
                    _sendEventArgsPool.Push(ref arg);
                }
            }

            Console.WriteLine("NETWORK INIT OK!!!");
        }

        public void SocketListen()
        {
            _socketListener.Start(OnConnectClient);
        }

        public void OnConnectClient(ref Socket socket, ref Client client)
        {
            Console.WriteLine($"[{DateTime.Now}] TID : {Thread.CurrentThread.ManagedThreadId}] Connect Success!");

            //if (_receiveEventArgsPool.Count == 0)
            //IncreaseClientPool();

            SocketAsyncEventArgs receiveArgs = _receiveEventArgsPool.Pop();
            SocketAsyncEventArgs sendArgs = _sendEventArgsPool.Pop();

            //Client? newClient = receiveArgs.UserToken as Client;
            //IPEndPoint clientAddr = (IPEndPoint)socket.RemoteEndPoint;
            //newClient.ipAddr = clientAddr.Address.ToString();
            //newClient.Reset();

            //Console.WriteLine("New Client Connect: {0}, socket handle={1}", newClient.ipAddr, socket.Handle);

            //if (_callbackSessionCreated != null)
            //    _callbackSessionCreated(newClient);

            BeginReceive(ref socket, ref receiveArgs, ref sendArgs);
        }

        private void BeginReceive(ref Socket socket, ref SocketAsyncEventArgs receiveArgs, ref SocketAsyncEventArgs sendArgs)
        {
            Console.WriteLine($"[{DateTime.Now}] TID : {Thread.CurrentThread.ManagedThreadId}] Begin Receive...");

            Client? client = receiveArgs.UserToken as Client;
            client.SetEventArgs(ref socket, ref receiveArgs, ref sendArgs);

            bool pending = socket.ReceiveAsync(receiveArgs);

            if (!pending)
                ProcessReceive(ref receiveArgs);
        }

        private void ProcessReceive(ref SocketAsyncEventArgs e)
        {
            Client client = e.UserToken as Client;

            //Console.WriteLine($"[{DateTime.Now}] TID : {Thread.CurrentThread.ManagedThreadId}] Receiving...");

            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                byte[] b = e.Buffer;
                //client.OnReceive(ref b, e.Offset, e.BytesTransferred);
                string recvData = System.Text.Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                byte[] sendArray = System.Text.Encoding.UTF8.GetBytes("[^^]" + recvData);

                //Console.WriteLine();
                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}]" + " CLIENT SEND : {0}", recvData);

                client._sendEvnetArgs.SetBuffer(sendArray);
                bool pending1 = client.socket.SendAsync(client._sendEvnetArgs);
                if (pending1 == false)
                {
                    OnSendCompleted(null, client._sendEvnetArgs);
                }

                bool pending = client.socket.ReceiveAsync(e);
                if (!pending)
                    ProcessReceive(ref e);
                //Array.Clear(e.Buffer, e.Offset, e.BytesTransferred);
            }
            else
            {
                // disconnected.
                //Console.WriteLine("error={0}, transferred={1}", e.SocketError, e.BytesTransferred);
                Console.WriteLine($"[{DateTime.Now}] TID : {Thread.CurrentThread.ManagedThreadId}] Disconnected!!!");
                CloseClient(client);
            }
        }

        public void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                ProcessReceive(ref e);
                return;
            }

            throw new ArgumentException("was not a receive.");
        }

        public void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            Client client = e.UserToken as Client;
            //ProcessSend(e);
        }

        public void CloseClient(Client client)
        {
            client?.OnRemoved();

            _receiveEventArgsPool?.Push(client._receiveEventArgs);
            _sendEventArgsPool?.Push(client._sendEvnetArgs);
        }
    }
}

