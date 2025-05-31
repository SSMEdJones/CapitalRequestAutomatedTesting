namespace CapitalRequest.API.Models
{
    public class ActiveDirectoryUser
    {
        public ActiveDirectoryUser(string samAccountName, string givenName, string surname, string employeeId)
        {
            Email = samAccountName;
            FirstName = givenName;
            LastName = surname;
            FullName = $"{FirstName} {LastName}";
            UserId = employeeId;
        }

        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserId { get; set; }
    }
}
