using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ThreeByte.Network;

namespace ThreeByte.Network
{
    public class KissBox : IDisposable, INotifyPropertyChanged
    {
        //Default port is 9812
        private static readonly int TCP_PORT = 9812;

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
        
        public KissBox(string ipAddress) {
            _link = new AsyncNetworkLink(ipAddress, TCP_PORT);
            _link.DataReceived += new EventHandler(_link_DataReceived);

        }

        void _link_DataReceived(object sender, EventArgs e) {
            
        }


        private bool _disposed = false;
        public void Dispose() {
            if(_disposed) {
                throw new ObjectDisposedException("KissBox");
            }
            _disposed = true;
            _link.DataReceived -= _link_DataReceived;
            _link.Dispose();
        }

        public void SetRelay(int slot, int channel, bool state){
            byte slotByte = (byte)slot;
            byte channelByte = (byte)channel;
            byte stateByte = (state ? (byte)0x01 : (byte)0x00);
            byte[] message = new byte[] { 0xA5, slotByte, channelByte, stateByte };
            _link.SendMessage(message);
        }

        public bool GetRelay(int slot, int channel) {
            byte slotByte = (byte)slot;
            byte channelByte = (byte)channel;
            byte[] message = new byte[] { 0xA2, slotByte, channelByte };
            _link.SendMessage(message);

            //Wait for the response
            //TODO: Implement this
            return false;
        }

    }
}
