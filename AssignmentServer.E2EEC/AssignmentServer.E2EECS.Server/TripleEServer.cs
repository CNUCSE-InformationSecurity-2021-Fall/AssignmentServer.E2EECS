using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AssignmentServer.E2EECS.Server
{
    public class TripleEServer
    {
        readonly InternalSocket isck;
        readonly Dictionary<string, InternalSocketContext> sessions;

        readonly List<TripleEMethods> RelayResponseMethods = new() {
                TripleEMethods.KEYXCHG,
                TripleEMethods.KEYXCHGRST,
                TripleEMethods.KEYXCHGOK, 
                TripleEMethods.KEYXCHGFAIL,
                TripleEMethods.MSGSENDOK,
                TripleEMethods.MSGSENDFAIL
        };

        public TripleEServer()
        {
            isck = new InternalSocket(8080);
            isck.OnDataReceived = InternalSocketReceived;

            sessions = new Dictionary<string, InternalSocketContext>();

            isck.Ignite();
        }

        public bool SessionAdd(string name, InternalSocketContext context)
        {
            if (sessions.ContainsKey(name))
                return false;

            sessions.Add(name, context);
            return true;
        }

        public InternalSocketContext SessionGet(string name)
        {
            return sessions.GetValueOrDefault(name);
        }

        public bool SessionClose(string name)
        {
            if (!sessions.ContainsKey(name))
                return false;

            return sessions.Remove(name);
        }

        public void SessionValidate(TripleEContext context, string credential)
        {
            var session = SessionGet(credential);

            if (session is null)
                throw new Exception("session not exists");

            if (session.AcceptedSocket.RemoteEndPoint != context.SocketContext.AcceptedSocket.RemoteEndPoint)
                throw new Exception("session and connection does not match");

        }

        private byte[] DispatchMethod(TripleEContext context)
        {
            TripleEContext result = null;

            if (RelayResponseMethods.Contains(context.Method)) 
            {
                result = this.Method_RELAY(context);
            }
            else
            {
                var serverType = typeof(TripleEServerExtensions);
                var method = serverType.GetMethod($"Method_{context.Method}");

                if (method is null)
                    throw new Exception("Invalid Method");

                result = method.Invoke(null, new object[] { this, context }) as TripleEContext;
            }

            return result?.Serialize() ?? Array.Empty<byte>();
        }

        private TripleEContext ParseBuffer(byte[] buffer)
        {
            using var stream = new MemoryStream(buffer);
            using var reader = new StreamReader(stream);

            var preamble = reader?.ReadLine()?.Split(' ');

            if (preamble is null)
            {
                return null;
            }

            if (preamble.Length != 2 || preamble[0] != "3EPROTO")
            {
                return null;
            }

            TripleEContext context = new()
            {
                Method = Enum.Parse<TripleEMethods>(preamble[1]),
                Headers = new()
            };

            while (true)
            {
                var line = reader.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(line))
                {
                    break;
                }

                var firstColonPos = line.IndexOf(':');
                var valueStartPos = firstColonPos + 1;

                if (firstColonPos == -1)
                    break;

                var key = line[0..firstColonPos].Trim('\0', '\n', ' ');
                var value = line[valueStartPos..].Trim('\0', '\n', ' ');

                context.Headers.Add(key, value);
            }

            context.Body = reader.ReadToEnd()?.Trim();

            return context;
        }

        public void RelayRequest(InternalSocketContext to, TripleEContext context)
        {
            if (!context.Headers.ContainsKey("Timestamp"))
                context.Headers.Add("Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            var data = context.Serialize();
            to.AcceptedSocket.SendAsync(data, SocketFlags.None);
        }

        private byte[] InternalSocketReceived(InternalSocketContext primitiveContext)
        {
            if (primitiveContext is null)
                return Array.Empty<byte>();

            try
            {
                var context = ParseBuffer(primitiveContext.Buffer);

                if (context is null) 
                {
                    Console.WriteLine("[ERROR] Failed to parse buffer into context");
                    return Array.Empty<byte>();
                }

                context.SocketContext = primitiveContext;

                return DispatchMethod(context);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is not null)
                    ex = ex.InnerException;

                Console.WriteLine("[ERROR] {0}", ex.Message);

                primitiveContext.Invalid = true;
                return TripleEContext.ErrorContext(ex.Message).Serialize();
            }
        }
    }
}
