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
    public class PowerStrip : IDisposable, INotifyPropertyChanged
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

        private TcpClient _client;
        private string _ipAddress;
        public PowerStrip(string ipAddress) {
            _ipAddress = ipAddress;
        }

        private bool _disposed = false;
        public void Dispose() {
            if(_disposed) {
                throw new ObjectDisposedException("PowerStrip");
            }
            _disposed = true;
        }

        public void Power(int outlet, bool state) {
            try {
                WebClient c = new WebClient();
                c.Credentials = new NetworkCredential("admin", "admin");
                string commandUri = string.Format("http://{0}/cmd.cgi?$A3 {1} {2}", _ipAddress, outlet, (state ? 1 : 0));
                string response = c.DownloadString(commandUri);
                log.DebugFormat("Response: {0}", response);
            } catch(Exception ex) {
                log.Error("Error setting power state", ex);
            }
        }
        

    }
}
