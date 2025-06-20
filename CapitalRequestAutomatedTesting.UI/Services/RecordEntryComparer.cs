using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using Newtonsoft.Json;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public class RecordEntryComparer : IEqualityComparer<RecordEntry>
    {
        public bool Equals(RecordEntry x, RecordEntry y)
        {
            if (x.Operation != y.Operation) return false;
            return JsonConvert.SerializeObject(x.Data) == JsonConvert.SerializeObject(y.Data);
        }

        public int GetHashCode(RecordEntry obj)
        {
            return HashCode.Combine(obj.Operation, JsonConvert.SerializeObject(obj.Data));
        }
    }
}
