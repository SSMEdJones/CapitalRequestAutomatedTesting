using System.Reflection;

namespace CapitalRequestAutomatedTesting.UI.Helpers
{
    public static class DictionaryHelper
    {
        public static IDictionary<string, string> ToDictionary<T>(T model)
        {
            var dict = new Dictionary<string, string>();

            if (model == null)
                return dict;

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                var value = prop.GetValue(model);
                dict[prop.Name] = value?.ToString() ?? string.Empty;
            }
            //foreach (var prop in props)
            //{
            //    var value = prop.GetValue(model);
            //    if (value == null)
            //    {
            //        dict[prop.Name] = string.Empty;
            //    }
            //    else if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
            //    {
            //        var nestedProps = prop.PropertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            //        foreach (var nested in nestedProps)
            //        {
            //            var nestedValue = nested.GetValue(value);
            //            dict[$"{prop.Name}.{nested.Name}"] = nestedValue?.ToString() ?? string.Empty;
            //        }
            //    }
            //    else
            //    {
            //        dict[prop.Name] = value.ToString();
            //    }
            //}


            return dict;
        }

    }
}