using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;
using System.Threading;
using System.IO;
using System.Collections.ObjectModel;

using log4net;


namespace ThreeByte.Network
{

    public class NetworkServerManager : IDisposable, INotifyPropertyChanged
    {
        private readonly ILog log = LogManager.GetLogger(typeof(NetworkServerManager));

        //Observable Interface
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #region Public Properties
        public int Port { get; private set; }

        public ObservableCollection<TcpClient> CurrentClients {
            get;
            private set;
        }
        private object _clientLock = new object(); //Ensures serialized modification to CurrentClients collection

        private Exception _error;
        public Exception Error {
            get {
                return _error;
            }
            private set {
                Exception oldError = _error;
                _error = value;
                if(oldError != _error) {
                    NotifyPropertyChanged("Error");
                }
            }
        }
        #endregion //Public Properties

        private TcpListener _tcpListener;
        private object _serverLock = new object();

        private IAsyncResult _acceptResult = null;


        public NetworkServerManager(int port) {
            Port = port;
            CurrentClients = new ObservableCollection<TcpClient>();

            _tcpListener = new TcpListener(IPAddress.Any, Port);
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

            Stop();
        }

        public event EventHandler<TcpClientEventArgs> ClientConnected;
        public event EventHandler<TcpClientEventArgs> ClientPurged;

        public void Start() {
            log.Debug("Listener Start: " + Port);
            _stopped = false;
            try {
                _tcpListener.Start();
            } catch(Exception ex) {
                log.Error("Error starting listener", ex);
                Error = ex;
            }
            _acceptResult = _tcpListener.BeginAcceptTcpClient(AcceptCallback, _tcpListener);
        }

        private void AcceptCallback(IAsyncResult asyncResult) {
            log.Debug("Accept Callback");
            try {
                TcpListener listener = (TcpListener)(asyncResult.AsyncState);
                TcpClient newClient = listener.EndAcceptTcpClient(asyncResult);
                lock(_clientLock) {
                    CurrentClients.Add(newClient);
                }

                if(ClientConnected != null) {
                    ClientConnected(this, new TcpClientEventArgs(newClient));
                }
            } catch(Exception ex) {
                log.Error("Error accepting client", ex);
                Error = ex;
            }

            if(!_stopped) {
                _acceptResult = _tcpListener.BeginAcceptTcpClient(AcceptCallback, _tcpListener);
            }
        }


        private bool _stopped = true;
        public void Stop() {
            _stopped = true;
            try {
                //if(_acceptResult != null) {
                //    _tcpListener.EndAcceptTcpClient(_acceptResult);
                //}
                //_acceptResult = null;

                _tcpListener.Stop();
                lock(_clientLock) {
                    foreach(TcpClient c in CurrentClients) {
                        c.Client.Close();
                    }
                    CurrentClients.Clear();
                }
            } catch(Exception ex) {
                log.Error("Error stopping listener", ex);
                Error = ex;
            }
        }

        public void PurgeDisconnectedClients() {
            Collection<TcpClient> clientsToRemove = new Collection<TcpClient>();

            lock(_clientLock) {
                foreach(TcpClient c in CurrentClients) {
                    if(!(c.Connected)) {
                        clientsToRemove.Add(c);
                    }
                }

                foreach(TcpClient c in clientsToRemove) {
                    CurrentClients.Remove(c);

                }
            }
            foreach(TcpClient c in clientsToRemove) {
                if(ClientPurged != null) {
                    ClientPurged(this, new TcpClientEventArgs(c));
                }
            }
        }

        public List<TcpClient> GetCurrentClientsList() {
            lock(_clientLock) {
                return CurrentClients.ToList();
            }
        }

    }

    public class TcpClientEventArgs : EventArgs
    {
        public TcpClient Client { get; private set; }

        public TcpClientEventArgs(TcpClient client) {
            Client = client;
        }
    }
}
