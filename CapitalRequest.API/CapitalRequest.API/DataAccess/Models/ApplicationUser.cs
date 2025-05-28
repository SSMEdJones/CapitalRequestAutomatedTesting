using System.ComponentModel.DataAnnotations.Schema;

namespace CapitalRequest.API.DataAccess.Models
{
    public class ApplicationUser
    {
        public int Id { get; set; }
        public int? ApplicationRoleId { get; set; }
        public virtual string UserId { get; set; }
        public string Email { get; set; }
        public bool? ReportAccess { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [NotMapped]
        public virtual string ApplicationRoleName { get; set; }
        [NotMapped]
        public string AccessRoleName { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Updated { get; set; }
        public string UpdatedBy { get; set; }
    }
}