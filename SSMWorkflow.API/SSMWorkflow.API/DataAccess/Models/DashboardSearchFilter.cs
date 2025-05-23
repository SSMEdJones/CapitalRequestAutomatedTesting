using System.ComponentModel.DataAnnotations;

namespace SSMWorkflow.API.DataAccess.Models
{
    public class DashboardSearchFilter
    {
        [Required]
        public int CapitalFundingYear { get; set; }

        public bool HistoricalDataOnly { get; set; }
    }
}