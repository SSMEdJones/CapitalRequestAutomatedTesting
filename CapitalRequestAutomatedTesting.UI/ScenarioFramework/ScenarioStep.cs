
namespace ScenarioFramework
{
    using System;
    using System.Threading.Tasks;

    public class ScenarioStep
    {
        public int StepNumber { get; set; }
        public string Description { get; set; }
        public Func<Task<StepResult>> Action { get; set; }
        public StepResult Result { get; set; }
    }
}
