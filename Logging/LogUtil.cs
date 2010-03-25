﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Deployment.Application;
using System.Net;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;

namespace ThreeByte.Logging
{
    public static class LogUtil
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LogUtil));

        /// <summary>
        /// For reporting purposes, gather and report useful information about the execution context
        /// </summary>
        /// <returns></returns>
        public static string GetDiagnosticString() {
            string context = string.Format("Timestamp: {0}\n", DateTime.Now);

            try {
                context += string.Format("Hostname: {0}\n", System.Environment.MachineName);
                context += string.Format("IPAddress: {0}\n", GetIPLocalAddress());
                context += string.Format("Username: {0}\n", System.Environment.UserName);
                if(ApplicationDeployment.IsNetworkDeployed) {
                    context += string.Format("Application Version Number: {0}\n", ApplicationDeployment.CurrentDeployment.CurrentVersion);
                } else {
                    context += "Application Version: Desktop/Debug\n";
                }
            } catch(Exception ex) {
                log.Error("Cannot create diagnostic string", ex);
                context += "Exception: " + ex.Message;
            }
            return context;
        }

        public static string GetIPLocalAddress() {

            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            string ipAddress = string.Empty;
            foreach(IPAddress ip in hostEntry.AddressList) {
                ipAddress += ip.ToString() + "; ";
            }
            return ipAddress;
        }

        public static string GetRecentEventString(){
            string eventString = "Recent Events:\n";
            try {
                Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

                foreach(IAppender app in hierarchy.Root.Appenders) {
                    if(app is RecentEventAppender) {
                        eventString += app.Name + ": " + ((RecentEventAppender)app).GetRecentEventString();
                    }
                }    
            } catch(Exception ex) {
                eventString += string.Format("Cannot create recent event string: {0}", ex.Message);
            }
            return eventString;

        }

    }
}
