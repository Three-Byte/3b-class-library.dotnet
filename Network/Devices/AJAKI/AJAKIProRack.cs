using log4net;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreeByte.Network.Devices {
    public class AJAKIProRack {
        private static readonly ILog log = LogManager.GetLogger(typeof(AJAKIProRack));

        private RestClient rackClient;
        private readonly string RACK_ADDRESS;
        public AJAKIProRack(string rackAddress) {
            this.RACK_ADDRESS = rackAddress;
            rackClient = new RestClient(rackAddress);

            Connect();

            Task.Factory.StartNew(GetTimeCode, TaskCreationOptions.LongRunning);
        }

        private JObject connectionJson = null;

        /// <summary>
        /// Connect to the AJA KI-Pro Rack
        /// </summary>
        public void Connect() {
            if (connectionJson != null) {
                return;
            }
            var request = new RestRequest("json");
            request.AddParameter("action", "connect");
            request.AddParameter("configid", 0);
            request.Method = Method.GET;

            var response = rackClient.Execute(request).Content;
            if (!string.IsNullOrWhiteSpace(response)) {
                connectionJson = JObject.Parse(response);
            }
        }

        private List<object> WaitForConfigEvents() {
            var request = new RestRequest("json");
            request.AddParameter("action", "wait_for_config_events");
            request.AddParameter("configid", 0);

            string content = "";
            rackClient.ExecuteAsync(request, response => {
                content = response.Content;
            });

            //TODO: Figure out what the rack is giving us
            return new List<object>();
        }

        /// <summary>
        /// Turn the AJA device on or off
        /// </summary>
        /// <param name="powerOn">Desired power state</param>
        public bool SetPower(bool powerOn) {
            bool powerSet = false;

            var request = new RestRequest("SetParam");
            request.AddParameter("power", powerOn);
            request.Method = Method.POST;


            string content = "";
            rackClient.ExecuteAsync(request, response => {
                content = response.Content;
            });

            return powerSet;
        }

        public List<AjaClip> GetAjaClips() {
            var request = new RestRequest("clips");
            request.Method = Method.GET;
            string content = "";
            var response = rackClient.Execute(request);
            content = response.Content;
            return AjaClip.Load(content);
        }

        private string loadUrlContent(string url){
            WebClient client = new WebClient();
            return client.DownloadString(new Uri(url));
        }

        public AjaClip CurrentClip() {
            var result = loadUrlContent(RACK_ADDRESS + "options?eParamID_CurrentClip");
            var selected = ThreeByte.Network.Util.JavascriptParser<ajaPropertyVal>.Parse(result).Single();
            var match = GetAjaClips().Where(i => i.clipname == selected.value).FirstOrDefault();
            return match;
        }

        /// <summary>
        /// Send a transport command 
        /// </summary>
        /// <param name="command">transport command (Play, Pause, Stop, etc)</param>
        /// <returns></returns>
        public bool SendTransportCommand(TransportCommand command) {
            bool commandSet = false;

            var request = new RestRequest("options");
            //request.AddParameter("action", "set");
            request.AddParameter("paramName", "eParamID_TransportCommand");
            request.AddParameter("newValue", command.CmdString());
            request.Method = Method.POST;

            string content = "";
            rackClient.ExecuteAsync(request, response => {
                content = response.Content;
            });

            return commandSet;
        }

        /// <summary>
        /// Get the current transport state
        /// </summary>
        /// <returns></returns>
        public string GetTransportState() {
            var request = new RestRequest("options");
            request.AddParameter("eParamID_TransportState", "");
            request.Method = Method.GET;

            string content = "";
            rackClient.ExecuteAsync(request, response => {
                content = response.Content;
            });

            return content;
        }

        public event EventHandler TimecodeChanged;
        private void OnTimecodeChanged() {
            var handler = TimecodeChanged;
            if (handler != null) {
                handler(this, new EventArgs());
            }
        }

        public event EventHandler CurrentClipChanged;
        private void OnCurrentClipChanged() {
            var handler = CurrentClipChanged;
            if (handler != null) {
                handler(this, new EventArgs());
            }
        }

        public string Timecode { get; set; }

        private void GetTimeCode(object state) {
            while (true) {
                if (connectionID == null) {
                    Thread.Sleep(50);
                    continue;
                }
                var url = RACK_ADDRESS + "json?action=wait_for_config_events&configid=0&connectionid=" + connectionID;
                log.InfoFormat("Url: {0}", url);
                var response = loadUrlContent(url);
                JArray r = JArray.Parse(response);
                var paramID = r[0]["param_id"].ToString();
                if (paramID== "eParamID_DisplayTimecode") {
                    this.Timecode = r[0]["str_value"].ToString();
                    OnTimecodeChanged();
                    continue;
                }
                Thread.Sleep(50);
            }
        }

        private int getClipIndex(AjaClip clip) {
            var all = GetAjaClips();
            for (int i = 0; i < all.Count; i++) {
                if (all[i].clipname == clip.clipname) {
                    return i;
                }
            }
            return -1;
        }

        public bool GoToClip(AjaClip clip) {
            var idx = getClipIndex(clip);
            if (idx == -1) {
                return false;
            }

            var url = RACK_ADDRESS + "config?action=set&paramid=eParamID_GoToPlaylistIndex&value=" + idx.ToString()
                + "&configid=0";
            log.InfoFormat("Url: {0}", url);
            var response = loadUrlContent(url);

            return true;
        }
        private string connectionID {
            get {
                if (connectionJson == null) {
                    return null;
                }
                return connectionJson.Value<string>("connectionid");
            }
        }
    }

    public enum TransportCommand {
        Play,
        Pause,
        Stop
    }

    public static class TransportCommandExt {

        public static string CmdString(this TransportCommand cmd) {
            switch (cmd) {
                case TransportCommand.Play:
                    return "1";
                case TransportCommand.Stop:
                    return "4";
            }
            throw new NotImplementedException();
        }
    }

    public class ajaPropertyVal {
        public ajaPropertyVal() {

        }
        public string value { get; set; }
        public string text { get; set; }
        public string selected { get; set; }
    }
}
