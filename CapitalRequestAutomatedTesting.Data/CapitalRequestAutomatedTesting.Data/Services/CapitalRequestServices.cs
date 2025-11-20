using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.DataAccess.Services.Api;
using CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.Data.Services
{

    public interface ICapitalRequestServices
    {
        // Assets
        Task<CapitalRequest.API.Models.Asset> GetAsset(int id);
        Task<List<CapitalRequest.API.Models.Asset>> GetAllAssets(AssetSearchFilter filter);
        Task DeleteAllAssets(AssetSearchFilter filter);

        //// Attachments
        Task<CapitalRequest.API.Models.Attachment> GetAttachment(int id);
        Task<List<CapitalRequest.API.Models.Attachment>> GetAllAttachments(AttachmentSearchFilter filter);
        Task DeleteAllAttachments(AttachmentSearchFilter filter);

        //// EmailTemplates
        Task<CapitalRequest.API.Models.EmailTemplate> GetEmailTemplate(int id);
        Task<List<CapitalRequest.API.Models.EmailTemplate>> GetAllEmailTemplates(EmailTemplateSearchFilter filter);

        //// Proposals
        Task<CapitalRequest.API.Models.Proposal> GetProposal(int id);
        Task<List<CapitalRequest.API.Models.Proposal>> GetAllProposals(ProposalSearchFilter filter);

        Task DeleteProposal(int id);

        //// ProvidedInfos
        Task<CapitalRequest.API.Models.ProvidedInfo> GetProvidedInfo(int id);
        Task<List<CapitalRequest.API.Models.ProvidedInfo>> GetAllProvidedInfos(ProvidedInfoSearchFilter filter);
        Task DeleteAllProvidedInfos(ProvidedInfoSearchFilter filter);

        //// Quotes
        Task<CapitalRequest.API.Models.Quote> GetQuote(int id);
        Task<List<CapitalRequest.API.Models.Quote>> GetAllQuotes(QuoteSearchFilter filter);
        Task DeleteAllQuotes(QuoteSearchFilter filter);

        //// RequestedInfos
        Task<CapitalRequest.API.Models.RequestedInfo> GetRequestedInfo(int id);
        Task<List<CapitalRequest.API.Models.RequestedInfo>> GetAllRequestedInfos(RequestedInfoSearchFilter filter);
        Task DeleteAllRequestedInfos(RequestedInfoSearchFilter filter);
        Task<CapitalRequest.API.Models.RequestedInfo> UpdateRequestedInfos(CapitalRequest.API.Models.RequestedInfo requestedInfo);

        //// ReviewerGroups
        Task<CapitalRequest.API.Models.ReviewerGroup> GetReviewerGroup(int id);
        Task<List<CapitalRequest.API.Models.ReviewerGroup>> GetAllReviewerGroups(ReviewerGroupSearchFilter filter);

        //// Reviewers
        Task<CapitalRequest.API.Models.Reviewer> GetReviewer(int id);
        Task<List<CapitalRequest.API.Models.Reviewer>> GetAllReviewers(ReviewerSearchFilter filter);
        Task<List<CapitalRequest.API.Models.Reviewer>> GetReviewers(int segmentId);

        //// WBSs
        Task<CapitalRequest.API.Models.Wbs> GetWbs(int id);
        Task<List<CapitalRequest.API.Models.Wbs>> GetAllWbss(WbsSearchFilter filter);
        Task DeleteAllWbss(WbsSearchFilter filter);

        //// WorkflowTemplates
        Task<CapitalRequest.API.Models.WorkflowTemplate> GetWorkflowTemplate(int id);
        Task<List<CapitalRequest.API.Models.WorkflowTemplate>> GetAllWorkflowTemplates(WorkflowTemplateSearchFilter filter);

        //// WorkflowActions
        Task<List<CapitalRequest.API.Models.WorkflowAction>> GetAllWorkflowActions(WorkflowActionSearchFilter filter);

        //// ApplicationUser
        Task<CapitalRequest.API.Models.ApplicationUser> GetApplicationUser(string userId);
        Task<List<CapitalRequest.API.Models.ApplicationUser>> GetAllApplicationUsers(ApplicationUserSearchFilter filter);

        //// DeletedReviewers
        Task<List<CapitalRequest.API.Models.DeletedReviewer>> GetAllDeletedReviewers(DeletedReviewerSearchFilter filter);

        Task<List<CapitalRequest.API.Models.ProjectType>> GetAllProjectTypes();
        Task<List<CapitalRequest.API.Models.CapitalPoolIdentifier>> GetAllCapitalPoolIdentifiers();
        Task<List<CapitalRequest.API.Models.CapitalPool>> GetAllCapitalPools();

    }

    public class CapitalRequestServices : ICapitalRequestServices
    {
        private readonly IAssets _assets;
        private readonly IAttachments _attachments;
        private readonly IProposals  _proposals;
        private readonly IProvidedInfos _providedInfos;
        private readonly IQuotes _quotes;
        private readonly IRequestedInfos _requestedInfos;
        private readonly IReviewerGroups _reviewerGroups;
        private readonly IReviewers _reviewers;
        private readonly IWBSs _wbss;
        private readonly IWorkflowTemplates _workflowTemplates;
        private readonly IEmailTemplates _emailTemplates;
        private readonly IWorkflowActions _workflowActions;
        private readonly IApplicationUsers _applicationUsers;
        private readonly IDeletedReviewers _deletedReviewers;
        private readonly IProjectTypes _projectTypes;
        private readonly ICapitalPoolIdentifiers _capitalPoolIdentifiers;
        private readonly ICapitalPools _capitalPools;

        public CapitalRequestServices(
            IAssets assets,
            IAttachments attachments,
            IProposals proposals,
            IProvidedInfos providedInfos,
            IQuotes quotes,
            IRequestedInfos requestedInfos,
            IReviewerGroups reviewerGroups,
            IReviewers reviewers,
            IWBSs wbss,
            IWorkflowTemplates workflowTemplates,
            IEmailTemplates emailTemplates,
            IWorkflowActions workflowActions,
            IApplicationUsers applicationUsers,
            IDeletedReviewers deletedReviewers,
            IProjectTypes projectTypes,
            ICapitalPoolIdentifiers capitalPoolIdentifiers,
            ICapitalPools capitalPools)
        {
            _assets = assets;
            _attachments = attachments;
            _proposals = proposals;
            _providedInfos = providedInfos;
            _quotes = quotes;
            _requestedInfos = requestedInfos;
            _reviewerGroups = reviewerGroups;
            _reviewers = reviewers;
            _wbss = wbss;
            _workflowTemplates = workflowTemplates;
            _emailTemplates = emailTemplates;
            _workflowActions = workflowActions;
            _applicationUsers = applicationUsers;
            _deletedReviewers = deletedReviewers;
            _projectTypes = projectTypes;
            _capitalPoolIdentifiers = capitalPoolIdentifiers;
            _capitalPools = capitalPools;
        }

        #region Assets
        public Task<CapitalRequest.API.Models.Asset> GetAsset(int id)
        {
            return _assets.Get(id);
        }

        public Task<List<CapitalRequest.API.Models.Asset>> GetAllAssets(AssetSearchFilter filter)
        {
            return _assets.GetAll(filter);
        }

        public async Task DeleteAllAssets(AssetSearchFilter filter)
        {
           await _assets.DeleteAll(filter);
           
        }
        #endregion
        #region Attachments
        public Task<CapitalRequest.API.Models.Attachment> GetAttachment(int id)
        {
            return _attachments.Get(id);
        }

        public Task<List<CapitalRequest.API.Models.Attachment>> GetAllAttachments(AttachmentSearchFilter filter)
        {
            return _attachments.GetAll(filter); 
        }

        public async Task DeleteAllAttachments(AttachmentSearchFilter filter)
        {
            await _attachments.DeleteAll(filter);

        }
        #endregion

        #region EmailTemplates
        public Task<CapitalRequest.API.Models.EmailTemplate> GetEmailTemplate(int id)
        {
            return _emailTemplates.Get(id);
        }

        public Task<List<CapitalRequest.API.Models.EmailTemplate>> GetAllEmailTemplates(EmailTemplateSearchFilter filter)
        {
            return _emailTemplates.GetAll(filter);
        }
        #endregion


        #region Proposals
        public Task<CapitalRequest.API.Models.Proposal> GetProposal (int id)
        {
            return _proposals.Get(id);
        }
        public Task<List<CapitalRequest.API.Models.Proposal>> GetAllProposals(ProposalSearchFilter filter)
        {
            return _proposals.GetAll(filter);

        }

        public async Task DeleteProposal(int id)
        {
            await _proposals.Delete(id);
            //delete CapitalRequest..Proposal
            //delete CapitalRequest..RequestedInfo
            //delete CapitalRequest..ProvidedInfo

            //delete from WorkflowInstanceActionHistory
            //delete from WorkflowInstance
            //delete from WorkflowStepResponder
            //delete from WorkflowStepOption
            //delete from workflowstep
            //delete from WorkflowStakeholder
            //delete from workflow
        }
        #endregion

        public async Task<List<CapitalRequest.API.Models.ProjectType>> GetAllProjectTypes()
        {
            return await _projectTypes.GetAll();
        }

        public async Task<List<CapitalRequest.API.Models.CapitalPool>> GetAllCapitalPools()
        {
            return await _capitalPools.GetAll();
        }
        #region ProvidedInfos
        public Task<CapitalRequest.API.Models.ProvidedInfo> GetProvidedInfo(int id)
        {
            return _providedInfos.Get(id);
        }

        public Task<List<CapitalRequest.API.Models.ProvidedInfo>> GetAllProvidedInfos(ProvidedInfoSearchFilter filter)
        {
            return _providedInfos.GetAll(filter);
        }

        public async Task DeleteAllProvidedInfos(ProvidedInfoSearchFilter filter)
        {
            await _providedInfos.DeleteAll(filter);

        }
        #endregion

        #region Quotes
        public Task<CapitalRequest.API.Models.Quote> GetQuote(int id)
        {
            return _quotes.Get(id);
        }

        public Task<List<CapitalRequest.API.Models.Quote>> GetAllQuotes(QuoteSearchFilter filter)
        {
            return _quotes.GetAll(filter);
        }

        public async Task DeleteAllQuotes(QuoteSearchFilter filter)
        {
            await _quotes.DeleteAll(filter);

        }
        #endregion

        #region RequestedInfos
        public Task<CapitalRequest.API.Models.RequestedInfo> GetRequestedInfo(int id)
        {
            return _requestedInfos.Get(id);
        }

        public async Task<List<CapitalRequest.API.Models.RequestedInfo>> GetAllRequestedInfos(RequestedInfoSearchFilter filter)
        {
            return await _requestedInfos.GetAll(filter);
        }

        public async Task DeleteAllRequestedInfos(RequestedInfoSearchFilter filter)
        {
            await _requestedInfos.DeleteAll(filter);

        }

        public async Task<CapitalRequest.API.Models.RequestedInfo> UpdateRequestedInfos(CapitalRequest.API.Models.RequestedInfo requestedInfo)
        {
            return await _requestedInfos.Update(requestedInfo);

        }
        #endregion

        #region ReviewerGroups
        public Task<CapitalRequest.API.Models.ReviewerGroup> GetReviewerGroup(int id)
        {
            return _reviewerGroups.Get(id);
        }

        public Task<List<CapitalRequest.API.Models.ReviewerGroup>> GetAllReviewerGroups(ReviewerGroupSearchFilter filter)
        {
            return _reviewerGroups.GetAll(filter);
        }
        #endregion

        #region Reviewers
        public Task<CapitalRequest.API.Models.Reviewer> GetReviewer(int id)
        {
            return _reviewers.Get(id);
        }

        public Task<List<CapitalRequest.API.Models.Reviewer>> GetAllReviewers(ReviewerSearchFilter filter)
        {
            return _reviewers.GetAll(filter);
        }

        public Task<List<CapitalRequest.API.Models.Reviewer>> GetReviewers(int segmentId)
        {
            var filter = new ReviewerSearchFilter
            {
                SegmentId = segmentId
            };
            return _reviewers.GetAll(filter);
        }

        #endregion

        #region Wbss
        public Task<CapitalRequest.API.Models.Wbs> GetWbs(int id)
        {
            return _wbss.Get(id);
        }

        public Task<List<CapitalRequest.API.Models.Wbs>> GetAllWbss(WbsSearchFilter filter)
        {
            return _wbss.GetAll(filter);
        }

        public async Task DeleteAllWbss(WbsSearchFilter filter)
        {
            await _wbss.DeleteAll(filter);

        }
        #endregion

        #region WorkflowTemplates
        public Task<CapitalRequest.API.Models.WorkflowTemplate> GetWorkflowTemplate(int id)
        {
            return _workflowTemplates.Get(id);
        }

        public Task<List<CapitalRequest.API.Models.WorkflowTemplate>> GetAllWorkflowTemplates(WorkflowTemplateSearchFilter filter)
        {
            return _workflowTemplates.GetAll(filter);
        }
        #endregion

        #region WorkflowActions

        public Task<List<CapitalRequest.API.Models.WorkflowAction>> GetAllWorkflowActions(WorkflowActionSearchFilter filter)
        {
            return _workflowActions.GetAll(filter);
        }

        public Task<CapitalRequest.API.Models.ApplicationUser> GetApplicationUser(string userId)
        {
            return _applicationUsers.Get(userId);
        }

        public Task<List<CapitalRequest.API.Models.ApplicationUser>> GetAllApplicationUsers(ApplicationUserSearchFilter filter)
        {
            return _applicationUsers.GetAll(filter);
        }

        public Task<List<CapitalRequest.API.Models.DeletedReviewer>> GetAllDeletedReviewers(DeletedReviewerSearchFilter filter)
        {
            return _deletedReviewers.GetAll(filter);
        }

        public async Task<List<CapitalRequest.API.Models.CapitalPoolIdentifier>> GetAllCapitalPoolIdentifiers()
        {
            return await _capitalPoolIdentifiers.GetAll();
        }

    }

    #endregion
}
