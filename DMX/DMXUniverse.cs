using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ThreeByte.DMX {
    //Each DMX Universe contains 512 channels.    
    public class DMXUniverse {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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

        //public void SetValues(Dictionary<int, byte> values) {
        //    if(DmxController != null) {
        //        DmxController.SetValues(values);
        //    }
        //}

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


        public void SetToSixteenBitChannelDefault() {
            int i = this.StartChannel;
            List<string> newNames = new List<string>();
            foreach(LightChannel channel in this.LightChannels.ToList()) {
                string name = "";
                try {
                    channel.CoarseChannel = i;
                    channel.FineChannel = i + 1;
                    var n1 = this.LightChannels.Where(j => j.FineChannel == i || j.CoarseChannel == i).Last().Name;
                    var nameMatch = this.LightChannels.Where(j => j.FineChannel == i + 1 || j.CoarseChannel == i + 1);
                    var n2 = nameMatch.Last().Name;
                    name = n1;
                    if (n1 != n2) {
                        name = string.Format("{0}|{1}", n1, n2);
                    }

                    if (string.IsNullOrWhiteSpace(channel.Name)) {
                        channel.Name = string.Format("Channel_{0}_{1}", channel.CoarseChannel, channel.FineChannel);
                    }
                    if (i > this.NumberOfChannels - this.StartChannel) {
                        this.LightChannels.Remove(channel);
                    }
                } catch (Exception ex) {
                    continue;
                } finally {
                    newNames.Add(name);
                    i += 2;
                }
            }

            newNames = newNames.Take(this.LightChannels.Count()).ToList();
            for (int k = 0; k < newNames.Count(); k++) {
                this.LightChannels[k].Name = newNames[k];
            }

        }

        public void SetToEightBitChannelDefault() {
            int i = this.StartChannel;
            Dictionary<int, string> idxName = new Dictionary<int,string>();
            foreach(LightChannel channel in this.LightChannels) {
                channel.CoarseChannel = i++;
                channel.FineChannel = 0;
                var name = channel.Name;
                if (name.Contains('|')) {
                    int doub = channel.CoarseChannel * 2;
                    var s = name.Split('|');
                    idxName[doub - 1] = s.First();
                    idxName[doub] = s.Last();
                }
            
                if(string.IsNullOrWhiteSpace(name)) {
                    channel.Name = string.Format("Channel_{0}_{1}", channel.CoarseChannel, channel.FineChannel);
                }
            }

            foreach (var n in idxName.OrderBy(j => j.Key)) {
                var idx = n.Key - 1;
                if (idx > this.LightChannels.Count() - 1) {
                    var universeID = this.LightChannels.First().UniverseID;
                    this.LightChannels.Add(new LightChannel() { Name = n.Value, UniverseID = universeID, CoarseChannel = idx + 1, FineChannel = 0 });
                } else {
                    this.LightChannels[idx].Name = n.Value;
                }
            }

            while(i < this.NumberOfChannels + this.StartChannel) {
                this.LightChannels.Add(new LightChannel() {
                    CoarseChannel = i,
                    FineChannel = 0,
                    Name = string.Format("Channel_{0}_{1}", i++, 0)
                });

            }
        }
    }
}
