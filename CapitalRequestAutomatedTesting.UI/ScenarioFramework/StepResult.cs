
namespace ScenarioFramework
{
    using System;

    public class StepResult
    {
        public bool Success { get; set; }
        public string Output { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime ExecutedAt { get; set; }
    }
}
