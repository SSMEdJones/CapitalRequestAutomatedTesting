using Infrastructure.ApiDiagnostics;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public class FormDataContext : IFormDataContext
    {
        private string? _formData;

        public void Set(string xml)
        {
            _formData = xml;
        }

        public string? Get()
        {
            return _formData;
        }
    }
}