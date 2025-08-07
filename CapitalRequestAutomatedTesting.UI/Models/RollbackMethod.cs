using CapitalRequestAutomatedTesting.UI.Enums;

namespace CapitalRequestAutomatedTesting.UI.Models
{
    public class RollbackMethod
    {
        public string ServiceName { get; set; }
        public string MethodName { get; set; }
        public List<object> Parameters { get; set; }
        public CrudOperationType Operation { get; set; }
    }

}
