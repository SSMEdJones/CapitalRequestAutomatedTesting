namespace CapitalRequestAutomatedTesting.UI.Models
{

    public class Request
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string ITReviewStatus { get; set; }
        public string FacilitiesReviewStatus { get; set; }
        public string SupplyChainReviewStatus { get; set; }
        public string EPMOReviewStatus { get; set; }
        public string PurchasingReviewStatus { get; set; }
        public string FinanceReviewStatus { get; set; }
        public string VPOpsReviewStatus { get; set; }
        public string VPFinanceReviewStatus { get; set; }
        public List<string> GroupsThatHaveTakenAction { get; set; }
    }

}