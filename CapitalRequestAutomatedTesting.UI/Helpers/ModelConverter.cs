using CapitalRequestAutomatedTesting.UI.CustomAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Diagnostics;
using System.Reflection;

namespace CapitalRequestAutomatedTesting.UI.Helpers
{
    public static class ModelConverter
    {

        public static IDictionary<string, string> ToDictionaryExcluding<T>(T model)
        {
            var dict = new Dictionary<string, string>();
            if (model == null) return dict;

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var attributes = prop.GetCustomAttributes(true);
                foreach (var attr in attributes)
                {
                    Debug.WriteLine(attr.GetType().FullName);
                }
                // Skip if marked with IgnoreForLogging or ValidateNever
                bool skip = Attribute.IsDefined(prop, typeof(IgnoreForLogging)) ||
                            Attribute.IsDefined(prop, typeof(ValidateNeverAttribute));

                if (skip)
                    continue;

                var value = prop.GetValue(model);
                dict[prop.Name] = value?.ToString() ?? string.Empty;
            }



            return dict;
        }
    }

}



