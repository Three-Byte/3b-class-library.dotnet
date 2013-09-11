using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace ThreeByte.Network
{
    public static class NetworkExtensions
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NetworkExtensions));

        public static XElement PostAPICall(this string url, XElement xmlRequest) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
            request.Method = "POST";
            request.ContentType = "text/xml";

            Stream reqStream = request.GetRequestStream();

            string pubData = xmlRequest.ToString();
            byte[] stringArray = Encoding.UTF8.GetBytes(pubData);

            reqStream.Write(stringArray, 0, stringArray.Length);
            reqStream.Close();

            XElement response = XElement.Load(request.GetResponse().GetResponseStream());

            if (response.Name == "Error") {
                Exception ex = new Exception(string.Format("Error in Post API call: {0}", response));
                log.Error(ex);
                throw ex;
            }
            return response;
        }
    }
}
