using System;
using System.Runtime.InteropServices;

namespace Employee_Monitoring_System.Services
{
    public static class IdleTimeDetector
    {
        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        public static TimeSpan GetIdleTime()
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);

            if (!GetLastInputInfo(ref lastInputInfo))
                return TimeSpan.Zero;

            uint idleTime = ((uint)Environment.TickCount - lastInputInfo.dwTime);
            return TimeSpan.FromMilliseconds(idleTime);
        }
    }
}
