namespace Infrastructure.ApiDiagnostics
{
    public interface IFormDataContext
    {
        void Set(string xml);
        string? Get();
        void SetInvocationContext(MethodInvocationContext context);
        MethodInvocationContext? GetInvocationContext();
    }
}