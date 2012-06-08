using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.ComponentModel;
using log4net;
using ThreeByte.Serial;

namespace ThreeByte.DMX
{
    /// <summary>
    /// For output only DMX transmission using Enttec DMX USB PRO
    /// </summary>
    public class VariableDMXControl : INotifyPropertyChanged, IDisposable, IDMXControl
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DMXControl));

        //Declarations
        private const byte SEND_DMX_TX_MODE = 6;
        private const byte DMX_START_CODE = 0x7E;
        private const byte DMX_END_CODE = 0xE7;
        private const byte DMX_HEADER_LENGTH = 4;

        private SerialLink serialLink;
        private byte[] _dmxValues;

        public string COMPort {
            get { return serialLink.COMPort; }
            set {
                serialLink.COMPort = value;
            }
        }

        public string HardwareID { get { return COMPort; } }

        private int _dmxPacketSize;
        public int DMXPacketSize {
            get { return _dmxPacketSize; }
            set {
                _dmxPacketSize = value;
                if(_dmxValues != null) {
                    byte[] tmpDmxValues = _dmxValues;
                    _dmxValues = new byte[_dmxPacketSize];
                    tmpDmxValues.CopyTo(_dmxValues, 0);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private int StartChannel = 0;

        public VariableDMXControl(string comPort) : this(comPort, 512) { }

        public VariableDMXControl(string comPort, int channelCount, int startChannel = 0) {
            DMXPacketSize = channelCount + startChannel;
            _dmxValues = new byte[DMXPacketSize];
            StartChannel = startChannel;
            serialLink = new SerialLink(comPort);
        }

        public bool IsOpen {
            get {
                try {
                    return serialLink.IsOpen;
                } catch(Exception ex) {
                    log.Error("IsOpen Exception", ex);
                }
                return false;
            }
        }

        private bool _enabled = true;  //Default
        public bool Enabled {
            get {
                return _enabled;
            }
            set {
                _enabled = value;
                NotifyPropertyChanged("Enabled");
            }
        }

        public byte this[int i] {
            get {
                lock(_dmxValues) {
                    return _dmxValues[i];
                }
            }
            set {
                lock(_dmxValues) {
                    _dmxValues[i] = value;
                    SendDMXData(_dmxValues);
                }
            }
        }

        public void SetAll(byte val) {
            lock(_dmxValues) {
                for(int i = 1; i < 512; i++) {
                    _dmxValues[i] = val;
                }
                SendDMXData(_dmxValues);
            }
        }

        public void SetValues(Dictionary<int, byte> values) {
            lock(_dmxValues) {
                foreach(int i in values.Keys) {
                    _dmxValues[i] = values[i];
                }
                SendDMXData(_dmxValues);
            }
        }

        public void SetValues(Dictionary<int, byte> values, int startChannel) {
            lock(_dmxValues) {
                Console.WriteLine("Start Channel: " + startChannel);
                foreach(int i in values.Keys) {
                    _dmxValues[i - startChannel] = values[i];
                    Console.WriteLine(string.Format("SetValues: _dmxValues[{0}]: {1}, values[{2}]", i - startChannel, _dmxValues[i - startChannel], i));
                }

                SendDMXData(_dmxValues);
            }
        }

        private bool _wasOpen = false;
        private void SendDMXData(byte[] data) {
            if(!Enabled) {
                return;
            }

            if(data.Length > DMXPacketSize){
                throw new ArgumentOutOfRangeException("DMX data is limited to 512 bytes");
            }

            if(_wasOpen != IsOpen) {
                NotifyPropertyChanged("IsOpen");
            }
            _wasOpen = IsOpen;


            byte[] sendBuffer = new byte[DMXPacketSize + DMX_HEADER_LENGTH + 1];
            //HEADER
            sendBuffer[0] = DMX_START_CODE;
            sendBuffer[1] = SEND_DMX_TX_MODE;
            sendBuffer[2] = (byte)(DMXPacketSize & 0xFF); //LSB
            sendBuffer[3] = (byte)(DMXPacketSize >> 8); //MSB
            //DATA VALUES
            data.CopyTo(sendBuffer, DMX_HEADER_LENGTH);
            //FOOTER
            sendBuffer[sendBuffer.Length - 1] = DMX_END_CODE;

            try {
                serialLink.SendData(sendBuffer);
            } catch(Exception ex) {
                log.Error(ex);
            }

        }

        public void Dispose() {
            serialLink.Dispose();
        }

    }
}
