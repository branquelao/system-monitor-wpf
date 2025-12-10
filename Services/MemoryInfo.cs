using System;
using System.Management;

namespace SystemMonitorWpf.Services
{
    public static class MemoryInfo
    {
        public static (ulong Total, ulong Used, double Percent) GetRam()
        {
            var searcher = new ManagementObjectSearcher(
                "SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");

            foreach (ManagementObject obj in searcher.Get())
            {
                ulong total = (ulong)obj["TotalVisibleMemorySize"]; // KB
                ulong free = (ulong)obj["FreePhysicalMemory"];      // KB

                ulong used = total - free;
                double percent = (double)used / total * 100.0;

                // convertemos de KB para bytes (x1024)
                return (total * 1024, used * 1024, percent);
            }

            return (0, 0, 0);
        }
    }
}
