using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ThreeByte.Network;
using log4net;
using System.Threading;

namespace ThreeByte.Network
{
    public class ChristieLX700 : IDisposable, INotifyPropertyChanged
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ChristieLX700));

        #region Public Properties
        //Observable Interface
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        /// <summary>
        /// Used to uniquely identify this device instance
        /// </summary>
        public string Name { get; private set; }

        public bool IsPowerOn { get; private set; }

        #endregion Public Properties

        private AsyncNetworkLink _link;
        private Timer _pollTimer;
        private static readonly TimeSpan POLL_INTERVAL = TimeSpan.FromSeconds(6);
        private DateTime lastResponseTime = DateTime.MinValue;
        private static readonly TimeSpan RESPONSE_TIMEOUT = TimeSpan.FromSeconds(60);

        public ChristieLX700(string name, string ipAddress, int port) {
            Name = name;
            _link = new AsyncNetworkLink(ipAddress, port);
            _link.DataReceived += new EventHandler(_link_DataReceived);

            _pollTimer = new Timer(PollTimerCallback);
            _pollTimer.Change(POLL_INTERVAL, POLL_INTERVAL);
        }

        void _link_DataReceived(object sender, EventArgs e) {
            while(_link.HasData) {
                byte[] data = _link.GetMessage();
                log.InfoFormat("Data Received: {0}", printBytes(data));
                string message = Encoding.ASCII.GetString(data);
                ParseMessage(message);
            }
        }

        private enum ParseState { None, Power };
        private ParseState parseState;
        private void ParseMessage(string message) {
            switch(parseState) {
                case ParseState.Power:
                    if(message.StartsWith("00")) {
                        IsPowerOn = true;
                    } else {
                        IsPowerOn = false;
                        // Some other power state (consider this off
                    }
                    parseState = ParseState.None;
                    lastResponseTime = DateTime.Now;
                    break;
                case ParseState.None:
                default:
                    // Received an unknown message - ignore
                    break;
            }
        }

        private string printBytes(byte[] data) {
            StringBuilder sb = new StringBuilder();
            foreach(byte b in data) {
                sb.AppendFormat("{0:X2},", b);
            }
            return sb.ToString();
        }

        private bool _disposed = false;
        public void Dispose() {
            if(_disposed) {
                throw new ObjectDisposedException("ChistieLX700");
            }
            _disposed = true;
            _link.DataReceived -= _link_DataReceived;
            _link.Dispose();
        }

        private void PollTimerCallback(object state) {
            try {
                PollPower();
                // TODO: Wait for parse state to be None before continuing if you want to poll for sometihng else

                if(lastResponseTime < (DateTime.Now - RESPONSE_TIMEOUT)) {
                    // If we haven't heard from the projector since the timeot, we can't assert that the projector is on
                    IsPowerOn = false;
                }
            } catch(Exception ex) {
                log.Error("Error polling projector", ex);
            }
        }

        private void PollPower() {
            parseState = ParseState.Power;
            string message = "CR0\r\n"; // Read Status
            _link.SendMessage(Encoding.ASCII.GetBytes(message));
        }



        public void Power(bool state) {
            if(state) {
                string message = "C00\r\n";
                _link.SendMessage(Encoding.ASCII.GetBytes(message));
            } else {
                string message = "C01\r\n";
                _link.SendMessage(Encoding.ASCII.GetBytes(message));
            }
        }

        public void VideoMute(bool state) {
            if(state) {
                string message = "C0D\r\n";
                _link.SendMessage(Encoding.ASCII.GetBytes(message));
            } else {
                string message = "C0E\r\n";
                _link.SendMessage(Encoding.ASCII.GetBytes(message));
            }
        }

        /// <summary>
        /// Bumps the D. ZOOM up button on the remote if increment is true, bumps down (decrement) otherwise
        /// </summary>
        /// <param name="increment">UP if true, DOWN otherwise</param>
        public void DigitalZoom(bool increment) {
            if(increment) {
                string message = "C30\r\n";
                _link.SendMessage(Encoding.ASCII.GetBytes(message));
            } else {
                string message = "C31\r\n";
                _link.SendMessage(Encoding.ASCII.GetBytes(message));
            }
        }

    }
}
