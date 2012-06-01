using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeByte.DMX {
    public class LightChannel {

        public string Name { get; set; }
        public int CoarseChannel { get; set; }
        public int FineChannel { get; set; }
        public int UniverseID { get; set; }

        public LightChannel() {
        }
    }
}
