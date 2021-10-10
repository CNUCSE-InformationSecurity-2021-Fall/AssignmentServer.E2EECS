using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentServer.E2EECS.Server
{
    public static class TripleEServerExtensions
    {
        public static TripleEContext Method_RELAY(this TripleEServer server, TripleEContext reqContext)
        {
            var algo = reqContext.Headers.GetValueOrDefault("Algo");
            var from = reqContext.Headers.GetValueOrDefault("From");
            var to = reqContext.Headers.GetValueOrDefault("To");

            if (algo is null || from is null || to is null)
                throw new Exception("Necessary header omitted; Algo, From and To are mandatory");

            server.SessionValidate(reqContext, from);

            var route = new TripleERoute(server, from, to);
            server.RelayRequest(route.To, reqContext);

            Console.WriteLine($"[RELAY] {reqContext.Method} {from} -> {to}");

            var result = TripleEContext.ResponseContext();
            result.Method = TripleEMethods.RELAYOK;

            return result;
        }

        public static TripleEContext Method_CONNECT(this TripleEServer server, TripleEContext reqContext)
        {
            var credential = reqContext.Headers.GetValueOrDefault("Credential");

            if (credential is null)
                throw new Exception("Username is not specified");

            // check session duplication
            if (server.SessionGet(credential) is not null)
                throw new Exception("Duplicated Username: " + credential);

            var result = TripleEContext.ResponseContext();
            result.Method = TripleEMethods.ACCEPT;
            result.Body = credential;

            server.SessionAdd(credential, reqContext.SocketContext);
            
            var timeout = new InternalSocketTimer();
            timeout.OnTimerTimeout = () =>
            {
                Console.WriteLine("[CONNECT] Connection idle timeout: {0}", credential);
                
                server.SessionClose(credential);
                reqContext.SocketContext.AcceptedSocket.Close();
            };

            reqContext.SocketContext.Timer = timeout;
            Console.WriteLine("[CONNECT] Connection successful: {0}", credential);

            return result;
        }

        public static TripleEContext Method_DISCONNECT(this TripleEServer server, TripleEContext reqContext)
        {
            var credential = reqContext.Headers.GetValueOrDefault("Credential");

            if (credential is null)
                throw new Exception("credential not specified");

            server.SessionValidate(reqContext, credential);

            var result = TripleEContext.ResponseContext();

            result.Method = TripleEMethods.BYE;
            result.Body = null;

            server.SessionClose(credential);

            Console.WriteLine("[DISCONNECT] Connection successfully closed: {0}", credential);

            return result;
        }

        public static TripleEContext Method_MSGSEND(this TripleEServer server, TripleEContext reqContext)
        {
            var from = reqContext.Headers.GetValueOrDefault("From");
            var to = reqContext.Headers.GetValueOrDefault("To");
            var nonce = reqContext.Headers.GetValueOrDefault("Nonce");

            if (from is null || to is null)
                throw new Exception("Necessary header omitted; Algo, From and To are mandatory");

            var result = TripleEContext.ResponseContext();

            if (nonce is not null)
                result.Headers.Add("Nonce", nonce);

            try
            {
                var route = new TripleERoute(server, from, to);
                var relay = TripleEContext.ResponseContext();

                relay.Method = TripleEMethods.MSGRECV;
                relay.Headers.Add("From", from);
                relay.Headers.Add("To", to);
                relay.Body = reqContext.Body;

                server.RelayRequest(route.To, relay);

                result.Method = TripleEMethods.MSGSENDOK;
            }
            catch
            {
                result.Method = TripleEMethods.MSGSENDFAIL;
            }

            return result;
        }
    }
}
