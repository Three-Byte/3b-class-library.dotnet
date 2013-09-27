using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeByte.Schedule
{
    public interface IScheduledEvent
    {
        TimeSpan StartTime { get; }
        void RunEvent();
    }
}
