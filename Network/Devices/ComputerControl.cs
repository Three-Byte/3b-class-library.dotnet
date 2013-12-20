using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using log4net;

namespace ThreeByte.Network.Devices {
    public class ComputerControl : INotifyPropertyChanged {
        private static readonly ILog log = LogManager.GetLogger(typeof(ComputerControl));
        
        public string Host { get; private set; }
        public string MacAddress { get; private set; }

        private bool _online = false;
        public bool Online {
            get {
                return _online;
            }
            private set {
                if(_online != value) {
                    _online = value;
                    OnPropertyChanged("Online");
                }
            }
        }

        private static readonly TimeSpan PING_INTERVAL = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan NEVER = TimeSpan.FromMilliseconds(-1);
        private static readonly TimeSpan PING_TIMEOUT = TimeSpan.FromSeconds(10);

        private AsyncUdpLink _sender;
        private WakeOnLan _wakeOnLan;

        public ComputerControl(string host, string macAddress, string broadcastWakeAddress = null) {
            Host = host;
            MacAddress = macAddress;
            //This port = 0 means that the next available port number will be assigned
            _sender = new AsyncUdpLink(host, NetworkShutdownManager.UDP_LISTEN_PORT);
            _sender.DataReceived += _sender_DataReceived;
            _wakeOnLan = new WakeOnLan(MacAddress);
            if(!string.IsNullOrWhiteSpace(broadcastWakeAddress)) {
                _wakeOnLan.BroadcastAddress = IPAddress.Parse(broadcastWakeAddress);
            }
            _pingTimer = new Timer(timerCallback);
            _pingTimer.Change(PING_INTERVAL, NEVER);
        }

        private Timer _pingTimer;
        private DateTime _lastAckTimestamp = DateTime.Now;

        //Currently we are pinging on a 2 second timer and if 10 seconds go by without a response
        //we set status to offline
        private void timerCallback(object state) {
            Ping();
            if(DateTime.Now > _lastAckTimestamp + PING_TIMEOUT) {
                Online = false;
            } else {
                Online = true;
            }
            _pingTimer.Change(PING_INTERVAL, NEVER);
        }

        private void _sender_DataReceived(object sender, EventArgs e) {
            while(_sender.HasData) {
                string response = Encoding.ASCII.GetString(_sender.GetMessage());
                if(response.Trim() == "PONG") {
                    Online = true;
                }
            }            
        }

        public void Startup() {
            log.InfoFormat("Wake Computer: {0}", MacAddress);
            _wakeOnLan.Wake();
        }

        public void Shutdown() {
            log.InfoFormat("Shutdown Computer: {0}", Host);
            byte[] cmdBytes = Encoding.ASCII.GetBytes("SHUTDOWN\r\n");
            _sender.SendMessage(cmdBytes);
        }

        public void Restart() {
            byte[] cmdBytes = Encoding.ASCII.GetBytes("RESTART\r\n");
            _sender.SendMessage(cmdBytes);
        }

        public void Ping() {
            try {
                System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
                PingReply pingReply = ping.Send(Host);
                if(pingReply.Status == IPStatus.Success) {
                    Online = true;
                    _lastAckTimestamp = DateTime.Now;
                } else {
                    Online = false;
                }
            } catch {
                //Swallow Ping exceptions - it just means the computer cannot be verified
            }
        }

        /// <summary>
        /// Static utility convenience method to send a single ping request to the specified hostname.
        /// This method returns false if an exception is raised for any reason.
        /// </summary>
        /// <param name="hostname">the hostname of the device to ping</param>
        /// <returns>true if a ping response is received, false otherwise.</returns>
        public static bool Ping(string hostname) {
            try {
                System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
                PingReply pingReply = ping.Send(hostname);
                return (pingReply.Status == IPStatus.Success);
            } catch {
                return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string info) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if(handler != null) {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
