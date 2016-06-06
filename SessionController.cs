using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Build.Tasks;
using Microsoft.Win32;

namespace LockForMe
{
    public class SessionController
    {
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        // time of system idle
        private int _idleInterval;
        // function to log/display all messages
        private Func<string, bool> _doLog;
        // Timer used for check last user input
        private Timer _timer;

        public SessionController(Func<string, bool> logFunc)
        {
            IdleInterval = 120;
            _doLog = logFunc;

            // configure the timer object, for one check at every second
            _timer = new Timer();
            _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            _timer.Interval = 1000;
            _timer.Enabled = true;
        }

        public void SetTimerOn()
        {
            _timer.Enabled = true;
        }
        public void SetTimerOff()
        {
            _timer.Enabled = false;
        }

        public int IdleInterval
        {
            get
            {
                return _idleInterval;
            }
            set
            {
                if (value < 10)
                    _idleInterval = 10;
                else if (value > 3600)
                    _idleInterval = 3600;
                else
                    _idleInterval = value;
            }
        }

        private static void LockSession()
        {
            Process.Start("rundll32.exe", "user32.dll,LockWorkStation");
        }

        private void SetIdleTime(string value)
        {
            try
            {
                value = value.Remove(0, 5).Trim();
            }
            catch
            {
                value = "";
            }

            int lNew;
            if (int.TryParse(value, out lNew))
            {
                IdleInterval = lNew;
                DoLog($"New Idle time set: {IdleInterval} seconds.");
            }
            else
            {
                DoLog($"Error setting new Idle time: \"{lNew}\" is not valid.");
            }
        }

        private void DoLog(string mens)
        {
            if (_doLog != null)
            {
                _doLog(mens);
            }
        }

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static double GetSystemIdleTime()
        {
            uint idleTime = 0;
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;

            uint envTicks = (uint)Environment.TickCount;

            if (GetLastInputInfo(ref lastInputInfo))
            {
                uint lastInputTick = lastInputInfo.dwTime;

                idleTime = envTicks - lastInputTick;
            }

            return ((idleTime > 0) ? (idleTime / 1000) : 0);
        }

        public void CheckCommand(string command)
        {
            var lCmd = command.ToUpper();

            if (lCmd.Equals("LOCK"))
                LockSession();
            else if (lCmd.Contains("TIME"))
                SetIdleTime(lCmd);
            else if (lCmd.Equals("LAST"))
                DoLog(GetSystemIdleTime().ToString(CultureInfo.CurrentCulture));
            else if (lCmd.Equals("SHOW"))
                ShowWindowTaskBar(true);
            else if (lCmd.Equals("HIDE"))
                ShowWindowTaskBar(false);
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (GetSystemIdleTime() >= IdleInterval)
            {
                DoLog(" ");
                DoLog("Your session is locked due to inactivity.");
                LockSession();
            }
        }

        public void ShowWindowTaskBar(bool value)
        {
            var handle = GetConsoleWindow();
            DoLog("Hide from taskbar: " + (value ? "Off" : "On"));
            ShowWindow(handle, value ? SW_SHOW : SW_HIDE);
        }
    }
}