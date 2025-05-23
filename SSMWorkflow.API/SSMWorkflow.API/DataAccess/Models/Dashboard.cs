namespace SSMWorkflow.API.DataAccess.Models
{
    public class Dashboard
    {
        public int ReqId { get; set; }

        public string Author { get; set; }


        public string UserId { get; set; }

        public int? RegionId { get; set; }

        public string RegionName { get; set; }

        public int? SegmentId { get; set; }

        public string SegmentNumber { get; set; }

        public string? CapitalPoolIdentifier { get; set; }

        public string CapitalPool { get; set; }


        public string ProjectName { get; set; }

        public bool? OffCycle { get; set; }

        public string PendingBy { get; set; }

        public DateTime Pending { get; set; }

        public string SubmittedBy { get; set; }

        public DateTime? Submitted { get; set; }
        public DateTime ITReviewDate { get; set; }

        public string ITReviewName { get; set; }


        public string ITReviewStatus { get; set; }

        public DateTime FacilitiesReviewDate { get; set; }

        public string FacilitiesReviewName { get; set; }


        public string FacilitiesReviewStatus { get; set; }

        public DateTime? SupplyChainReviewDate { get; set; }

        public string SupplyChainReviewName { get; set; }


        public string SupplyChainReviewStatus { get; set; }

        public DateTime EPMOReviewDate { get; set; }

        public string EPMOReviewName { get; set; }


        public string EPMOReviewStatus { get; set; }

        public DateTime PurchasingReviewDate { get; set; }

        public string PurchasingReviewName { get; set; }


        public string PurchasingReviewStatus { get; set; }

        public DateTime FinanceReviewDate { get; set; }

        public string FinanceReviewName { get; set; }


        public string FinanceReviewStatus { get; set; }

        public DateTime VPOpsReviewDate { get; set; }

        public string VPOpsReviewName { get; set; }


        public string VPOpsReviewStatus { get; set; }

        public DateTime VPFinanceReviewDate { get; set; }

        public string VPFinanceReviewName { get; set; }


        public string VPFinanceReviewStatus { get; set; }

        public decimal? Budget { get; set; }

        public string ProjectNumber { get; set; }


        public string WbsNumber { get; set; }

        public bool? MultipleWBS { get; set; }
        public bool? IsMovingForward { get; set; }
        public DateTime? Cancelled { get; set; }
        public string? CancelledBy { get; set; } = string.Empty;
    }
}