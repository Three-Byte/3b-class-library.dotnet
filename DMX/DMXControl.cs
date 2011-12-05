using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.ComponentModel;
using log4net;

namespace ThreeByte.DMX
{
    /// <summary>
    /// For output only DMX transmission using Enttec DMX USB PRO
    /// </summary>
    public class DMXControl : INotifyPropertyChanged, IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DMXControl));

        //Declarations
        private const byte SEND_DMX_TX_MODE = 6;
        private const byte DMX_START_CODE = 0x7E;
        private const byte DMX_END_CODE = 0xE7;
        private const byte DMX_HEADER_LENGTH = 4;
        private const int DMX_PACKET_SIZE = 512;

        private SerialPort _serialPort;
        private Stream _serialPortStream;
        private object _serialLock = new object();
        private byte[] _dmxValues;
        private readonly string _comPort;

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public DMXControl(string comPort) {
            _comPort = comPort;
            _dmxValues = new byte[DMX_PACKET_SIZE];
        }

        private bool _openError = false;
        public void Init() {
            try {
                lock(_serialLock) {
                    if(_isDisposed) {
                        return;
                    }
                    //If you don't hold onto and dispose of the stream explicity it will cause an
                    //uncatchable UnauthorizedAccessException
                    //See: http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=140018
                    if(_serialPortStream != null) {
                        _serialPortStream.Dispose();
                    }
                    if(_serialPort != null) {
                        _serialPort.Dispose();
                    }
                    _serialPort = new SerialPort(_comPort);
                }
                _serialPort.Open();
                _serialPortStream = _serialPort.BaseStream;
                _openError = false;
            } catch(Exception ex) {
                if(!_openError || _wasOpen){
                    log.Error("Error opening serial port: " + _comPort, ex);
                }
                _openError = true;
            }
            NotifyPropertyChanged("IsOpen");
        }

        public bool IsOpen {
            get {
                try {
                    lock(_serialLock) {
                        return _serialPort.IsOpen;
                    }
                } catch(Exception ex) {
                    log.Error("IsOpen Exception", ex);
                }
                return false;
            }
        }

        public byte this[int i] {
            get {
                return _dmxValues[i];
            }
            set {
                _dmxValues[i] = value;
                SendDMXData(_dmxValues);
            }
        }

        public void SetAll(byte val) {
            for(int i = 1; i < 512; i++) {
                _dmxValues[i] = val;
            }
            SendDMXData(_dmxValues);
        }
        private bool _wasOpen = false;
        private void SendDMXData(byte[] data) {
            if(data.Length > DMX_PACKET_SIZE){
                throw new ArgumentOutOfRangeException("DMX data is limited to 512 bytes");
            }

            if(_wasOpen != IsOpen) {
                NotifyPropertyChanged("IsOpen");
            }
            _wasOpen = IsOpen;


            byte[] sendBuffer = new byte[DMX_PACKET_SIZE + DMX_HEADER_LENGTH + 1];
            //HEADER
            sendBuffer[0] = DMX_START_CODE;
            sendBuffer[1] = SEND_DMX_TX_MODE;
            sendBuffer[2] = DMX_PACKET_SIZE & 0xFF; //LSB
            sendBuffer[3] = DMX_PACKET_SIZE >> 8; //MSB
            //DATA VALUES
            data.CopyTo(sendBuffer, DMX_HEADER_LENGTH);
            //FOOTER
            sendBuffer[sendBuffer.Length - 1] = DMX_END_CODE;
            
            try {
                lock(_serialLock) {
                    _serialPort.Write(sendBuffer, 0, sendBuffer.Length);
                }
            } catch(Exception ex) {
                if (!_openError)
                {
                    log.Error("Serial Transmit Error", ex);
                }
                try {
                    lock(_serialLock) {
                        _serialPort.Close();
                    }
                } catch(Exception ex2) {
                    if (!_openError)
                    {
                        log.Error("Error closing the serial port", ex2);
                    }
                }
                Init();
            }
        }

        private bool _isDisposed = false;
        public void Dispose() {
            
            lock(_serialLock) {
                if(_isDisposed) {
                    throw new ObjectDisposedException("DMX Control Previously disposed");
                }
                _isDisposed = true;
                //If you don't hold onto and dispose of the stream explicity it will cause an
                //uncatchable UnauthorizedAccessException
                //See: http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=140018
                if(_serialPortStream != null) {
                    _serialPortStream.Dispose();
                    _serialPortStream = null;
                }
                if(_serialPort != null) {
                    _serialPort.Dispose();
                    _serialPort = null;
                }
            }
        }

    }
}
