using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Syndll2
{
    /// <summary>
    /// This class makes sure that only one client at a time can talk to any particular terminal.
    /// This is important, because the 700 series terminals that we are targeting are actually serial devices,
    /// with an internal ethernet to serial bridge.  The actual terminal hardware is only capable of performing
    /// one command at a time.  HOWEVER - we do want to be able to talk to *different* terminals at the same time.
    /// </summary>
    internal static class GateKeeper
    {
        private static readonly object Locker = new object();
        private static readonly HashSet<IPEndPoint> Connections = new HashSet<IPEndPoint>();

        public static void Enter(IPEndPoint endPoint, TimeSpan timeout)
        {
            if (endPoint == null)
                return;

            var sw = new Stopwatch();
            sw.Start();

            while (true)
            {
                if (sw.Elapsed > timeout)
                    throw new TimeoutException("Timeout occurred while waiting for an open connection to the terminal.");

                lock (Locker)
                {
                    if (!Connections.Contains(endPoint))
                    {
                        Connections.Add(endPoint);
                        return;
                    }
                }

                // This delay is essential to keep the loop from eating up too many CPU cycles
                Thread.Sleep(10);
            }
        }

#if NET_45
        public static async Task EnterAsync(IPEndPoint endPoint, TimeSpan timeout)
        {
            if (endPoint == null)
                return;

            var sw = new Stopwatch();
            sw.Start();

            while (true)
            {
                if (sw.Elapsed > timeout)
                    throw new TimeoutException("Timeout occurred while waiting for an open connection to the terminal.");

                lock (Locker)
                {
                    if (!Connections.Contains(endPoint))
                    {
                        Connections.Add(endPoint);
                        return;
                    }
                }

                // This delay is essential to keep the loop from eating up too many CPU cycles
                await Task.Delay(10);
            }
        }
#endif

        public static void Exit(IPEndPoint endPoint)
        {
            if (endPoint == null)
                return;

            // We will do this in a new thread as to not block the results.
            Task.Factory.StartNew(() =>
                {
                    // A brief delay here will give the terminal time to catch up between rapid connections.
                    // The delay is required to pass unit tests.
                    Thread.Sleep(10);

                    lock (Locker)
                    {
                        if (Connections.Contains(endPoint))
                            Connections.Remove(endPoint);
                    }
                });
        }
    }
}
