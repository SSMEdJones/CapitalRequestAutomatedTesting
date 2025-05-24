using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.DataAccess.Services.Api;
using SSMWorkflow.API.Models;
using Dashboard = SSMWorkflow.API.Models.Dashboard;

namespace SSMWorkflow.API.DataAccess.Services
{
    public interface IWorkflowServices
    {
        Task<Guid> AddWorkFlow(CreateUpdateWorkFlow workFlow);

        Task<Guid> AddWorkFlowStep(CreateUpdateWorkFlowStep workFlowStep);

        Task<List<WorkFlowStepViewModel>> GetAllWorkFlowSteps(Guid workFlowId);

        Task<WorkFlowStepViewModel> GetWorkFlowStep(Guid workFlowStepId);

        Task<WorkFlowStepViewModel> UpdateWorkflowStep(CreateUpdateWorkFlowStep createWorkFlowStep, Guid workflowStepID);

        Task<Guid> AddWorkflowInstance(CreateUpdateWorkFlowInstance workflowInstance);

        Task<List<WorkFlowInstanceViewModel>> GetAllWorkflowInstances(Guid WorkflowID);

        Task<Guid> AddWorkflowInstanceActionHistory(CreateUpdateWorkFlowInstanceActionHistory workflowInstanceActionHistory);

        Task<Guid> AddWorkFlowStakeholder(CreateUpdateWorkFlowStakeholder workFlowStakeholder);

        Task<Guid> AddWorkFlowStepOption(CreateUpdateWorkFlowStepOption workFlowStepOption);

        Task<WorkFlowStepOptionViewModel> UpdateWorkFlowStepOption(CreateUpdateWorkFlowStepOption createWorkFlowStepOption, Guid optionID);

        Task<List<WorkFlowStepOptionViewModel>> GetAllWorkFlowStepOptions(Guid workFlowStepId);

        Task<WorkFlowStepOptionViewModel> GetWorkFlowStepOption(Guid optionId);

        Task<Guid> AddWorkFlowStepResponder(CreateUpdateWorkFlowStepResponder workFlowStepResponder);

        Task DeleteWorkFlowStepResponder(Guid responderID);

        public Task<List<WorkFlowStepResponderViewModel>> GetAllAddWorkFlowStepResponder(Guid workFlowStepId);

        Task<List<WorkFlowStakeholderViewModel>> GetAllWorkFlowStakeholders(Guid workflowID);

        Task SendCapitalRequestGroupNotificationsAsync(NotificationSearchFilter notificationSearchFilter);

        Task SendCapitalRequestGroupNotifications(NotificationSearchFilter notificationSearchFilter);

        Task<List<Notification>> GetCapitalRequestGroupNotifications(NotificationSearchFilter notificationSearchFilter);

        Task<List<Dashboard>> GetCapitalRequestDashboard(DashboardSearchFilter dashboardSearchFilter);
    }

    public class WorkflowServices : IWorkflowServices
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

        public WorkflowServices(
            ISSMWorkFlow ssmMWorkFlow,
            ISSMWorkFlowStakeholder ssmMWorkFlowStakeholder,
            ISSMWorkFlowStep ssmMWorkFlowStep,
            ISSMWorkFlowStepOption ssmMWorkFlowStepOption,
            ISSMWorkFlowStepResponder ssmMWorkFlowStepResponder,
            ISSMWorkFlowInstance ssmMWorkFlowInstance,
            ISSMWorkFlowInstanceActionHistory ssmMWorkFlowInstanceActionHistory,
            ISSMNotification ssmMNotification,
            IDashboards dashboards)
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
        }

        public Task<Guid> AddWorkFlow(CreateUpdateWorkFlow workFlow)
        {
            return _ssmMWorkFlow.Add(workFlow);
        }

        public Task<Guid> AddWorkFlowStep(CreateUpdateWorkFlowStep workFlowStep)
        {
            return _ssmMWorkFlowStep.Add(workFlowStep);
        }

        public Task<List<WorkFlowStepViewModel>> GetAllWorkFlowSteps(Guid workflowID)
        {
            return _ssmMWorkFlowStep.GetAll(workflowID);
        }

        public Task<WorkFlowStepViewModel> GetWorkFlowStep(Guid workFlowStepId)
        {
            return _ssmMWorkFlowStep.Get(workFlowStepId);
        }

        public Task<WorkFlowStepViewModel> UpdateWorkflowStep(CreateUpdateWorkFlowStep createWorkFlowStep, Guid workflowStepID)
        {
            return _ssmMWorkFlowStep.Update(createWorkFlowStep, workflowStepID);
        }

        public Task<Guid> AddWorkflowInstance(CreateUpdateWorkFlowInstance workflowInstance)
        {
            return _ssmMWorkFlowInstance.Add(workflowInstance);
        }

        public Task<Guid> AddWorkflowInstanceActionHistory(CreateUpdateWorkFlowInstanceActionHistory workflowInstanceActionHistory)
        {
            return _ssmMWorkFlowInstanceActionHistory.Add(workflowInstanceActionHistory);
        }

        public Task<Guid> AddWorkFlowStakeholder(CreateUpdateWorkFlowStakeholder workFlowStakeholder)
        {
            return _ssmMWorkFlowStakeholder.Add(workFlowStakeholder);
        }

        public Task<List<WorkFlowStakeholderViewModel>> GetAllWorkFlowStakeholders(Guid workflowID)
        {
            return _ssmMWorkFlowStakeholder.GetAll(workflowID);
        }

        public Task<Guid> AddWorkFlowStepOption(CreateUpdateWorkFlowStepOption workFlowStepOption)
        {
            return _ssmMWorkFlowStepOption.Add(workFlowStepOption);
        }

        public Task<WorkFlowStepOptionViewModel> UpdateWorkFlowStepOption(CreateUpdateWorkFlowStepOption createWorkFlowStepOption, Guid optionID)
        {
            return _ssmMWorkFlowStepOption.Update(createWorkFlowStepOption, optionID);
        }

        public Task<List<WorkFlowStepOptionViewModel>> GetAllWorkFlowStepOptions(Guid workFlowStepId)
        {
            return _ssmMWorkFlowStepOption.GetAll(workFlowStepId);
        }

        public Task<WorkFlowStepOptionViewModel> GetWorkFlowStepOption(Guid optionId)
        {
            return _ssmMWorkFlowStepOption.Get(optionId);
        }

        public Task<Guid> AddWorkFlowStepResponder(CreateUpdateWorkFlowStepResponder workFlowStepResponder)
        {
            return _ssmMWorkFlowStepResponder.Add(workFlowStepResponder);
        }

        public Task DeleteWorkFlowStepResponder(Guid responderID)
        {
            return _ssmMWorkFlowStepResponder.Delete(responderID);
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

        public Task<List<Dashboard>> GetCapitalRequestDashboard(DashboardSearchFilter dashboardSearchFilter)
        {
            return _dashboards.GetDashboardData(dashboardSearchFilter);
        }
    }
}