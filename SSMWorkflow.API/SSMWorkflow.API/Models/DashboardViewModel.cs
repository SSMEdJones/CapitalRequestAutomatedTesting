
namespace SSMWorkflow.API.Models
{
    public class DashboardViewModel
    {

        //public AnnualCapitalProcess AnnualCapitalProcess { get; set; }


        public string ShowAnnualCapitalProcess { get; set; }


        public string Wacc { get; set; }


        public string CapitalMemoFileLoc { get; set; }


        public string DashboardDataTableNoDataMessage { get; set; }

        public bool HistoricalDataOnly { get; set; }

        public IEnumerable<Dashboard> DashBoard { get; set; }

    }
}