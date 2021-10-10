using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentServer.E2EECS.Server
{
    public class TripleERoute
    {
        public InternalSocketContext From { get; set; }
        public InternalSocketContext To { get; set; }

        public TripleERoute(TripleEServer server, string from, string to)
        {
            var fromSession = server.SessionGet(from);

            if (fromSession is null)
                throw new Exception($"Session {from} not found");

            var toSession = server.SessionGet(to);

            if (toSession is null)
                throw new Exception($"Session {to} not found");

            From = fromSession;
            To = toSession;
        }
    }
}
