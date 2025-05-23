using System.ComponentModel.DataAnnotations.Schema;

namespace SSMWorkflow.API.Models
{
    public class WorkFlowInstanceViewModel
    {
        public Guid WorkflowInstanceID { get; set; }
        public Guid WorkflowID { get; set; }
        public Guid CurrentWorkflowStepID { get; set; }

        public string CurrentWorkflowState { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? Updated { get; set; }

        public string UpdatedBy { get; set; }


        [NotMapped]
        public bool Delete { get; set; }

        [NotMapped]
        public bool Edit { get; set; }
    }
}