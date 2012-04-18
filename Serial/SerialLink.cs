using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.ComponentModel;
using System.IO.Ports;
using System.IO;
using System.Threading;

namespace ThreeByte.Serial {



    public class SerialLink : IDisposable, INotifyPropertyChanged {

        private readonly ILog log = LogManager.GetLogger(typeof(SerialLink));

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private const int MAX_DATA_SIZE = 100;

        #region Public Properties

        public string COMPort { get; set; }

        public bool HasData {
            get { return _incomingData.Count > 0; }
        }

        private bool _enabled;
        /// <summary>
        /// Gets or sets a value indicating whether messages should be propogated through the serial port or not
        /// </summary>
        public bool Enabled {
            get {
                return _enabled;
            }
            set {
                _enabled = value;
                if(!_enabled) {
                    SafeClose();
                } else if(!IsOpen) {
                    SafeConnect();
                }
                NotifyPropertyChanged("Enabled");
            }
        }

        private bool _isOpen;
        /// <summary>
        /// Gets a value indicating whether or not the current serial port is open
        /// </summary>
        public bool IsOpen {
            get {
                return _isOpen;
            }
            private set {
                if(value != _isOpen) {
                    _isOpen = value;
                    NotifyPropertyChanged("IsConnected");
                }
            }
        }

        public event EventHandler DataReceived;

        private Exception _error;
        public Exception Error {
            get {
                return _error;
            }
            set {
                Exception oldError = _error;
                _error = value;
                if(oldError != _error) {
                    NotifyPropertyChanged("Error");
                }
            }
        }

        #endregion Public Properties

        #region Private Members

        private List<byte[]> _incomingData;

        private SerialPort _serialPort;
        private object _serialLock = new object();

        private bool _disposed = false;

        private IAsyncResult _writeResult = null;
        private IAsyncResult _readResult = null;

        #endregion Private Members

        public SerialLink(string comPort, bool enabled = true) {
            COMPort = comPort;

            _incomingData = new List<byte[]>();

            Enabled = true;
        }

        private void SafeClose() {
            log.Debug("Safe Close");

            lock(_serialLock) {
                if(_serialPort != null) {
                    if(_serialPort.IsOpen) {
                        _serialPort.Close();
                        _serialPort.DataReceived -= _serialPort_DataReceived;
                    }
                }
                _serialPort = null;

                lock(_incomingData) {
                    _incomingData.Clear();
                }

                IsOpen = false;
            }
        }

        public void Dispose() {
            if(_disposed) {
                return;
            }

            _disposed = true;
            log.Info("Cleaning up serial ports");

            SafeClose();
        }

        private void SafeConnect() {
            if(_disposed) {
                return;
            }

            lock(_serialLock) {
                if(_serialPort == null || !IsOpen) {
                    SafeClose();
                    _serialPort = new SerialPort(COMPort);
                    _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);
                }

                if(!IsOpen) {
                    log.Info("Connecting: " + COMPort);
                    try {
                        _serialPort.Open();                        
                        IsOpen = true;
                    } catch(Exception ex) {
                        log.Error("Serial Connection Error", ex);
                        Error = ex;
                        IsOpen = false;
                    }
                }
            }
        }

        void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e) {
            ReceiveData();
        }

        private void SafeConnect(object state) {
            SafeConnect();
        }

        public void SendData(byte[] message) {
            if(Enabled) {
                lock(_serialLock) {
                    try {
                        _serialPort.Write(message, 0, message.Length);
                    } catch(Exception ex) {
                        log.Error("Cannot write serial data", ex);
                        Error = ex;
                        IsOpen = false;
                        SafeClose();
                        SafeConnect();
                    }
                }
            }
        }

        private void ReceiveData() {
            if(Enabled) {
                bool hasNewData = false;
                lock(_serialLock) {
                    try {
                        int bytesToRead = _serialPort.BytesToRead;
                        byte[] buf = new byte[bytesToRead];
                        int bytesRead = _serialPort.Read(buf, 0, bytesToRead);

                        _incomingData.Add(buf);
                        hasNewData = true;
                        if(_incomingData.Count > MAX_DATA_SIZE) {
                            log.Error("Too many incoming messages to handle: " + _incomingData.Count);
                            _incomingData.RemoveAt(_incomingData.Count - 1);
                        }

                        Error = null;
                        IsOpen = true;
                    } catch(Exception ex) {
                        log.Error("Error reading from serial", ex);
                        Error = ex;
                        IsOpen = false;
                        SafeConnect();
                    }
                }

                if(hasNewData && DataReceived != null && !_disposed) {
                    DataReceived(this, new EventArgs());
                }
            }
        }

        public byte[] GetMessage() {
            if(_disposed) {
                throw new ObjectDisposedException("Cannot get message from disposed SerialLink");
            }

            //Return null if the link is not enabled
            if(!Enabled) return null;

            byte[] newMessage = null;
            lock(_incomingData) {
                if(HasData) {
                    newMessage = _incomingData[0];
                    _incomingData.RemoveAt(0);
                }
            }
            return newMessage;
        }
    }
}
