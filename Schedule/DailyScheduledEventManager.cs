using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ThreeByte.Schedule
{
    public class DailyScheduledEventManager
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DailyScheduledEventManager));

        public DailyScheduledEventManager() {
            _pollTimer = new Timer(TimerCheck);

            _pollTimer.Change(POLL_INTERVAL, NEVER);
        }

        private Timer _pollTimer;
        private static readonly TimeSpan POLL_INTERVAL = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan ONE_MINUTE = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan NEVER = TimeSpan.FromMilliseconds(-1);

        private List<IScheduledEvent> Events = new List<IScheduledEvent>();
        private Dictionary<IScheduledEvent, DateTime> EventRunTimestamps = new Dictionary<IScheduledEvent, DateTime>();

        public void RegisterEvent(IScheduledEvent scheduledEvent) {
            lock(Events) {
                Events.Add(scheduledEvent);
            }
        }

        private void TimerCheck(object state) {
            try {
                TimerCheckCore();
            } catch(Exception ex) {
                log.Error("TimerCheck failed", ex);
            }
            _pollTimer.Change(POLL_INTERVAL, NEVER);
        }

        private void TimerCheckCore() {
            DateTime now = DateTime.Now;
            DayOfWeek today = now.DayOfWeek;

            int nowMinutes = (int)now.TimeOfDay.TotalMinutes;

            List<IScheduledEvent> eventsToRun = new List<IScheduledEvent>();

            lock(Events) {
                foreach(IScheduledEvent e in Events) {
                    int eventMinutes = (int)e.StartTime.TotalMinutes;
                    DateTime lastRunTime = DateTime.MinValue;
                    if(EventRunTimestamps.ContainsKey(e)) {
                        lastRunTime = EventRunTimestamps[e];
                    }
                    if(nowMinutes == eventMinutes && (now - lastRunTime) > ONE_MINUTE) {
                        //Make sure the event was not triggered within the past minute
                        eventsToRun.Add(e);
                    }
                }

                foreach(IScheduledEvent e in eventsToRun) {
                    log.InfoFormat("Event triggered: {0}", now);
                    try {
                        e.RunEvent();
                    } catch(Exception ex) {
                        log.Error("Error running event", ex);
                    }
                    EventRunTimestamps[e] = now;
                }
            }

        }
    }
}
