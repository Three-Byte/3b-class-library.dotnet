using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ThreeByte.DMX {
    //Each DMX Universe contains 512 channels.    
    public class DMXUniverse {

        #region Public Properties

        public DMXUniverse(int id) {
            ID = id;
            LightChannels = new List<LightChannel>();
            DMXValues = new Dictionary<int, byte>();
        }

        public int ID { get; private set; }
        public string Label { 
            get; set; 
        }

        private int _startChannel;
        public int StartChannel {
            get {
                return _startChannel;
            }
            set {
                _startChannel = value;
                UpdateDMXValueRange();
            }
        }

        private int _numberOfChannels;
        public int NumberOfChannels {
            get {
                return _numberOfChannels;
            }
            set {
                _numberOfChannels = value;
                UpdateDMXValueRange();
            }
        }
        public IDMXControl DmxController { get; set; }
        public Dictionary<int, byte> DMXValues { get; private set; }
        public List<LightChannel> LightChannels { get; set; }

        #endregion Public Properties

        public void Blackout() {

        }

        private void UpdateDMXValueRange() {

            HashSet<int> includedChannels = new HashSet<int>(DMXValues.Keys);
            foreach(int c in includedChannels) {
                if((c < StartChannel) || (c > (StartChannel + NumberOfChannels))) {
                    DMXValues.Remove(c);
                }
            }

            for(int c = StartChannel; c < StartChannel + NumberOfChannels; ++c) {
                if(!DMXValues.ContainsKey(c)) {
                    DMXValues[c] = 0;
                }
            }

        }

        public void SetValues(Dictionary<int, byte> values) {
            if(DmxController != null) {
                DmxController.SetValues(values);
            }
        }

        public void SetValues(Dictionary<int, byte> values, int startChannel) {
            if(DmxController != null) {
                DmxController.SetValues(values, startChannel);
            }
        }

        public XElement ToXml() {
            XElement dmxUXML = new XElement("DMXUniverse");
            dmxUXML.Add(new XAttribute("ID", ID));
            dmxUXML.Add(new XAttribute("Label", Label ?? ""));
            dmxUXML.Add(new XAttribute("StartChannel", StartChannel));
            dmxUXML.Add(new XAttribute("NumberOfChannels", NumberOfChannels));

            if(LightChannels != null) {
                XElement lights = new XElement("LightChannels");
                foreach(LightChannel lc in LightChannels) {
                    XElement lcElement = new XElement("LightChannel");
                    lcElement.Add(new XAttribute("Name", lc.Name ?? ""));
                    lcElement.Add(new XAttribute("Fine", lc.FineChannel));
                    lcElement.Add(new XAttribute("Coarse", lc.CoarseChannel));
                    lcElement.Add(new XAttribute("ID", lc.ID));
                    lcElement.Add(new XAttribute("Preview", lc.PreviewColor.ToString()));
                    lights.Add(lcElement);
                }

                dmxUXML.Add(lights);
            }

            return dmxUXML;
        }

        public static DMXUniverse FromXml(XElement dmxUConfig) {
            DMXUniverse dmxU = new DMXUniverse(int.Parse(dmxUConfig.Attribute("ID").Value.ToString()));
            dmxU.Label = dmxUConfig.Attribute("Label").Value.ToString();
            dmxU.StartChannel = int.Parse(dmxUConfig.Attribute("StartChannel").Value.ToString());
            dmxU.NumberOfChannels = int.Parse(dmxUConfig.Attribute("NumberOfChannels").Value.ToString());

            if(dmxUConfig.Element("LightChannels") != null) {
                dmxU.LightChannels = new List<LightChannel>();
                foreach(XElement lc in dmxUConfig.Element("LightChannels").Elements("LightChannel")) {
                    string previewColor = string.Empty;
                    if(lc.Attribute("Preview") != null) {
                        previewColor = lc.Attribute("Preview").Value;
                    }
                    dmxU.LightChannels.Add(new LightChannel() {
                        Name = lc.Attribute("Name").Value,
                        CoarseChannel = int.Parse(lc.Attribute("Coarse").Value),
                        FineChannel = int.Parse(lc.Attribute("Fine").Value),
                        UniverseID = dmxU.ID,
                        PreviewColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(previewColor)});
                }
            }

            return dmxU;
        }

    }
}
