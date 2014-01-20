using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ThreeByte.Network.Devices {
    public class AjaClip {
        public AjaClip(string name) {
            this.Name = name;
            this.Timecode = "00:00:04";
        }
        public string Name { get; set; }
        public string Timecode { get; set; }

        internal static List<AjaClip> Load(string content) {

            ////TODO: user Iron JS to parse this string into an object 
            ///and query the individual parameters

            var result = content.TrimEnd(';');
            var tokens = result.Split(' ').ToList();
            result = string.Concat(tokens.Select(i => {
                var split = i.Split(':');
                if (i.TakeWhile(c => c == ':').Count() == 1) {
                    return "\"" + split[0] + "\"" + ":" + split[1];
                } else {
                    return i + " ";
                }
            }));

            //var content1 = IronJS.Hosting.FSharp.createContext();
            //IronJS.Hosting.CSharp.Context ctx = new IronJS.Hosting.CSharp.Context();
            //ctx.Execute("var a= " + content);
            //var result2 = ctx.Execute("a");

            throw new NotImplementedException();
        }
    }
}