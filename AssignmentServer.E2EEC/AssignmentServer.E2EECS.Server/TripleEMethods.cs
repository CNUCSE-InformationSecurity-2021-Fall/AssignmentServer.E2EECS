using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentServer.E2EECS.Server
{
    public enum TripleEMethods
    {
        UNKNOWN, RELAYOK,

        /* Request Methods */
        CONNECT, DISCONNECT, KEYXCHG, KEYXCHGRST, MSGSEND,

        /* Response Methods */
        ACCEPT, DENY, BYE, KEYXCHGOK, KEYXCHGFAIL, MSGSENDOK, MSGSENDFAIL, MSGRECV
    }
}
