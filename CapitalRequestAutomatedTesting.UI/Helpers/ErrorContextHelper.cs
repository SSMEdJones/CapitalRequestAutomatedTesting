using System.Xml.Linq;

namespace CapitalRequestAutomatedTesting.UI.Helpers
{

    public static class ErrorContextHelper
    {
        public static Dictionary<string, string> CaptureFormData(HttpRequest request)
        {
            var formData = new Dictionary<string, string>();

            if (request.HasFormContentType)
            {
                foreach (var key in request.Form.Keys)
                {
                    formData[key] = request.Form[key];
                }
            }

            return formData;
        }

        public static string SerializeFormData(Dictionary<string, string> formData)
        {
            var xml = new XElement("form",
                formData.Select(kvp =>
                    new XElement("item",
                        new XAttribute("name", kvp.Key),
                        new XElement("value", new XAttribute("string", kvp.Value ?? string.Empty))
                    )
                )
            );

            return xml.ToString();
        }

        public static Dictionary<string, string> ParseFormXml(string xml)
        {
            var doc = XDocument.Parse(xml);
            return doc.Descendants("item")
                      .ToDictionary(
                          x => x.Attribute("name")?.Value,
                          x => x.Element("value")?.Attribute("string")?.Value
                      );
        }
    }

}
