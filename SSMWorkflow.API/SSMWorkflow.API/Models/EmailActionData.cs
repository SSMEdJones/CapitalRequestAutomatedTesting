#nullable disable

using System.Text.RegularExpressions;

namespace SSMWorkflow.API.Models
{
    public class EmailActionData
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string RequestingGroupName { get; set; }
        public string RequestedGroup { get; set; }
        public string RequestDate { get; set; }

       
    }


}
