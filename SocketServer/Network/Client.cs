using SocketServer.Common;
using System;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace SocketServer.Network
{
	public class Client
	{
        public Socket? socket { get; private set; }

        public SocketAsyncEventArgs? _receiveEventArgs { get; private set; }
        public SocketAsyncEventArgs? _sendEvnetArgs { get; private set; }

        public Client()
		{
		}

        public void SetEventArgs(ref Socket socket, ref SocketAsyncEventArgs receiveArgs, ref SocketAsyncEventArgs sendArgs)
        {
            _receiveEventArgs = receiveArgs;
            _sendEvnetArgs = sendArgs;
            this.socket = socket;
        }

        public void OnReceive(ref byte[] buffer, int offset, int transfered)
        {
            //_receiver.OnReceive(ref buffer, offset, transfered, OnMessage);
        }

        public void OnRemoved()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Disconnect Exception: message={0}, tracert={1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                socket.Close();
            }

            /*
            SERVER_QUERY.Instance.DisconnectGateway(panID, serverID);
            GAMEWAY_MESSAGE_HANDLER.Instance.OnRemove(uID, this);

            //_scheduleTypes.Clear();
            _sendQ.Clear();
            _receiver.Clear();
            ipAddr = "";
            gatewayID = 0;
            panID = 0;
            serverID = 0;
            tagAssociationElapsedTime = DEFINED.TAG_ASSOCIATION_ELAPSED_TIME;

            _gatewayState = E_GATEWAY_STATE.NONE;
            //_tags.Clear();
            */
        }
    }
}

