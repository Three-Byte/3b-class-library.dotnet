using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;
using System.Threading;
using System.IO;

using log4net;
using System.IO.Ports;
using ThreeByte.Network;
using System.Threading.Tasks;

/// author: chris@3-byte.com

namespace ThreeByte.Network {

    /// <summary>
    /// Implements an abstraction around a stream that handles gathering and raises configurable application-layer messages
    /// </summary>
    public class FramedStreamLink : IDisposable, INotifyPropertyChanged {
        private readonly ILog log = LogManager.GetLogger(typeof(FramedStreamLink));

        //Observable Interface
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private const int MAX_DATA_SIZE = 2048;

        private Stream _stream;

        public NetworkFrame SendFrame { get; set; }
        public NetworkFrame ReceiveFrame { get; set; }

        public event EventHandler DataReceived;

        private List<string> _incomingData;
        private MemoryStream _incomingBuffer;

        public FramedStreamLink(Stream stream) {

            _incomingBuffer = new MemoryStream(MAX_DATA_SIZE);
            _incomingData = new List<string>();

            _stream = stream;
            startReading();
        }

        public bool HasData {
            get {
                return (_incomingData.Count > 0);
            }
        }

        #region Implements IDisposable
        private bool _disposed = false;
        /// <summary>
        /// Implementation of IDisposable interface.
        /// Clients of this class are responsible for calling it.
        /// </summary>
        public void Dispose() {
            if(_disposed) {
                return;  //Dispose has already been called
            }
            _disposed = true;
            log.Info("Cleaning up stream resources");
            _stream.Dispose();
        }
        #endregion //Implements IDisposable

        void _serialLink_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == "Enabled"
                || e.PropertyName == "IsConnected"
                || e.PropertyName == "Error") {
                NotifyPropertyChanged(e.PropertyName);
            }
        }

        private int _headerPos = 0;
        private int _footerPos = 0;

        private void startReading() {
            Task readTask = new Task(() => {
                // Don't use await here, because we aren't actually waiting on that delegate.
                readLoop().Wait();
            }, TaskCreationOptions.LongRunning);
            readTask.ContinueWith(t =>{
                if(t.IsFaulted) {
                    log.Error("Read loop error", t.Exception);
                    this.Dispose(); // force the object to shutdown
                }
            });
            readTask.Start();
        }

        private async Task readLoop() {
            byte[] buffer = new byte[MAX_DATA_SIZE];

            while(true) {
                bool hasNewData = false;

                byte[] header = new byte[0];
                if(ReceiveFrame != null && ReceiveFrame.Header != null) {
                    header = ReceiveFrame.Header;
                }

                byte[] footer = new byte[0];
                if(ReceiveFrame != null && ReceiveFrame.Footer != null) {
                    footer = ReceiveFrame.Footer;
                }

                int bytesRead = await _stream.ReadAsync(buffer, 0, MAX_DATA_SIZE);

                lock(_incomingBuffer) {
                    for(int i = 0; i < bytesRead; i++) {
                        if(_headerPos < header.Length - 1 && buffer[i] == header[_headerPos]) {
                            _headerPos++;
                        } else if(_headerPos == header.Length - 1 && buffer[i] == header[_headerPos]) {
                            _headerPos = 0;
                            _footerPos = 0;
                            _incomingBuffer.Position = 0; //Reset to the beginning
                        } else if(_footerPos < footer.Length - 1 && buffer[i] == footer[_footerPos]) {
                            _footerPos++;
                        } else if(_footerPos == footer.Length - 1 && buffer[i] == footer[_footerPos]) {
                            _footerPos = 0;  //Reset Footer

                            string newMessage = Encoding.UTF8.GetString(_incomingBuffer.GetBuffer(), 0, (int)_incomingBuffer.Position);
                            if(newMessage.Trim() != string.Empty) {
                                //log.Debug("Adding Message: " + newMessage.Substring(0, Math.Min(30, newMessage.Length)));
                                lock(_incomingData) {
                                    _incomingData.Add(newMessage);
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

                if(hasNewData && DataReceived != null && !_disposed) {
                    DataReceived(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Asynchronously sends the tcp message, waiting until the connection is reestablihsed if necessary
        /// </summary>
        /// <param name="message"></param>
        public async void SendMessage(string message) {
            if(_disposed) {
                throw new ObjectDisposedException("Cannot send message on disposed FramedSerialLink");
            }

            //Add the header and footer
            byte[] header = new byte[0];
            if(SendFrame != null && SendFrame.Header != null) {
                header = SendFrame.Header;
            }
            byte[] footer = new byte[0];
            if(SendFrame != null && SendFrame.Footer != null) {
                footer = SendFrame.Footer;
            }
            byte[] messageBytes = new byte[message.Length + header.Length + footer.Length];

            header.CopyTo(messageBytes, 0);
            Encoding.UTF8.GetBytes(message, 0, message.Length, messageBytes, header.Length);
            footer.CopyTo(messageBytes, message.Length + header.Length);

            //log.Info("Debug: " + string.Format("{0} {1} {2}\r", message.Length, header.Length, messageBytes.ToString()));

            if(_stream != null) {
                try {
                    await _stream.WriteAsync(messageBytes, 0, messageBytes.Length);
                } catch(ObjectDisposedException ode) {
                    log.Error(ode.Message);
                } catch(Exception ex) {
                    //Also possible for the serial link to raise and UnauthorizedAccessException here
                    log.Error("SendMessage error", ex);
                }
            }
        }

        /// <summary>
        /// Fetches and removes (pops) the next available message as received on this link in order (FIFO)
        /// </summary>
        /// <returns>null if the link is not Enabled or there are no messages currently queued to return, a string otherwise.</returns>
        public string GetMessage() {
            if(_disposed) {
                throw new ObjectDisposedException("Cannot get message from disposed FramedSerialLink");
            }
            string newMessage = null;
            lock(_incomingData) {
                if(HasData) {
                    newMessage = _incomingData[0];
                    //throw new InvalidOperationException("Cannot return any data [" + newMessage + "]");
                    _incomingData.RemoveAt(0);
                }
            }
            return newMessage;
        }

    }
}
