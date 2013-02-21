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
    public class SoundwebFader : INotifyPropertyChanged
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
        private readonly FramedByteNetworkLink _networkLink;

        #region FramedNetworkLink Pass-Through Properties
        public string Address {
            get { return _networkLink.Address; }
        }

        public int Port {
            get { return _networkLink.Port; }
        }

        /// <summary>
        /// Gets or sets a value indicating where messages should be propogated to the network or not
        /// </summary>
        public bool Enabled {
            get { return _networkLink.Enabled; }
            set { _networkLink.Enabled = value; }
        }

        /// <summary>
        /// Gets a value indicating whether or not there is current network activity with this node
        /// </summary>
        public bool IsConnected {
            get { return _networkLink.IsConnected; }
        }

        /// <summary>
        /// Gets a representation of the last Exception that was thrown by the underlying connection
        /// </summary>
        public Exception Error {
            get { return _networkLink.Error; }
        }
        #endregion //FramedNetworkLink Pass-Through Properties


        public SoundwebFader(string ipAddress, byte[] nodeID, byte[] virtualDeviceID, byte[] objectID) {
            _nodeID = nodeID;
            _virtualDeviceID = virtualDeviceID;
            _objectID = objectID;

            //Initialize header for future use
            _header = new byte[8];
            _nodeID.CopyTo(_header, 0);
            _virtualDeviceID.CopyTo(_header, 2);
            _objectID.CopyTo(_header, 3);
            

            _networkLink = new FramedByteNetworkLink(ipAddress, 1023);  //Soundweb TCP communcation port
            _networkLink.SendFrame = new NetworkFrame() { Header = new byte[] { 0x02 }, Footer = new byte[] { 0x03 } };
            _networkLink.ReceiveFrame = _networkLink.SendFrame;
            _networkLink.DataReceived += new EventHandler(_networkLink_DataReceived);

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

        
        void _networkLink_DataReceived(object sender, EventArgs e) {
            //Got Data
            byte[] newData = ReceiveAndUnpackMessage();
            
            try {
                UpdateFromMessage(newData);
            } catch(Exception ex) {
                log.Error("Error retrieving level data", ex);
            }
        }

        //private static readonly LevelToByteConverter _byteConverter = new LevelToByteConverter();
        private void UpdateFromMessage(byte[] data) {

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


        private static bool IsSpecialByte(byte b) {
            return (b == 0x02 || b == 0x03 || b == 0x06
                    || b == 0x15 || b == 0x1B);
        }

        private void PackAndSendMessage(byte[] data) {
            List<byte> packedData = new List<byte>();

            byte checksum = 0;
            packedData.Add(0x02); //STX

            foreach(byte b in data) {
                checksum ^= b;

                if(IsSpecialByte(b)) {
                    packedData.Add(0x1B);//Escape Byte
                    packedData.Add((byte)(b + 0x80));
                } else {
                    packedData.Add(b);
                }
            }
            if(IsSpecialByte(checksum)) {
                packedData.Add(0x1B);//Escape Byte
                packedData.Add((byte)(checksum + 0x80));
            } else {
                packedData.Add(checksum);
            }

            packedData.Add(0x03); //ETX

            _networkLink.SendData(packedData.ToArray());
        }


        private byte[] ReceiveAndUnpackMessage() {

            List<byte> unpackedData = new List<byte>();
            bool escape = false;
            byte checksum = 0;

            //We only want to act on the most recently received message, so additional calls here are ignored (which is OK)
            byte[] data = _networkLink.GetData();
            while (_networkLink.HasData)
            {
                data = _networkLink.GetData();
            }
            if(data == null) {  //Cannot iterate over a null collection, so must test and return here
                return null;
            }
            foreach(byte b in data) {
                if(b == 0x1B) { //Escape
                    escape = true;
                } else {
                    if(escape) {
                        unpackedData.Add((byte)(b - 0x80));
                        checksum ^= (byte)(b - 0x80);
                    } else {
                        unpackedData.Add(b);
                        checksum ^= b;
                    }
                    escape = false;
                }
            }

            if(checksum != 0) {
                log.Warn("Invalid checksum for message: " + checksum);
                return null;
            }
            return unpackedData.ToArray();
        }



        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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

    }
}
