using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentServer.E2EECS.Server
{
    public class TripleEContext
    {
        public InternalSocketContext SocketContext { get; set; }

        public TripleEMethods Method { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }

        public byte[] Serialize()
        {
            var payloadBuilder = new StringBuilder("3EPROTO ");

            payloadBuilder.AppendFormat("{0}\n", Method.ToString());

            Headers.Keys.ToList()
                   .ForEach(key =>
                        payloadBuilder.AppendFormat("{0}:{1}\n", key, Headers[key])
                   );

            payloadBuilder.AppendLine();
            payloadBuilder.Append(Body);

            return Encoding.UTF8.GetBytes(payloadBuilder.ToString());
        }

        public static TripleEContext ResponseContext()
        {
            return new()
            {
                Method = TripleEMethods.UNKNOWN,
                Headers = new()
                {
                    { "Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                },
                Body = null
            };
        }

        public static TripleEContext ErrorContext(string message = null)
        {
            var context = ResponseContext();

            context.Method = TripleEMethods.DENY;
            context.Body = message.Trim('\0', ' ', '\n');

            return context;
        }
    }
}
