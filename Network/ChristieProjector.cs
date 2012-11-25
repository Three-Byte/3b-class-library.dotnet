using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ThreeByte.Network;

namespace ThreeByte.Network
{
    public class ChristieProjector : IDisposable, INotifyPropertyChanged
    {
        //Default port is 10000
        private static readonly int TCP_PORT = 10000;

        #region Public Properties
        //Observable Interface
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion Public Properties

        private AsyncNetworkLink _link;

        public ChristieProjector(string ipAddress) {
            _link = new AsyncNetworkLink(ipAddress, TCP_PORT);
            _link.DataReceived += new EventHandler(_link_DataReceived);

        }

        void _link_DataReceived(object sender, EventArgs e) {
            //See if this is the Password Prompt
            while(_link.HasData) {
                byte[] message = _link.GetMessage();
                Console.WriteLine("Got Message: {0}", Encoding.ASCII.GetString(message));
                if(IsPasswordPrompt(message)) {
                    SendPasword();
                }
            }
        }


        private bool _disposed = false;
        public void Dispose() {
            if(_disposed) {
                throw new ObjectDisposedException("ChristieProjector");
            }
            _disposed = true;
            _link.DataReceived -= _link_DataReceived;
            _link.Dispose();
        }

        public void Power(bool state) {
            if(state) {
                //Power On
                byte[] data = new byte[] { (byte)'C', (byte)'0', (byte)'0', 0x0D, 0x0A };
                _link.SendMessage(data);
            } else {
                //Power Off
                byte[] data = new byte[] { (byte)'C', (byte)'0', (byte)'1', 0x0D, 0x0A };
                _link.SendMessage(data);
            }
        }

        public bool IsPasswordPrompt(byte[] message) {
            return Encoding.ASCII.GetString(message).Contains("PASSWORD");
        }

        public void SendPasword() {
            //Password: 0000
            Console.WriteLine("Sending Password");
            byte[] data = new byte[] { (byte)'0', (byte)'0', (byte)'0', (byte)'0', 0x0D, 0x0A };
            _link.SendMessage(data);
        }

    }
}
