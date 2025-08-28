namespace Infrastructure.ApiDiagnostics
{
    public class MethodInvocationContext
    {
        public string ServiceName { get; set; } = string.Empty;
        public string MethodName { get; set; } = string.Empty;
        public List<object> Parameters { get; set; } = new List<object>();
    }
}