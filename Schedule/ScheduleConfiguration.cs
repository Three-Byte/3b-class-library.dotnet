using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using System.IO;
using log4net;

namespace ThreeByte.Schedule
{
    public class ScheduleConfiguration : DependencyObject
    {
        private readonly ILog log = LogManager.GetLogger(typeof(ScheduleConfiguration));

        #region Dependency Properties
        public static readonly DependencyProperty StartupTimeProperty =
            DependencyProperty.Register("StartupTime", typeof(TimeSpan), typeof(ScheduleConfiguration), new UIPropertyMetadata(new TimeSpan(0)));

        public TimeSpan StartupTime {
            get { return (TimeSpan)GetValue(StartupTimeProperty); }
            set { SetValue(StartupTimeProperty, value); }
        }

        public static readonly DependencyProperty ShutdownTimeProperty =
            DependencyProperty.Register("ShutdownTime", typeof(TimeSpan), typeof(ScheduleConfiguration), new UIPropertyMetadata(new TimeSpan(0)));

        public TimeSpan ShutdownTime {
            get { return (TimeSpan)GetValue(ShutdownTimeProperty); }
            set { SetValue(ShutdownTimeProperty, value); }
        }

            #region Weekday Properties
        public static readonly DependencyProperty SundayEnabledProperty =
           DependencyProperty.Register("SundayEnabled", typeof(bool), typeof(ScheduleConfiguration), new UIPropertyMetadata(true));

        public bool SundayEnabled {
            get { return (bool)GetValue(SundayEnabledProperty); }
            set { SetValue(SundayEnabledProperty, value); }
        }

        public static readonly DependencyProperty MondayEnabledProperty =
           DependencyProperty.Register("MondayEnabled", typeof(bool), typeof(ScheduleConfiguration), new UIPropertyMetadata(true));

        public bool MondayEnabled {
            get { return (bool)GetValue(MondayEnabledProperty); }
            set { SetValue(MondayEnabledProperty, value); }
        }

        public static readonly DependencyProperty TuesdayEnabledProperty =
           DependencyProperty.Register("TuesdayEnabled", typeof(bool), typeof(ScheduleConfiguration), new UIPropertyMetadata(true));

        public bool TuesdayEnabled {
            get { return (bool)GetValue(TuesdayEnabledProperty); }
            set { SetValue(TuesdayEnabledProperty, value); }
        }

        public static readonly DependencyProperty WednesdayEnabledProperty =
           DependencyProperty.Register("WednesdayEnabled", typeof(bool), typeof(ScheduleConfiguration), new UIPropertyMetadata(true));

        public bool WednesdayEnabled {
            get { return (bool)GetValue(WednesdayEnabledProperty); }
            set { SetValue(WednesdayEnabledProperty, value); }
        }

        public static readonly DependencyProperty ThursdayEnabledProperty =
                   DependencyProperty.Register("ThursdayEnabled", typeof(bool), typeof(ScheduleConfiguration), new UIPropertyMetadata(true));

        public bool ThursdayEnabled {
            get { return (bool)GetValue(ThursdayEnabledProperty); }
            set { SetValue(ThursdayEnabledProperty, value); }
        }
        
        public static readonly DependencyProperty FridayEnabledProperty =
                   DependencyProperty.Register("FridayEnabled", typeof(bool), typeof(ScheduleConfiguration), new UIPropertyMetadata(true));

        public bool FridayEnabled {
            get { return (bool)GetValue(FridayEnabledProperty); }
            set { SetValue(FridayEnabledProperty, value); }
        }
        
        public static readonly DependencyProperty SaturdayEnabledProperty =
                   DependencyProperty.Register("SaturdayEnabled", typeof(bool), typeof(ScheduleConfiguration), new UIPropertyMetadata(true));

        public bool SaturdayEnabled {
            get { return (bool)GetValue(SaturdayEnabledProperty); }
            set { SetValue(SaturdayEnabledProperty, value); }
        }
            #endregion //Weekday Properties

            #region Blackout Dates
        public static readonly DependencyProperty BlackoutEnabledProperty =
                   DependencyProperty.Register("BlackoutEnabled", typeof(bool), typeof(ScheduleConfiguration), new UIPropertyMetadata(false));

        public bool BlackoutEnabled {
            get { return (bool)GetValue(BlackoutEnabledProperty); }
            set { SetValue(BlackoutEnabledProperty, value); }
        }

        public static readonly DependencyProperty BlackoutStartDateProperty =
           DependencyProperty.Register("BlackoutStartDate", typeof(DateTime), typeof(ScheduleConfiguration), new UIPropertyMetadata(new DateTime(0)));

        public DateTime BlackoutStartDate {
            get { return (DateTime)GetValue(BlackoutStartDateProperty); }
            set { SetValue(BlackoutStartDateProperty, value); }
        }

        public static readonly DependencyProperty BlackoutEndDateProperty =
           DependencyProperty.Register("BlackoutEndDate", typeof(DateTime), typeof(ScheduleConfiguration), new UIPropertyMetadata(new DateTime(0)));

        public DateTime BlackoutEndDate {
            get { return (DateTime)GetValue(BlackoutEndDateProperty); }
            set { SetValue(BlackoutEndDateProperty, value); }
        }
            #endregion //Blackout Dates

        #endregion //Dependency Properties


        public string SchedulePath { get; set; }

        /// <summary>
        /// Commits the current state of this schedule configuration to the Database
        /// </summary>
        public void Save() {

            XElement scheduleNode = new XElement("Schedule");
            scheduleNode.Add(new XElement("StartupTime", StartupTime));
            scheduleNode.Add(new XElement("ShutdownTime", ShutdownTime));

            scheduleNode.Add(new XElement("SundayEnabled", SundayEnabled));
            scheduleNode.Add(new XElement("MondayEnabled", MondayEnabled));
            scheduleNode.Add(new XElement("TuesdayEnabled", TuesdayEnabled));
            scheduleNode.Add(new XElement("WednesdayEnabled", WednesdayEnabled));
            scheduleNode.Add(new XElement("ThursdayEnabled", ThursdayEnabled));
            scheduleNode.Add(new XElement("FridayEnabled", FridayEnabled));
            scheduleNode.Add(new XElement("SaturdayEnabled", SaturdayEnabled));

            scheduleNode.Add(new XElement("BlackoutEnabled", BlackoutEnabled));
            scheduleNode.Add(new XElement("BlackoutStartDate", BlackoutStartDate));
            scheduleNode.Add(new XElement("BlackoutEndDate", BlackoutEndDate));

            try {
                File.WriteAllText(SchedulePath, scheduleNode.ToString());
            } catch(Exception ex) {
                log.Error("Cannot write schedule file", ex);
            }

            //DataUtil.SetConfigValue("StartupTime", StartupTime.ToString());
            //DataUtil.SetConfigValue("ShutdownTime", ShutdownTime.ToString());

            //DataUtil.SetConfigValue("SundayEnabled", SundayEnabled.ToString());
            //DataUtil.SetConfigValue("MondayEnabled", MondayEnabled.ToString());
            //DataUtil.SetConfigValue("TuesdayEnabled", TuesdayEnabled.ToString());
            //DataUtil.SetConfigValue("WednesdayEnabled", WednesdayEnabled.ToString());
            //DataUtil.SetConfigValue("ThursdayEnabled", ThursdayEnabled.ToString());
            //DataUtil.SetConfigValue("FridayEnabled", FridayEnabled.ToString());
            //DataUtil.SetConfigValue("SaturdayEnabled", SaturdayEnabled.ToString());

            //DataUtil.SetConfigValue("BlackoutEnabled", BlackoutEnabled.ToString());
            //DataUtil.SetConfigValue("BlackoutStartDate", BlackoutStartDate.ToString());
            //DataUtil.SetConfigValue("BlackoutEndDate", BlackoutEndDate.ToString());
        }


        //Reloads the state of the schedule configuration from the database
        public void Reload() {

            try {
                XElement scheduleNode = XElement.Parse(File.ReadAllText(SchedulePath));

                StartupTime = TimeSpan.Parse(scheduleNode.Element("StartupTime").Value);
                ShutdownTime = TimeSpan.Parse(scheduleNode.Element("ShutdownTime").Value);

                SundayEnabled = bool.Parse(scheduleNode.Element("SundayEnabled").Value);
                MondayEnabled = bool.Parse(scheduleNode.Element("MondayEnabled").Value);
                TuesdayEnabled = bool.Parse(scheduleNode.Element("TuesdayEnabled").Value);
                WednesdayEnabled = bool.Parse(scheduleNode.Element("WednesdayEnabled").Value);
                ThursdayEnabled = bool.Parse(scheduleNode.Element("ThursdayEnabled").Value);
                FridayEnabled = bool.Parse(scheduleNode.Element("FridayEnabled").Value);
                SaturdayEnabled = bool.Parse(scheduleNode.Element("SaturdayEnabled").Value);

                BlackoutEnabled = bool.Parse(scheduleNode.Element("BlackoutEnabled").Value);
                BlackoutStartDate = DateTime.Parse(scheduleNode.Element("BlackoutStartDate").Value);
                BlackoutEndDate = DateTime.Parse(scheduleNode.Element("BlackoutEndDate").Value);
                
                
                //StartupTime = TimeSpan.Parse(DataUtil.GetConfigValue("StartupTime"));
                //ShutdownTime = TimeSpan.Parse(DataUtil.GetConfigValue("ShutdownTime"));

                //SundayEnabled = bool.Parse(DataUtil.GetConfigValue("SundayEnabled"));
                //MondayEnabled = bool.Parse(DataUtil.GetConfigValue("MondayEnabled"));
                //TuesdayEnabled = bool.Parse(DataUtil.GetConfigValue("TuesdayEnabled"));
                //WednesdayEnabled = bool.Parse(DataUtil.GetConfigValue("WednesdayEnabled"));
                //ThursdayEnabled = bool.Parse(DataUtil.GetConfigValue("ThursdayEnabled"));
                //FridayEnabled = bool.Parse(DataUtil.GetConfigValue("FridayEnabled"));
                //SaturdayEnabled = bool.Parse(DataUtil.GetConfigValue("SaturdayEnabled"));

                //BlackoutEnabled = bool.Parse(DataUtil.GetConfigValue("BlackoutEnabled"));
                //BlackoutStartDate = DateTime.Parse(DataUtil.GetConfigValue("BlackoutStartDate"));
                //BlackoutEndDate = DateTime.Parse(DataUtil.GetConfigValue("BlackoutEndDate"));
            } catch(Exception ex) {
                log.Error("Schedule information is corrupt", ex);
            }
        }


    }
}
