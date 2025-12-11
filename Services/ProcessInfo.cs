using System;
using System.Collections.Generic;
using System.Text;

namespace SystemMonitorWPF.Services
{
    public class ProcessInfo
    {
        public int PID { get; set; }
        public string Name { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
    }
}
