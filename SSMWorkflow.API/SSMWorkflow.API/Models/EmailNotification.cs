#nullable disable

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace SSMWorkflow.API.Models
{
    public class EmailNotification
    {
        public int Id { get; set; }
        public Guid WorkflowStepId { get; set; }
        public string WorkflowName { get; set; }
        public string WorkflowDescription { get; set; }
        public string WorkflowState { get; set; }
        public string StepName { get; set; }
        public string StepDescription { get; set; }
        public string Action { get; set; }
        public string EmailMessage { get; set; }
        public string Recipients { get; set; }
        public string Subject { get; set; }
        public string Priority { get; set; }

        private string _emailQuery;
        public string EmailQuery
        {
            get => _emailQuery;
            set
            {
                _emailQuery = value;
                EmailQueryDetails = ParseEmailQuery(_emailQuery);
            }
        }

        public DateTime? Created { get; set; }
        public EmailQueryDetails EmailQueryDetails { get; private set; }

        private EmailQueryDetails ParseEmailQuery(string query)
        {
            if (string.IsNullOrEmpty(query)) return null;

            var parts = query.Split(',');

            return new EmailQueryDetails
            {
                WorkflowStepId = parts.Length > 1 ? parts[1].Trim('\'') : null,
                EmailTemplateId = parts.Length > 2 ? parts[2].Trim('\'') : null,
                ReviewerGroupId = parts.Length > 3 ? parts[3].Trim('\'') : null,
                Action = parts.Length > 4 ? parts[4].Trim('\'') : null,
                OptionId = parts.Length > 5 ? parts[5]?.Trim('\'') : null,
                RequestedInfoId = parts.Length > 6 ? parts[6]?.Trim('\'') : null                
            };
        }
                
    }
   
}
