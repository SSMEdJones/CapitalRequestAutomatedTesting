
namespace ScenarioFramework
{
    using System;
    using System.Threading.Tasks;

    public class ScenarioRunner
    {
        public async Task<ScenarioStatus> RunScenario(TestScenario scenario)
        {
            scenario.Status = ScenarioStatus.InProgress;

            foreach (var step in scenario.Steps)
            {
                try
                {
                    step.Result = await step.Action();
                    step.Result.ExecutedAt = DateTime.UtcNow;

                    if (!step.Result.Success)
                    {
                        scenario.Status = ScenarioStatus.Failed;
                        return scenario.Status;
                    }
                }
                catch (Exception ex)
                {
                    step.Result = new StepResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message,
                        ExecutedAt = DateTime.UtcNow
                    };
                    scenario.Status = ScenarioStatus.Failed;
                    return scenario.Status;
                }
            }

            scenario.Status = ScenarioStatus.Completed;
            return scenario.Status;
        }
    }
}
