using System;
using System.Threading;

namespace AssignmentServer.E2EECS.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new TripleEServer();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
