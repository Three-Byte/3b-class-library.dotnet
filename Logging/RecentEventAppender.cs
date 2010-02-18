using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using log4net.Appender;
using log4net.Core;


namespace ThreeByte.Logging
{
    public class RecentEventAppender : AppenderSkeleton
    {
        Queue<LoggingEvent> _eventQueue;

        public int RecentEventLimit { get; set; }

        public RecentEventAppender() {
            RecentEventLimit = 30;  //Defaults to a reasonable number of events
            _eventQueue = new Queue<LoggingEvent>(RecentEventLimit);
        }

        protected override void Append(LoggingEvent loggingEvent) {

            lock(_eventQueue) {
                _eventQueue.Enqueue(loggingEvent);
                while(_eventQueue.Count > RecentEventLimit) {
                    _eventQueue.Dequeue();  //These get abandoned
                }
            }
        }

        /// <summary>
        /// Returns the currently queued list of recent events.  This is synchronized with the Append method, so this will
        /// hold up the logging of other events.  This method is not expected to be called frequently. 
        /// </summary>
        /// <returns></returns>
        public List<LoggingEvent> GetRecentEvents() {
            lock(_eventQueue) {
                return _eventQueue.ToList();  //Must lock during implicit enumeration
            }
        }

        public string GetRecentEventString() {
            List<LoggingEvent> recentEvents = GetRecentEvents();
            string logString = string.Empty;
            foreach(LoggingEvent loggedEvent in recentEvents) {
                logString += string.Format("{0}\n", loggedEvent.RenderedMessage);
            }
            return logString;
        }

    }
}
