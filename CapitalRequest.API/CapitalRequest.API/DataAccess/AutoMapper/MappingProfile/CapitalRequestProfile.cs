using AutoMapper;
using CapitalRequest.DATA.Model;
using CapitalRequest.DATA.Repositories;
using CapitalRequest.UI.Enum;
using CapitalRequest.UI.Models;
using CapitalRequest.UI.Utilities;
using CapitalRequest.UI.Wrappers;
using SSMWorkflow.Data.DataAccess.Models;
using SSMWorkflow.Data.Models;
using Dashboard = SSMWorkflow.Data.DataAccess.Models.Dashboard;

using db = CapitalRequest.DATA.Model;
using vm = CapitalRequest.UI.Models;

namespace CapitalRequest.API.DataAccess.AutoMapper.MappingProfile
{
    public class CapitalRequestProfile : Profile
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IActiveDirectoryWrapper _activeDirectoryWrapper;

        public CapitalRequestProfile(IHttpContextAccessor httpContextAccessor, IActiveDirectoryWrapper activeDirectoryWrapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _activeDirectoryWrapper = activeDirectoryWrapper;

            //Home
            CreateMap<db.AnnualCapitalProcess, vm.AnnualCapitalProcess>();
            CreateMap<vm.AnnualCapitalProcess, db.AnnualCapitalProcess>()
                .ForMember(dest => dest.Created, o => o.MapFrom(src => src.Type == Constants.ANNUALCAPITALPROCESS_TYPE_ADD ? src.Created : src.Created == DateTime.MinValue ? DateTime.Now : src.Created))
                .ForMember(dest => dest.CreatedBy, o => o.MapFrom(src => src.Type == Constants.ANNUALCAPITALPROCESS_TYPE_ADD ? src.CreatedBy : string.IsNullOrWhiteSpace(src.CreatedBy) ? GetUserId() : src.CreatedBy))
                .ForMember(dest => dest.Updated, o => o.MapFrom(src => src.Type == Constants.ANNUALCAPITALPROCESS_TYPE_ADD ? DateTime.Now : (DateTime?)null))
                .ForMember(dest => dest.UpdatedBy, o => o.MapFrom(src => src.Type == Constants.ANNUALCAPITALPROCESS_TYPE_ADD ? GetUserId() : null));

            //Proposal
            CreateMap<db.Proposal, vm.Proposal>()
                .ForMember(dest => dest.CostCenter, o => o.MapFrom(src => src.CostCenter.ToString()))
                .ForMember(dest => dest.TotalProjectCost, o => o.MapFrom(src => Money.Convert(src.TotalProjectCost)))
                .ForMember(dest => dest.SalesTax, o => o.MapFrom(src => Money.Convert(src.SalesTax)))
                .ForMember(dest => dest.FreightAmount, o => o.MapFrom(src => Money.Convert(src.FreightAmount)))
                .ForMember(dest => dest.TotalCapital, o => o.MapFrom(src => Money.Convert(src.TotalCapital)))
                .ForMember(dest => dest.Year1, o => o.MapFrom(src => Money.Convert(src.Year1)))
                .ForMember(dest => dest.Year2, o => o.MapFrom(src => Money.Convert(src.Year2)))
                .ForMember(dest => dest.Year3, o => o.MapFrom(src => Money.Convert(src.Year3)))
                .ForMember(dest => dest.NetPresentValue, o => o.MapFrom(src => Money.Convert(src.NetPresentValue)))
                .ForMember(dest => dest.InternalRateOfReturn, o => o.MapFrom(src => Percentage.Convert(src.InternalRateOfReturn)))
                .ForMember(dest => dest.IsMovingForward, o => o.MapFrom(src => src.IsMovingForward == null ? true : src.IsMovingForward))
                .ForMember(dest => dest.SubmitButtonPressed, o => o.MapFrom(src => false))
                .ForMember(dest => dest.ReturnOnInvestment, o => o.MapFrom(src => Percentage.Convert(src.ReturnOnInvestment)))
                .ForMember(dest => dest.OriginalCapitalFundingYear, o => o.MapFrom(src => src.CapitalFundingYear))
                .ForMember(dest => dest.IncludePurchasingGroup, o => o.MapFrom(src => src.IncludePurchasingGroup == null ? true : src.IncludePurchasingGroup));

            CreateMap<vm.Proposal, db.Proposal>()
                .ForMember(dest => dest.CostCenter, o => o.MapFrom(src => Convert.ToInt64(src.CostCenter)))
                .ForMember(dest => dest.TotalProjectCost, o => o.MapFrom(src => src.TotalProjectCost == null ? 0.0m : Money.Convert(src.TotalProjectCost)))
                .ForMember(dest => dest.SalesTax, o => o.MapFrom(src => Money.Convert(src.SalesTax)))
                .ForMember(dest => dest.FreightAmount, o => o.MapFrom(src => Money.Convert(src.FreightAmount)))
                .ForMember(dest => dest.TotalCapital, o => o.MapFrom(src => Money.Convert(src.TotalCapital)))
                .ForMember(dest => dest.Year1, o => o.MapFrom(src => Money.Convert(src.Year1)))
                .ForMember(dest => dest.Year2, o => o.MapFrom(src => Money.Convert(src.Year2)))
                .ForMember(dest => dest.Year3, o => o.MapFrom(src => Money.Convert(src.Year3)))
                .ForMember(dest => dest.NetPresentValue, o => o.MapFrom(src => Money.Convert(src.NetPresentValue)))
                .ForMember(dest => dest.InternalRateOfReturn, o => o.MapFrom(src => Percentage.Convert(src.InternalRateOfReturn)))
                .ForMember(dest => dest.ReturnOnInvestment, o => o.MapFrom(src => Percentage.Convert(src.ReturnOnInvestment)))
                .ForMember(dest => dest.SegmentId, o => o.MapFrom(src => src.Segment == null ? src.SegmentId : GetSegmentId(src.Segment, src.Region)))
                .ForMember(dest => dest.Created, o => o.MapFrom(src => src.IsEdit ? src.Created : src.Created == DateTime.MinValue ? DateTime.Now : src.Created))
                .ForMember(dest => dest.CreatedBy, o => o.MapFrom(src => src.IsEdit ? src.CreatedBy : string.IsNullOrWhiteSpace(src.CreatedBy) ? GetUserId() : src.CreatedBy))
                .ForMember(dest => dest.Updated, o => o.MapFrom(src => src.IsEdit ? DateTime.Now : (DateTime?)null))
                .ForMember(dest => dest.UpdatedBy, o => o.MapFrom(src => src.IsEdit ? GetUserId() : null))
                .ForMember(dest => dest.Overridden, o => o.MapFrom(src => src.OverrideWorkflow && src.Overridden == null ? DateTime.Now : src.Overridden))
                .ForMember(dest => dest.OverriddenBy, o => o.MapFrom(src => src.OverrideWorkflow && src.Overridden == null ? GetUserId() : src.OverriddenBy));

            CreateMap<vm.Proposal, vm.Proposal>()
                    .ForMember(dest => dest.IsProjectOverOneMillionDollars, o => o.MapFrom(src => false))
                    .ForMember(dest => dest.IsFundedFromBaseCapital, o => o.MapFrom(src => true))
                    .ForMember(dest => dest.IsPartOfAnnualCapitalReviewProcessNOTOffCycleRequest, o => o.MapFrom(src => false))
                    .ForMember(dest => dest.IsProjectManagerDesired, o => o.MapFrom(src => (int)ProjectManagerDesired.No))
                    .ForMember(dest => dest.AffectsMultipleSegments, o => o.MapFrom(src => false))
                    .ForMember(dest => dest.IsMovingForward, o => o.MapFrom(src => true))
                    .ForMember(dest => dest.SubmitButtonPressed, o => o.MapFrom(src => false))
                    .ForMember(dest => dest.Author, o => o.MapFrom(src => GetUserName()))
                    .ForMember(dest => dest.AuthorEmail, o => o.MapFrom(src => GetUserEmail()))
                    .ForMember(dest => dest.UserId, o => o.MapFrom(src => GetUserId()))
                    .ForMember(dest => dest.ButtonMode, o => o.MapFrom(src => Constants.DISPLAY_MODE_NEW));

            // Quotes
            CreateMap<vm.Quote, db.Quote>();
            CreateMap<db.Quote, vm.Quote>();

            //WBS
            CreateMap<vm.Proposal, vm.WBS>()
                .ForMember(dest => dest.ProposalId, o => o.MapFrom(src => src.Id))
                .ForMember(dest => dest.Id, o => o.MapFrom(src => Constants.NEW_ID))
                .ForMember(dest => dest.Created, o => o.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedBy, o => o.MapFrom(src => GetUserId()));

            CreateMap<vm.WBS, WbsNumber>().ConvertUsing<WBS_TO_WBSNUMBER>();

            CreateMap<vm.WBS, db.WBS>()
                .ForMember(dest => dest.Created, o => o.MapFrom(src => src.Id != 0 ? src.Created : src.Created == null ? DateTime.Now : src.Created))
                .ForMember(dest => dest.CreatedBy, o => o.MapFrom(src => src.Id != 0 ? src.CreatedBy : string.IsNullOrWhiteSpace(src.CreatedBy) ? GetUserId() : src.CreatedBy))
                .ForMember(dest => dest.Updated, o => o.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.UpdatedBy, o => o.MapFrom(src => GetUserId()));

            CreateMap<db.WBS, vm.WBS>()
                .ForMember(dest => dest.WbsNumber, o => o.MapFrom(src => new WbsNumber()));

            CreateMap<WBSApproval, db.Proposal>()
                .ForMember(dest => dest.Id, o => o.MapFrom(src => src.ProposalId))
                .ForMember(dest => dest.CapitalPoolIdentifiers, o => o.MapFrom(src => src.CapitalPoolIdentifierId))
                .ForMember(dest => dest.CapitalPool, o => o.MapFrom(src => src.CapitalPoolId))
                .ForMember(dest => dest.Updated, o => o.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.UpdatedBy, o => o.MapFrom(src => GetUserId()));

            //WorkFlow
            //TODO Constants and verify values

            CreateMap<vm.Proposal, CreateUpdateWorkFlow>()
                .ForMember(dest => dest.WorkflowName, o => o.MapFrom<string>(src => src.ProjectName))
                .ForMember(dest => dest.WorkflowDescription, o => o.MapFrom<string>(src => src.ProjectDescription))
                .ForMember(dest => dest.ValidFrom, o => o.MapFrom(src => DateTime.Today))
                .ForMember(dest => dest.StakeholderNotificationType, o => o.MapFrom(src => Constants.STAKE_HOLDER_NOTIFICATION_TYPE))
                .ForMember(dest => dest.CompleteMessage, o => o.MapFrom(src => Constants.COMPLETE_MESSAGE))
                .ForMember(dest => dest.CancelledMessage, o => o.MapFrom(src => Constants.CANCELLED_MESSAGE))
                .ForMember(dest => dest.Created, o => o.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedBy, o => o.MapFrom(src => GetUserId()));

            //WorkflowStep
            CreateMap<WorkflowTemplate, CreateUpdateWorkFlowStep>()
                .ForMember(dest => dest.StepName, o => o.MapFrom(src => src.StepName))
                .ForMember(dest => dest.StepDescription, o => o.MapFrom(src => src.StepDescription))
                .ForMember(dest => dest.StakeholderMessage, o => o.MapFrom(src => src.StepDescription))
                .ForMember(dest => dest.Created, o => o.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedBy, o => o.MapFrom(src => GetUserId())); ;

            //WorkFlowStakeholder
            CreateMap<vm.ReviewerGroup, CreateUpdateWorkFlowStakeholder>()
                .ForMember(dest => dest.Stakeholder, o => o.MapFrom(src => src.Name))
                .ForMember(dest => dest.isGroup, o => o.MapFrom(src => true))
                .ForMember(dest => dest.Created, o => o.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedBy, o => o.MapFrom(src => GetUserId()));

            //WorkflowStepOption
            CreateMap<vm.Reviewer, CreateUpdateWorkFlowStepOption>()
                .ForMember(dest => dest.OptionName, o => o.MapFrom(src => src.Email))
                .ForMember(dest => dest.Created, o => o.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedBy, o => o.MapFrom(src => GetUserId()));

            CreateMap<ActiveDirectoryUser, vm.Reviewer>()
                .ForMember(dest => dest.Email, o => o.MapFrom(src => src.Email.ToLower()))
                .ForMember(dest => dest.Created, o => o.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedBy, o => o.MapFrom(src => GetUserId()));

            CreateMap<WorkFlowStepOptionViewModel, CreateUpdateWorkFlowStepOption>();

            //WorkflowInstance
            CreateMap<CreateUpdateWorkFlowStep, CreateUpdateWorkFlowInstance>()
                .ForMember(dest => dest.CurrentWorkflowState, o => o.MapFrom(src => src.StepName));

            CreateMap<WorkflowInstance, WorkFlowInstanceViewModel>();

            CreateMap<WorkFlowInstanceViewModel, WorkflowInstance>();

            CreateMap<CreateUpdateWorkFlowInstance, WorkflowInstance>();

            CreateMap<WorkflowInstance, CreateUpdateWorkFlowInstanceActionHistory>()
               .ForMember(dest => dest.WorkflowInstanceID, o => o.MapFrom(src => src.WorkflowInstanceID))
               .ForMember(dest => dest.WorkflowStepID, o => o.MapFrom(src => src.CurrentWorkflowStepID))
               .ForMember(dest => dest.Completed, o => o.MapFrom(src => DateTime.Now))
               .ForMember(dest => dest.CompletedBy, o => o.MapFrom(src => GetUserId()));

            //WorkflowStepResponder
            CreateMap<WorkFlowStepOptionViewModel, CreateUpdateWorkFlowStepResponder>()
                .ForMember(dest => dest.isGroup, o => o.MapFrom(src => src.ReviewerGroupId != 0))
                .ForMember(dest => dest.WorkflowStepOptionID, o => o.MapFrom(src => src.OptionID))
                .ForMember(dest => dest.Created, o => o.MapFrom(src => DateTime.Now));

            CreateMap<vm.Proposal, db.ProvidedInfo>()
               .ForMember(dest => dest.RequestedInfoId, o => o.MapFrom(src => src.ProvidedInfo.RequestedInfoId))
               .ForMember(dest => dest.ProvidedInformation, o => o.MapFrom(src => src.ProvidedInfo.ProvidedInformation))
               .ForMember(dest => dest.ReviewerId, o => o.MapFrom(src => src.ProvidedInfo.ReviewerId))
               .ForMember(dest => dest.Created, o => o.MapFrom(src => DateTime.Now))
               .ForMember(dest => dest.CreatedBy, o => o.MapFrom(src => GetUserId()));

            CreateMap<vm.Proposal, db.RequestedInfo>()
                .ForMember(dest => dest.ProposalId, o => o.MapFrom(src => src.Id))
                .ForMember(dest => dest.RequestingReviewerGroupId, o => o.MapFrom(src => src.ReviewerGroupId))
                .ForMember(dest => dest.ReviewerGroupId, o => o.MapFrom(src => src.RequestedInfo.ReviewerGroupId))
                .ForMember(dest => dest.RequestedInformation, o => o.MapFrom(src => src.RequestedInfo.RequestedInformation))
                .ForMember(dest => dest.RequestingReviewerId, o => o.MapFrom(src => src.RequestedInfo.RequestingReviewerId))
                .ForMember(dest => dest.WorkflowStepOptionId, o => o.MapFrom(src => src.RequestedInfo.WorkflowStepOptionId))
                .ForMember(dest => dest.Created, o => o.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedBy, o => o.MapFrom(src => GetUserId()))
                .ForMember(dest => dest.IsOpen, o => o.MapFrom(src => true));

            CreateMap<db.RequestedInfo, vm.RequestedInfo>();

            CreateMap<vm.RequestedInfo, db.RequestedInfo>()
                .ForMember(dest => dest.Updated, o => o.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.UpdatedBy, o => o.MapFrom(src => GetUserId()))
                .ForMember(dest => dest.IsOpen, o => o.MapFrom(src => false));

            CreateMap<vm.ProvidedInfo, db.ProvidedInfo>();

            CreateMap<db.ReviewerGroup, vm.ReviewerGroup>();

            CreateMap<db.Asset, vm.Asset>()
                .ForMember(dest => dest.AssetNumberReadOnly, o => o.MapFrom(src => src.AssetNumber));
            CreateMap<vm.Asset, db.Asset>();

            CreateMap<vm.Proposal, vm.Asset>()
                .ForMember(dest => dest.ProposalId, o => o.MapFrom(src => src.Id))
                .ForMember(dest => dest.Deleted, o => o.MapFrom(src => false))
                .ForMember(dest => dest.Id, o => o.MapFrom(src => 0));

            CreateMap<db.WbsTemplate, vm.WbsTemplate>();
            CreateMap<db.AssetTemplate, vm.AssetTemplate>();

            CreateMap<vm.Proposal, vm.Reviewer>()
                .ForMember(dest => dest.RegionId, o => o.MapFrom(src => src.Region))
                .ForMember(dest => dest.SegmentId, o => o.MapFrom(src => src.SegmentId))
                .ForMember(dest => dest.ReviewerGroupId, o => o.MapFrom(src => src.ReviewerGroupId))
                .ForMember(dest => dest.UserId, o => o.MapFrom(src => GetUserId()))
                .ForMember(dest => dest.FirstName, o => o.MapFrom(src => GetFirstName(GetFullName(GetUserId()))))
                .ForMember(dest => dest.LastName, o => o.MapFrom(src => GetLastName(GetFullName(GetUserId()))))
                .ForMember(dest => dest.FullName, o => o.MapFrom(src => GetFullName(GetUserId())))
                .ForMember(dest => dest.Email, o => o.MapFrom(src => GetUserEmail()))
                .ForMember(dest => dest.Created, o => o.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedBy, o => o.MapFrom(src => GetUserId()));

            CreateMap<db.Reviewer, vm.Reviewer>()
                .ForMember(dest => dest.RegionName, o => o.MapFrom(src => src.RegionName == null ? GetNameFromTable("Region", src.RegionId) : src.RegionName))
                .ForMember(dest => dest.SegmentName, o => o.MapFrom(src => src.SegmentName == null ? GetNameFromTable("Segment", src.SegmentId) : src.SegmentName))
                .ForMember(dest => dest.ReviewerGroupName, o => o.MapFrom(src => src.ReviewerGroupName == null ? GetNameFromTable("ReviewerGroup", src.ReviewerGroupId) : src.ReviewerGroupName));

            CreateMap<vm.Reviewer, db.Reviewer>()
                .ForMember(dest => dest.Created, o => o.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedBy, o => o.MapFrom(src => GetUserId()));

            CreateMap<vm.ReviewerMaintenance, db.Reviewer>()
                .ForMember(dest => dest.FirstName, o => o.MapFrom(src => GetFirstName(src.FullName)))
                .ForMember(dest => dest.LastName, o => o.MapFrom(src => GetLastName(src.FullName)))
                .ForMember(dest => dest.FullName, o => o.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Created, o => o.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedBy, o => o.MapFrom(src => GetUserId()));

            CreateMap<db.Reviewer, db.ApplicationUser>()
                .ForMember(dest => dest.ApplicationRoleId, o => o.MapFrom(src => GetApplicationRoleId(Constants.APPLICATION_ROLE_NAME_REVIEWER)))
                .ForMember(dest => dest.Created, o => o.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedBy, o => o.MapFrom(src => GetUserId()));

            CreateMap<Dashboard, SSMWorkflow.Data.Models.Dashboard>();

            CreateMap<ActiveDirectoryUser, vm.ApplicationUser>()
                .ForMember(dest => dest.Created, o => o.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedBy, o => o.MapFrom(src => GetUserId()));

            CreateMap<db.ApplicationUser, vm.ApplicationUser>();

            CreateMap<vm.ApplicationUser, vm.ApplicationUser>();

            CreateMap<vm.ApplicationUser, db.ApplicationUser>()
                .ForMember(dest => dest.Updated, o => o.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.UpdatedBy, o => o.MapFrom(src => GetUserId()));

            CreateMap<vm.AccessMaintenance, db.ApplicationUser>()
                .ForMember(dest => dest.Created, o => o.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedBy, o => o.MapFrom(src => GetUserId()));

            CreateMap<db.UserRegion, vm.UserRegion>()
                .ForMember(dest => dest.RegionName, o => o.MapFrom(src => src.RegionId == null ? string.Empty : GetRegionName(src.RegionId)))
                .ForMember(dest => dest.FullName, o => o.MapFrom(src => src.RegionId == null ? string.Empty : GetFullName(src.UserId)))
                .ForMember(dest => dest.ApplicationRoleName, o => o.MapFrom(src => Constants.APPLICATION_ROLE_NAME_REGIONAL));

            CreateMap<vm.AccessMaintenance, db.UserRegion>()
                .ForMember(dest => dest.Created, o => o.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedBy, o => o.MapFrom(src => GetUserId()));

            CreateMap<vm.AccessMaintenance, db.Reviewer>()
                .ForMember(dest => dest.Email, o => o.MapFrom(src => src.Email.ToLower()))
                .ForSourceMember(dest => dest.RegionId, o => o.DoNotValidate())
                .ForMember(dest => dest.Email, o => o.MapFrom(src => src.Email.ToLower()))
                .ForMember(dest => dest.Created, o => o.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedBy, o => o.MapFrom(src => GetUserId()));

            CreateMap<db.Attachment, vm.Attachment>();

            CreateMap<vm.Proposal, db.Attachment>()
                .ForMember(dest => dest.ProposalId, o => o.MapFrom(src => src.Id))
                .ForMember(dest => dest.DateUploaded, o => o.MapFrom(src => DateTime.Now));

            CreateMap<vm.Proposal, db.Quote>()
                .ForMember(dest => dest.ProposalId, o => o.MapFrom(src => src.Id))
                .ForMember(dest => dest.DateUploaded, o => o.MapFrom(src => DateTime.Now));

            CreateMap<WorkFlowStepViewModel, WorkflowStep>()
                .ForMember(dest => dest.Updated, o => o.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.UpdatedBy, o => o.MapFrom(src => GetUserId()))
                .ForMember(dest => dest.IsComplete, o => o.MapFrom(src => true));

            CreateMap<db.AnnualCapitalProcess, vm.AnnualCapitalProcess>();


            CreateMap<vm.WACC, vm.WACC>()
                .ForMember(dest => dest.Type, o => o.MapFrom(src => "Add WACC"))
                .ForMember(dest => dest.Created, o => o.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedBy, o => o.MapFrom(src => GetUserId()));

            CreateMap<vm.WACC, db.WACC>()
                .ForMember(dest => dest.Amount, o => o.MapFrom(src => Money.Convert(src.Amount)))
                .ForMember(dest => dest.Updated, o => o.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.UpdatedBy, o => o.MapFrom(src => GetUserId()));

            CreateMap<db.WACC, vm.WACC>()
                .ForMember(dest => dest.Type, o => o.MapFrom(src => "Edit WACC"))
                .ForMember(dest => dest.Amount, o => o.MapFrom(src => Money.Convert(src.Amount)));

            CreateMap<db.ReportGroup, vm.ReportGroup>();
            CreateMap<db.Report, vm.Report>();

            CreateMap<db.Region, vm.Region>()
                .ForMember(dest => dest.ActiveStatus, o => o.MapFrom(src => src.ActiveStatus == null ? 0 : src.ActiveStatus));

            CreateMap<vm.Region, db.Region>();

            CreateMap<db.Segment, vm.Segment>();

            CreateMap<vm.Segment, db.Segment>();

            CreateMap<db.RequestedInfoThread, vm.RequestedInfoThread>();
            CreateMap<db.WorkflowAction, vm.WorkflowAction>();
        }

        private static int GetApplicationRoleId(string applicationRoleName)
        {
            var applicationRoleRepo = new ApplicationRoleRepository();

            return applicationRoleRepo.GetApplicationRoleByName(applicationRoleName).Id;
        }

        private static string GetRegionName(int id)
        {
            var regionRepository = new RegionRepository();

            var region = regionRepository.GetRegionById(id);

            if (region == null)
            {
                return null;
            }
            else
            {
                return region.Name; ;
            }
        }

        private static string GetFullName(string userId)
        {
            var applicationUserRepo = new ApplicationUserRepository();
            var applicationUser = applicationUserRepo.GetApplicationUserbyUserId(userId);

            if (applicationUser == null)
            {
                return null;
            }
            else
            {
                return applicationUser.FullName; ;
            }
        }

        private static string GetApplicationRoleName(int id)
        {
            var applicationRoleRepo = new ApplicationRoleRepository();
            var application = applicationRoleRepo.GetApplicationRoleById(id);

            return application.Name; ;
        }

        //TODO Refactor into utility
        private string GetFirstName(string fullName)
        {
            var names = fullName.Split();
            return names[0];
        }

        private string GetLastName(string fullName)
        {
            var names = fullName.Split();
            return names[1];
        }

        public virtual string GetUserId()
        {
            var userId = string.Empty;
            var httpContext = _httpContextAccessor.HttpContext;
            var currentUser = httpContext.User;

            if (currentUser != null &&
                currentUser.Identity != null &&
                currentUser.Identity.Name != null)
            {
                string? user = currentUser.Identity.Name.ToString();
                if (user != null)
                {
                    var userInfo = user.Split('\\');
                    if (userInfo.Count() > 1)
                    {
                        userId = userInfo[1];
                    }
                    else
                    {
                        userId = userInfo[0];
                    }
                }
            }
            return userId;
        }

        private string GetUserName()
        {
            var userName = _activeDirectoryWrapper.GetDisplayName(GetUserId());
            if (userName.Contains(','))
            {
                string[] UserDisplayName = userName.Split(',');
                return $"{UserDisplayName[1].Trim()} {UserDisplayName[0].Trim()} ";
            }
            else
            {
                return string.Empty;
            }
        }

        private string GetUserEmail()
        {
            return _activeDirectoryWrapper.GetMail(GetUserId()).ToLower();
        }

        private int? GetSegmentId(string segmentNumber, int regionId)
        {
            var segmentRepository = new SegmentRepository();
            var segment = segmentRepository.GetSegmentByNumber(segmentNumber, regionId);

            if (segment == null)
            {
                return null;
            }
            else
            {
                return segment.Id;
            }
        }

        private string GetNameFromTable(string table, int? id)
        {
            var name = string.Empty;

            if (id != null)
            {
                switch (table)
                {
                    //TODO Constants
                    case "Region":
                        name = new RegionRepository().GetRegionById(id.Value).Name;
                        break;

                    case "Segment":
                        var segment = new SegmentRepository().GetSegmentById(id.Value);
                        name = $"{segment.SegmentNumber} - {segment.LocationName}";
                        break;

                    case "ReviewerGroup":
                        var reviewerGroup = new ReviewerGroupRepository().GetReviewerGroupById(id.Value);
                        name = $"{reviewerGroup.StepNumber} - {reviewerGroup.Name} - {reviewerGroup.ReviewerType}";

                        break;

                    default:
                        break;
                }
            }

            return name;
        }
    }
}