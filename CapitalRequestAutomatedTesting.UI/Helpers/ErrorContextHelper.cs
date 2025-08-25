using System.Xml.Linq;
using Infrastructure.Utilities.Xml;

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
            return FormDataXmlBuilder.Parse(xml);
        }

        private static string BuildFormXml(Dictionary<string, string> properties)
        {
            return FormDataXmlBuilder.Build(properties);
        }
    }

}
