using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ThreeByte.Network.Devices
{
    public class NECMonitor: INotifyPropertyChanged
    {

        #region Implements INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private FramedByteNetworkLink _link;

        public NECMonitor(string ipAddress, int port) {

            _link = new FramedByteNetworkLink(ipAddress, port);
            _link.SendFrame = new NetworkFrame() { Header = new byte[] { 0x01 }, Footer = new byte[] { 0x0D } };
            _link.ReceiveFrame = _link.SendFrame;

            _link.DataReceived += _link_DataReceived;
        }

        void _link_DataReceived(object sender, EventArgs e) {
            while(_link.HasData) {

            }
        }

        private bool[] _idsToMonitor = new bool[byte.MaxValue + 1];

        public void SetIDsToMonitor(params byte[] ids) {
            HashSet<byte> idSet = new HashSet<byte>(ids);
            for(int i = 0; i < _idsToMonitor.Length; ++i) {
                _idsToMonitor[i] = idSet.Contains((byte)i);
            }
        }

        private bool[] _status;
        public bool[] Status {
            get {
                return _status;
            }
            private set {
                _status = value;
                NotifyPropertyChanged("Status");
                NotifyPropertyChanged("StatusString");
            }
        }

        public string StatusString {
            get {
                bool[] status = Status;
                if(status == null) {
                    return string.Empty;
                }
                StringBuilder sb = new StringBuilder();
                for(int i = 0; i < status.Length; ++i){
                    if(_idsToMonitor[i]) {
                        sb.AppendFormat("  {0:00}", i);
                    }
                }
                sb.AppendLine();
                for(int i = 0; i < status.Length; ++i) {
                    if(_idsToMonitor[i]) {
                        sb.AppendFormat("  {0:00}", status[i]);
                    }
                }
                return sb.ToString();
            }
        }

        public void PowerOn() {
            for(byte i = 0; i < _idsToMonitor.Length; ++i) {
                if(_idsToMonitor[i]) {
                    PowerOn(i);
                }
            }
        }

        public void PowerOn(byte id) {
            _link.SendData(PowerOnMessage(id));
        }

        public void PowerOff() {
            for(byte i = 0; i < _idsToMonitor.Length; ++i) {
                if(_idsToMonitor[i]) {
                    PowerOff(i);
                }
            }
        }

        public void PowerOff(byte id) {
            _link.SendData(PowerOffMessage(id));
        }

        private static byte CalculateBlockCheckCode(byte[] message) {
            return CalculateBlockCheckCode(message, 0, message.Length - 1);
        }

        private static byte CalculateBlockCheckCode(byte[] message, int start, int end) {
            byte bcc = 0x00;
            for(int i = start; i <= end; i++) {
                bcc ^= message[i];
            }
            return bcc;
        }

        private static byte[] PowerOnMessage(byte id) {
            byte[] cmd = new byte[] {
            //0x01,
            (byte)'0',
            id,
            (byte)'0',
            (byte)'A',
            (byte)'0',
            (byte)'C',
            0x02,
            (byte)'C',
            (byte)'2',
            (byte)'0',
            (byte)'3',
            (byte)'D',
            (byte)'6',
            (byte)'0',
            (byte)'0',
            (byte)'0',
            (byte)'1',
            0x03,
            0x00//BCC,
            //0x0D
            };
            cmd[18] = CalculateBlockCheckCode(cmd);
            return cmd;
        }

        private static byte[] PowerOffMessage(byte id) {
            byte[] cmd = new byte[] {
            //0x01,
            (byte)'0',
            id,
            (byte)'0',
            (byte)'A',
            (byte)'0',
            (byte)'C',
            0x02,
            (byte)'C',
            (byte)'2',
            (byte)'0',
            (byte)'3',
            (byte)'D',
            (byte)'6',
            (byte)'0',
            (byte)'0',
            (byte)'0',
            (byte)'4',
            0x03,
            0x00//BCC,
            //0x0D
            };
            cmd[18] = CalculateBlockCheckCode(cmd);
            return cmd;
        }

        private static byte[] QueryPowerMessage(byte id) {
            byte[] cmd = new byte[] {
            //0x01,
            (byte)'0',
            id,
            (byte)'0',
            (byte)'A',
            (byte)'0',
            (byte)'6',
            0x02,
            (byte)'0',
            (byte)'1',
            (byte)'D',
            (byte)'6',
            0x03,
            0x00//BCC,
            //0x0D
            };
            cmd[12] = CalculateBlockCheckCode(cmd);
            return cmd;
        }

        private static byte[] PowerOnResponse(byte id) {
            byte[] cmd = new byte[] {
            //0x01,
            (byte)'0',
            id,
            (byte)'0',
            (byte)'B',
            (byte)'1',
            (byte)'2',
            0x02,
            (byte)'0',
            (byte)'2',
            (byte)'0',
            (byte)'0',
            (byte)'D',
            (byte)'6',
            (byte)'0',
            (byte)'0',
            (byte)'0',
            (byte)'0',
            (byte)'0',
            (byte)'4',
            (byte)'0',
            (byte)'0',
            (byte)'0',
            (byte)'1',
            0x03,
            0x00//BCC,
            //0x0D
            };
            cmd[24] = CalculateBlockCheckCode(cmd);
            return cmd;
        }

        private static byte[] PowerOffResponse(byte id) {
            byte[] cmd = new byte[] {
            //0x01,
            (byte)'0',
            id,
            (byte)'0',
            (byte)'B',
            (byte)'1',
            (byte)'2',
            0x02,
            (byte)'0',
            (byte)'2',
            (byte)'0',
            (byte)'0',
            (byte)'D',
            (byte)'6',
            (byte)'0',
            (byte)'0',
            (byte)'0',
            (byte)'0',
            (byte)'0',
            (byte)'4',
            (byte)'0',
            (byte)'0',
            (byte)'0',
            (byte)'4',
            0x03,
            0x00//BCC,
            //0x0D
            };
            cmd[24] = CalculateBlockCheckCode(cmd);
            return cmd;
        }

    }
}
