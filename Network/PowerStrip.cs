using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ThreeByte.Network;
using System.Net.Sockets;

namespace ThreeByte.Network
{
    public class PowerStrip : IDisposable, INotifyPropertyChanged
    {
        //Telnet: port is 23
        private static readonly int TCP_PORT = 23;

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
        public PowerStrip(string ipAddress) {
            try {
                _client = new TcpClient();
                _client.Connect(ipAddress, TCP_PORT);
            } catch(Exception ex) {

            }
        }

        private bool _disposed = false;
        public void Dispose() {
            if(_disposed) {
                throw new ObjectDisposedException("PowerStrip");
            }
            _disposed = true;
            if(_client != null) {
                if(_client.Client != null) {
                    _client.Client.Close();
                }
                _client.Close();
            }
            _client = null;
        }

        public void Power(int outlet, bool state) {
            string message = string.Format("pset {0} {1}\r\n", outlet, (state ? "1" : "0"));
            byte[] data = Encoding.ASCII.GetBytes(message);
            _client.GetStream().Write(data, 0, data.Length);
        }
        

    }
}
