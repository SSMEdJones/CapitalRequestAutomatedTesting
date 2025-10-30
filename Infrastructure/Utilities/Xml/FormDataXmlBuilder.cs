#nullable disable

using System.Text;
using System.Xml.Linq;

namespace Infrastructure.Utilities.Xml
{

    public static class FormDataXmlBuilder
    {
        public static string Build(IDictionary<string, string> formData)
        {
            if (formData == null || !formData.Any())
                return "<form />";

            var formElement = new XElement("form");

            foreach (var kvp in formData)
            {
                var fieldElement = new XElement("field",
                    new XAttribute("name", kvp.Key),
                    new XCData(kvp.Value ?? string.Empty));

                formElement.Add(fieldElement);
            }

            var doc = new XDocument(formElement);
            var sb = new StringBuilder();
            using var writer = new StringWriter(sb);
            doc.Save(writer);
            return sb.ToString();
        }

        public static Dictionary<string, string> Parse(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return new Dictionary<string, string>();
                
            try
            {
                var doc = XDocument.Parse(xml);
                return doc.Descendants("field")
                    .ToDictionary(
                        x => x.Attribute("name")?.Value,
                        x => x.Value
                    );
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }
    }

}
