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

            ////TODO: user Iron JS to parse this string into an object 
            ///and query the individual parameters
            List<AjaClip> toReturn = new List<AjaClip>();
            string currentKey = "";
            AjaClip inspectionClip = new AjaClip();
            var tokens = content.Split(' ');
            for (int i = 0; i < tokens.Count(); i++) {
                var token = tokens[i];
                if (token == "[" || token == "]") {
                    continue;
                }
                if (token.Contains("}")) {
                    toReturn.Add(inspectionClip);
                    inspectionClip = new AjaClip();
                    continue;
                }

                if (char.IsLetter(token.First())) {
                    currentKey = token.Substring(0, token.Length - 1);
                    continue;
                }
                if (token.First() == '"') {
                    var val = token.TrimEnd(',').Trim('"');

                    while(string.Concat(token.Skip(token.Length - 2)) != "\"," && token.Last() != '"') {
                        token = tokens[++i];
                        val += " " + token;
                        
                    }
                    ///Value
                    switch (currentKey) {
                        case "clipname":
                            inspectionClip.clipname = val;
                            break;
                        case "timestamp":
                            inspectionClip.timestamp = val;
                            break;
                        case "fourcc":
                            inspectionClip.fourcc = val;
                            break;
                        case "width":
                            inspectionClip.width = val;
                            break;
                        case "height":
                            inspectionClip.height = val;
                            break;
                        case "framecount":
                            inspectionClip.framecount = val;
                            break;
                        case "framerate":
                            inspectionClip.framerate = val;
                            break;
                        case "interlace":
                            inspectionClip.interlace = val;
                            break;
                        default:
                            throw new Exception();
                    }
                    continue;
                }
            }
            return toReturn;
        }
    }
}