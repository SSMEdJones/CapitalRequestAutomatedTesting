namespace CapitalRequest.API.Models
{
    public class WbsNumber
    {
        public int Id { get; set; }
        public int ProposalId { get; set; }

        public int TypeofProject {get; set; }

        public string TypeofProjectShortName { get; set; }
        public string CompanyCode { get; set; }

        public string CapitalPoolIdentifierShortName { get; set; }
        public string CapitalPoolShortName { get; set; }

        public string CapitalFundingYearShortName { get; set; }

        public string UniqueIdentifier { get; set; }

        public string Sequence { get; set; }

    }
}
