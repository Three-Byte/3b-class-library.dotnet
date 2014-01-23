using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeByte.Network;
using log4net;
using System.Threading;

namespace ThreeByte.Network.Devices
{
    public class BlackMagicVideoRouter : INotifyPropertyChanged
    {

        #region Implements INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private static readonly ILog log = LogManager.GetLogger(typeof(BlackMagicVideoRouter));
        private static int POINT_COUNT;
        private static readonly int TCP_PORT = 9990;
        private static readonly TimeSpan NEVER = TimeSpan.FromMilliseconds(-1);
        private static readonly TimeSpan PING_INTERVAL = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan PING_TIMEOUT = TimeSpan.FromSeconds(10);

        private static readonly string VIDEO_OUTPUT_ROUTING = "VIDEO OUTPUT ROUTING:\n";
        private static readonly string ACK = "ACK";

        private FramedNetworkLink _link;
        private Timer _pingTimer;
        private DateTime _lastAckTimestamp = DateTime.Now;

        public BlackMagicVideoRouter(string ipAddress, int pointCount = 16) {
            POINT_COUNT = pointCount;
            _link = new FramedNetworkLink(ipAddress, TCP_PORT) { ReceiveFrame = new NetworkFrame() { Footer = new byte[] { 0x0A, 0x0A } } };
            _link.DataReceived += _link_DataReceived;

            _pingTimer = new Timer(PingTimerCallback);
            _pingTimer.Change(PING_INTERVAL, NEVER);
        }

        void _link_DataReceived(object sender, EventArgs e) {
            
            while(_link.HasData){
                string message = _link.GetMessage();
                //log.InfoFormat("MESSAGE: {0}", message);
                ParseMessage(message);
            }
        }

        void PingTimerCallback(object state) {
            Ping();
            if(DateTime.Now > _lastAckTimestamp + PING_TIMEOUT) {
                Connected = false;
            }
            _pingTimer.Change(PING_INTERVAL, NEVER);
        }

        private bool _connected;
        public bool Connected {
            get {
                return _connected;
            }
            set {
                if(_connected != value) {
                    _connected = value;
                    NotifyPropertyChanged("Connected");
                    if(_connected) {
                        //If we just connected, get the current status
                        GetVideoStatus();
                    }
                }
            }
        }

        private int[] _output = new int[POINT_COUNT];
        public int[] Output {
            get {
                return _output;
            }
            private set {
                _output = value;
                NotifyPropertyChanged("Output");
                NotifyPropertyChanged("OutputString");
            }
        }

        public string OutputString {
            get {
                int[] output = Output;
                if(output == null) {
                    return string.Empty;
                }
                StringBuilder sb = new StringBuilder();
                for(int i = 0; i < POINT_COUNT; ++i) {
                    sb.AppendFormat("  {0:00}", i);
                }
                sb.AppendLine();
                for(int i = 0; i < POINT_COUNT; ++i) {
                    sb.AppendFormat("  {0:00}", output[i]);
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points">input output pairs, ex: in1, out0, in2, out1</param>
        public void SetOutput(params int[] points) {
            if((points.Length % 2) != 0) {
                throw new ArgumentException("Output config must be specified as pairs of integers: <IN1, OUT1, IN2, OUT2>", "points");
            }
            
            //Construct the right command
            StringBuilder cmd = new StringBuilder();
            cmd.Append(VIDEO_OUTPUT_ROUTING);
            for(int i = 0; i < points.Length; i += 2) {
                cmd.AppendFormat("{0} {1}\n", points[i + 1], points[i]);
            }
            cmd.Append("\n");
            _link.SendMessage(cmd.ToString());

            GetVideoStatus();
        }

        private void ParseMessage(string message) {

            if(message == ACK) {
                _lastAckTimestamp = DateTime.Now;
                Connected = true;
            } else if(message.StartsWith(VIDEO_OUTPUT_ROUTING)) {
                //Parse the video status
                //log.Info("Parse video status");
                string[] pointConfig = message.Replace(VIDEO_OUTPUT_ROUTING, string.Empty).Split('\n');
                int[] newOutput = new int[POINT_COUNT];
                foreach(string point in pointConfig){
                    string[] inOut = point.Split(' ');
                    int outPoint = int.Parse(inOut[0]);
                    int inPoint = int.Parse(inOut[1]);
                    newOutput[outPoint] = inPoint;
                }
                Output = newOutput;
            }
        }

        private void Ping() {
            _link.SendMessage("PING:\n\n");
        }

        private void GetVideoStatus() {
            _link.SendMessage("VIDEO OUTPUT ROUTING:\n\n");
        }


    }
}
