using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SystemMonitorWpf.Services
{
    public class ProcessCpuTracker
    {
        private readonly Dictionary<int, TimeSpan> _lastCpuTimes = new();
        private readonly int _coreCount;

        public ProcessCpuTracker()
        {
            _coreCount = Environment.ProcessorCount;
        }

        public double GetCpu(Process p)
        {
            try
            {
                TimeSpan current = p.TotalProcessorTime;

                if (!_lastCpuTimes.ContainsKey(p.Id))
                {
                    _lastCpuTimes[p.Id] = current;
                    return 0;
                }

                TimeSpan last = _lastCpuTimes[p.Id];
                TimeSpan delta = current - last;

                _lastCpuTimes[p.Id] = current;

                double cpuPercent = (delta.TotalMilliseconds / 1000.0)
                                  / _coreCount * 100.0;

                return Math.Round(cpuPercent, 2);
            }
            catch
            {
                return 0;
            }
        }
    }
}
