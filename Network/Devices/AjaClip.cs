using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ThreeByte.Network.Devices {
    public class AjaClip {
        public string Name {
            get {
                return this.clipname.Split('.').First();
            }
        }

        public string Timecode {
            get {
                return "timecode...";
            }
        }
        public string clipname { get; set; }
        public string timestamp { get; set; }
        public string fourcc { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string framecount { get; set; }
        public string framerate { get; set; }
        public string interlace { get; set; }

        internal static List<AjaClip> Load(string content) {
            return ThreeByte.Network.Util.JavascriptParser<AjaClip>.Parse(content);
        }            
    }
}