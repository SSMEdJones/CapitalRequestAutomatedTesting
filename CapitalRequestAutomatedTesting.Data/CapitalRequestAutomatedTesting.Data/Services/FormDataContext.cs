namespace CapitalRequestAutomatedTesting.Data.Services
{
    public interface IFormDataContext
    {
        void Set(string xml);
        string? Get();
    }

    public class FormDataContext : IFormDataContext
    {
        private string? _formDataXml;

        public void Set(string xml)
        {
            _formDataXml = xml;
        }

        public string? Get()
        {
            return _formDataXml;
        }
    }

}
