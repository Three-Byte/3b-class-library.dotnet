using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using log4net;

namespace ThreeByte.DMX {
    public class DMXRouter : INotifyPropertyChanged {

        private static readonly ILog log = LogManager.GetLogger(typeof(DMXRouter));

        private const int DMX_UNIVERSE_SIZE = 512;
        public static int TOTAL_DMX_CHANNELS = 4096;

        #region Public Properties
        //DMX Universes are numbered sequentially, (ie: dmxUniverse 0 has channels 0 - 511, 1 has channels 512 - 1023, etc)
        public List<DMXUniverse> DMXUniverses = new List<DMXUniverse>();

        /// <summary>
        /// Dictionary of DMX Channel/8-bit values
        /// </summary>
        private Dictionary<int, byte> _dmxValues = new Dictionary<int, byte>();        
        public Dictionary<int, byte> DMXValues {
            get {
                return _dmxValues;
            }
            set {
                if(value != _dmxValues) {
                    _dmxValues = value;
                    NotifyPropertyChanged("DMXValues");
                }
            }
        }

        /// <summary>
        /// String representation of the DMXValues Dictionary
        /// </summary>
        public List<string> DMXValueList {
            get {
                return ConvertDictionaryToString(_dmxValues);
            }
        }        
        #endregion Public Properties

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Forces Property Change Notification
        /// </summary>
        public void Refresh() {
            NotifyPropertyChanged("DMXValueList");
        }

        /// <summary>
        /// Constructor for DMXRouter
        /// </summary>
        public DMXRouter() {
            for(int i = 0; i < 8; i++) {
                DMXUniverses.Add(new DMXUniverse() { ID = i, NumberOfChannels = 0 });
            }

            LoadExistingUniverses();
        }

        /// <summary>
        /// Converts Dictionary pairs to Key + NewLine + Value string
        /// </summary>
        private List<string> ConvertDictionaryToString(Dictionary<int, byte> dict) {
            List<string> list = new List<string>();

            foreach(int o in dict.Keys) {
                list.Add(o.ToString() + Environment.NewLine + dict[o]);
            }

            return list;
        }
        
        /// <summary>
        /// If the xml config file exists, load up date from the file and populate the DMXUniverses
        /// </summary>
        public void LoadExistingUniverses() {
            string filePath = "Config\\DMXUniverseConfig.xml";

            if(File.Exists(filePath)) {
                XElement root = XElement.Load(filePath);
                
                for(int i=0; i < DMXUniverses.Count; i++) {
                    XElement xmlUniverse = (from u in root.Elements("DMXUniverse")
                                            where int.Parse(u.Attribute("ID").Value.ToString()) == DMXUniverses[i].ID
                                            select u).FirstOrDefault();

                    DMXUniverses[i] = DMXUniverse.FromXml(xmlUniverse);

                    if(DMXUniverses[i].StartChannel != 0 && DMXUniverses[i].NumberOfChannels != 0) {
                        SetUniverseChannels(DMXUniverses[i].ID, DMXUniverses[i].StartChannel, DMXUniverses[i].NumberOfChannels);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the channel/universe association
        /// </summary>
        /// <param name="universeID">DMX Universe ID</param>
        /// <param name="startingChannel">Universe Initial Channel Number</param>
        /// <param name="numberOfChannels">The number of channels that belong to the universe</param>
        private DMXUniverse SetUniverseChannels(int universeID, int startingChannel, int numberOfChannels = DMX_UNIVERSE_SIZE) {
            //Check that we're not asking for more channels than a universe can handle
            if(numberOfChannels > DMX_UNIVERSE_SIZE) {
                throw new DMXRouterException("Number of Channels is greater than 512");
            }

            //Check that the start channel isn't beyond the total number of channels
            if(startingChannel > TOTAL_DMX_CHANNELS) {
                throw new DMXRouterException("Starting channel exceeds the maximum channel number");
            }

            //Check that the channel range isn't outside the bounds of the available channels
            if(startingChannel + DMX_UNIVERSE_SIZE > TOTAL_DMX_CHANNELS) {
                throw new DMXRouterException("The number of channels being requested exceeds the maximum of " + TOTAL_DMX_CHANNELS + " channels");
            }

            DMXUniverse dmxU = DMXUniverses.SingleOrDefault(d => d.ID == universeID);

            if(dmxU != null) {                

                //Check our current DMX Universe against all others for used channels
                foreach(DMXUniverse dmx in DMXUniverses.Where(d => d.ID != dmxU.ID)) {
                    if(dmx.DMXValues != null) {
                        if(dmx.DMXValues.ContainsKey(startingChannel)) {
                            throw new DMXRouterException("The supplied starting channel already belongs to a DMX Universe");
                        }
                    }

                    int minChannel = dmxU.StartChannel;
                    int maxChannel = minChannel + DMX_UNIVERSE_SIZE - 1;
                    if((dmx.StartChannel > minChannel && dmx.StartChannel < maxChannel) ||
                        (dmx.StartChannel + DMX_UNIVERSE_SIZE - 1 > minChannel && dmx.StartChannel + DMX_UNIVERSE_SIZE - 1 < maxChannel)) {
                        throw new DMXRouterException("The channels requested are already in use.");
                    }                        
                }

                dmxU.StartChannel = startingChannel;
                dmxU.NumberOfChannels = numberOfChannels;

                //Initialize a dictionary of dmx values for the current dmx universe
                Dictionary<int, byte> initialValues = new Dictionary<int, byte>();
                for(int i = startingChannel; i < startingChannel + dmxU.NumberOfChannels; i++) {
                    initialValues[i] = 0;
                }                

                //Set the dmx values for the universe
                if(initialValues.Count > 0) {
                    dmxU.DmxController.SetValues(initialValues, dmxU.StartChannel);
                    dmxU.DMXValues = initialValues;
                    CombineDMXValues();
                }
            }

            return dmxU;
        }

        /// <summary>
        /// Set dmx values for an arbitrary set of dmx channels
        /// DMX channels are filtered into the proper DMX Universe
        /// </summary>
        /// <param name="dmxChannelVals">Dictionary of channel/16-bit dmx value pairs</param>
        public void SetDMXValues(Dictionary<int, int> dmxChannelVals) {
            Dictionary<int, byte> translatedDMXVals = Get8BitDMXValues(dmxChannelVals);

            foreach(int i in translatedDMXVals.Keys) {
                foreach(DMXUniverse dmxU in DMXUniverses) {
                    if(dmxU.DMXValues != null) {
                        if(dmxU.DMXValues.ContainsKey(i)) {
                            dmxU.DMXValues[i] = translatedDMXVals[i];
                        }
                    }
                }
            }

            foreach(DMXUniverse dmxU in DMXUniverses) {
                if(dmxU.DMXValues != null) {
                    dmxU.DmxController.SetValues(dmxU.DMXValues, dmxU.StartChannel);
                }
            }

            CombineDMXValues();
        }

        /// <summary>
        /// Sets dmx values for a given set of channels
        /// </summary>
        /// <param name="channelValues">Dictionary of dmx channel/value pairs</param>
        private void CombineDMXValues(){
            DMXValues = new Dictionary<int, byte>();

            foreach(DMXUniverse dmxU in DMXUniverses) {
                if(dmxU.DmxController != null){
                    if(dmxU.DMXValues != null) {
                        DMXValues = DMXValues.Union(dmxU.DMXValues).ToDictionary(a => a.Key, b => b.Value);
                    }
                }
            }
        }


        /// <summary>
        /// Retrieves DMX Values for a specific universe or all universes if not specified
        /// Most likely will not be used.
        /// </summary>
        /// <param name="universeID">DMX Universe ID</param>
        /// <returns>Dictionary of DMX channel/value pairs</returns>
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
                    dmxValues = dmxValues.Union(dmxU.DMXValues).ToDictionary(a => a.Key, b => b.Value);
                }

                dmxValues = dmxValues.Select( d => new { d.Key, d.Value }).ToDictionary( d => d.Key, d => d.Value );                    
            }

            return dmxValues;
        }

        /// <summary>
        /// Updates the universe and saves the xml for dmx universes
        /// </summary>
        /// <param name="dmxU">DMX Universe to save</param>
        public void SaveTheUniverse(DMXUniverse dmxU) {
            try {
                DMXUniverse universeToSave = SetUniverseChannels(dmxU.ID, dmxU.StartChannel, dmxU.NumberOfChannels);

                DMXUniverses[universeToSave.ID] = universeToSave;

                string folderPath = "Config";
                string filePath = folderPath + "\\DMXUniverseConfig.xml";

                if(!Directory.Exists(folderPath)) {
                    Directory.CreateDirectory(folderPath);
                }

                XElement dmxUniverseConfig = new XElement("SkySpace");
                foreach(DMXUniverse d in DMXUniverses) {
                    dmxUniverseConfig.Add(d.ToXml());
                }

                dmxUniverseConfig.Save(filePath);
            } catch(DMXRouterException dex) {
                log.Warn(dex);
                throw dex;
            } catch(Exception ex) {
                log.Error(ex);
            }
        }

        /// <summary>
        /// Converts 16-bit channel/values to 8-bit channel/values
        /// </summary>
        /// <param name="DMX16BitValues">Dictionary of DMX channel/16-bit value pairs</param>
        /// <returns></returns>
        private Dictionary<int, byte> Get8BitDMXValues(Dictionary<int, int> DMX16BitValues) {
            Dictionary<int, byte> dmx8BitValues = new Dictionary<int, byte>();

            foreach(int i in DMX16BitValues.Keys) {
                dmx8BitValues.Add(i, (byte)(DMX16BitValues[i] / 256));
                dmx8BitValues.Add(i + 1, (byte)(DMX16BitValues[i] % 256));
            }

            return dmx8BitValues;
        }
    }

    [Serializable]
    public class DMXRouterException : Exception {
        public DMXRouterException() { }
        public DMXRouterException(string message) : base(message) { }
        public DMXRouterException(string message, Exception inner) : base(message, inner) { }
        protected DMXRouterException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

}
