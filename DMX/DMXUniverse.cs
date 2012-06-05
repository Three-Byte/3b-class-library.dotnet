using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ThreeByte.DMX {
    //Each DMX Universe contains 512 channels.    
    public class DMXUniverse {

        #region Public Properties

        public int ID { get; set; }
        public string Label { 
            get; set; 
        }
        public int StartChannel { get; set; }
        public int NumberOfChannels { get; set; }
        public IDMXControl DmxController { get; set; }
        public Dictionary<int, byte> DMXValues { get; set; }
        public List<LightChannel> LightChannels { get; set; }

        #endregion Public Properties

        public XElement ToXml() {
            XElement dmxUXML = new XElement("DMXUniverse");
            dmxUXML.Add(new XAttribute("ID", ID));
            dmxUXML.Add(new XAttribute("Label", Label ?? ""));
            dmxUXML.Add(new XAttribute("StartChannel", StartChannel));
            dmxUXML.Add(new XAttribute("NumberOfChannels", NumberOfChannels));

            if(DmxController != null) {
                XElement controller = new XElement("Controller");
                if(DmxController is VariableDMXControl) {
                    controller.Add(new XAttribute("COMPort", ((DMXControl)DmxController).COMPort));
                }
                controller.Add(new XAttribute("Enabled", DmxController.Enabled));

                dmxUXML.Add(controller);
            }

            if(LightChannels != null) {
                XElement lights = new XElement("LightChannels");
                foreach(LightChannel lc in LightChannels) {
                    XElement lcElement = new XElement("LightChannel");
                    lcElement.Add(new XAttribute("Name", lc.Name ?? ""));
                    lcElement.Add(new XAttribute("Coarse", lc.CoarseChannel));
                    lcElement.Add(new XAttribute("Fine", lc.FineChannel));
                    lights.Add(lcElement);
                }

                dmxUXML.Add(lights);
            }

            return dmxUXML;
        }

        public static DMXUniverse FromXml(XElement dmxUConfig) {
            DMXUniverse dmxU = new DMXUniverse();

            dmxU.ID = int.Parse(dmxUConfig.Attribute("ID").Value.ToString());
            dmxU.Label = dmxUConfig.Attribute("Label").Value.ToString();
            dmxU.StartChannel = int.Parse(dmxUConfig.Attribute("StartChannel").Value.ToString());
            dmxU.NumberOfChannels = int.Parse(dmxUConfig.Attribute("NumberOfChannels").Value.ToString());

            if(dmxUConfig.Element("Controller") != null) {
                XElement controller = dmxUConfig.Element("Controller");
                dmxU.DmxController = new VariableDMXControl(controller.Attribute("COMPort").Value.ToString(), dmxU.NumberOfChannels, dmxU.StartChannel);
                dmxU.DmxController.Enabled = bool.Parse(controller.Attribute("Enabled").Value.ToString());               
            }

            if(dmxUConfig.Element("LightChannels") != null) {
                dmxU.LightChannels = new List<LightChannel>();
                foreach(XElement lc in dmxUConfig.Element("LightChannels").Elements("LightChannel")) {
                    dmxU.LightChannels.Add(new LightChannel() { Name = lc.Attribute("Name").Value.ToString(), CoarseChannel = int.Parse(lc.Attribute("Coarse").Value.ToString()), FineChannel = int.Parse(lc.Attribute("Fine").Value.ToString()), UniverseID = dmxU.ID });
                }
            }

            return dmxU;
        }

    }
}
