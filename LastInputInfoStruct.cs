using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
struct LASTINPUTINFO
{
    public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));

    [MarshalAs(UnmanagedType.U4)]
    public UInt32 cbSize;
    [MarshalAs(UnmanagedType.U4)]
    public UInt32 dwTime;
}