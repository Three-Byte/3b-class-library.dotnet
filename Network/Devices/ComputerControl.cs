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

namespace ThreeByte.Network.Devices {
    public class ComputerControl : INotifyPropertyChanged {
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

        public ComputerControl(string host, string macAddress) {
            this.host = host;
            //This port = 0 means that the next available poert number will be assigned
            _sender = new AsyncUdpLink(host, NetworkShutdownManager.UDP_LISTEN_PORT);
            _sender.DataReceived += _sender_DataReceived;
            _wakeOnLan = new WakeOnLan(macAddress);
            _pingTimer = new Timer(timerCallback);
            _pingTimer.Change(PING_INTERVAL, NEVER);
        }

        private string host = null;
        private Timer _pingTimer;
        private DateTime _lastAckTimestamp = DateTime.Now;

        //Currently we are pinging on a 5 second timer and if 10 seconds go by without a response
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
            _wakeOnLan.Wake();
        }

        public void Shutdown() {
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
                PingReply pingReply = ping.Send(host);
                if(pingReply.Status == IPStatus.Success) {
                    Online = true;
                    _lastAckTimestamp = DateTime.Now;
                } else {
                    Online = false;
                }
            } catch (Exception ex){
                //Log this exception here.
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
