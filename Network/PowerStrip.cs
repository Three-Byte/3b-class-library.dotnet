using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ThreeByte.Network;
using System.Net.Sockets;
using System.Net;

namespace ThreeByte.Network
{
    public class PowerStrip : IDisposable, INotifyPropertyChanged
    {
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
            WebClient c = new WebClient();
            c.Credentials = new NetworkCredential("admin", "admin");
            string commandUri = string.Format("http://{0}/cmd.cgi?$A3 {1} {2}", _ipAddress, outlet, (state ? 1 : 0));
            string response = c.DownloadString(commandUri);
            Console.WriteLine("Response: {0}", response);
        }
        

    }
}
