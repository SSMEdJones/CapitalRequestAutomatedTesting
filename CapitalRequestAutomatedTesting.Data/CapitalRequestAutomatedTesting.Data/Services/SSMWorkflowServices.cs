using CapitalRequest.API.DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using SSMWorkflow.API.DataAccess.ConfigurationSettings;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.DataAccess.Services.Api;
using SSMWorkflow.API.Models;
using System.Diagnostics;


namespace CapitalRequestAutomatedTesting.Data.Services
{
    public interface ISSMWorkflowServices
    {

        Task<List<WorkFlowStepViewModel>> GetAllWorkFlowSteps(Guid workFlowId);

        Task<WorkFlowStepViewModel> GetWorkflowStep(Guid workFlowStepId);

        Task<List<WorkFlowInstanceViewModel>> GetAllWorkflowInstances(Guid WorkflowID);

        Task<List<WorkFlowStepOptionViewModel>> GetAllWorkFlowStepOptions(Guid workFlowStepId);

        Task<WorkFlowStepOptionViewModel> GetWorkFlowStepOption(Guid optionId);

        Task<List<WorkFlowStepResponderViewModel>> GetAllAddWorkFlowStepResponder(Guid workFlowStepId);
        Task DeleteWorkflowStepResponder(Guid responderId);

        Task<List<WorkFlowStakeholderViewModel>> GetAllWorkFlowStakeholders(Guid workflowID);

        Task SendCapitalRequestGroupNotificationsAsync(NotificationSearchFilter notificationSearchFilter);

        Task SendCapitalRequestGroupNotifications(NotificationSearchFilter notificationSearchFilter);

        Task<List<Notification>> GetCapitalRequestGroupNotifications(NotificationSearchFilter notificationSearchFilter);

        Task<List<SSMWorkflow.API.Models.Dashboard>> GetCapitalRequestDashboard(DashboardSearchFilter dashboardSearchFilter);

        Task<List<WorkFlowInstanceActionHistoryViewModel>> GetAllWorkflowInstanceActionHistory(WorkFlowInstanceActionHistorySearchFilter filter);

        Task<SSMWorkflow.API.Models.EmailNotification> GeEmailNotification(int id);
        Task DeleteEmailNotification(SSMWorkflow.API.Models.EmailNotification emailNotification);
        Task<List<SSMWorkflow.API.Models.EmailNotification>> GetAllEmailNotifications(EmailNotificationSearchFilter filter);
        Task<List<SSMWorkflow.API.Models.Dashboard>> GetAllDashboards(DashboardSearchFilter filter);
    }
    public class SSMWorkflowServices : ISSMWorkflowServices
    {
        private readonly ISSMWorkFlow _ssmMWorkFlow;
        private readonly ISSMWorkFlowStakeholder _ssmMWorkFlowStakeholder;
        private readonly ISSMWorkFlowStep _ssmMWorkFlowStep;
        private readonly ISSMWorkFlowStepOption _ssmMWorkFlowStepOption;
        private readonly ISSMWorkFlowStepResponder _ssmMWorkFlowStepResponder;
        private readonly ISSMWorkFlowInstance _ssmMWorkFlowInstance;
        private readonly ISSMWorkFlowInstanceActionHistory _ssmMWorkFlowInstanceActionHistory;
        private readonly ISSMNotification _ssmMNotification;
        private readonly IEmailNotifications _emailNotification;
        private readonly IDashboards _dashboards;
        private readonly SSMWorkFlowSettings _settings;


        public SSMWorkflowServices(
            ISSMWorkFlow ssmMWorkFlow,
            ISSMWorkFlowStakeholder ssmMWorkFlowStakeholder,
            ISSMWorkFlowStep ssmMWorkFlowStep,
            ISSMWorkFlowStepOption ssmMWorkFlowStepOption,
            ISSMWorkFlowStepResponder ssmMWorkFlowStepResponder,
            ISSMWorkFlowInstance ssmMWorkFlowInstance,
            ISSMWorkFlowInstanceActionHistory ssmMWorkFlowInstanceActionHistory,
            ISSMNotification ssmMNotification,
            IEmailNotifications emailNotification,
            IDashboards dashboards,
            IOptionsMonitor<SSMWorkFlowSettings> settings)
        {
            _ssmMWorkFlow = ssmMWorkFlow;
            _ssmMWorkFlowStakeholder = ssmMWorkFlowStakeholder;
            _ssmMWorkFlowStep = ssmMWorkFlowStep;
            _ssmMWorkFlowStepOption = ssmMWorkFlowStepOption;
            _ssmMWorkFlowStepResponder = ssmMWorkFlowStepResponder;
            _ssmMWorkFlowInstance = ssmMWorkFlowInstance;
            _ssmMWorkFlowInstanceActionHistory = ssmMWorkFlowInstanceActionHistory;
            _ssmMNotification = ssmMNotification;
            _emailNotification = emailNotification;
            _dashboards = dashboards;
            _settings = settings.CurrentValue;

            Debug.WriteLine($"✅ Loaded BaseApiUrl from config: {_settings.BaseApiUrl}");
            Debug.WriteLine($"✅ Loaded ProjectReviewLink from config: {_settings.ProjectReviewLink}");
        }

        public async Task<List<WorkFlowStepViewModel>> GetAllWorkFlowSteps(Guid workflowID)
        {
            return await _ssmMWorkFlowStep.GetAll(workflowID);
        }

        public async Task<WorkFlowStepViewModel> GetWorkflowStep(Guid workFlowStepId)
        {
            return await _ssmMWorkFlowStep.Get(workFlowStepId);
        }

        public async Task<WorkFlowViewModel> GetWorkflow(Guid workFlowId)
        {
            return await _ssmMWorkFlow.Get(workFlowId);
        }


        public async Task<List<WorkFlowStakeholderViewModel>> GetAllWorkFlowStakeholders(Guid workflowID)
        {
            return await _ssmMWorkFlowStakeholder.GetAll(workflowID);
        }

        public async Task<List<WorkFlowStepOptionViewModel>> GetAllWorkFlowStepOptions(Guid workFlowStepId)
        {
            return await _ssmMWorkFlowStepOption.GetAll(workFlowStepId);
        }

        public async Task<WorkFlowStepOptionViewModel> GetWorkFlowStepOption(Guid optionId)
        {
            return await _ssmMWorkFlowStepOption.Get(optionId);
        }

        public async Task<List<WorkFlowStepResponderViewModel>> GetAllAddWorkFlowStepResponder(Guid workFlowStepId)
        {
            return await _ssmMWorkFlowStepResponder.GetAll(workFlowStepId);
        }

        public async Task DeleteWorkflowStepResponder(Guid responderId)
        {
            await _ssmMWorkFlowStepResponder.Delete(responderId);
        }

        public async Task SendCapitalRequestGroupNotificationsAsync(NotificationSearchFilter notificationSearchFilter)
        {
            await _ssmMNotification.SendCapitalRequestGroupNotificationsAsync(notificationSearchFilter);
        }

        public async Task SendCapitalRequestGroupNotifications(NotificationSearchFilter notificationSearchFilter)
        {
            await _ssmMNotification.SendCapitalRequestGroupNotifications(notificationSearchFilter);
        }

        public async Task<List<Notification>> GetCapitalRequestGroupNotifications(NotificationSearchFilter notificationSearchFilter)
        {
            return await _ssmMNotification.GetCapitalRequestGroupNotifications(notificationSearchFilter);
        }

        public async Task<List<WorkFlowInstanceViewModel>> GetAllWorkflowInstances(Guid workflowID)
        {
            return await _ssmMWorkFlowInstance.GetAll(workflowID);
        }

        public async Task<List<SSMWorkflow.API.Models.Dashboard>> GetCapitalRequestDashboard(DashboardSearchFilter dashboardSearchFilter)
        {
            return await _dashboards.GetDashboardData(dashboardSearchFilter);
        }

        public async Task<WorkFlowInstanceActionHistoryViewModel> GetWorkflowInstanceActionHistory(Guid Optionid)
        {
            return await _ssmMWorkFlowInstanceActionHistory.Get(Optionid);
        }

        public async Task<List<WorkFlowInstanceActionHistoryViewModel>> GetAllWorkflowInstanceActionHistory(WorkFlowInstanceActionHistorySearchFilter filter)
        {
            return await _ssmMWorkFlowInstanceActionHistory.GetAll(filter);
        }

        public async Task<SSMWorkflow.API.Models.EmailNotification> GeEmailNotification(int id)
        {
            return await _emailNotification.Get(id);
        }

        public async Task<List<SSMWorkflow.API.Models.EmailNotification>> GetAllEmailNotifications(EmailNotificationSearchFilter filter)
        {
            return await _emailNotification.GetAll(filter);
        }

        public async Task DeleteEmailNotification(SSMWorkflow.API.Models.EmailNotification emailNotification)
        {
             await _emailNotification.Delete(emailNotification);
        }

        public async Task<List<SSMWorkflow.API.Models.Dashboard>> GetAllDashboards(DashboardSearchFilter filter)
        {
            return await _dashboards.GetDashboardData(filter);
        }
        
    }

}
