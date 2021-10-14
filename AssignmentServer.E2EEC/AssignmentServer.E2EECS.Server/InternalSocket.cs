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

        public Func<InternalSocketContext, byte[]> OnDataReceived;
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

        public void Ignite()
        {
            Console.WriteLine("[INNER_SOCKET://{0}] Ignition", masterPort);
            WaitAccept();
        }

        private void WaitAccept()
        {
            masterSocket.BeginAccept(AcceptCallback, null);
            Interlocked.Increment(ref waitingSockets);

            Console.WriteLine("[INNER_SOCKET://{0}] Waiting {1} sockets", masterPort, waitingSockets);
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
                WaitAccept();
            }

            try
            {
                var context = new InternalSocketContext
                {
                    AcceptedSocket = masterSocket.EndAccept(iar),
                    Buffer = new byte[1024],
                    Timer = new InternalSocketTimer(idleTimeMax)
                };

                context.Timer.OnTimerTimeout = 
                    () => {
                        context.AcceptedSocket?.Close();
                        context.Buffer = null;
                        context.Invalid = true;
                    };

                context.AcceptedSocket
                       .BeginReceive(context.Buffer, 0, context.Buffer.Length,
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

            if (context.AcceptedSocket is null || !context.AcceptedSocket.Connected)
                return;

            try
            {
                Interlocked.Exchange(ref idleTime, 0);

                var receivedBytes = context.AcceptedSocket.EndReceive(iar);

                if (receivedBytes == 0) {
                    context.AcceptedSocket.Close();
                    return;
                }

                context.Buffer = context.Buffer[0..receivedBytes];

                var resultByteList = OnDataReceived?.Invoke(context) ?? Array.Empty<byte>();
                var zeroPos = 0;

                for (var i = 0; i < resultByteList.Length; ++i) {
                    if (resultByteList[i] == 0x00) break;
                    zeroPos++;
                }
                
                var result = resultByteList[0..zeroPos];

                if (result.Length > 0)
                {
                    context.AcceptedSocket.BeginSend(
                        result, 0, result.Length,
                        SocketFlags.None, SendCallback, context);
                }
                else
                {
                    context.AcceptedSocket
                           .BeginReceive(context.Buffer, 0, context.Buffer.Length,
                                         SocketFlags.None, ReceiveCallback, context);
                }
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

                context.Buffer = new byte[4096];
                context.AcceptedSocket
                       .BeginReceive(context.Buffer, 0, context.Buffer.Length,
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
        public byte[] Buffer { get; set; }
        public bool Invalid { get; set; }
        public InternalSocketTimer Timer { get; set; }
    }
}
