using SSMWorkflow.API.DataAccess.ConfiguratonSettings;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.DataAccess.Services.Api;
using SSMWorkflow.API.Models;
using Microsoft.Extensions.Options;
using System.Diagnostics;


namespace CapitalRequestAutomatedTesting.Data
{
    public interface ISSMWorkflowServices
    {

        Task<List<WorkFlowStepViewModel>> GetAllWorkFlowSteps(Guid workFlowId);

        Task<WorkFlowStepViewModel> GetWorkflowStep(Guid workFlowStepId);

        Task<List<WorkFlowInstanceViewModel>> GetAllWorkflowInstances(Guid WorkflowID);

        Task<List<WorkFlowStepOptionViewModel>> GetAllWorkFlowStepOptions(Guid workFlowStepId);

        Task<WorkFlowStepOptionViewModel> GetWorkFlowStepOption(Guid optionId);

        public Task<List<WorkFlowStepResponderViewModel>> GetAllAddWorkFlowStepResponder(Guid workFlowStepId);

        Task<List<WorkFlowStakeholderViewModel>> GetAllWorkFlowStakeholders(Guid workflowID);

        Task SendCapitalRequestGroupNotificationsAsync(NotificationSearchFilter notificationSearchFilter);

        Task SendCapitalRequestGroupNotifications(NotificationSearchFilter notificationSearchFilter);

        Task<List<Notification>> GetCapitalRequestGroupNotifications(NotificationSearchFilter notificationSearchFilter);

        Task<List<SSMWorkflow.API.Models.Dashboard>> GetCapitalRequestDashboard(DashboardSearchFilter dashboardSearchFilter);

        Task<List<WorkFlowInstanceActionHistoryViewModel>> GetAllWorkflowInstanceActionHistory(WorkFlowInstanceActionHistorySearchFilter filter);

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
            IDashboards dashboards,
            IOptions<SSMWorkFlowSettings> settings)
        {
            _ssmMWorkFlow = ssmMWorkFlow;
            _ssmMWorkFlowStakeholder = ssmMWorkFlowStakeholder;
            _ssmMWorkFlowStep = ssmMWorkFlowStep;
            _ssmMWorkFlowStepOption = ssmMWorkFlowStepOption;
            _ssmMWorkFlowStepResponder = ssmMWorkFlowStepResponder;
            _ssmMWorkFlowInstance = ssmMWorkFlowInstance;
            _ssmMWorkFlowInstanceActionHistory = ssmMWorkFlowInstanceActionHistory;
            _ssmMNotification = ssmMNotification;
            _dashboards = dashboards;
            _settings = settings.Value;

            Debug.WriteLine($"✅ Loaded BaseApiUrl from config: {_settings.BaseApiUrl}");
        }

        public Task<List<WorkFlowStepViewModel>> GetAllWorkFlowSteps(Guid workflowID)
        {
            return _ssmMWorkFlowStep.GetAll(workflowID);
        }

        public Task<WorkFlowStepViewModel> GetWorkflowStep(Guid workFlowStepId)
        {
            return _ssmMWorkFlowStep.Get(workFlowStepId);
        }

        public Task<List<WorkFlowStakeholderViewModel>> GetAllWorkFlowStakeholders(Guid workflowID)
        {
            return _ssmMWorkFlowStakeholder.GetAll(workflowID);
        }

        public Task<List<WorkFlowStepOptionViewModel>> GetAllWorkFlowStepOptions(Guid workFlowStepId)
        {
            return _ssmMWorkFlowStepOption.GetAll(workFlowStepId);
        }

        public Task<WorkFlowStepOptionViewModel> GetWorkFlowStepOption(Guid optionId)
        {
            return _ssmMWorkFlowStepOption.Get(optionId);
        }

        public Task<List<WorkFlowStepResponderViewModel>> GetAllAddWorkFlowStepResponder(Guid workFlowStepId)
        {
            return _ssmMWorkFlowStepResponder.GetAll(workFlowStepId);
        }

        public async Task SendCapitalRequestGroupNotificationsAsync(NotificationSearchFilter notificationSearchFilter)
        {
            await _ssmMNotification.SendCapitalRequestGroupNotificationsAsync(notificationSearchFilter);
        }

        public Task SendCapitalRequestGroupNotifications(NotificationSearchFilter notificationSearchFilter)
        {
            return _ssmMNotification.SendCapitalRequestGroupNotifications(notificationSearchFilter);
        }

        public Task<List<Notification>> GetCapitalRequestGroupNotifications(NotificationSearchFilter notificationSearchFilter)
        {
            return _ssmMNotification.GetCapitalRequestGroupNotifications(notificationSearchFilter);
        }

        public Task<List<WorkFlowInstanceViewModel>> GetAllWorkflowInstances(Guid workflowID)
        {
            return _ssmMWorkFlowInstance.GetAll(workflowID);
        }

        public Task<List<SSMWorkflow.API.Models.Dashboard>> GetCapitalRequestDashboard(DashboardSearchFilter dashboardSearchFilter)
        {
            return _dashboards.GetDashboardData(dashboardSearchFilter);
        }

        public Task<WorkFlowInstanceActionHistoryViewModel> GetWorkflowInstanceActionHistory(Guid Optionid)
        {
            return _ssmMWorkFlowInstanceActionHistory.Get(Optionid);
        }

        public Task<List<WorkFlowInstanceActionHistoryViewModel>> GetAllWorkflowInstanceActionHistory(WorkFlowInstanceActionHistorySearchFilter filter)
        {
            return _ssmMWorkFlowInstanceActionHistory.GetAll(filter);
        }

    }

}
