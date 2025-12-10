using System;
using System.Management;

namespace SystemMonitorWpf.Services
{
    public static class CpuInfo
    {
        private static ulong lastIdleTime = 0;
        private static ulong lastTotalTime = 0;
        private static bool firstRun = true;

        public static double GetCpuUsage()
        {
            var searcher = new ManagementObjectSearcher(
                "SELECT * FROM Win32_PerfRawData_PerfOS_Processor WHERE Name=\"_Total\"");

            foreach (ManagementObject obj in searcher.Get())
            {
                ulong idle = (ulong)obj["PercentIdleTime"];
                ulong total = (ulong)obj["Timestamp_Sys100NS"];

                if (firstRun)
                {
                    lastIdleTime = idle;
                    lastTotalTime = total;
                    firstRun = false;
                    return 0;
                }

                ulong idleDelta = idle - lastIdleTime;
                ulong totalDelta = total - lastTotalTime;

                lastIdleTime = idle;
                lastTotalTime = total;

                double usage = 100.0 - ((double)idleDelta / totalDelta * 100.0);
                return Math.Round(usage, 2);
            }

            return 0;
        }
    }
}
