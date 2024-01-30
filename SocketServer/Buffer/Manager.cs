using System;
using System.Net.Sockets;
using SocketServer.Common;

namespace SocketServer.Buffer
{
	public class BufferManager
	{
        private int _bufferSize;

        private Stack<byte[]> _buffers = new Stack<byte[]>();

        public int Count { get { return _buffers.Count; } }

        public BufferManager(int bufferSize = Define.BUFFER_SIZE)
        {
            _bufferSize = bufferSize;
        }

        public bool SetBuffer(ref SocketAsyncEventArgs args)
        {
            try
            {
                byte[] buffer = new byte[_bufferSize];
                _buffers.Push(buffer);
                args.SetBuffer(buffer, 0, _bufferSize);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}

