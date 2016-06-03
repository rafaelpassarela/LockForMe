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
            _session = new SessionController(LogMens);
            
            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);

            Console.WriteLine("The Session Switch is being monitored now.");
            Console.WriteLine(" - To close, type CLOSE and hit Return.");
            Console.WriteLine(" - To change the Idle time, type TIME 60, when 60 is the seconds to lock the       session for you, the default is 120 seconds.");
            Console.WriteLine(" - The first param is the TIME, so call LockForMe 15 for a 15 sec idle.");

            if (args.Length > 0)
                _session.CheckCommand("TIME " + args[0]);

            var cmd = "";
            while (cmd != null && !cmd.ToUpper().Equals("CLOSE"))
            {
                Console.Write("Enter Command: ");
                cmd = Console.ReadLine();
                _session.CheckCommand(cmd);
            }
        }

        private static bool LogMens(string mens)
        {
            Console.WriteLine("  " + mens);
            return true;
        }

        static void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    LogMens("--> You left your desk");
                    // when lock the session, not need the idle verification
                    _session.SetTimerOff();
                    break;
                case SessionSwitchReason.SessionUnlock:
                case SessionSwitchReason.SessionLogon:
                    // when you login the session, start the idle verification
                    _session.SetTimerOn();
                    LogMens("<-- You returned to your desk");
                    break;
            }
        }
    }
}
