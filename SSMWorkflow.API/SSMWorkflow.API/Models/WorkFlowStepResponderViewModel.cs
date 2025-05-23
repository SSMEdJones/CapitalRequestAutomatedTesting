using System.ComponentModel.DataAnnotations.Schema;

namespace SSMWorkflow.API.Models
{
    public class WorkFlowStepResponderViewModel
    {
        public Guid WorkflowStepID { get; set; }
        public Guid ResponderID { get; set; }
        public bool isGroup { get; set; }

        public string Responder { get; set; }

        public int? ReviewerGroupId { get; set; }
        public Guid WorkflowStepOptionID { get; set; }
        public string ResponderType { get; set; } = string.Empty;
        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? Updated { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;

        [NotMapped]
        public bool Delete { get; set; }

        [NotMapped]
        public bool Edit { get; set; }
    }
}