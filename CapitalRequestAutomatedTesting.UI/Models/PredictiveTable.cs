namespace CapitalRequestAutomatedTesting.UI.Models
{
    public class PredictiveTable
    {
        public int Id { get; set; }
        public int PredictiveDataId { get; set; }
        public string DatabaseName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
    }
}
