using log4net;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeByte.Network.Devices {
    public class AJAKIProRack {
        private static readonly ILog log = LogManager.GetLogger(typeof(AJAKIProRack));

        private RestClient rackClient;

        public AJAKIProRack(string rackAddress) {
            rackClient = new RestClient(rackAddress);

            //Connect();           

            //ThreadPool.QueueUserWorkItem(GetTimeCode);
        }


        /// <summary>
        /// Connect to the AJA KI-Pro Rack
        /// </summary>
        public void Connect() {
            var request = new RestRequest("json");
            request.AddParameter("action", "connect");
            request.AddParameter("configid", 0);
            request.Method = Method.GET;

            string content = "";
            rackClient.ExecuteAsync(request, response => {
                content = response.Content;
            });
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

        /// <summary>
        /// Send a transport command 
        /// </summary>
        /// <param name="command">transport command (Play, Pause, Stop, etc)</param>
        /// <returns></returns>
        public bool SendTransportCommand(TransportCommand command) {
            bool commandSet = false;

            var request = new RestRequest("config");
            request.AddParameter("action", "set");
            request.AddParameter("paramid", "eParamID_TransportCommand");
            request.AddParameter("value", command.ToString());
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

        private void GetTimeCode(object state) {
            while (true) {
                List<object> events = WaitForConfigEvents();

                foreach (var e in events) {
                    if (e.ToString() == "eParamID_DisplayTimecode") {

                    }
                }
            }
        }
    }

    public enum TransportCommand {
        Play,
        Pause,
        Stop
    }
}
