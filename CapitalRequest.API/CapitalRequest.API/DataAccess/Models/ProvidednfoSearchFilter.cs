namespace CapitalRequest.API.DataAccess.Models
{
    public class ProvidedInfoSearchFilter
    {
        public int? Id { get; set; }
        public int? RequestedInfoId { get; set; }
        public int? ReviewerGroupId { get; set; }
        public int? ReviewerId { get; set; }        
    }
}
