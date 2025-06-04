namespace SSMWorkflow.API.DataAccess.Models
{
    public class EmailNotificationSearchFilter
    {
        public Guid WorkflowStepId { get; set; }
        public string EmailQuery { get; set; }
    }
}
