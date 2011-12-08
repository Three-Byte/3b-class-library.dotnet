using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Timers;
using log4net;

namespace ThreeByte.DMX
{
    public class LightingChannels: INotifyPropertyChanged
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LightingChannels));

        private readonly DMXControl _dmxController;
        private readonly Timer _autosaveTimer;

        /// <summary>
        /// Maps LightingChannel numbers (Channels) to DMX data channels (Dimmers)
        /// As Specified in Feature #4703
        /// </summary>
        private static readonly Dictionary<int, int> CHANNEL_DIMMER_MAPPING;

        static LightingChannels() {
            CHANNEL_DIMMER_MAPPING = new Dictionary<int, int>();
            CHANNEL_DIMMER_MAPPING[0] = 1;
            CHANNEL_DIMMER_MAPPING[1] = 2;
            CHANNEL_DIMMER_MAPPING[2] = 3;
            CHANNEL_DIMMER_MAPPING[3] = 4;
            CHANNEL_DIMMER_MAPPING[4] = 7;
            CHANNEL_DIMMER_MAPPING[5] = 8;
            CHANNEL_DIMMER_MAPPING[6] = 9;
            CHANNEL_DIMMER_MAPPING[7] = 10;
            CHANNEL_DIMMER_MAPPING[8] = 13;
            CHANNEL_DIMMER_MAPPING[9] = 14;
            CHANNEL_DIMMER_MAPPING[10] = 15;
            CHANNEL_DIMMER_MAPPING[11] = 16;
            CHANNEL_DIMMER_MAPPING[12] = 19;
            CHANNEL_DIMMER_MAPPING[13] = 20;
            CHANNEL_DIMMER_MAPPING[14] = 21;
            CHANNEL_DIMMER_MAPPING[15] = 22;
        }

        private readonly Dictionary<int, int> _dimmerMap;

        public LightingChannels(int channelCount, DMXControl dmxController) : this(channelCount, null, dmxController) {
            
        }

        public LightingChannels(int channelCount, Dictionary<int, int> dimmerMap, DMXControl dmxController) {

            _channels = new byte[channelCount];

            _dimmerMap = dimmerMap;

            _dmxController = dmxController;
            _dmxController.Init();

            Recall();

            _autosaveTimer = new Timer();
            _autosaveTimer.Interval = 10000;
            _autosaveTimer.AutoReset = false;
            _autosaveTimer.Elapsed += new ElapsedEventHandler(_autosaveTimer_Elapsed);
        }

        private bool _isBlackout = false;
        public bool IsBlackout {
            get { return _isBlackout; }
            private set {
                _isBlackout = value;
                NotifyPropertyChanged("IsBlackout");
            }
        }

        public void Blackout() {
            Store();
            IsBlackout = true;

            for(int i = 0; i < _channels.Length; i++){
                _channels[i] = 0;
                UpdateChannel(i);
                NotifyPropertyChanged("Item[]");
            }
            
        }


        public void RestoreLevels() {
            Recall();
            for(int i = 0; i < _channels.Length; i++) {
                UpdateChannel(i);
            }
            NotifyPropertyChanged("Item[]");
            IsBlackout = false;
        }

        void _autosaveTimer_Elapsed(object sender, ElapsedEventArgs e) {
            Store();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private byte[] _channels;

        public byte this[int i]{
            get{
                return _channels[i];
            }
            set{
                if(_isBlackout) {
                    return;  // Don't update levels when blackedout
                }
                _channels[i] = value;
                //NotifyPropertyChanged("Item[" + i + "]");
                NotifyPropertyChanged("Item[]");
                UpdateChannel(i);
                ResetSaveTimer();
            }
        }

        private void ResetSaveTimer() {
            lock(_autosaveTimer) {
                _autosaveTimer.Stop();
                _autosaveTimer.Start();
            }

        }
        private void UpdateChannel(int i) {
            if(_dimmerMap != null) {
                _dmxController[_dimmerMap[i]] = _channels[i];
            } else {
                _dmxController[i + 1] = _channels[i];
            }
            //_dmxController[CHANNEL_DIMMER_MAPPING[i]] = _channels[i];
        }

        private void Store() {
            if(_isBlackout) {
                return; //Don't persist blackout levels
            }

            string stateString = string.Empty;
            for(int i = 0; i < _channels.Length; i++) {
                stateString += string.Format("{0:000};", _channels[i]);
            }
            log.Debug("Storing lighting values: " + stateString);
            //DataUtil.SetConfigValue("LightingLevels", stateString);
        }

        private void Recall() {
            string stateString = string.Empty;// = DataUtil.GetConfigValue("LightingLevels");

            string[] values = stateString.Split(';');
            int i = 0;
            foreach(string v in values) {
                byte newByte = 0;
                if(byte.TryParse(v, out newByte)) {
                    _channels[i++] = newByte;
                }
            }
            NotifyPropertyChanged("Item[]");
        }

    }
}
