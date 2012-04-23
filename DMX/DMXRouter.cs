using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.ComponentModel;
using System.Diagnostics;

namespace ThreeByte.DMX {
    public class DMXRouter : INotifyPropertyChanged {

        private static readonly ILog log = LogManager.GetLogger(typeof(DMXRouter));

        private const int DMX_UNIVERSE_SIZE = 512;
        private const int TOTAL_DMX_CHANNELS = 4096;

        #region Public Properties
        //DMX Universes are numbered sequentially, (ie: dmxUniverse 0 has channels 0 - 511, 1 has channels 512 - 1023, etc)
        public List<DMXUniverse> DMXUniverses = new List<DMXUniverse>();
        #endregion Public Properties


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Constructor for DMXRouter
        /// </summary>
        public DMXRouter() {
            for(int i = 0; i < 8; i++) {
                DMXUniverses.Add(new DMXUniverse() { ID = i, NumberOfChannels = DMX_UNIVERSE_SIZE });
            }
        }

        /// <summary>
        /// Sets the channel/universe association
        /// </summary>
        /// <param name="universeID">DMX Universe ID</param>
        /// <param name="startingChannel">Universe Initial Channel Number</param>
        /// <param name="numberOfChannels">The number of channels that belong to the universe</param>
        public void SetUniverseChannels(int universeID, int startingChannel, int numberOfChannels = DMX_UNIVERSE_SIZE) {
            Debug.Assert(numberOfChannels <= DMX_UNIVERSE_SIZE, "Number of Channels is greater than 512");
            Debug.Assert(startingChannel <= TOTAL_DMX_CHANNELS, "Starting channel exceeds the maximum channel number");

            DMXUniverse dmxU = DMXUniverses.SingleOrDefault(d => d.ID == universeID);

            if(dmxU != null) {
                if(startingChannel + numberOfChannels > TOTAL_DMX_CHANNELS) {
                    throw new Exception("The number of channels being requested exceeds the maximum of " + TOTAL_DMX_CHANNELS + " channels");
                }

                foreach(DMXUniverse dmx in DMXUniverses.Where(d => d.ID != dmxU.ID)) {
                    int minChannel = dmx.DMXValues.Min(d => d.Key);
                    int maxChannel = minChannel + DMX_UNIVERSE_SIZE - 1;
                    if((dmxU.DMXValues.Min(d => d.Key) > minChannel && dmxU.DMXValues.Min(d => d.Key) < maxChannel) ||
                        (dmxU.DMXValues.Max(d => d.Key) > minChannel && dmxU.DMXValues.Max(d => d.Key) < maxChannel)){
                        throw new Exception("The channels requested are already in use.");
                    }

                    if(dmx.DMXValues.ContainsKey(startingChannel)) {
                        throw new Exception("The supplied starting channel already belongs to a DMX Universe");
                    }
                }

                dmxU.StartChannel = startingChannel;
                dmxU.NumberOfChannels = numberOfChannels;
            }
        }

        /// <summary>
        /// Sets dmx values for a given set of channels
        /// </summary>
        /// <param name="channelValues">Dictionary of dmx channel/value pairs</param>
        public void SetDMXValues(Dictionary<int, byte> channelValues){
            int minChannel = channelValues.Min(c => c.Key);
            int maxChannel = channelValues.Max(c => c.Key);

            foreach(DMXUniverse dmxU in DMXUniverses) {
                if(minChannel >= dmxU.StartChannel && maxChannel < dmxU.StartChannel + dmxU.NumberOfChannels) {
                    dmxU.DmxController.SetValues(channelValues);
                    dmxU.DMXValues = channelValues;
                }
            }
        }

        public Dictionary<int, byte> GetDMXValues(int universeID = -1) {
            Dictionary<int, byte> dmxValues = new Dictionary<int, byte>();
            if(universeID > -1) {
                // Get DMX Values for specific universe
                DMXUniverse dmxU = DMXUniverses.SingleOrDefault(d => d.ID == universeID);

                if(dmxU != null) {
                    dmxValues = dmxU.DMXValues;
                }
            } else {
                // Get all DMX Values from All Universes
                foreach(DMXUniverse dmxU in DMXUniverses) {
                    dmxValues.Union(dmxU.DMXValues);
                }

                dmxValues = dmxValues.Select( d => new { d.Key, d.Value }).ToDictionary( d => d.Key, d => d.Value );                    
            }

            return dmxValues;
        }
    }

    //Each DMX Universe contains 512 channels.    
    public class DMXUniverse {
        public int ID { get; set; }
        public int StartChannel { get; set; }
        public int NumberOfChannels { get; set; }
        public IDMXControl DmxController { get; set; }
        public Dictionary<int, byte> DMXValues { get; set; }
    }
}
