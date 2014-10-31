using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using log4net;
using ThreeByte.Converters;

namespace ThreeByte.Network.Devices
{
    public class SoundwebFader : SoundwebBlock
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SoundwebFader));

        private readonly byte[] _nodeID;
        private readonly byte[] _virtualDeviceID;
        private readonly byte[] _objectID;
        private readonly byte[] _header; //Address + State Variable

        private readonly byte[] SV_MASTER_GAIN = new byte[] { 0x00, 0x60 };  //State variable for master gain
        private readonly byte[] SV_MUTE = new byte[] { 0x00, 0x61 };  //State variable for master gain
        private readonly byte[] ZERO_DATA = new byte[] { 0x00, 0x00, 0x00, 0x00 };

        private Timer _pollingTimer;

        public SoundwebFader(string ipAddress, byte[] nodeID, byte[] virtualDeviceID, byte[] objectID) : base(ipAddress) {
            _nodeID = nodeID;
            _virtualDeviceID = virtualDeviceID;
            _objectID = objectID;

            //Initialize header for future use
            _header = new byte[8];
            _nodeID.CopyTo(_header, 0);
            _virtualDeviceID.CopyTo(_header, 2);
            _objectID.CopyTo(_header, 3);
            
            _pollingTimer = new Timer(PollValue, null, TimeSpan.FromMilliseconds(1), TimeSpan.FromSeconds(1));
        }

        public void Bump(int percent) {
            byte[] bumpMessage = new byte[13];
            bumpMessage[0] = 0x90;  //Bump

            _header.CopyTo(bumpMessage, 1);
            SV_MASTER_GAIN.CopyTo(bumpMessage, 7);

            percent = percent * short.MaxValue;
            BitConverter.GetBytes(percent).Reverse().ToArray().CopyTo(bumpMessage, 9);

            PackAndSendMessage(bumpMessage);
            SubscribeLevel(); //Update the fader level right away
        }

        public void SetPercent(int newPercent) {
            byte[] percentMessage = new byte[13];
            percentMessage[0] = 0x8D;  //SetPercent

            _header.CopyTo(percentMessage, 1);
            SV_MASTER_GAIN.CopyTo(percentMessage, 7);

            int percent = newPercent * short.MaxValue;
            BitConverter.GetBytes(percent).Reverse().ToArray().CopyTo(percentMessage, 9);

            PackAndSendMessage(percentMessage);
            SubscribeLevel(); //Update the fader level right away
            
            //The fader level will be updated by callback. To simulate for testing:
            //FaderLevel = (int)(((newPercent / 100.0) * 280000.0) - 280000);
        }

        public void Mute(bool mute) {
            byte[] muteMessage = new byte[13];
            muteMessage[0] = 0x88;  //Set Val

            _header.CopyTo(muteMessage, 1);
            SV_MUTE.CopyTo(muteMessage, 7);

            int muteVal = (mute ? 1 : 0);
            BitConverter.GetBytes(muteVal).Reverse().ToArray().CopyTo(muteMessage, 9);

            PackAndSendMessage(muteMessage);
            SubscribeMute(); //Update the mute setting right away
        }

        private void PollValue(object state)
        {
            SubscribeLevel();
            SubscribeMute();
        }


        public void SubscribeLevel() {
            byte[] subscribeMessage = new byte[13];
            subscribeMessage[0] = 0x89;  //Subscribe

            _header.CopyTo(subscribeMessage, 1);
            SV_MASTER_GAIN.CopyTo(subscribeMessage, 7);

            ZERO_DATA.CopyTo(subscribeMessage, 9);

            PackAndSendMessage(subscribeMessage);
        }

        public void SubscribeMute() {
            byte[] subscribeMessage = new byte[13];
            subscribeMessage[0] = 0x89;  //Subscribe

            _header.CopyTo(subscribeMessage, 1);
            SV_MUTE.CopyTo(subscribeMessage, 7);

            ZERO_DATA.CopyTo(subscribeMessage, 9);

            PackAndSendMessage(subscribeMessage);
        }

        public void Unsubscribe() {
            byte[] unsubscribeMessage = new byte[13];
            unsubscribeMessage[0] = 0x8A;  //Unsubscribe

            _header.CopyTo(unsubscribeMessage, 1);
            ZERO_DATA.CopyTo(unsubscribeMessage, 9);

            PackAndSendMessage(unsubscribeMessage);
        }

        protected override void UpdateFromMessage(byte[] data) {

            if(data.Length != 14) {
                //Not a valid message
                throw new ArgumentOutOfRangeException("data", "Level mesage must be 14 bytes long");
            }
            if(data[0] != 0x88){
                throw new ArgumentException("data", "Level response must have id 0x88 not " + data[0]);
            }
            if(data[7] == SV_MASTER_GAIN[0] && data[8] == SV_MASTER_GAIN[1]) {
                //Parse this as a level
                byte[] levelBytes = new byte[4];
                Array.Copy(data, 9, levelBytes, 0, 4);
                FaderLevel = BitConverter.ToInt32(levelBytes.Reverse().ToArray(), 0);
            } else if(data[7] == SV_MUTE[0] && data[8] == SV_MUTE[1]) {
                //Parse this as a mute indicator
                byte[] levelBytes = new byte[4];
                Array.Copy(data, 9, levelBytes, 0, 4);
                IsMuted = BitConverter.ToBoolean(levelBytes.Reverse().ToArray(), 0);
            }
        }

        private int _faderLevel;
        public int FaderLevel {
            get { return _faderLevel; }
            private set {
                _faderLevel = value;
                NotifyPropertyChanged("FaderLevel");
            }
        }

        private bool _isMuted;
        public bool IsMuted {
            get { return _isMuted; }
            private set {
                _isMuted = value;
                NotifyPropertyChanged("IsMuted");
            }
        }

        public static int LevelToPercent(int level) {
            int maxValue = 0;
            int minValue = -280000;
            level = Math.Max(level, minValue);
   
            return (int)Math.Round(((double)(level - minValue) / (double)(maxValue - minValue)) * 100.0);
        }

    }
}
