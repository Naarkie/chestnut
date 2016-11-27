using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using StackExchange.Redis;

namespace Tracker
{
    public class Program
    {
        static void Main(string[] args)
        {
            Tracker tracker = new Tracker(6969);
            tracker.AnnounceInterval = 60;
            tracker.Start();

            Console.ReadKey();
        }

    }
}