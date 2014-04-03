using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ThreeByte.Network;
using System.Net.Sockets;
using System.Net;
using log4net;

namespace ThreeByte.Network
{
    public class PowerStrip : INotifyPropertyChanged
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PowerStrip));

        #region Public Properties
        //Observable Interface
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion Public Properties

        private string _ipAddress;
        public PowerStrip(string ipAddress) {
            _ipAddress = ipAddress;
        }

        private Dictionary<int, bool> _powerStates = new Dictionary<int, bool>();
        public bool this[int port] {
            get {
                if(_powerStates.ContainsKey(port)) {
                    return _powerStates[port];
                }
                return false; // report false for all other ports that don't exist
            }
        }

        public void Power(int outlet, bool state) {
            try {
                WebClient c = new WebClient();
                c.Credentials = new NetworkCredential("admin", "admin");
                string commandUri = string.Format("http://{0}/cmd.cgi?$A3 {1} {2}", _ipAddress, outlet, (state ? 1 : 0));
                string response = c.DownloadString(commandUri);
                //log.DebugFormat("Response: {0}", response);
            } catch(Exception ex) {
                log.Error(string.Format("Error setting power state: {0} Outlet {1} [{2}]", _ipAddress, outlet, state), ex);
            }
        }

        public void PollState() {
            try {
                WebClient c = new WebClient();
                c.Credentials = new NetworkCredential("admin", "admin");
                string commandUri = string.Format("http://{0}/cmd.cgi?$A5", _ipAddress);
                string response = c.DownloadString(commandUri);

                log.DebugFormat("Response: {0}", response);

                // Expected response: xxxx,cccc,tttt
                // read right to left for each field, eg - 01 means port 1 is on
                char[] powerStates = response.Split(',')[0].ToCharArray();
                for(int i = 0; i < powerStates.Length; i++) {
                    char powerBit = powerStates[powerStates.Length - 1 - i];
                    if(powerBit == '1') {
                        _powerStates[i + 1] = true;
                    } else if(powerBit == '0') {
                        _powerStates[i + 1] = false;
                    }
                }

            } catch(Exception ex) {
                log.Error(string.Format("Error getting power state: {0}", _ipAddress), ex);
            }
        }
    }
}
