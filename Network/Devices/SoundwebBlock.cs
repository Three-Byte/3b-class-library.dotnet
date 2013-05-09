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
    public abstract class SoundwebBlock : INotifyPropertyChanged
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SoundwebBlock));

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


        public SoundwebBlock(string ipAddress) {            
            _networkLink = new FramedByteNetworkLink(ipAddress, 1023);  //Soundweb TCP communcation port
            _networkLink.SendFrame = new NetworkFrame() { Header = new byte[] { 0x02 }, Footer = new byte[] { 0x03 } };
            _networkLink.ReceiveFrame = _networkLink.SendFrame;
            _networkLink.DataReceived += new EventHandler(_networkLink_DataReceived);
        }
        
        protected static bool IsSpecialByte(byte b) {
            return (b == 0x02 || b == 0x03 || b == 0x06
                    || b == 0x15 || b == 0x1B);
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

        //Can be override in base classes
        protected virtual void UpdateFromMessage(byte[] data) {

        }

        protected void PackAndSendMessage(byte[] data) {
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

        protected byte[] ReceiveAndUnpackMessage() {

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
        protected void NotifyPropertyChanged(string propertyName) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
