using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using log4net;

namespace ThreeByte.Schedule
{
    public class ScheduleService : IDisposable
    {
        private readonly TimeSpan ONE_MINUTE = TimeSpan.FromMinutes(1);
        private readonly TimeSpan THIRTY_SECONDS = TimeSpan.FromSeconds(30); 

        public ScheduleService(Dispatcher dispatcher) {
            Configuration = new ScheduleConfiguration();
            Configuration.Reload();
            _mostRecentStartup = DateTime.MinValue;
            _mostRecentShutdown = DateTime.MinValue;

            //_timer = new Timer(SchedulePoll, null, THIRTY_SECONDS, THIRTY_SECONDS);
            _timer = new DispatcherTimer(THIRTY_SECONDS, DispatcherPriority.Normal, _timer_Tick, dispatcher);
            _timer.Start();
        }

        public ScheduleService(Dispatcher dispatcher, ScheduleConfiguration configuration) {
            Configuration = configuration;
            Configuration.Reload();
            _mostRecentStartup = DateTime.MinValue;
            _mostRecentShutdown = DateTime.MinValue;

            //_timer = new Timer(SchedulePoll, null, THIRTY_SECONDS, THIRTY_SECONDS);
            _timer = new DispatcherTimer(THIRTY_SECONDS, DispatcherPriority.Normal, _timer_Tick, dispatcher);
            _timer.Start();
        }

       
        public ScheduleConfiguration Configuration {
            get;
            private set;
        }

        public event EventHandler Startup;
        public event EventHandler Shutdown;

        private  DispatcherTimer _timer;
        private DateTime _mostRecentStartup;
        private DateTime _mostRecentShutdown;
        private readonly ILog log = LogManager.GetLogger(typeof(ScheduleService));

        void _timer_Tick(object sender, EventArgs e) {
            SchedulePoll();
        }

        private void SchedulePoll() {
            DateTime now = DateTime.Now;
            //Reload the schedule from the database
            Configuration.Reload();

            //Check if the current time matches the Startup or Shutdown time
            int startupMinutes = (int)Configuration.StartupTime.TotalMinutes;
            int shutdownMinutes = (int)Configuration.ShutdownTime.TotalMinutes;
            int nowMinutes = (int)now.TimeOfDay.TotalMinutes;


            //**** STARTUP ****
            if(nowMinutes == startupMinutes
                && (now - _mostRecentStartup) > ONE_MINUTE ){
                //Make sure the event was not triggered within the past minute

                if(IsEnabledOnDay(now) && Startup != null) {
                    Startup(this, EventArgs.Empty);
                    _mostRecentStartup = now;
                }
            }

            //**** SHUTDOWN ****
            if(nowMinutes == shutdownMinutes
                && (now - _mostRecentShutdown) > ONE_MINUTE) {
                //Make sure the event was not triggered within the past minute

                if(IsEnabledOnDay(now) && Shutdown != null) {
                    Shutdown(this, EventArgs.Empty);
                    _mostRecentShutdown = now;
                }
            }

        }

        private bool IsEnabledOnDay(DateTime date) {
            //Check to see if we are in a blackout period
            if(Configuration.BlackoutEnabled) {
                if(date.Date >= Configuration.BlackoutStartDate.Date && date.Date <= Configuration.BlackoutEndDate.Date) {
                    log.Info("Bypassing schedule during blackout period");
                    return false;
                }
            }

            //Check if the schedule is enabled for today
            switch(date.DayOfWeek) {
                case DayOfWeek.Sunday: {
                        if(!Configuration.SundayEnabled) {
                            log.Info("Schedule is disabled on " + date.DayOfWeek);
                            return false;
                        }
                        break;
                    }
                case DayOfWeek.Monday: {
                        if(!Configuration.MondayEnabled) {
                            log.Info("Schedule is disabled on " + date.DayOfWeek);
                            return false;
                        }
                        break;
                    }
                case DayOfWeek.Tuesday: {
                        if(!Configuration.TuesdayEnabled) {
                            log.Info("Schedule is disabled on " + date.DayOfWeek);
                            return false;
                        }
                        break;
                    }
                case DayOfWeek.Wednesday: {
                        if(!Configuration.WednesdayEnabled) {
                            log.Info("Schedule is disabled on " + date.DayOfWeek);
                            return false;
                        }
                        break;
                    }
                case DayOfWeek.Thursday: {
                        if(!Configuration.ThursdayEnabled) {
                            log.Info("Schedule is disabled on " + date.DayOfWeek);
                            return false;
                        }
                        break;
                    }
                case DayOfWeek.Friday: {
                        if(!Configuration.FridayEnabled) {
                            log.Info("Schedule is disabled on " + date.DayOfWeek);
                            return false;
                        }
                        break;
                    }
                case DayOfWeek.Saturday: {
                        if(!Configuration.SaturdayEnabled) {
                            log.Info("Schedule is disabled on " + date.DayOfWeek);
                            return false;
                        }
                        break;
                    }
            }

            return true;
        }

        public void Dispose() {
            _timer.Stop();
        }
    }
}
