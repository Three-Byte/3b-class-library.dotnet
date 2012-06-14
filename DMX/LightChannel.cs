using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace ThreeByte.DMX {
    public class LightChannel {

        public string ID { get { return string.Format("LC_{0}_{1}/{2}", UniverseID, CoarseChannel, FineChannel); } }
        public string Name { get; set; }
        public int CoarseChannel { get; set; }
        public int FineChannel { get; set; }
        public int UniverseID { get; set; }
        public Color PreviewColor{ get; set;}
    
    }
}
