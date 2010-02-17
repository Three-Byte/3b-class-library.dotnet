using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using log4net;

namespace ThreeByte.Network
{
    public delegate void FrameBasedEvent();

    public class FrameBasedEventManager : DependencyObject
    {
        private readonly ILog log = LogManager.GetLogger(typeof(FrameBasedEventManager));

        public FrameBasedEventManager() {
            SceneFrameCount = 0;
            _frameEvents = new Dictionary<int, FrameBasedEvent>();
        }


        public FrameBasedEventManager(int sceneFrameCount)
            : this() {
            SceneFrameCount = sceneFrameCount;
        }



        public int SceneFrameCount { get; private set; }


        private Dictionary<int, FrameBasedEvent> _frameEvents;

        public void AddEvent(int frameNumber, FrameBasedEvent frameEvent) {
            _frameEvents[frameNumber] = frameEvent;
        }

        public static readonly DependencyProperty FrameNumberProperty =
           DependencyProperty.Register("FrameNumber", typeof(int), typeof(FrameBasedEventManager), new UIPropertyMetadata(0, FrameNumberChanged));


        public int FrameNumber {
            get {
                return (int)GetValue(FrameNumberProperty);
            }
            set {
                SetValue(FrameNumberProperty, value);
            }
        }

        private static void FrameNumberChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            FrameBasedEventManager manager = (FrameBasedEventManager)obj;
            //manager.log.Debug("Frame Number updated: " + manager.FrameNumber);
            manager.Update();

        }

        private int _sceneFrameNumber;

        private void Update() {
            //log.Debug("Update");

            if(SceneFrameCount > 0) {
                _sceneFrameNumber = FrameNumber % SceneFrameCount;
            } else {
                _sceneFrameNumber = FrameNumber;
            }

            if(_frameEvents.ContainsKey(_sceneFrameNumber)) {
                FrameBasedEvent frameEvent = _frameEvents[_sceneFrameNumber];
                log.Info("Invoking: " + frameEvent);
                Dispatcher.BeginInvoke(frameEvent);
            }
        }

    }
}
