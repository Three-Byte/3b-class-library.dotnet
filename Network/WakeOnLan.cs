﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Globalization;
using System.Net.NetworkInformation;
using log4net;

namespace ThreeByte.Network
{
    /// <summary>
    /// Based primarily on source from:
    /// http://www.codeproject.com/KB/IP/cswol.aspx
    /// </summary>
    public class WakeOnLan : UdpClient
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(WakeOnLan));

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
            BroadcastAddress = IPAddress.Broadcast;
        }

        public IPAddress BroadcastAddress {
            get;
            set;
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
                        if(ua.Address.AddressFamily != AddressFamily.InterNetwork
                            || ua.Address.Equals(IPAddress.Loopback)) {  //IPv4 but not loopback
                            continue;
                        }

                        try {
                            this.Client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                            this.Client.Bind(new IPEndPoint(ua.Address, 10001));
                            this.EnableBroadcast = true;
                            this.Send(packet, byteCount, new IPEndPoint(BroadcastAddress, 10001));
                            this.Client.Close();
                        } catch(Exception ex) {
                            log.Error("Error broadcasting packet", ex);
                        }
                    }

                }
            }
        }

    }
}
