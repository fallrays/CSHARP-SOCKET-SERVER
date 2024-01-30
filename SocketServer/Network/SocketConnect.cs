using System;
using System.Net.Sockets;

namespace SocketServer.Network
{
	public class SocketConnect
	{
		public SocketConnect()
		{

		}

        public void OnConnectClient(ref Socket socket, ref Client client)
		{
			Console.WriteLine("Client Connect Success...");
		}



    }
}

