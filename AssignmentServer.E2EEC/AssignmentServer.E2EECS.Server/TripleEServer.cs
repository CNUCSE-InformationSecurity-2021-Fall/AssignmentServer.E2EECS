using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentServer.E2EECS.Server
{
    public class TripleEServer
    {
        readonly InternalSocket isck;
        readonly Dictionary<string, TripleEContext> sessions;

        public TripleEServer()
        {
            isck = new InternalSocket(8080);
            isck.OnDataReceived = InternalSocketReceived;

            sessions = new Dictionary<string, TripleEContext>();
        }

        public bool SessionAdd(string name, TripleEContext context)
        {
            if (sessions.ContainsKey(name))
                return false;

            sessions.Add(name, context);
            return true;
        }

        public TripleEContext SessionGet(string name)
        {
            return sessions.GetValueOrDefault(name);
        }

        public bool SessionClose(string name)
        {
            if (!sessions.ContainsKey(name))
                return false;

            return sessions.Remove(name);
        }

        private byte[] DispatchMethod(TripleEContext context)
        {
            var serverType = typeof(TripleEServer);
            var method = serverType.GetMethod(context.Method.ToString());

            if (method is null)
                throw new Exception("Invalid Method");
            
            var result = method.Invoke(this, new object[] { context }) as TripleEContext;

            return result.Serialize();
        }

        private byte[] InternalSocketReceived(InternalSocketContext primitiveContext)
        {
            if (primitiveContext is null)
                return Array.Empty<byte>();

            try
            {
                using var stream = new MemoryStream(primitiveContext.Buffer);
                using var reader = new StreamReader(stream);

                var preamble = reader.ReadLine().Split(' ');

                if (preamble.Length != 2 || preamble[0] != "3EPROTO")
                    throw new Exception("Invalid packet send");

                TripleEContext context = new()
                {
                    Method = Enum.Parse<TripleEMethods>(preamble[1]),
                    Headers = new ()
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

                    var key = line[0..firstColonPos];
                    var value = line[valueStartPos..];

                    context.Headers.Add(key, value);
                }

                context.Body = reader.ReadToEnd();

                return DispatchMethod(context);
            }
            catch (Exception ex)
            {
                primitiveContext.Invalid = true;
                return TripleEContext.ErrorContext(ex.Message).Serialize();
            }
        }
    }
}
