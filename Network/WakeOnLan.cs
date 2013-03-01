using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Globalization;
using System.Net.NetworkInformation;


namespace ThreeByte.Network
{
    /// <summary>
    /// Based primarily on source from:
    /// http://www.codeproject.com/KB/IP/cswol.aspx
    /// </summary>
    public class WakeOnLan : UdpClient
    {
        private string _macAddress;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="MACAddress">In the format "001F2903AF4B"</param>
        public WakeOnLan(string MACAddress) : base() {
            if(MACAddress == null || MACAddress.Length != 12) {
                throw new ArgumentNullException("MAC Address must be specified as 12 hex characters");
            }
            _macAddress = MACAddress;
        }

        public void Wake() {
            int counter = 0;
            //buffer to be send
            byte[] bytes = new byte[1024];   // more than enough :-)

            //first 6 bytes should be 0xFF
            for(int y = 0; y < 6; y++)
                bytes[counter++] = 0xFF;
            
            //now repeat MAC 16 times
            for(int y = 0; y < 16; y++) {
                int i = 0;
                for(int z = 0; z < 6; z++) {
                    bytes[counter++] =
                        byte.Parse(_macAddress.Substring(i, 2),
                        NumberStyles.HexNumber);
                    i += 2;
                }
            }

            //now send wake up packet
            BroadcastAllAdapters(bytes, counter);
        }

        private void BroadcastAllAdapters(byte[] packet, int byteCount) {
            foreach(NetworkInterface i in NetworkInterface.GetAllNetworkInterfaces()) {
                if(i.OperationalStatus == OperationalStatus.Up) {
                    foreach(UnicastIPAddressInformation ua in i.GetIPProperties().UnicastAddresses) {
                        Console.WriteLine(ua.Address.AddressFamily);
                        if(ua.Address.AddressFamily == AddressFamily.InterNetwork && !ua.Address.Equals(IPAddress.Loopback)) {  //IPv4 but not loopback
                            this.Connect(ua.Address, 10001);
                            if(this.Active) {
                                this.Client.SetSocketOption(SocketOptionLevel.Socket,
                                                          SocketOptionName.Broadcast, 0);
                            } else {
                                throw new ArgumentNullException("The UDP Client is not active");
                            }
                            this.Send(packet, byteCount);
                        }

                    }


                }
            }
        }

    }
}
