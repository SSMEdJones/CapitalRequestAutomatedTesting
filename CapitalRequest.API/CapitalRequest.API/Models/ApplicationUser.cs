namespace CapitalRequest.API.Models
{
    public class ApplicationUser
    {
        public int Id { get; set; }
        public int? ApplicationRoleId { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public bool ReportAccess { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ApplicationRoleName { get; set; }
        public string AccessRoleName { get; set; }

        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Updated { get; set; }
        public string UpdatedBy { get; set; }
    }
}