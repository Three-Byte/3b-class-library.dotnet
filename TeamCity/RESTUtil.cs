using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;

namespace ThreeByte.TeamCity {
    public static class RESTUtil {


        public static bool TriggerBackup(string hostname, string filename, string user, string password, bool includeConfigs = true, bool includeDatabase = true, bool includeBuildLogs = true) {            
            var client = new RestClient(hostname);
            client.Authenticator = new HttpBasicAuthenticator(user, password);
            client.ClearHandlers();

            var request = new RestRequest("httpAuth/app/rest/server/backup?fileName={fn}&includeConfigs={ic}&includeDatabase={id}&includeBuildLogs={ibl}", Method.POST);
    
            request.AddUrlSegment("fn", filename);
            request.AddUrlSegment("ic", includeConfigs.ToString().ToLower());
            request.AddUrlSegment("id", includeDatabase.ToString().ToLower());
            request.AddUrlSegment("ibl", includeBuildLogs.ToString().ToLower());

            var response = client.Execute(request);
            var content = response.Content; // raw content as string

            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }
    }
}
