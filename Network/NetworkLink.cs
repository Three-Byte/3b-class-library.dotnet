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


namespace ThreeByte.Network
{
    public class NetworkLink : IDisposable, INotifyPropertyChanged
    {
        private readonly ILog log = LogManager.GetLogger(typeof(NetworkLink));

        //Observable Interface
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private const int BUF_SIZE = 1024;
        private const int READ_TIMEOUT = 1000;
        private const int MAX_DATA_SIZE = 100;
        private const byte HEADER = 0x02;
        private const byte FOOTER = 0x03;

        public string Address { get; set; }
        public int Port { get; set; }

        public bool HasData {
            get {
                return (_incomingData.Count > 0);
            }
        }


        private bool _enabled;
        /// <summary>
        /// Gets or sets a value indicating where messages should be propogated to the network or not
        /// </summary>
        public bool Enabled {
            get {
                return _enabled;
            }
            set {
                _enabled = value;
                if(!_enabled) {
                    ThreadPool.QueueUserWorkItem(SafeCloseAsync);
                    //SafeClose();
                }
                NotifyPropertyChanged("Enabled");
            }
        }

        private bool _isConnected;
        /// <summary>
        /// Gets a value indicating whether or not there is current network activity with this node
        /// </summary>
        public bool IsConnected {
            get{
             return _isConnected;   
            }
            private set {
                if(value != _isConnected) {
                    _isConnected = value;
                    NotifyPropertyChanged("IsConnected");
                }
            }
        }

        public event EventHandler DataReceived;
       
        private List<string> _incomingData;
        private List<string> _outgoingData;

        private TcpClient _tcpClient;
        private object _clientLock = new object();
        private BackgroundWorker netWorker;



        public NetworkLink(string address, int port) {
            Address = address;
            Port = port;
            
            _incomingData = new List<string>();
            _outgoingData = new List<string>();

            netWorker = new BackgroundWorker();
            netWorker.DoWork += new DoWorkEventHandler(netWorker_DoWork);
            netWorker.WorkerSupportsCancellation = true;

            netWorker.RunWorkerAsync();

            Enabled = true;  //Default is true so we don't break anything
        }


        private bool _disposed = false;
        /// <summary>
        /// Implementation of IDisposable interface.  Cancels the thread and releases resources.
        /// Clients of this class are responsible for calling it.
        /// </summary>
        public void Dispose() {
            if(_disposed) {
                return;  //Dispose has already been called
            }
            log.Info("Cleaning up network resources");
            netWorker.CancelAsync();
            netWorker = null;

            SafeClose();
        }

        private void netWorker_DoWork(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = (BackgroundWorker)sender;

            //Name this thread for easier referencing / debugging
            Thread.CurrentThread.Name = string.Format("NetworkLink: {0}/{1}", Address, Port);
 
            //const string DELIM = "[/TCP]\0";
            //string stringBuffer = string.Empty;

            MemoryStream memStream = new MemoryStream(2048);

            while(!worker.CancellationPending) {

                if(Enabled) {
                    SafeConnect();
                } else {
                    //The link is currently disabled
                    SafeClose();
                }

                //Send any existing messages in the queue
                if(Enabled) {
                    lock(_outgoingData) {
                        if(_outgoingData.Count > 0) {
                            string s = _outgoingData[0];
                            byte[] buffer = new byte[s.Length + 2]; //Plus Header and Footer
                            buffer[0] = HEADER;
                            Encoding.ASCII.GetBytes(s, 0, s.Length, buffer, 1);
                            buffer[buffer.Length - 1] = FOOTER;  //End transmit character
                            try {
                                _tcpClient.GetStream().Write(buffer, 0, buffer.Length);  //May throw SocketException
                                _outgoingData.RemoveAt(0);
                                IsConnected = true;
                            } catch(Exception ex) {
                                log.Debug("Cannot connect to the node to get the stream", ex);
                                IsConnected = false;
                            }
                        }
                    }
                } else {
                    //The link is currently disabled
                    lock(_outgoingData) {
                        _outgoingData.Clear();
                    }
                }

                //Retrieve any messages in the queue
                bool hasNewData = false;
                if(Enabled) {
                    try {
                        if(_tcpClient.Available > 0) {
                            NetworkStream stream = _tcpClient.GetStream();
                            stream.ReadTimeout = READ_TIMEOUT;
                            byte[] buffer = new byte[BUF_SIZE];
                            int bytesRead = 0;

                            while(stream.DataAvailable) {
                                //log.Debug("Read " + bytesRead + " so far");
                                bytesRead = stream.Read(buffer, 0, BUF_SIZE);
                                for(int i = 0; i < bytesRead; i++) {
                                    if(buffer[i] == HEADER) {
                                        memStream.Position = 0; //Reset to the beginning
                                    } else if(buffer[i] == FOOTER) {
                                        string newMessage = Encoding.ASCII.GetString(memStream.GetBuffer(), 0, (int)memStream.Position);
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
                                        memStream.Position = 0;
                                    } else {
                                        memStream.WriteByte(buffer[i]);
                                    }

                                }
                            }

                            IsConnected = true;
                        }


                    } catch(Exception ex) {
                        log.Warn("Error reading the stream", ex);
                        IsConnected = false;
                    }
                } else {
                    //The link is currently disabled
                    lock(_incomingData) {
                        _incomingData.Clear();
                    }
                }

                if(hasNewData && DataReceived != null) {
                    DataReceived(this, new EventArgs());
                }

                Thread.Sleep(1); //Yield the process before continuing          
            }
        }

        private void SafeCloseAsync(object state) {
            SafeClose();
        }


        /// <summary>
        /// Very carefully checks and shuts down the tcpClient and sets it to null
        /// </summary>
        /// <param name="client"></param>
        private void SafeClose() {
            lock(_clientLock) {
                if(_tcpClient != null) {
                    if(_tcpClient.Client != null) {
                        _tcpClient.Client.Close();
                    }
                    _tcpClient.Close();
                }
                _tcpClient = null;
            }
        }

        /// <summary>
        /// Carefully check to see if the link is connected or can be reestablished
        /// </summary>
        private void SafeConnect() {

            try {
                Monitor.Enter(_clientLock);
                if(_tcpClient == null || !_tcpClient.Connected) {
                    Monitor.Exit(_clientLock);
                    SafeClose();
                    Monitor.Enter(_clientLock);
                    _tcpClient = new TcpClient();
                }

                //See if the TCP connection is open
                if(!_tcpClient.Connected) {
                    try {
                        //Try to open it.
                        _tcpClient.Connect(Address, Port);
                        IsConnected = true;
                    } catch(Exception ex) {
                        log.Debug("Cannot connect to client", ex);
                        IsConnected = false;
                    }
                }
            } catch(Exception ex) {
                log.Error("SafeConnect Threading Error", ex);
                throw ex;
            } finally {
                Monitor.Exit(_clientLock);  //Must release this lock
            }
        }


        /// <summary>
        /// Asynchronously sends the tcp message, waiting until the connection is reestablihsed if necessary
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string message) {
            if(_disposed) {
                throw new ObjectDisposedException("Cannot send message on disposed NetworkLink");
            }

            //Don't do anything if the link is not enabled
            if(!Enabled) return;

            lock(_outgoingData) {
                _outgoingData.Add(message);

                if(_outgoingData.Count > MAX_DATA_SIZE) {
                    //Purge messages from the end of the list to prevent overflow
                    log.Error("Too many outgoing messages to handle: " + _outgoingData.Count);
                    _outgoingData.RemoveAt(_outgoingData.Count - 1);
                }
            }
        }


        public string GetMessage() {
            if(_disposed) {
                throw new ObjectDisposedException("Cannot get message from disposed NetworkLink");
            }

            //Don't do anything if the link is not enabled
            if(!Enabled) return null;

            string newMessage = string.Empty;

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
