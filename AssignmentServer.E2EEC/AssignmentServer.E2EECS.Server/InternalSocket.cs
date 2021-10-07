using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AssignmentServer.E2EECS.Server
{
    public class InternalSocket
    {
        private Socket masterSocket;
        private int masterPort = -1;
        private int waitingSockets = 0;

        private int idleTimeMax = 300;
        private int idleTime = 0;

        public Func<int, byte[], byte[]> OnDataReceived;
        public Action OnSocketTimeout;

        public InternalSocket(int port)
        {
            masterPort = port;
            masterSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            InitializeSocket();
        }

        private void InitializeSocket()
        {
            masterSocket.Bind(new IPEndPoint(IPAddress.Any, masterPort));
            masterSocket.Listen(1024);

            Console.WriteLine("[INNER_SOCKET://{0}] Successfuly bound internal socket", masterPort);
        }

        public InternalSocket SetTimeout(int newIdleTime)
        {
            if (newIdleTime < 1)
                return this;

            idleTimeMax = newIdleTime;
            return this;
        }

        public InternalSocket RegisterReceiver(Func<int, byte[], byte[]> receiver)
        {
            OnDataReceived = receiver;
            return this;
        }

        public void Ignite()
        {
            Console.WriteLine("[INNER_SOCKET://{0}] Ignition", masterPort);

            masterSocket.BeginAccept(AcceptCallback, null);

            if (idleTimeMax > 0)
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(1000);
                        Interlocked.Increment(ref idleTime);

                        if (idleTime > idleTimeMax)
                        {
                            lock (masterSocket)
                            {
                                Console.WriteLine("[INNER_SOCKET://{0}] Master socket terminated due to timeout", masterPort);

                                masterSocket.Close();
                                masterSocket = null;

                                OnSocketTimeout?.Invoke();

                                break;
                            }
                        }
                    }
                });
            }
        }

        private void AcceptCallback(IAsyncResult iar)
        {
            if (masterSocket is null)
            {
                return;
            }

            Interlocked.Decrement(ref waitingSockets);

            if (waitingSockets == 0)
            {
                masterSocket.BeginAccept(AcceptCallback, null);
                Interlocked.Increment(ref waitingSockets);
            }

            try
            {
                var context = new InternalSocketContext
                {
                    AcceptedSocket = masterSocket.EndAccept(iar),
                    ContextBuffer = new byte[1024]
                };

                context.AcceptedSocket
                       .BeginReceive(context.ContextBuffer, 0, context.ContextBuffer.Length,
                                     SocketFlags.None, ReceiveCallback, context);
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("NullReferenceException: {0}", ex.Message);
            }
            catch (SocketException ex)
            {
                var localEp = masterSocket.LocalEndPoint as IPEndPoint;

                Console.WriteLine("[INNER_SOCKET://{0}/ACCEPT] {1}", localEp.Port, ex.Message);
            }
        }

        private void ReceiveCallback(IAsyncResult iar)
        {
            var context = iar.AsyncState as InternalSocketContext;

            if (context is null)
                return;

            try
            {
                Interlocked.Exchange(ref idleTime, 0);

                var receivedBytes = context.AcceptedSocket.EndReceive(iar);
                var result = OnDataReceived?.Invoke(receivedBytes, context.ContextBuffer)
                             ?? Array.Empty<byte>();

                context.AcceptedSocket
                       .BeginSend(result, 0, result.Length,
                                  SocketFlags.None, SendCallback, context);
            }
            catch (SocketException ex)
            {
                var localEp = masterSocket.LocalEndPoint as IPEndPoint;

                Console.WriteLine("[INNER_SOCKET://{0}/RECEIVE] {1}", localEp.Port, ex.Message);
            }
        }

        private void SendCallback(IAsyncResult iar)
        {
            var context = iar.AsyncState as InternalSocketContext;

            if (context is null)
                return;

            try
            {
                var sentBytes = context.AcceptedSocket.EndSend(iar);

                context.AcceptedSocket
                       .BeginReceive(context.ContextBuffer, 0, context.ContextBuffer.Length,
                                     SocketFlags.None, ReceiveCallback, context);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("[INNER_SOCKET://{0}/SEND] {1}", masterPort, ex.Message);
            }
        }
    }

    public class InternalSocketContext
    {
        public Socket AcceptedSocket { get; set; }
        public byte[] ContextBuffer { get; set; }
    }
}
