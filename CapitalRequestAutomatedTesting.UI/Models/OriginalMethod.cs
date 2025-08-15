using CapitalRequestAutomatedTesting.UI.Enums;

namespace CapitalRequestAutomatedTesting.UI.Models
{
    public class OriginalMethod
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public string ScenarioId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string MethodName { get; set; } = string.Empty;
        public List<object>? Parameters { get; set; }
        public CrudOperationType Operation { get; set; }
    }
}