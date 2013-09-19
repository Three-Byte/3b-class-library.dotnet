using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ThreeByte.Extension
{
    public static class IEnumerableExtensions
    {
        public static XElement ToXml<T>(this IEnumerable<T> source, string parent) {
            var type = typeof(T);
            XElement xml = new XElement(parent);
            if (type.GetMethod("ToXml") == null) {
                return xml;
            }

            foreach (T item in source) {
                xml.Add(type.GetMethod("ToXml").Invoke(item, null));
            }

            return xml;
        }
    }
}
