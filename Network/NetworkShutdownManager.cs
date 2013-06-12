using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using log4net;

namespace ThreeByte.Network
{
    public class NetworkShutdownManager
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NetworkShutdownManager));

        public static readonly int UDP_LISTEN_PORT = 16009;

        public NetworkShutdownManager() {
            ThreadPool.QueueUserWorkItem(ListenLoop);
        }

        private void ListenLoop(object state){
            //Open a UDP listener on port 16009
            UdpClient udpClient = new UdpClient(UDP_LISTEN_PORT);

            bool listening = true;
            IPEndPoint remoteHost = new IPEndPoint(IPAddress.Any, 0);
            byte[] dataBytes;

            log.InfoFormat("UDP Listener started on port {0}", UDP_LISTEN_PORT);

            while(listening) {
                log.InfoFormat("Listening: {0} / {1}", remoteHost.Address.ToString(), remoteHost.Port);
                dataBytes = udpClient.Receive(ref remoteHost);
                log.DebugFormat("Received {0} Bytes: [{1}]", dataBytes.Length, Encoding.ASCII.GetString(dataBytes));
                //Incoming commands must be received as a single packet.
                string stringIn = Encoding.ASCII.GetString(dataBytes).ToUpper();
                log.DebugFormat("StringIn = [{0}]", stringIn);

                //Parse messages separated by cr
                int delimPos = stringIn.IndexOf("\n");
                while(delimPos >= 0) {
                    string message = stringIn.Substring(0, delimPos + 1).Trim();
                    stringIn = stringIn.Remove(0, delimPos + 1);  //remove the message
                    delimPos = stringIn.IndexOf("\n");

                    log.DebugFormat("Message: {0}", message);

                    if(message == "EXIT") {
                        listening = false;
                    } else if(message == "PING") {
                        string responseString = "PONG";
                        byte[] sendBytes = Encoding.ASCII.GetBytes(responseString);
                        udpClient.Send(sendBytes, sendBytes.Length, remoteHost);
                    } else if(message == "APPRESTART") {
                        log.Info("Restarting Application");
                        RaiseAppRestart();
                    } else if(message == "REBOOT" || message == "RESTART") {
                        log.Info("Rebooting now");
                        //listening = false;
                        System.Diagnostics.Process.Start("shutdown", "/r /f /t 3 /c \"Reboot Triggered\" /d p:0:0");
                    } else if(message == "SHUTDOWN") {
                        log.Info("Shutting down now");
                        System.Diagnostics.Process.Start("shutdown", "/s /f /t 3 /c \"Shutdown Triggered\" /d p:0:0");
                    }
                }
            }
        }

        public event EventHandler AppRestart;

        private void RaiseAppRestart() {
            if(AppRestart != null) {
                AppRestart(this, EventArgs.Empty);
            }
        }
    }
}
