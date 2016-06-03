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
        private static bool isLock;
        static void Main(string[] args)
        {
            _session = new SessionController();
            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);

            Console.WriteLine("The Session Switch is being monitored now.");
            Console.WriteLine(" - To close, type CLOSE and hit Return.");
            Console.WriteLine(" - To change the Idle time, type TIME 60, when 60 is the seconds to lock the       session for you.");
            Console.WriteLine(" ");

            var cmd = "";
            while (!cmd.ToUpper().Equals("CLOSE"))
            {
                cmd = Console.ReadLine();
            }
        }

        static void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    //I left my desk
                    isLock = true;
                    Console.WriteLine("I left my desk");
                    break;
                case SessionSwitchReason.SessionUnlock:
                case SessionSwitchReason.SessionLogon:
                    //I returned to my desk
                    isLock = false;
                    Console.WriteLine("I returned to my desk");
                    break;
            }
        }
    }
}
