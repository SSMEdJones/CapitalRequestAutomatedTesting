using Infrastructure.ApiDiagnostics;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public class FormDataContext : IFormDataContext
    {
        private string? _formDataXml;
        private MethodInvocationContext? _invocationContext;

        public void Set(string xml)
        {
            _formDataXml = xml;
        }

        public string? Get()
        {
            return _formDataXml;
        }
        
        
        public void SetInvocationContext(MethodInvocationContext context)
        {
            _invocationContext = context;
        }

        MethodInvocationContext? IFormDataContext.GetInvocationContext()
        {
            return _invocationContext;
        }
    }
}