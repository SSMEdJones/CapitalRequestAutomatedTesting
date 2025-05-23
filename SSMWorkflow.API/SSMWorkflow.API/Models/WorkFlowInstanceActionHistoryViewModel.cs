using System.ComponentModel.DataAnnotations.Schema;

namespace SSMWorkflow.API.Models
{
    public class WorkFlowInstanceActionHistoryViewModel
    {
        public Guid WorkflowInstanceActionHistoryID { get; set; }
        public Guid WorkflowInstanceID { get; set; }
        public Guid WorkflowStepID { get; set; }
        public Guid? OptionID { get; set; }

        public string Action { get; set; }

        public DateTime Completed { get; set; }

        public string CompletedBy { get; set; }


        [NotMapped]
        public bool Delete { get; set; }

        [NotMapped]
        public bool Edit { get; set; }
    }
}