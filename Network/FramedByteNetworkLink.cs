﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;
using System.Threading;
using System.IO;

using log4net;


namespace ThreeByte.Network
{

    public class FramedByteNetworkLink : IDisposable, INotifyPropertyChanged
    {
        private readonly ILog log = LogManager.GetLogger(typeof(FramedByteNetworkLink));

        //Observable Interface
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private const int MAX_DATA_SIZE = 100;

        private AsyncNetworkLink _networkLink;

        public NetworkFrame SendFrame { get; set; }
        public NetworkFrame ReceiveFrame { get; set; }

        public bool HasData {
            get {
                return (_incomingData.Count > 0);
            }
        }

        #region AsyncNetworkLink Pass-Through Properties
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
            set {
                _networkLink.Enabled = value;
                if(!value) {
                    //If the link is disabled, clear the buffer of messages
                    lock(_incomingData) {
                        _incomingData.Clear();
                    }
                }
            }
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
        #endregion //AsyncNetworkLink Pass-Through Properties


        public event EventHandler DataReceived;
       
        private List<byte[]> _incomingData;
        private MemoryStream _incomingBuffer;


        public FramedByteNetworkLink(string address, int port) {
            
            _incomingBuffer = new MemoryStream(2048);
            _incomingData = new List<byte[]>();

            _networkLink = new AsyncNetworkLink(address, port);
            _networkLink.DataReceived += new EventHandler(_networkLink_DataReceived);
            _networkLink.PropertyChanged += new PropertyChangedEventHandler(_networkLink_PropertyChanged);
            
        }

        #region Implements IDisposable
        private bool _disposed = false;
        /// <summary>
        /// Implementation of IDisposable interface.  Cancels the thread and releases resources.
        /// Clients of this class are responsible for calling it.
        /// </summary>
        public void Dispose() {
            if(_disposed) {
                return;  //Dispose has already been called
            }
            _disposed = true;
            log.Info("Cleaning up network resources");
            _networkLink.Dispose();
        }
        #endregion //Implements IDisposable

        void _networkLink_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == "Enabled"
                || e.PropertyName == "IsConnected"
                || e.PropertyName == "Error") {
                NotifyPropertyChanged(e.PropertyName);
            }
        }

        private int _headerPos = 0;
        private int _footerPos = 0;

        void _networkLink_DataReceived(object sender, EventArgs e) {
            bool hasNewData = false;

            byte[] header = new byte[0];
            if(ReceiveFrame != null && ReceiveFrame.Header != null) {
                header = ReceiveFrame.Header;
            }

            byte[] footer = new byte[0];
            if(ReceiveFrame != null && ReceiveFrame.Footer != null) {
                footer = ReceiveFrame.Footer;
            }

            while(_networkLink.HasData) {
                lock(_incomingBuffer) {
                    byte[] buffer = _networkLink.GetMessage();

                    //Must validate this buffer - see issue #4934
                    if(buffer == null) {
                        break;
                    }
                    for(int i = 0; i < buffer.Length; i++) {
                        if(_headerPos < header.Length - 1 && buffer[i] == header[_headerPos]) {
                            _headerPos++;
                        } else if(_headerPos == header.Length - 1 && buffer[i] == header[_headerPos]) {
                            _headerPos = 0;
                            _incomingBuffer.Position = 0; //Reset to the beginning
                        } else if(_footerPos < footer.Length - 1 && buffer[i] == footer[_footerPos]) {
                            _footerPos++;
                        } else if(_footerPos == footer.Length - 1 && buffer[i] == footer[_footerPos]) {

                            byte[] newData = new byte[(int)_incomingBuffer.Position];
                            Array.Copy(_incomingBuffer.GetBuffer(), newData, newData.Length);
                            //string newMessage = Encoding.ASCII.GetString(_incomingBuffer.GetBuffer(), 0, (int)_incomingBuffer.Position);
                            if(newData.Length > 0) {
                                //log.Debug("Adding Message: " + newMessage.Substring(0, Math.Min(30, newMessage.Length)));
                                lock(_incomingData) {
                                    _incomingData.Add(newData);
                                    if(_incomingData.Count > MAX_DATA_SIZE) {
                                        //Purge messages from the end of the list to prevent overflow
                                        log.Error("Too many incoming messages to handle: " + _incomingData.Count);
                                        _incomingData.RemoveAt(_incomingData.Count - 1);
                                    }
                                }
                            }
                            hasNewData = true;
                            _incomingBuffer.Position = 0;
                        } else {
                            _headerPos = 0;
                            _incomingBuffer.WriteByte(buffer[i]);
                        }
                    }
                }
            }

            if(hasNewData && DataReceived != null) {
                DataReceived(this, new EventArgs());
            }
        }

        /// <summary>
        /// Asynchronously sends the tcp message, waiting until the connection is reestablihsed if necessary
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string message) {
            SendData(Encoding.ASCII.GetBytes(message));
        }

        /// <summary>
        /// Asynchronously sends the tcp data, waiting until the connection is reestablihsed if necessary
        /// </summary>
        /// <param name="message"></param>
        public void SendData(byte[] data) {
            if(_disposed) {
                throw new ObjectDisposedException("Cannot send message on disposed FramedNetworkLink");
            }

            //Don't do anything if the link is not enabled
            if(!Enabled)
                return;

            //Add the header and footer
            byte[] header = new byte[0];
            if(SendFrame != null && SendFrame.Header != null) {
                header = SendFrame.Header;
            }
            byte[] footer = new byte[0];
            if(SendFrame != null && SendFrame.Footer != null) {
                footer = SendFrame.Footer;
            }
            byte[] messageBytes = new byte[data.Length + header.Length + footer.Length];

            header.CopyTo(messageBytes, 0);
            data.CopyTo(messageBytes, header.Length);
            footer.CopyTo(messageBytes, data.Length + header.Length);

            //log.Info("Debug: " + string.Format("{0} {1} {2}\r", message.Length, header.Length, messageBytes.ToString()));

            if(_networkLink != null)
                try {
                    _networkLink.SendMessage(messageBytes);
                } catch(ObjectDisposedException ode) {
                    log.Error(ode.Message);
                }
        }

        /// <summary>
        /// Fetches and removes (pops) the next available message as received on this link in order (FIFO)
        /// </summary>
        /// <returns>null if the link is not Enabled or there are no messages currently queued to return, a string otherwise.</returns>
        public string GetMessage() {
            if(_disposed) {
                throw new ObjectDisposedException("Cannot get message from disposed NetworkLink");
            }

            //Return null if the link is not enabled
            if(!Enabled) return null;

            byte[] data = GetData();
            if(data == null){
                return null;
            }
            return Encoding.ASCII.GetString(data);
        }


        /// <summary>
        /// Fetches and removes (pops) the next available group of bytes as received on this link in order (FIFO)
        /// </summary>
        /// <returns>null if the link is not Enabled or there is no data currently queued to return, an array of bytes otherwise.</returns>
        public byte[] GetData() {
            if(_disposed) {
                throw new ObjectDisposedException("Cannot get message from disposed NetworkLink");
            }

            //Return null if the link is not enabled
            if(!Enabled)
                return null;

            byte[] newData = null;
            lock(_incomingData) {
                if(HasData) {
                    newData = _incomingData[0];
                    _incomingData.RemoveAt(0);
                }
            }
            return newData;
        }

    }
}
