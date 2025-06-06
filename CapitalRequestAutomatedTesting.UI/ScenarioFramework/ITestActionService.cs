
namespace ScenarioFramework
{
    using System.Threading.Tasks;

    public interface ITestActionService
    {
        Task<StepResult> CreateAndSubmitRequest();
        Task<StepResult> RequestMoreInfo();
        Task<StepResult> DeleteReviewer();
        Task<StepResult> VerifyReviewersUnlocked();
    }
}
