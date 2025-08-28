using CapitalRequest.API.CustomAttributes;
using CapitalRequest.API.Enums;
using Microsoft.AspNetCore.Http;
using SSMWorkflow.API.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CapitalRequest.API.Models
{
    public class Proposal
    {
        public Proposal() { }

        #region Basic Info
        [DisplayName("Id")]
        public int Id { get; set; }

        [IgnoreForLogging]
        [DisplayName("Project Name")]
        public string? ProjectName { get; set; }

        [IgnoreForLogging]
        [DisplayName("Project CE Number")]
        public string? ProjectCENumber { get; set; }

        [DisplayName("Region")]
        public int Region { get; set; }

        [DisplayName("Segment")]
        public string Segment { get; set; }

        public int SegmentId { get; set; }

        [IgnoreForLogging]
        [DisplayName("Company Code")]
        public string CompanyCode { get; set; }

        [IgnoreForLogging]
        [DisplayName("Author")]
        [RegularExpression("^[a-zA-Z-' ]*$", ErrorMessage = "*")]
        public string? Author { get; set; }

        [IgnoreForLogging]
        public string UserId { get; set; } // AuthorId

        [IgnoreForLogging]
        [DisplayName("Project Description (Please provide a few sentences describing the project, including what markets/regions/programs will be impacted)")]
        public string? ProjectDescription { get; set; }

        [IgnoreForLogging]
        [DisplayName("In a few sentences, describe the overall goal and business objectives of the project")]
        public string? OverallGoal { get; set; }

        [IgnoreForLogging]
        [DisplayName("What would be the impact if this project isn't implemented?")]
        public string? ImpactIfNotImplemented { get; set; }

        [IgnoreForLogging]
        [RegularExpression("^\\$?([1-9]\\d{0,2}(,\\d{3})*(\\.\\d{2})?|[1-9]\\d*(\\.\\d{2})?|0?\\.(?!00)\\d{2})$", ErrorMessage = "Enter number greater than $0")]
        [DisplayName("Total Project Cost")]
        public string TotalProjectCost { get; set; }

        [IgnoreForLogging]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        [DisplayName("Sales tax")]
        public string SalesTax { get; set; }

        [IgnoreForLogging]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        [DisplayName("Freight Amount")]
        public string FreightAmount { get; set; }

        [IgnoreForLogging]
        [DisplayName("Cost Center")]
        public string CostCenter { get; set; }

        [IgnoreForLogging]
        [DisplayName("Is the Project Over One Million Dollars?")]
        public bool IsProjectOverOneMillionDollars { get; set; } = false;

        [IgnoreForLogging]
        [Phone(ErrorMessage = "Invalid Phone Number")]
        [RegularExpression(@"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$")]
        [DisplayName("Author Phone")]
        public string? AuthorPhone { get; set; }

        [IgnoreForLogging]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid email address")]
        [DisplayName("Author Email")]
        public string? AuthorEmail { get; set; }

        [IgnoreForLogging]
        [DisplayName(@"Ministry (Include Specific ship to location/address on next page, field ""Delivery Location Extra detail"")")]
        public string? MinistryName { get; set; }

        [IgnoreForLogging]
        [DisplayName("Is this being funded from base capital?")]
        public bool IsFundedFromBaseCapital { get; set; } = true;

        [IgnoreForLogging]
        [DisplayName("Is this request an OFF-CYCLE request?")]
        public bool IsPartOfAnnualCapitalReviewProcessNOTOffCycleRequest { get; set; }

        [IgnoreForLogging]
        [DisplayName("Override Workflow")]
        public bool? OverrideWorkflow { get; set; }

        [IgnoreForLogging]
        public List<WorkflowAction> WorkflowActions { get; set; }

        [IgnoreForLogging]
        public string WorkflowCaption { get; set; } = "Workflow";

        [IgnoreForLogging]
        [DisplayName("Does project require PO?")]
        public bool IncludePurchasingGroup { get; set; }
        #endregion

        #region Additional Info Page

        [IgnoreForLogging]
        [StringLength(50)]
        [RegularExpression("^[a-zA-Z-' ]*$", ErrorMessage = "*")]
        [DisplayName("Requester")]
        public string? Requestor { get; set; }

        [IgnoreForLogging]
        [Phone(ErrorMessage = "Invalid Phone Number")]
        [RegularExpression(@"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$", ErrorMessage = "Invalid Phone")]
        [DisplayName("Requester Phone")]
        public string? RequestorPhone { get; set; }

        [IgnoreForLogging]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid email address")]
        [DisplayName("Requester Email")]
        public string? RequestorEmail { get; set; }

        [IgnoreForLogging]
        [StringLength(50)]
        [RegularExpression("^[a-zA-Z-' ]*$", ErrorMessage = "*")]
        [DisplayName("Project Manager")]
        public string? ProjectManager { get; set; }

        [IgnoreForLogging]
        [Phone(ErrorMessage = "Invalid Phone Number")]
        [RegularExpression(@"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$", ErrorMessage = "Invalid Phone")]
        [DisplayName("Project Manager Phone")]
        public string? ProjectManagerPhone { get; set; }

        [IgnoreForLogging]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid email address")]
        [DisplayName("Project manager email")]
        public string? ProjectManagerEmail { get; set; }

        [IgnoreForLogging]
        [DisplayName("Has this been ordered by purchasing?")]
        public bool IsOrderedByPurchasing { get; set; }

        [IgnoreForLogging]
        [DisplayName("Quote Signed? If not, please have signed by appropriate person before moving this forward.")]
        public bool IsQuotesSigned { get; set; }

        [IgnoreForLogging]
        [DisplayName("Quotes (File name cannot exceed 255 characters.)")]
        public List<Quote> Quotes { get; set; } = new List<Quote>();

        [IgnoreForLogging]
        [DisplayName("Multiple WBS Numbers Requested?")]
        public bool IsMultipleWBSNumbersRequested { get; set; }

        [IgnoreForLogging]
        [DisplayName("How many WBS numbers are requested?")]
        public int NumberOfWBSNumbersRequested { get; set; }

        //public IList<WBS> WBS { get; set; } = new List<WBS>();

        [IgnoreForLogging]
        [DisplayName("Add a Quotes")]
        public List<IFormFile>? QuoteFiles { get; set; }

        [IgnoreForLogging]
        [DisplayName("Cost Center Key Contact")]
        [RegularExpression("^[a-zA-Z-' ]*$", ErrorMessage = "*")]
        public string? CostCenterKeyContactName { get; set; }

        [IgnoreForLogging]
        [Phone(ErrorMessage = "Invalid Phone Number")]
        [RegularExpression(@"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$", ErrorMessage = "Invalid Phone")]
        [DisplayName("Cost Center Key Contact Phone")]
        public string? CostCenterKeyContactPhone { get; set; }

        [IgnoreForLogging]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid email address")]
        [DisplayName("Cost Center Key Contact Email")]
        public string? CostCenterKeyContactEmail { get; set; }

        [IgnoreForLogging]
        [DisplayName("Approver Name")]
        [RegularExpression("^[a-zA-Z-' ]*$", ErrorMessage = "*")]
        public string? ApproverName { get; set; }

        [IgnoreForLogging]
        [Phone(ErrorMessage = "Invalid Phone Number")]
        [RegularExpression(@"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$", ErrorMessage = "Invalid Phone")]
        [DisplayName("Approver Phone")]
        public string? ApproverPhone { get; set; }

        [IgnoreForLogging]
        [DisplayName("Delivery Location")]
        public string? DeliveryLocation { get; set; }

        [IgnoreForLogging]
        [DisplayName("Delivery Location Extra Details (Include specific ship to location/address)")]
        public string? DeliveryLocationExtraDetails { get; set; }

        [IgnoreForLogging]
        [DisplayName("Year of Purchase")]
        public int? YearOfPurchase { get; set; }

        [IgnoreForLogging]
        public IList<Asset> Assets { get; set; } = new List<Asset>();

        [IgnoreForLogging]
        [DisplayName("Capital Funding Year")]
        public int CapitalFundingYear { get; set; }

        [IgnoreForLogging]
        [DisplayName("Capital Category")]
        public int CapitalCategory { get; set; }

        [IgnoreForLogging]
        [DisplayName("Existing asset affected by project (Enter asset number or description, dept, etc.)")]
        public string? AssetNumber { get; set; }

        [IgnoreForLogging]
        public int NumberOfAssets { get; set; }

        [IgnoreForLogging]
        public int MaxAssets { get; set; }

        [IgnoreForLogging]
        [DisplayName("Is this Clinical Equipment?")]
        public bool IsClinicalEquipment { get; set; }

        [IgnoreForLogging]
        [DisplayName("Is this a Replacement Item?")]
        public bool IsReplacementItem { get; set; }

        [IgnoreForLogging]
        [DisplayName("Is this a transfer?")]
        public bool IsTransfer { get; set; }

        [IgnoreForLogging]
        [DisplayName("Is this item being scrapped?")]
        public bool IsScrapped { get; set; }

        [IgnoreForLogging]
        [DisplayName("Certification of Need?")]
        public bool IsCertificationNeeded { get; set; }

        [IgnoreForLogging]
        [DisplayName("Levelpath")]
        public string ScoutID { get; set; }

        [IgnoreForLogging]
        [DisplayName("Capital Pool Identifiers")]
        public int CapitalPoolIdentifiers { get; set; }

        [IgnoreForLogging]
        [DisplayName("Capital Pool")]
        public int CapitalPool { get; set; }

        #endregion

        #region Over a Million Page

        [IgnoreForLogging]
        [DisplayName("Strategic Alignment and Project Objectives")]
        public string? StrategicAlignmentAndProjectObjectives { get; set; }

        [IgnoreForLogging]
        [DisplayName("Project Background")]
        public string? ProjectBackground { get; set; }

        [IgnoreForLogging]
        [DisplayName("Project Evaluation")]
        public string? ProjectEvaluation { get; set; }

        [IgnoreForLogging]
        [DisplayName("Recommendation")]
        public string? Recommendation { get; set; }

        [IgnoreForLogging]
        [DisplayName("Additional Information")]
        public string? AdditionalInformation { get; set; }

        [IgnoreForLogging]
        [DisplayName("Financial Information")]
        public string? FinancialInformation { get; set; }

        [IgnoreForLogging]
        [DisplayName("Total Capital")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public string TotalCapital { get; set; }

        [IgnoreForLogging]
        [DisplayName("Year 1")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public string Year1 { get; set; }

        [IgnoreForLogging]
        [DisplayName("Year 2")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public string? Year2 { get; set; }

        [IgnoreForLogging]
        [DisplayName("Year 3")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public string? Year3 { get; set; }

        [IgnoreForLogging]
        [DisplayName("(Baseline Scenario) Net Present Value")]
        public string NetPresentValue { get; set; }

        [IgnoreForLogging]
        [DisplayName("(Baseline Scenario) Internal Rate of Return")]
        public string InternalRateOfReturn { get; set; }

        [IgnoreForLogging]
        [DisplayName("(Baseline Scenario) Return on investment")]
        public string ReturnOnInvestment { get; set; }

        [IgnoreForLogging]
        [DisplayName("(Baseline Scenario) Payback Period")]
        public string PaybackPeriod { get; set; }

        [IgnoreForLogging]
        [DisplayName("Project Rank")]
        public string? ProjectRank { get; set; }

        [IgnoreForLogging]
        [DisplayName("Project Champion")]
        [RegularExpression("^[a-zA-Z-' ]*$", ErrorMessage = "*")]
        public string? ProjectChampion { get; set; }

        [IgnoreForLogging]
        [DisplayName("Champion Phone")]
        [RegularExpression(@"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$", ErrorMessage = "Invalid Phone")]
        [Phone(ErrorMessage = "Invalid Phone Number")]
        public string? ChampionPhone { get; set; }

        [IgnoreForLogging]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid email address")]
        [DisplayName("Champion Email")]
        public string? ChampionEmail { get; set; }


        [IgnoreForLogging]
        [DisplayName("Is this project moving forward?")]
        public bool IsMovingForward { get; set; }

        [IgnoreForLogging]
        [DisplayName("Cancelled or Not Approved?")]
        public int IsCancelledOrNotApproved { get; set; }

        [IgnoreForLogging]
        [DisplayName("Why isn’t the project moving forward?")]
        public string? ReasonNotMovingForward { get; set; }


        [IgnoreForLogging]
        [DisplayName("Attachments (File name cannot exceed 255 characters.)")]
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();


        #endregion
        #region Attach and Submit Page
        [IgnoreForLogging]
        [DisplayName("Add an Attachment")]
        public List<IFormFile>? AttachmentFiles { get; set; }

        [IgnoreForLogging]
        [DisplayName("Add an Attachment")]
        public List<IFormFile>? AddInfoFiles { get; set; }
        [IgnoreForLogging]
        [DisplayName("Status")]
        public int Status { get; set; }
        #endregion

        [IgnoreForLogging]
        // [DisplayName("Is this a System led project managed by the EPMO team?")]
        public int IsProjectManagerDesired { get; set; } = (int)ProjectManagerDesired.No;

        [IgnoreForLogging]
        [DisplayName("Does this project affect multiple regions/markets/segments?")]
        public bool AffectsMultipleSegments { get; set; }

        [IgnoreForLogging]
        [DisplayName("What is the project's approximate desired start date?")]
        public DateTime? StartDate { get; set; }

        [IgnoreForLogging]
        [DisplayName("What is the project's estimated end date?")]
        public DateTime? EndDate { get; set; }       

        public Guid WorkflowId { get; set; }

        [IgnoreForLogging]
        public List<string> FileNames { get; set; } = new List<string>();

        

        [IgnoreForLogging]
        public int StepNumber { get; set; }

        [IgnoreForLogging]
        public string ActiveTab { get; set; } = string.Empty;

        [IgnoreForLogging]
        public string ActiveView { get; set; } = string.Empty;

        [IgnoreForLogging]
        public string ActionType { get; set; } = string.Empty;

        [IgnoreForLogging]
        public string ResponseMessage { get; set; } = string.Empty;

        [IgnoreForLogging]
        public string ResponseMessageTimeOut { get; set; }

        [IgnoreForLogging]
        public string CreatedBy { get; set; } = string.Empty;

        [IgnoreForLogging]
        public DateTime Created { get; set; }

        [IgnoreForLogging]
        public DateTime? Updated { get; set; }

        [IgnoreForLogging]
        public string UpdatedBy { get; set; } = string.Empty;

        [IgnoreForLogging]
        public DateTime? Cancelled { get; set; }

        [IgnoreForLogging]
        public string? CancelledBy { get; set; } = string.Empty;

        [IgnoreForLogging]
        public DateTime? Overridden { get; set; }

        [IgnoreForLogging]
        public string? OverriddenBy { get; set; } = string.Empty;

        [IgnoreForLogging]
        public int ProvidedInfoId { get; set; }

        [IgnoreForLogging]
        public bool IsVpOfOps { get; set; }

        [IgnoreForLogging]
        public bool VerifyAndSendToVPFinance { get; set; }

        [IgnoreForLogging]
        public bool WBSApproved { get; set; }

        [IgnoreForLogging]
        public string Action { get; set; }

        [IgnoreForLogging]
        public string SaveAction { get; internal set; }

        [IgnoreForLogging]
        public bool SubmitButtonPressed { get; set; }

        [IgnoreForLogging]
        public string QuoteTypeText { get; set; }

        [IgnoreForLogging]
        public string AttachmentTypeText { get; set; }

        [IgnoreForLogging]
        public bool UserCanEdit { get; set; }

        [IgnoreForLogging]
        public bool ShowWorkflowAction { get; set; }

        [IgnoreForLogging]
        public bool IsAdmin { get; set; }

        [IgnoreForLogging]
        public bool DoNotProcess { get; set; }

        [IgnoreForLogging]
        public int HttpRequestTimeout { get; set; }

        //public List<RequestedInfoThread> RequestedInfoThreads { get; set; }

        [IgnoreForLogging]
        public int OriginalCapitalFundingYear { get; set; }

        [IgnoreForLogging]
        public string SubmitButtonCaption { get; set; }

        [IgnoreForLogging]
        public ReviewerGroup ReplyingGroup { get; set; } = new ReviewerGroup();


        [IgnoreForLogging]
        public RequestedInfo RequestedInfo { get; set; } = new RequestedInfo();

        [IgnoreForLogging]
        public Reviewer Reviewer { get; set; } = new Reviewer();

        [IgnoreForLogging]
        public ProvidedInfo ProvidedInfo { get; set; } = new ProvidedInfo();

        [IgnoreForLogging]
        public List<ReviewerGroup> ReviewerGroups { get; set; } = new List<ReviewerGroup>();

        [IgnoreForLogging]

        public ReviewerGroup RequestingGroup { get; set; } = new ReviewerGroup();

        public string ReturnedInformation { get; set; } = string.Empty;
        public string ButtonCaption { get; set; }

        public string ExpectedMessage { get; set; }


        public WorkFlowStepViewModel WorkflowStep { get; set; }

        [IgnoreForLogging]
        public List<WorkFlowStepOptionViewModel> WorkflowStepOptions { get; set; }

        [IgnoreForLogging]
        public Guid WorkflowStepId { get; set; }

        public int ReviewerGroupId { get; set; }

        public string ReviewerGroupName { get; set; }

        public int RequestingGroupId { get; set; }


        public int ReplyingGroupId { get; set; }

        public int ReviewerId { get; set; }

        public int RequestedInfoId { get; set; }

    }
}
