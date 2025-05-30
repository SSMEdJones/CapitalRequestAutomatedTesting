using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        [DisplayName("Project Name")]
        public string? ProjectName { get; set; }

        [DisplayName("Project CE Number")]
        public string? ProjectCENumber { get; set; }

        [DisplayName("Region")]
        public int Region { get; set; }

        [DisplayName("Segment")]
        public string Segment { get; set; }

        public int SegmentId { get; set; }

        [DisplayName("Company Code")]
        public string CompanyCode { get; set; }

        [DisplayName("Author")]
        [RegularExpression("^[a-zA-Z-' ]*$", ErrorMessage = "*")]
        public string? Author { get; set; }
        public string UserId { get; set; } // AuthorId

        [DisplayName("Project Description (Please provide a few sentences describing the project, including what markets/regions/programs will be impacted)")]
        public string? ProjectDescription { get; set; }

        [DisplayName("In a few sentences, describe the overall goal and business objectives of the project")]
        public string? OverallGoal { get; set; }

        [DisplayName("What would be the impact if this project isn't implemented?")]
        public string? ImpactIfNotImplemented { get; set; }

        [RegularExpression("^\\$?([1-9]\\d{0,2}(,\\d{3})*(\\.\\d{2})?|[1-9]\\d*(\\.\\d{2})?|0?\\.(?!00)\\d{2})$", ErrorMessage = "Enter number greater than $0")]
        [DisplayName("Total Project Cost")]
        public string TotalProjectCost { get; set; }

        [DisplayFormat(DataFormatString = "{0:C2}")]
        [DisplayName("Sales tax")]
        public string SalesTax { get; set; }

        [DisplayFormat(DataFormatString = "{0:C2}")]
        [DisplayName("Freight Amount")]
        public string FreightAmount { get; set; }

        [DisplayName("Cost Center")]
        public string CostCenter { get; set; }

        [DisplayName("Is the Project Over One Million Dollars?")]
        public bool IsProjectOverOneMillionDollars { get; set; } = false;

        [Phone(ErrorMessage = "Invalid Phone Number")]
        [RegularExpression(@"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$")]
        [DisplayName("Author Phone")]
        public string? AuthorPhone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid email address")]
        [DisplayName("Author Email")]
        public string? AuthorEmail { get; set; }

        [DisplayName(@"Ministry (Include Specific ship to location/address on next page, field ""Delivery Location Extra detail"")")]
        public string? MinistryName { get; set; }

        [DisplayName("Is this being funded from base capital?")]
        public bool IsFundedFromBaseCapital { get; set; } = true;

        [DisplayName("Is this request an OFF-CYCLE request?")]
        public bool IsPartOfAnnualCapitalReviewProcessNOTOffCycleRequest { get; set; }

        [DisplayName("Override Workflow")]
        public bool OverrideWorkflow { get; set; }

        public List<WorkflowAction> WorkflowActions { get; set; }
        public string WorkflowCaption { get; set; } = "Workflow";

        [DisplayName("Does project require PO?")]
        public bool IncludePurchasingGroup { get; set; }
        #endregion

        #region Additional Info Page
        //[DisplayName("Project Status")]
        //public int ProjectStatus { get; set; } = (int)Enum.Status.Pending;

        [StringLength(50)]
        [RegularExpression("^[a-zA-Z-' ]*$", ErrorMessage = "*")]
        [DisplayName("Requester")]
        public string? Requestor { get; set; }

        [Phone(ErrorMessage = "Invalid Phone Number")]
        [RegularExpression(@"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$", ErrorMessage = "Invalid Phone")]
        [DisplayName("Requester Phone")]
        public string? RequestorPhone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid email address")]
        [DisplayName("Requester Email")]
        public string? RequestorEmail { get; set; }

        [StringLength(50)]
        [RegularExpression("^[a-zA-Z-' ]*$", ErrorMessage = "*")]
        [DisplayName("Project Manager")]
        public string? ProjectManager { get; set; }

        [Phone(ErrorMessage = "Invalid Phone Number")]
        [RegularExpression(@"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$", ErrorMessage = "Invalid Phone")]
        [DisplayName("Project Manager Phone")]
        public string? ProjectManagerPhone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid email address")]
        [DisplayName("Project manager email")]
        public string? ProjectManagerEmail { get; set; }

        [DisplayName("Has this been ordered by purchasing?")]
        public bool IsOrderedByPurchasing { get; set; }

        [DisplayName("Quote Signed? If not, please have signed by appropriate person before moving this forward.")]
        public bool IsQuotesSigned { get; set; }

        [DisplayName("Quotes (File name cannot exceed 255 characters.)")]
        public List<Quote> Quotes { get; set; } = new List<Quote>();

        [DisplayName("Multiple WBS Numbers Requested?")]
        public bool IsMultipleWBSNumbersRequested { get; set; }

        [DisplayName("How many WBS numbers are requested?")]
        public int NumberOfWBSNumbersRequested { get; set; }

        //public IList<WBS> WBS { get; set; } = new List<WBS>();

        [DisplayName("Add a Quotes")]
        public List<IFormFile>? QuoteFiles { get; set; }

        [DisplayName("Cost Center Key Contact")]
        [RegularExpression("^[a-zA-Z-' ]*$", ErrorMessage = "*")]

        public string? CostCenterKeyContactName { get; set; }
        [Phone(ErrorMessage = "Invalid Phone Number")]
        [RegularExpression(@"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$", ErrorMessage = "Invalid Phone")]
        [DisplayName("Cost Center Key Contact Phone")]
        public string? CostCenterKeyContactPhone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid email address")]
        [DisplayName("Cost Center Key Contact Email")]
        public string? CostCenterKeyContactEmail { get; set; }

        [DisplayName("Approver Name")]
        [RegularExpression("^[a-zA-Z-' ]*$", ErrorMessage = "*")]
        public string? ApproverName { get; set; }

        [Phone(ErrorMessage = "Invalid Phone Number")]
        [RegularExpression(@"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$", ErrorMessage = "Invalid Phone")]
        [DisplayName("Approver Phone")]
        public string? ApproverPhone { get; set; }

        [DisplayName("Delivery Location")]
        public string? DeliveryLocation { get; set; }


        [DisplayName("Delivery Location Extra Details (Include specific ship to location/address)")]
        public string? DeliveryLocationExtraDetails { get; set; }

        [DisplayName("Year of Purchase")]
        public int? YearOfPurchase { get; set; }

        public IList<Asset> Assets { get; set; } = new List<Asset>();

        [DisplayName("Capital Funding Year")]
        public int CapitalFundingYear { get; set; }

        [DisplayName("Capital Category")]
        public int CapitalCategory { get; set; }

        [DisplayName("Existing asset affected by project (Enter asset number or description, dept, etc.)")]
        public string? AssetNumber { get; set; }

        public int NumberOfAssets { get; set; }
        public int MaxAssets { get; set; }
        [DisplayName("Is this Clinical Equipment?")]
        public bool IsClinicalEquipment { get; set; }

        [DisplayName("Is this a Replacement Item?")]
        public bool IsReplacementItem { get; set; }

        [DisplayName("Is this a transfer?")]
        public bool IsTransfer { get; set; }

        [DisplayName("Is this item being scrapped?")]
        public bool IsScrapped { get; set; }

        [DisplayName("Certification of Need?")]
        public bool IsCertificationNeeded { get; set; }

        [DisplayName("Levelpath")]
        public string ScoutID { get; set; }

        [DisplayName("Capital Pool Identifiers")]
        public int CapitalPoolIdentifiers { get; set; }


        [DisplayName("Capital Pool")]
        public int CapitalPool { get; set; }

        #endregion

        #region Over a Million Page

        [DisplayName("Strategic Alignment and Project Objectives")]
        public string? StrategicAlignmentAndProjectObjectives { get; set; }

        [DisplayName("Project Background")]
        public string? ProjectBackground { get; set; }

        [DisplayName("Project Evaluation")]
        public string? ProjectEvaluation { get; set; }

        [DisplayName("Recommendation")]
        public string? Recommendation { get; set; }

        [DisplayName("Additional Information")]
        public string? AdditionalInformation { get; set; }

        [DisplayName("Financial Information")]
        public string? FinancialInformation { get; set; }

        [DisplayName("Total Capital")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public string TotalCapital { get; set; }

        [DisplayName("Year 1")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public string Year1 { get; set; }

        [DisplayName("Year 2")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public string? Year2 { get; set; }

        [DisplayName("Year 3")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public string? Year3 { get; set; }

        [DisplayName("(Baseline Scenario) Net Present Value")]
        public string NetPresentValue { get; set; }

        [DisplayName("(Baseline Scenario) Internal Rate of Return")]
        public string InternalRateOfReturn { get; set; }

        [DisplayName("(Baseline Scenario) Return on investment")]
        public string ReturnOnInvestment { get; set; }

        [DisplayName("(Baseline Scenario) Payback Period")]
        public string PaybackPeriod { get; set; }

        [DisplayName("Project Rank")]
        public string? ProjectRank { get; set; }

        [DisplayName("Project Champion")]
        [RegularExpression("^[a-zA-Z-' ]*$", ErrorMessage = "*")]
        public string? ProjectChampion { get; set; }

        [DisplayName("Champion Phone")]
        [RegularExpression(@"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$", ErrorMessage = "Invalid Phone")]
        [Phone(ErrorMessage = "Invalid Phone Number")]
        public string? ChampionPhone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid email address")]
        [DisplayName("Champion Email")]
        public string? ChampionEmail { get; set; }


        [DisplayName("Is this project moving forward?")]
        public bool IsMovingForward { get; set; }

        [DisplayName("Cancelled or Not Approved?")]
        public int IsCancelledOrNotApproved { get; set; }

        [DisplayName("Why isn’t the project moving forward?")]
        public string? ReasonNotMovingForward { get; set; }


        [DisplayName("Attachments (File name cannot exceed 255 characters.)")]
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();


        #endregion
        #region Attach and Submit Page
        [DisplayName("Add an Attachment")]
        public List<IFormFile>? AttachmentFiles { get; set; }

        [DisplayName("Add an Attachment")]
        public List<IFormFile>? AddInfoFiles { get; set; }
        [DisplayName("Status")]
        public int Status { get; set; }
        #endregion


        public bool IsEdit { get; set; }
        public bool IsView { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsLocked { get; set; }
        public bool IsMovingForwardDisabled { get; set; }

        //public string ButtonMode { get; set; } = Constants.DISPLAY_MODE_NEW;
        //public ProposalSubmissionPages CurrentPage { get; set; } = ProposalSubmissionPages.BasicInfo;

        //[DisplayName("Is this a System led project managed by the EPMO team?")]
        //public int IsProjectManagerDesired { get; set; } = (int)ProjectManagerDesired.No;

        [DisplayName("Does this project affect multiple regions/markets/segments?")]
        public bool AffectsMultipleSegments { get; set; }

        [DisplayName("What is the project's approximate desired start date?")]
        public DateTime? StartDate { get; set; }
        [DisplayName("What is the project's estimated end date?")]
        public DateTime? EndDate { get; set; }

        [BindProperty]
        public List<SelectListItem> CancellationOptions
        {
            get
            {
                return new List<SelectListItem> {
                    new SelectListItem { Value = "1", Text = "Cancelled" },
                    new SelectListItem { Value = "2", Text = "Not Approved" }
                };
            }
        }

        [BindProperty]
        public List<SelectListItem> ProjectStatusOptions { get; set; }

        [BindProperty]
        public List<SelectListItem> Regions { get; set; }

        [BindProperty]
        public List<SelectListItem> CompanyCodes { get; set; }

        [BindProperty]
        public List<SelectListItem> CostCenters { get; set; }

        [BindProperty]
        public List<SelectListItem> MinistryNames { get; set; }

        [BindProperty]
        public List<SelectListItem> Segments { get; set; }

        [BindProperty]
        public List<SelectListItem> CapitalFundingYears { get; set; }

        [BindProperty]
        public List<SelectListItem> PurchasingYears { get; set; }

        [BindProperty]
        public List<SelectListItem> IncludePurchasingGroupList
        {
            get
            {
                return new List<SelectListItem> {
                    new() { Value = "1", Text = "Yes" },
                    new() { Value = "0", Text = "No" }
                };
            }
        }

        [BindProperty]
        public List<SelectListItem> CapitalPoolIdentifierList { get; set; }

        [BindProperty]
        public List<SelectListItem> CapitalPoolList { get; set; }

        //public List<WbsTemplate> WbsTemplates { get; set; }
        //public List<AssetTemplate> AssetTemplates { get; set; }

        public Guid WorkflowId { get; set; }

        public List<string> FileNames { get; set; } = new List<string>();

        public int ReviewerGroupId { get; set; }
        public int RequestingReviewerGroupId { get; set; }
        public int RequestedInfoId { get; set; }
        public RequestedInfo RequestedInfo { get; set; } = new RequestedInfo();
        public ProvidedInfo ProvidedInfo { get; set; } = new ProvidedInfo();
        public List<ReviewerGroup> ReviewerGroups { get; set; } = new List<ReviewerGroup>();

        public int StepNumber { get; set; }
        public string ActiveTab { get; set; } = string.Empty;
        public string ActiveView { get; set; } = string.Empty;

        public string ActionType { get; set; } = string.Empty;
        public string ResponseMessage { get; set; } = string.Empty;
        public string ResponseMessageTimeOut { get; set; }

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;

        public DateTime? Cancelled { get; set; }
        public string? CancelledBy { get; set; } = string.Empty;

        public DateTime? Overridden { get; set; }
        public string? OverriddenBy { get; set; } = string.Empty;

        public int ProvidedInfoId { get; set; }

        public bool IsVpOfOps { get; set; }

        public bool VerifyAndSendToVPFinance { get; set; }
        public bool WBSApproved { get; set; }

        public string Action { get; set; }
        public string SaveAction { get; internal set; }

        public bool SubmitButtonPressed { get; set; }

        public string QuoteTypeText { get; set; }
        public string AttachmentTypeText { get; set; }

        public bool UserCanEdit { get; set; }
        public bool ShowWorkflowAction { get; set; }
        public bool IsAdmin { get; set; }
        public bool DoNotProcess { get; set; }
        public int HttpRequestTimeout { get; set; }
        //public List<RequestedInfoThread> RequestedInfoThreads { get; set; }
        public int OriginalCapitalFundingYear { get; set; }
        public string SubmitButtonCaption { get; set; }

    }
}
