using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using ThreeByte.Network;
using System.ComponentModel;
using log4net;

namespace ThreeByte.Network.Devices
{
    public class WatchoutController : INotifyPropertyChanged
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(WatchoutController));

        public string Host { get; private set; }
        private static readonly int PORT = 3040;
        private FramedNetworkLink _netLink;
        private bool[] CurrentConditionalLayers = new bool[20];
        private HashSet<HashSet<int>> MutualExclusionSets;

        public bool IsConnected {
            get { return _netLink.IsConnected; }
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public WatchoutController(string host) {
            MutualExclusionSets = new HashSet<HashSet<int>>();
            MutualExclusionSets.Add(new HashSet<int>(new int[] { 1, 2, 3, 4 }));
            MutualExclusionSets.Add(new HashSet<int>(new int[] { 5, 6, 7, 8 }));
            MutualExclusionSets.Add(new HashSet<int>(new int[] { 9, 10, 11, 12 }));

            Host = host;
            _netLink = new FramedNetworkLink(host, PORT);
            _netLink.SendFrame = new NetworkFrame() { Footer = new byte[] { 0x0D, 0x0A } };
            _netLink.ReceiveFrame = _netLink.SendFrame;
            _netLink.DataReceived += new EventHandler(_netLink_DataReceived);
            _netLink.PropertyChanged += new PropertyChangedEventHandler(_netLink_PropertyChanged);
        }

        void _netLink_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == "IsConnected") {
                NotifyPropertyChanged("IsConnected");
            } else if(e.PropertyName == "Enabled") {
                NotifyPropertyChanged("Enabled");
            }
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

            log.DebugFormat("The message is [{0}]", message);
            if(string.IsNullOrEmpty(message)) {
                return;
            }
            LastMessage = message.Replace("\n", string.Empty).Replace("\r", string.Empty);

        }

        public void Play(string timeline = null) {
            string message = string.Format("run{0}", timeline == null ? string.Empty : " " + timeline);
            _netLink.SendMessage(message);
        }

        public void Stop(string timeline = null) {
            string message = string.Format("halt{0}", timeline == null ? string.Empty : " " + timeline);
            _netLink.SendMessage(message);
        }

        public void Kill(string timeline) {
            string message = string.Format("kill {0}", timeline);
            _netLink.SendMessage(message);
        }

        public void Standby(bool standby = true, int fadeTime = 1000) {
            string message = string.Format("standBy {0} {1}", (standby? "true" : "false"), fadeTime);
            _netLink.SendMessage(message);
        }

        public void Load(string show) {
            string message = string.Format("load {0}", show);
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
}
