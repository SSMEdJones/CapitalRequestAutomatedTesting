namespace CapitalRequestAutomatedTesting.UI.Interfaces
{
    public interface IRollbackHandler
    {
        Task<object> RollbackAsync(List<object> parameters);
    }

}
