using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using log4net;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace ThreeByte.DMX {
    public class PageMapping : INotifyPropertyChanged {

        private static readonly ILog log = LogManager.GetLogger(typeof(PageMapping));

        //each color encoder maps to an arbitrary number of channels
        //public Dictionary<int, List<int>> encoderChannels = new Dictionary<int, List<int>>();
        public Dictionary<int, List<LightChannel>> encoderChannels = new Dictionary<int, List<LightChannel>>();
        DMXRouter _dmxRouter;

        private int _id;
        public int ID {
            get { return _id; }
            set {
                if(_id != value) {
                    _id = value;
                    NotifyPropertyChanged("ID");
                }
            }
        }

        private string _pageName = "";
        public string PageName {
            get {
                return _pageName;   
            }
            set {
                if(_pageName != value) {
                    _pageName = value;
                    NotifyPropertyChanged("PageName");
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="numberOfEncoders">Number of Encoders</param>
        /// <param name="dmxRouter">The DMX router object</param>
        /// <param name="id">ID of the Page Mapping</param>
        public PageMapping(int numberOfEncoders, DMXRouter dmxRouter, int id) {
            for(int i = 0; i < numberOfEncoders; i++) {
                encoderChannels.Add(i, new List<LightChannel>());
            }

            _dmxRouter = dmxRouter;
            _id = id;
        }

        public PageMapping() { }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void AddChannels(int encoderID, List<LightChannel> lightChannels) {
            if(encoderChannels[encoderID].Count > 0) {
                encoderChannels[encoderID].Clear();
            }

            encoderChannels[encoderID] = lightChannels;
            NotifyPropertyChanged("encoderChannels");
        }

        public void UpdateDMXValues(int encoderID, int value) {
            Dictionary<int, int> dmxValues = new Dictionary<int, int>();

            foreach(LightChannel lc in encoderChannels[encoderID]) {
                dmxValues.Add(lc.CoarseChannel, (int)value);
                //dmxValues.Add(i, (int)value);
            }

            _dmxRouter.SetDMXValues(dmxValues);
            //_dmxRouter.Refresh();
        }

        public XElement ToXml() {
            XElement pmXML = new XElement("PageMapping");
            pmXML.Add(new XAttribute("ID", ID));
            pmXML.Add(new XAttribute("Name", PageName));
            
            foreach(int encoder in encoderChannels.Keys) {
                XElement encNode = new XElement("Encoder");
                encNode.Add(new XAttribute("ID", encoder));
                encNode.Add(new XAttribute("Channels", ListToCommaSeparated(encoderChannels[encoder])));
                pmXML.Add(encNode);
            }

            return pmXML;
        }

        public static PageMapping FromXml(XElement pageMappingConfig, DMXRouter dmxRouter) {
            int numberOfEncoders = pageMappingConfig.Elements("Encoder").Count();
            int pmID = int.Parse(pageMappingConfig.Attribute("ID").Value.ToString());

            PageMapping pageMapping = new PageMapping(numberOfEncoders, dmxRouter, pmID);
            pageMapping.PageName = pageMappingConfig.Attribute("Name").Value.ToString();
            pageMapping.encoderChannels = new Dictionary<int, List<LightChannel>>(); //new Dictionary<int, List<int>>();
            foreach(XElement encoder in pageMappingConfig.Elements("Encoder")) {
                pageMapping.encoderChannels.Add(int.Parse(encoder.Attribute("ID").Value.ToString()), CommaSeparatedToChannelList(encoder.Attribute("Channels").Value.ToString(), dmxRouter));
                //pageMapping.encoderChannels.Add(int.Parse(encoder.Attribute("ID").Value.ToString()), 
                //    CommaSeparatedToList(encoder.Attribute("Channels").Value.ToString()));
            }

            return pageMapping;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        public static List<int> CommaSeparatedToList(string cs) {
            List<int> intList = new List<int>();

            cs = cs.Replace(" ", "");

            if(cs.Length > 0) {
                string[] csArray = cs.Split(',');

                foreach(string s in csArray) {
                    intList.Add(int.Parse(s));
                }
            }

            return intList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        public static List<LightChannel> CommaSeparatedToChannelList(string cs, DMXRouter dmxRouter) {
            List<LightChannel> lcList = new List<LightChannel>();

            cs = cs.Replace(" ", "");

            if(cs.Length > 0) {
                string[] csArray = cs.Split(',');

                foreach(string s in csArray) {
                    foreach(DMXUniverse dmxU in dmxRouter.DMXUniverses) {
                        List<LightChannel> lcInUse = dmxU.LightChannels.Where(lc => lc.Name == s).ToList<LightChannel>();
                        if(lcInUse.Count() == 1) {
                            lcList.Add(lcInUse[0]);     
                        }
                    }
                }
            }

            return lcList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ListToCommaSeparated(List<int> list) {
            string cs = "";

            foreach(int i in list) {
                cs += i.ToString() + ",";
            }

            return cs.TrimEnd(',');
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ListToCommaSeparated(List<LightChannel> list) {
            string cs = "";

            foreach(LightChannel lc in list) {
                cs += lc.Name + ",";
            }

            return cs.TrimEnd(',');
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="depObj"></param>
        /// <returns></returns>
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject {
            if(depObj != null) {
                for(int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if(child != null && child is T) {
                        yield return (T)child;
                    }

                    foreach(T childOfChild in FindVisualChildren<T>(child)) {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
