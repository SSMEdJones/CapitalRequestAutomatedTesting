using System.ComponentModel.DataAnnotations.Schema;

namespace SSMWorkflow.API.Models
{
    public class WorkFlowStakeholderViewModel
    {
        public Guid WorkflowID { get; set; }
        public Guid StakeholderID { get; set; }
        public bool isGroup { get; set; }
        public bool isExternal { get; set; }

        public string Stakeholder { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? Updated { get; set; }
        public string? UpdatedBy { get; set; }

        [NotMapped]
        public bool Delete { get; set; }

        [NotMapped]
        public bool Edit { get; set; }
    }
}