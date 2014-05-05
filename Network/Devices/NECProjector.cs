using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ThreeByte.Network.Devices {
    public class NECProjector : INotifyPropertyChanged {

        private static readonly ILog log = LogManager.GetLogger(typeof(NECProjector));

        private FramedByteNetworkLink _link;
        //port = 7142
        public NECProjector(string ipAddress, int port) {
            _link = new FramedByteNetworkLink(ipAddress, port);
            _link.SendFrame = new NetworkFrame() { Header = new byte[] { 0x01 }, Footer = new byte[] { 0x0D } };
            _link.ReceiveFrame = _link.SendFrame;

            _link.DataReceived += _link_DataReceived;
        }

        private void _link_DataReceived(object sender, EventArgs e) {
            while(_link.HasData) {
                byte[] data = _link.GetData();
                //Leaving these in could cause a threaded memory error when running in a web service.
                //Rare and weird, but oh well:
                //http://stackoverflow.com/questions/12638810/nhibernate-race-condition-when-loading-entity
                //Console.WriteLine("Response: {0}", printBytes(data));
                //Console.WriteLine("String: {0}", Encoding.ASCII.GetString(data));
                ParseResponse(data);
            }
        }

        private void ParseResponse(byte[] data) {
            throw new NotImplementedException();
        }

        public void PowerOn(){
            _link.SendData(PowerOnMessage());
        }

        private byte[] PowerOnMessage() {
            byte[] cmd = new byte[] {
                0x02,//id1
                0x00,//id2
                0x00,
                0x00,
                0x00,
                0x02
            };

            return cmd;
        }

        public void PowerOff() {
            _link.SendData(PowerOffMessage());
        }

        private byte[] PowerOffMessage() {
            byte[] cmd = new byte[] {
                0x02,
                0x01,
                0x00,
                0x00,
                0x00,
                0x03
            };

            return cmd;
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
