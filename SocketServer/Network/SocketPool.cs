using System;
using System.Net.Sockets;

namespace SocketServer.Network
{
    public class SocketAsyncEventArgsPool
    {
        private Stack<SocketAsyncEventArgs> _pool;

        public SocketAsyncEventArgsPool(int capacity)
        {
            _pool = new Stack<SocketAsyncEventArgs>(capacity);
        }

        public void Push(ref SocketAsyncEventArgs evt)
        {
            if (evt != null)
            {
                lock (_pool)
                {
                    _pool.Push(evt);
                }
            }
            else
            {
                Console.WriteLine("event added to SocketAsyncEventArgsPool cannot be null.");
            }
        }

        public void Push(SocketAsyncEventArgs evt)
        {
            if (evt != null)
            {
                lock (_pool)
                {
                    _pool.Push(evt);
                }
            }
            else
            {
                Console.WriteLine("event added to SocketAsyncEventArgsPool cannot be null.");
            }
        }

        public SocketAsyncEventArgs Pop()
        {
            lock (_pool)
            {
                return _pool.Pop();
            }
        }

        public int Count { get { return _pool.Count; } }
    }
}

