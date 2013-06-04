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
                byte[] data = _link.GetData();
                //Leaving these in could cause a threaded memory error when running in a web service.
                //Rare and weird, but oh well:
                //http://stackoverflow.com/questions/12638810/nhibernate-race-condition-when-loading-entity
                //Console.WriteLine("Response: {0}", printBytes(data));
                //Console.WriteLine("String: {0}", Encoding.ASCII.GetString(data));
                ParseResponse(data);
            }
        }

       

        private string printBytes(byte[] data) {
            StringBuilder sb = new StringBuilder();
            foreach(byte b in data) {
                sb.AppendFormat("{0:X2} ", b);
            }
            return sb.ToString();
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

        private bool _isPowerOn;
        public bool IsPowerOn {
            get {
                return _isPowerOn;
            }
            set {
                _isPowerOn = value;
                NotifyPropertyChanged("IsPowerOn");
            }
        }


        public void PowerOn() {
            for(int i = 0; i < _idsToMonitor.Length; ++i) {
                if(_idsToMonitor[i]) {
                    PowerOn((byte)i);
                }
            }
        }

        public void PowerOn(byte id) {
            _link.SendData(PowerOnMessage(id));
        }

        public void PowerOff() {
            for(int i = 0; i < _idsToMonitor.Length; ++i) {
                if(_idsToMonitor[i]) {
                    PowerOff((byte)i);
                }
            }
        }

        public void PowerOff(byte id) {
            _link.SendData(PowerOffMessage(id));
        }

        public void QueryPower(byte id){
            _link.SendData(QueryPowerMessage(id));
        }

        public void QueryPower() {
            for(int i = 0; i < _idsToMonitor.Length; ++i) {
                if(_idsToMonitor[i]) {
                    QueryPower((byte)i);
                }
            }

        }

        private void ParseResponse(byte[] data) {

            if(data.Length < 4) {
                return;//Nothing worth inspecting here
            }

            byte[] dataSub = new byte[data.Length - 4];
            Array.Copy(data, 3, dataSub, 0, dataSub.Length);

            { //Check power on
                byte[] onResponse = PowerOnResponse((byte)'*');
                byte[] onResponseSub = new byte[onResponse.Length - 4];
                Array.Copy(onResponse, 3, onResponseSub, 0, onResponseSub.Length);

                if(onResponseSub.SequenceEqual(dataSub)) {
                    IsPowerOn = true;
                }
            }

            { //Check power off
                byte[] offResponse = PowerOffResponse((byte)'*');
                byte[] offResponseSub = new byte[offResponse.Length - 4];
                Array.Copy(offResponse, 3, offResponseSub, 0, offResponseSub.Length);

                if(offResponseSub.SequenceEqual(dataSub)) {
                    IsPowerOn = false;
                }
            }


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
