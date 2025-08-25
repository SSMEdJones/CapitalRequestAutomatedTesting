using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Infrastructure.Utilities.Xml
{
    public static class XmlFormatter
    {
        public static string ToXml(object obj)
        {
            if (obj == null) return string.Empty;

            var xmlSerializer = new XmlSerializer(obj.GetType());
            using var stringWriter = new StringWriter();
            xmlSerializer.Serialize(stringWriter, obj);
            return stringWriter.ToString();
        }
    }

}
