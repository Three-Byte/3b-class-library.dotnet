using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ThreeByte.Network;
using log4net;
using System.Text.RegularExpressions;

namespace ThreeByte.Network.Devices
{
    public class ExtronDVS : IDisposable, INotifyPropertyChanged
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ExtronDVS));

        #region Public Properties
        //Observable Interface
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion Public Properties

        //Default port for network enabled Extron devices
        //private static readonly int TCP_PORT = 23;

        private AsyncNetworkLink _link;

        public ExtronDVS(string ipAddress, int port = 23) {
            _link = new AsyncNetworkLink(ipAddress, port);
            _link.DataReceived += new EventHandler(_link_DataReceived);
        }

        void _link_DataReceived(object sender, EventArgs e) {
            while(_link.HasData) {
                byte[] data = _link.GetMessage();
                string message = Encoding.ASCII.GetString(data);
                log.InfoFormat("Data Received: {0}", message);
                ParseMessage(message);
            }
        }

        private void ParseMessage(string message) {
            try {
                Regex inputPattern = new Regex(@"In(\d+)");

                Match m = inputPattern.Match(message);
                if(m.Success) {
                    CurrentInput = int.Parse(m.Groups[1].Value);
                }
            } catch(Exception ex) {
                log.Warn("Error parsing message", ex);
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
                throw new ObjectDisposedException("ExtronDVS");
            }
            _disposed = true;
            _link.DataReceived -= _link_DataReceived;
            _link.Dispose();
        }

        public int CurrentInput{ get; private set; }

        public void Input(int input){
            string message = string.Format("{0}!\r\n", input);
            _link.SendMessage(Encoding.ASCII.GetBytes(message));
        }

    }
}
