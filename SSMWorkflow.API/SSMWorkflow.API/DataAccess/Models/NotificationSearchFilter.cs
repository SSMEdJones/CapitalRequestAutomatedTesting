using System.ComponentModel.DataAnnotations;

namespace SSMWorkflow.API.DataAccess.Models
{
    public class NotificationSearchFilter
    {
        public DateTime? Updated { get; set; }

        [Required]
        public Guid WorkflowStepId { get; set; }

        public int EmailTemplateId { get; set; }
        public int ReviewerGroupId { get; set; }

        public string Action { get; set; }

        public Guid WorkflowStepOptionId { get; set; }
        public int RequestedInfoId { get; set; }
    }
}