using Scriban;
using System.Collections.Generic;

namespace CapitalRequestAutomatedTesting.UI.Helpers
{
    public static class TemplateHelper
    {
        public static string Render(string templateText, Dictionary<string, object> model)
        {
            var template = Template.Parse(templateText);

            if (template.HasErrors)
            {
                // Optional: log or throw if needed
                throw new InvalidOperationException("Template parsing failed: " + string.Join("; ", template.Messages));
            }

            return template.Render(model);
        }
    }

}


