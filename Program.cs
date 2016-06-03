using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace LockForMe
{
    class Program
    {
        private static SessionController _session;
        static void Main(string[] args)
        {
            _session = new SessionController();

            Console.WriteLine("Esperando...");

            System.Threading.Thread.Sleep(5000);

            Console.Write(_session.IsSessionLock ? "Session Lock" : "Session Unlock");
        }
    }
}
