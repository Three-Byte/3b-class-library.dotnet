using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace ThreeByte.Network
{
    public class ShutdownTrigger
    {
    
        private UdpClient _sender;
        private IPEndPoint _target;

        public ShutdownTrigger(string ipAddress){
            _target = new IPEndPoint(IPAddress.Parse(ipAddress), 16009);
            _sender = new UdpClient();
        }

        public void Shutdown(){
            byte[] cmdBytes = Encoding.ASCII.GetBytes("SHUTDOWN\r\n");
            _sender.Send(cmdBytes, cmdBytes.Length, _target);
        }

        public void Restart() {
            byte[] cmdBytes = Encoding.ASCII.GetBytes("RESTART\r\n");
            _sender.Send(cmdBytes, cmdBytes.Length, _target);
        }
    }
}
