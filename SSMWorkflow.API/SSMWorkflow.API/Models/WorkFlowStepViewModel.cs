using System.ComponentModel.DataAnnotations.Schema;

namespace SSMWorkflow.API.Models
{
    public class WorkFlowStepViewModel
    {
        public Guid WorkflowID { get; set; }
        public Guid WorkflowStepID { get; set; }

        public string StepName { get; set; }


        public string StepDescription { get; set; }

        public bool isRoot { get; set; }
        public bool supervisorStep { get; set; }
        public string? ResponderMessage { get; set; }
        public string? ResponderWarningMessage { get; set; }

        public string StakeholderMessage { get; set; }

        public string? StakeholderWarningMessage { get; set; }
        public int? DaysTillDue { get; set; }
        public int? WarningDays1 { get; set; }
        public int? WarningDays2 { get; set; }
        public bool WarningDays2Daily { get; set; }
        public bool IsComplete { get; set; }
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