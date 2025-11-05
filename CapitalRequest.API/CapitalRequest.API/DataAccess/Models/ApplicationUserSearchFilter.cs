namespace CapitalRequest.API.DataAccess.Models
{
    public class ApplicationUserSearchFilter
    {
        public string? UserId { get; set; }
        public int? ApplicationRoleId { get; set; }
        public string? Email { get; set; }
        public bool? ReportAccess { get; set; }
        public string? FullName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }


    }
}
