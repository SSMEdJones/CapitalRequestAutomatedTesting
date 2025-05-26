using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapitalRequest.API.Models
{
    public class Proposal
    {
        public int Id { get; set; }

        public string ProjectName { get; set; }

        public string ProjectCenumber { get; set; }

        public int? Region { get; set; }

        public string CompanyCode { get; set; }

        public string Author { get; set; }

        public string ProjectDescription { get; set; }

        public decimal? TotalProjectCost { get; set; }

        public decimal? SalesTax { get; set; }

        public long? CostCenter { get; set; }

        public bool? IsProjectOverOneMillionDollars { get; set; }

        public string AuthorPhone { get; set; }

        public string AuthorEmail { get; set; }

        public decimal? FreightAmount { get; set; }

        public string MinistryName { get; set; }

        public bool? IsFundedFromBaseCapital { get; set; }

        public bool? IsPartOfAnnualCapitalReviewProcessNotoffCycleRequest { get; set; }

        public int? ProjectStatus { get; set; }

        public string Requestor { get; set; }

        public string RequestorPhone { get; set; }

        public string RequestorEmail { get; set; }

        public string ProjectManager { get; set; }

        public string ProjectManagerPhone { get; set; }

        public string ProjectManagerEmail { get; set; }

        public bool? IsOrderedByPurchasing { get; set; }

        public bool? IsQuotesSigned { get; set; }

        public string CostCenterKeyContactName { get; set; }

        public string CostCenterKeyContactPhone { get; set; }

        public string CostCenterKeyContactEmail { get; set; }

        public string ApproverName { get; set; }

        public string ApproverPhone { get; set; }

        public string DeliveryLocation { get; set; }

        public string DeliveryLocationExtraDetails { get; set; }

        public int? YearOfPurchase { get; set; }

        public int? CapitalFundingYear { get; set; }

        public int? TypeOfProject { get; set; }

        public int? CapitalCategory { get; set; }

        public int? CapitalPool { get; set; }

        public int? CapitalPoolIdentifiers { get; set; }

        public string ScoutId { get; set; }

        public string StrategicAlignmentAndProjectObjectives { get; set; }

        public string ProjectBackground { get; set; }

        public string ProjectEvaluation { get; set; }

        public string Recommendation { get; set; }

        public string AdditionalInformation { get; set; }

        public string FinancialInformation { get; set; }

        public decimal? Year1 { get; set; }

        public decimal? Year2 { get; set; }

        public decimal? Year3 { get; set; }

        public decimal? NetPresentValue { get; set; }

        public decimal? InternalRateOfReturn { get; set; }

        public decimal? ReturnOnInvestment { get; set; }

        public string PaybackPeriod { get; set; }

        public string ProjectRank { get; set; }

        public string ProjectChampion { get; set; }

        public string ChampionPhone { get; set; }

        public string ChampionEmail { get; set; }

        public bool? IsMovingForward { get; set; }

        public int? IsCancelledOrNotApproved { get; set; }

        public string ReasonNotMovingForward { get; set; }

        public int? Status { get; set; }

        public Guid? WorkflowId { get; set; }

        public DateTime? Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? Updated { get; set; }

        public string UpdatedBy { get; set; }

        public bool? OverrideWorkflow { get; set; }

        public int? IsProjectManagerDesired { get; set; }

        public bool? AffectsMultipleSegments { get; set; }

        public string Segment { get; set; }

        public string OverallGoal { get; set; }

        public string ImpactIfNotImplemented { get; set; }

        public decimal? TotalCapital { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool? Wbsapproved { get; set; }

        public int? SegmentId { get; set; }

        public string UserId { get; set; }

        public DateTime? Cancelled { get; set; }

        public string CancelledBy { get; set; }

        public DateTime? Overridden { get; set; }

        public string OverriddenBy { get; set; }

        public bool? IncludePurchasingGroup { get; set; }
    }
}
