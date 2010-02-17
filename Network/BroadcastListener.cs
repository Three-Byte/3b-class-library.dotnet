using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;
using log4net;

namespace ThreeByte.Network
{
    public class BroadcastListener : INotifyPropertyChanged {
        private readonly ILog log = LogManager.GetLogger(typeof(BroadcastListener));

        private IPEndPoint RECEIVE_ENDPOINT = new IPEndPoint(IPAddress.Any, 16006);

        private bool _running = true;
        private IAsyncResult _lastResult = null;


        public int TotalFrameCount { get; private set; }
        public string TimeString { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        //Allow the constructor to throw an exception if something goes wrong here
        public BroadcastListener()
        {
            UdpClient client = new UdpClient(RECEIVE_ENDPOINT.Port, AddressFamily.InterNetwork);
            _lastResult = client.BeginReceive(ReceiveMessage, client);
        
        }

        //TODO: Fix the way the last asynchronous thing is cleaned up
        public void Close() {
            _running = false;
            if(_lastResult != null) {
                log.Debug("Ending last receive");
                UdpClient client = (UdpClient)_lastResult.AsyncState;
                client.EndReceive(_lastResult, ref RECEIVE_ENDPOINT); 
            }
            
        }

        public void ReceiveMessage(IAsyncResult asyncResult) {
            //log.Debug("Got some data: ");

            UdpClient client = (UdpClient)asyncResult.AsyncState;

            try {
                Byte[] receivebytes = client.EndReceive(asyncResult, ref RECEIVE_ENDPOINT);
                ProcessBytes(receivebytes);
                //log.Debug("The Bytes are: " + Encoding.ASCII.GetString(receivebytes));
            } catch(Exception ex) {
                log.Error("Receive Error", ex);
            }

            _lastResult = null;

            if(_running) {
                _lastResult = client.BeginReceive(ReceiveMessage, client);
            }
        }

        private void ProcessBytes(Byte[] data) {
            //log.Debug("Looking at: " + data.Length + " bytes");

            try {
                //Hours
                int h = BitConverter.ToInt32(data, 0);

                //Minutes
                int m = BitConverter.ToInt32(data, 4);

                //Seconds
                int s = BitConverter.ToInt32(data, 8);

                //Frames
                int f = BitConverter.ToInt32(data, 12);

                TimeString = string.Format("{0}:{1}:{2}/{3}", h, m, s, f);
                NotifyPropertyChanged("TimeString");

                //Total Frame Count
                TotalFrameCount = BitConverter.ToInt32(data, 16);
                NotifyPropertyChanged("TotalFrameCount");
            } catch(Exception ex) {
                log.Error("Cannot Parse the heartbeat data", ex);
            }
        }
    
    }
}
