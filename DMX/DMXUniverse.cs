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

        #endregion Public Properties

        public XElement ToXml() {
            XElement dmxUXML = new XElement("DMXUniverse");
            dmxUXML.Add(new XAttribute("ID", ID));
            dmxUXML.Add(new XAttribute("Label", Label ?? ""));
            dmxUXML.Add(new XAttribute("StartChannel", StartChannel));
            dmxUXML.Add(new XAttribute("NumberOfChannels", NumberOfChannels));

            if(DmxController != null) {
                XElement controller = new XElement("Controller");
                controller.Add(new XAttribute("COMPort", DmxController.COMPort));
                controller.Add(new XAttribute("Enabled", DmxController.Enabled));

                dmxUXML.Add(controller);
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
                dmxU.DmxController = new VariableDMXControl(controller.Attribute("COMPort").Value.ToString(), dmxU.NumberOfChannels);
                dmxU.DmxController.Enabled = bool.Parse(controller.Attribute("Enabled").Value.ToString());
                dmxU.DmxController.Init();
            }

            return dmxU;
        }

    }
}
