using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentServer.E2EECS.Server
{
    public static class TripleEServerExtensions
    {
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

            return result;
        }
    }
}
