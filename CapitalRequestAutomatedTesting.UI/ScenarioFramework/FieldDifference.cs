namespace CapitalRequestAutomatedTesting.UI.ScenarioFramework
{
    public class FieldDifference
    {
        public string FieldName { get; set; }
        public object ValuePredictive { get; set; } // Formerly ValueA
        public object ValueActual { get; set; }     // Formerly ValueB
    }

}
