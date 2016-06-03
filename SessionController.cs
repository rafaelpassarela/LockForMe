using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LockForMe
{
    public class SessionController
    {
        private const int False = 0;

        private static readonly IntPtr WtsCurrentServer = IntPtr.Zero;

        private const int WtsSessionstateLock = 0;
        private const int WtsSessionstateUnlock = 1;

        private static readonly bool IsWin7;

        static SessionController()
        {
            var osVersion = Environment.OSVersion;
            IsWin7 = (osVersion.Platform == PlatformID.Win32NT
                   && osVersion.Version.Major == 6 && osVersion.Version.Minor == 1);
        }

        public bool IsSessionLock => GetSessionLockState(0) == LockState.Locked;

        [DllImport("wtsapi32.dll")]
        private static extern Int32 WTSQuerySessionInformation(
            IntPtr hServer,
            [MarshalAs(UnmanagedType.U4)] UInt32 SessionId,
            [MarshalAs(UnmanagedType.U4)] WTS_INFO_CLASS WTSInfoClass,
            out IntPtr ppBuffer,
            [MarshalAs(UnmanagedType.U4)] out UInt32 pBytesReturned
        );

        [DllImport("wtsapi32.dll")]
        private static extern void WTSFreeMemoryEx(
            WTS_TYPE_CLASS WTSTypeClass,
            IntPtr pMemory,
            uint numberOfEntries
        );

        private enum WTS_INFO_CLASS
        {
            WTSInitialProgram = 0,
            WTSApplicationName = 1,
            WTSWorkingDirectory = 2,
            WTSOEMId = 3,
            WTSSessionId = 4,
            WTSUserName = 5,
            WTSWinStationName = 6,
            WTSDomainName = 7,
            WTSConnectState = 8,
            WTSClientBuildNumber = 9,
            WTSClientName = 10,
            WTSClientDirectory = 11,
            WTSClientProductId = 12,
            WTSClientHardwareId = 13,
            WTSClientAddress = 14,
            WTSClientDisplay = 15,
            WTSClientProtocolType = 16,
            WTSIdleTime = 17,
            WTSLogonTime = 18,
            WTSIncomingBytes = 19,
            WTSOutgoingBytes = 20,
            WTSIncomingFrames = 21,
            WTSOutgoingFrames = 22,
            WTSClientInfo = 23,
            WTSSessionInfo = 24,
            WTSSessionInfoEx = 25,
            WTSConfigInfo = 26,
            WTSValidationInfo = 27,
            WTSSessionAddressV4 = 28,
            WTSIsRemoteSession = 29
        }

        private enum WTS_TYPE_CLASS
        {
            WTSTypeProcessInfoLevel0,
            WTSTypeProcessInfoLevel1,
            WTSTypeSessionInfoLevel1
        }

        public enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,
            WTSConnected,
            WTSConnectQuery,
            WTSShadow,
            WTSDisconnected,
            WTSIdle,
            WTSListen,
            WTSReset,
            WTSDown,
            WTSInit
        }

        public enum LockState
        {
            Unknown,
            Locked,
            Unlocked
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WTSINFOEX
        {
            public uint Level;
            public uint Reserved; /* I have observed the Data field is pushed down by 4 bytes so i have added this field as padding. */
            public WTSINFOEX_LEVEL Data;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WTSINFOEX_LEVEL
        {
            public WTSINFOEX_LEVEL1 WTSInfoExLevel1;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WTSINFOEX_LEVEL1
        {
            public uint SessionId;
            public WTS_CONNECTSTATE_CLASS SessionState;
            public int SessionFlags;

            /* I can't figure out what the rest of the struct should look like but as i don't need anything past the SessionFlags i'm not going to. */
        }

        public static LockState GetSessionLockState(uint sessionId)
        {
            IntPtr ppBuffer;
            uint pBytesReturned;

            var result = WTSQuerySessionInformation(
                    WtsCurrentServer,
                    sessionId,
                    WTS_INFO_CLASS.WTSSessionInfoEx,
                    out ppBuffer,
                    out pBytesReturned
            );

            if (result == False)
                return LockState.Unknown;

            var sessionInfoEx = Marshal.PtrToStructure<WTSINFOEX>(ppBuffer);

            if (sessionInfoEx.Level != 1)
                return LockState.Unknown;

            var lockState = sessionInfoEx.Data.WTSInfoExLevel1.SessionFlags;
            WTSFreeMemoryEx(WTS_TYPE_CLASS.WTSTypeSessionInfoLevel1, ppBuffer, pBytesReturned);

            /* Ref: https://msdn.microsoft.com/en-us/library/windows/desktop/ee621019(v=vs.85).aspx
                * Windows Server 2008 R2 and Windows 7:  Due to a code defect, the usage of the WTS_SESSIONSTATE_LOCK
                * and WTS_SESSIONSTATE_UNLOCK flags is reversed. That is, WTS_SESSIONSTATE_LOCK indicates that the
                * session is unlocked, and WTS_SESSIONSTATE_UNLOCK indicates the session is locked.
                * */
            switch (lockState)
            {
                case WtsSessionstateLock:
                    return IsWin7 ? LockState.Unlocked : LockState.Locked;
                case WtsSessionstateUnlock:
                    return IsWin7 ? LockState.Locked : LockState.Unlocked;
                default:
                    return LockState.Unknown;
            }

        }
    }
}