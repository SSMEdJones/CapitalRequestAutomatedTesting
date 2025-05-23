using System.ComponentModel.DataAnnotations;

namespace SSMWorkflow.API.DataAccess.Models
{
    public class WorkFlowInstanceActionHistorySearchFilter
    {
        public Guid WorkflowInstanceID { get; set; }
        public Guid? OptionID { get; set; }
    }
}