using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using ThreeByte.Network;
using System.ComponentModel;
using log4net;
using System.Threading;
using System.Text.RegularExpressions;

namespace ThreeByte.Network.Devices
{
    public class WatchoutDisplay : INotifyPropertyChanged
    {

        #region Implements INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private static readonly ILog log = LogManager.GetLogger(typeof(WatchoutDisplay));

        public string Host { get; private set; }
        private static readonly int PORT = 3039;
        private FramedNetworkLink _netLink;
        
        private bool[] CurrentConditionalLayers = new bool[20];
        private HashSet<HashSet<int>> MutualExclusionSets;

        private static readonly TimeSpan NEVER = TimeSpan.FromMilliseconds(-1);
        private static readonly TimeSpan PING_INTERVAL = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan PING_TIMEOUT = TimeSpan.FromSeconds(11);

        private static readonly TimeSpan STATUS_INTERVAL = TimeSpan.FromSeconds(1);

        private Timer _pingTimer;
        private Timer _statusTimer;
        private DateTime _lastAckTimestamp = DateTime.Now;

        public event EventHandler<ControlCueEventArgs> ControlCue;

        private void RaiseControlCue(string info) {
            if(ControlCue != null) {
                try {
                    ControlCue(this, new ControlCueEventArgs(info));
                } catch(Exception ex) {
                    //Don't let consumers bring you down
                    log.Error("Error executing control cue", ex);
                }
            }
        }

        #region Public Properties
        private bool _connected;
        public bool Connected {
            get {
                return _connected;
            }
            set {
                if(_connected != value) {
                    _connected = value;
                    NotifyPropertyChanged("Connected");
                    if(_connected) {
                        Authenticate();
                    }
                }
            }
        }

        public bool Enabled {
            get {
                return _netLink.Enabled;
            }
            set {
                _netLink.Enabled = value;
            }
        }

        private string _lastMessage;
        public string LastMessage {
            get {
                return _lastMessage;
            }
            private set {
                _lastMessage = value;
                NotifyPropertyChanged("LastMessage");
            }
        }

        private string _lastError;
        public string LastError {
            get {
                return _lastError;
            }
            private set {
                _lastError = value;
                NotifyPropertyChanged("LastError");
            }
        }

        private string _currentShow;
        public string CurrentShow {
            get {
                return _currentShow;
            }
            private set {
                _currentShow = value;
                NotifyPropertyChanged("CurrentShow");
            }
        }

        private TimeSpan _timecode;
        public TimeSpan TimeCode {
            get {
                return _timecode;
            }
            private set {
                _timecode = value;
                NotifyPropertyChanged("TimeCode");
            }
        }

        private bool _playing;
        public bool Playing {
            get {
                return _playing;
            }
            private set {
                _playing = value;
                NotifyPropertyChanged("Playing");
            }
        }

        private bool _standby;
        public bool StandBy {
            get {
                return _standby;
            }
            private set {
                _standby = value;
                NotifyPropertyChanged("StandBy");
            }
        }

        private bool _showActive;
        public bool ShowActive {
            get {
                return _showActive;
            }
            private set {
                _showActive = value;
                NotifyPropertyChanged("ShowActive");
            }
        }

        private int _clusterHealth;
        public int ClusterHealth {
            get {
                return _clusterHealth;
            }
            private set {
                _clusterHealth = value;
                NotifyPropertyChanged("ClusterHealth");
            }
        }

        private bool _programmerOnline;
        public bool ProgrammerOnline {
            get {
                return _programmerOnline;
            }
            private set {
                _programmerOnline = value;
                NotifyPropertyChanged("ProgrammerOnline");
            }
        }

        private bool _busy;
        public bool Busy {
            get {
                return _busy;
            }
            private set {
                _busy = value;
                NotifyPropertyChanged("Busy");
            }
        }

        private double _timelineRate;
        public double TimelineRate {
            get {
                return _timelineRate;
            }
            private set {
                _timelineRate = value;
                NotifyPropertyChanged("TimelineRate");
            }
        }

        #endregion Public Properties

        public WatchoutDisplay(string host) {
            log.InfoFormat("Initialize Watchout Display: {0}", host);
            MutualExclusionSets = new HashSet<HashSet<int>>();
            MutualExclusionSets.Add(new HashSet<int>(new int[] { 1, 2, 3, 4 }));
            MutualExclusionSets.Add(new HashSet<int>(new int[] { 5, 6, 7, 8 }));
            MutualExclusionSets.Add(new HashSet<int>(new int[] { 9, 10, 11, 12 }));

            Host = host;
            _netLink = new FramedNetworkLink(host, PORT);
            _netLink.SendFrame = new NetworkFrame() { Footer = new byte[] { 0x0D, 0x0A } };
            _netLink.ReceiveFrame = _netLink.SendFrame;
            _netLink.DataReceived += new EventHandler(_netLink_DataReceived);

            _pingTimer = new Timer(PingTimerCallback);
            _pingTimer.Change(PING_INTERVAL, NEVER);

            _statusTimer = new Timer(StatusTimerCallback);
            _statusTimer.Change(STATUS_INTERVAL, NEVER);
        }

        void PingTimerCallback(object state) {
            Ping();
            if(DateTime.Now > _lastAckTimestamp + PING_TIMEOUT) {
                Connected = false;
            }
            _pingTimer.Change(PING_INTERVAL, NEVER);
        }

        void StatusTimerCallback(object state) {
            GetStatus();
            _statusTimer.Change(STATUS_INTERVAL, NEVER);
        }

        void _netLink_DataReceived(object sender, EventArgs e) {

            while(_netLink.HasData) {
                string currentMessage = _netLink.GetMessage();

                if(!string.IsNullOrWhiteSpace(currentMessage)) {
                    ParseMessage(currentMessage);
                }
            }

        }

        private void ParseMessage(string message) {

            log.DebugFormat("<{0}> [{1}]", Host, message);
            if(string.IsNullOrEmpty(message)) {
                return;
            }
            _lastAckTimestamp = DateTime.Now;
            Connected = true;
            LastMessage = message.Replace("\n", string.Empty).Replace("\r", string.Empty);

            Regex errorPattern = new Regex(@"Error (\d+) (\d+) ""(.*)""");
            if(errorPattern.IsMatch(message)) {
                LastError = message;
                Match m = errorPattern.Match(message);
                if(m.Groups[3].Value.StartsWith("Not authorized")) {
                    Authenticate();
                    return;
                }
            }

            Regex replyPattern = new Regex(@"Reply ""([^""]*)"" (\w+) (\d+) (\w+) (\w+) (\w+) (\d+) (\w+) ([\d\.]+) (\w+)");
            if(replyPattern.IsMatch(message)) {
                //Parse the staus reply

                Match m = replyPattern.Match(message);
                if(m.Success) {
                    //Get the show name and timecode
                    CurrentShow = m.Groups[1].Value;
                    Busy = bool.Parse(m.Groups[2].Value);
                    ClusterHealth = int.Parse(m.Groups[3].Value);
                    ShowActive = bool.Parse(m.Groups[5].Value);
                    ProgrammerOnline = bool.Parse(m.Groups[6].Value);
                    TimeCode = TimeSpan.FromMilliseconds(int.Parse(m.Groups[7].Value));
                    Playing = bool.Parse(m.Groups[8].Value);
                    TimelineRate = double.Parse(m.Groups[9].Value);
                    StandBy = bool.Parse(m.Groups[10].Value);
                }
            }

            Regex controlCuePattern = new Regex(@"Information "":([\w-\s]+)""");
            if(controlCuePattern.IsMatch(message)) {
                Match m = controlCuePattern.Match(message);
                if(m.Success) {
                    //Raise Control Cue
                    RaiseControlCue(m.Groups[1].Value);
                } else {
                    log.InfoFormat("Control cue not recognized: {0}", message);
                }

            }
        }

        public void Authenticate() {
            _netLink.SendMessage("authenticate 1");
        }

        public void Ping() {
            _netLink.SendMessage("ping");
        }

        public void GetStatus() {
            _netLink.SendMessage("getStatus");
        }

        // Just send a raw message through to Watchout.
        public void SendRaw(string message) {
            _netLink.SendMessage(message);
        }

        public void Play(string timeline = null) {
            string message = "run";
            if(timeline != null) {
                message += string.Format(" \"{0}\"", timeline);
            }
            _netLink.SendMessage(message);
        }

        public void Stop(string timeline = null) {
            string message = "halt";
            if(timeline != null) {
                message += string.Format(" \"{0}\"", timeline);
            }
            _netLink.SendMessage(message);
        }

        public void Kill(string timeline) {
            string message = string.Format("kill \"{0}\"", timeline);
            _netLink.SendMessage(message);
        }

        public void Reset() {
            string message = "reset";
            _netLink.SendMessage(message);
        }

        public void Standby(bool standby = true, int fadeTime = 1000) {
            string message = string.Format("standBy {0} {1}", (standby? "true" : "false"), fadeTime);
            _netLink.SendMessage(message);
        }

        public void Load(string show) {
            string message = string.Format("load \"{0}\"", show);
            _netLink.SendMessage(message);
        }

        public void Update() {
            string message = string.Format("update");
            _netLink.SendMessage(message);
        }

        public void GoTo(int time, string timeline = null) {
            string message = string.Format("gotoTime {0}{1}", time, (timeline == null ? string.Empty : " " + timeline));
            _netLink.SendMessage(message);
        }

        public void SetInput(string name, float value) {
            string message = string.Format("setInput \"{0}\" {1}", name, value);
            _netLink.SendMessage(message);
        }

        public void EnableLayer(int layer) {
            //Check to see if this layer is in an exclusion sets
            foreach(HashSet<int> set in MutualExclusionSets) {
                if(set.Contains(layer)) {
                    //Set all other layers in this set as false
                    foreach(int exLayer in set) {
                        CurrentConditionalLayers[exLayer] = false;
                    }
                }
            }
            //Set the requested layer as true
            CurrentConditionalLayers[layer] = true;

            UpdateEnabledLayers();
        }

        private void UpdateEnabledLayers() {
            //Do the conversion here
            int layerValue = 0;
            for(int i = 0; i < CurrentConditionalLayers.Length; i++) {
                if(CurrentConditionalLayers[i]) {
                    layerValue += ((int)Math.Pow(2, i - 1));
                }
            }
            string message = string.Format("enableLayerCond {0}", layerValue);
            _netLink.SendMessage(message);
        }

        public void EnableExclusiveLayers(int[] layers) {
            for(int i = 0; i < CurrentConditionalLayers.Length; i++) {
                CurrentConditionalLayers[i] = false;
            }
            foreach(int i in layers) {
                CurrentConditionalLayers[i] = true;
            }
            UpdateEnabledLayers();
        }
    }

    public class ControlCueEventArgs : EventArgs{
        public string Information { get; private set; }

        public ControlCueEventArgs(string info){
            Information = info;
        }
    }
}
