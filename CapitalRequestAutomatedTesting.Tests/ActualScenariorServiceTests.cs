#nullable disable

using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Enums;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services.Actual;
using CapitalRequestAutomatedTesting.UI.Services.Predictive;
using Microsoft.Extensions.DependencyInjection;
using SSMWorkflow.API.DataAccess.Models;
using SSMWorkflow.API.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.Tests
{
    public class ActualScenariorServiceTests : IntegrationTestBase
    {
        private readonly IActualScenarioService _service;
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly ICapitalRequestServices _capitalRequestservices;
        private readonly IActualWorkflowStepOptionService _actualWorkflowStepOptionService;
        private readonly IPredictiveWorkflowInstanceHistoryService _predictiveWorkflowInstanceHistoryService;
        private readonly IPredictiveWorkflowStepOptionService _predictiveWorkflowStepOptionService;
        private readonly IMapper _mapper;

        public ActualScenariorServiceTests()
        {
            _service = _provider.GetRequiredService<IActualScenarioService>();
            _ssmWorkflowServices = _provider.GetRequiredService<ISSMWorkflowServices>();
            _capitalRequestservices = _provider.GetRequiredService<ICapitalRequestServices>();
            _actualWorkflowStepOptionService = _provider.GetRequiredService<IActualWorkflowStepOptionService>();
            _predictiveWorkflowInstanceHistoryService = _provider.GetRequiredService<IPredictiveWorkflowInstanceHistoryService>();
            _predictiveWorkflowStepOptionService = _provider.GetRequiredService<IPredictiveWorkflowStepOptionService>();
            _mapper = _provider.GetRequiredService<IMapper>();
        }

        [Fact]
        public async Task GenerateVerifyScenarioDataAsync_ReturnsActualData()
        {
            // Arrange
            var scenario = new ScenarioDetailsViewModel
            {
                ProposalId = 2787,
                ScenarioId = "SCN003",
                PartialViewName = "_VerifyRequest",
                DisplayText = "Verify Request",
                RequestingGroupId = 0,
                ReplyingGroupId = 0,
                VerifyingGroupId = 9,
                ReviewerId = 14090,
                RequestedInformation = string.Empty,
                ReturnedInformation = string.Empty,
                RequestedInfoId = 0,
                PredictiveCompletionStep = 0,
                CanExecuteActualSteps = true,
                VerifyAndSendToVPFinance = true,
                ExecutionDurationMinutes = 1000000
            };

            scenario.PredictiveData.Tables = await GeneratePredictiveTables(scenario);
            // Act
            var result = await _service.GenerateScenarioDataAsync(scenario);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(scenario.ScenarioId, result.ScenarioId);
        }

        private async Task<Dictionary<string, TableData>> GeneratePredictiveTables(ScenarioDetailsViewModel scenario)
        {
            var tables = new Dictionary<string, TableData>();
            var proposal = await _capitalRequestservices.GetProposal(scenario.ProposalId);
            proposal.ReviewerId = scenario.ReviewerId.Value;
            proposal.Reviewer = await _capitalRequestservices.GetReviewer(proposal.ReviewerId.Value);
            proposal.ReviewerGroupId = (int)scenario.VerifyingGroupId;

            // Generate WorkflowStepOption table
            await GenerateWorkflowStepOptionTable(tables, proposal);
            
            // Generate WorkflowInstanceActionHistory table  
            await GenerateWorkflowInstanceActionHistoryTable(tables, proposal);
            
            // Add more table generation methods as needed
            if (scenario.ScenarioId == "SCN001" || scenario.ScenarioId == "SCN004")
            {
                await GenerateEmailNotificationTable(tables, proposal);
            }

            return tables;
        }

        private async Task GenerateWorkflowStepOptionTable(Dictionary<string, TableData> tables, vm.Proposal proposal)
        {
            //var workflowStepOptions = await _actualWorkflowStepOptionService.GetNextStepWorkflowStepOptionsAsync(proposal);
            //workflowStepOptions.ForEach(x => x.WorkflowStepID = Guid.Empty);
            proposal.WorkflowStepId = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                .Where(x => x.IsComplete)
                .LastOrDefault().WorkflowStepID;

            proposal.WorkflowStep = await _ssmWorkflowServices.GetWorkflowStep(proposal.WorkflowStepId);
            proposal.Reviewer = await _capitalRequestservices.GetReviewer(proposal.ReviewerId.HasValue ? proposal.ReviewerId.Value : 0);

            var allOptions = (await _ssmWorkflowServices.GetAllWorkFlowStepOptions(proposal.WorkflowStepId))
               .Where(x => x.IsComplete || x.IsTerminate && x.OptionType == Constants.OPTION_TYPE_VERIFY)
               .ToList();

            // simulate AddWorkflowStepOption
            if (!allOptions.Any(x => x.OptionName.ToLower() == proposal.Reviewer.Email.ToLower()))
            {
                var newWorkflowStepOption = _mapper.Map<WorkflowStepOption>(proposal.Reviewer);
                newWorkflowStepOption.OptionType = allOptions.FirstOrDefault().OptionType;

                allOptions.Add(_mapper.Map<WorkFlowStepOptionViewModel>(newWorkflowStepOption));
            }

            proposal.WorkflowStepOptions = allOptions;


            var workflowStepOptions = await _predictiveWorkflowStepOptionService.CloseOptionsAsync(proposal, Guid.Empty, Constants.OPTION_TYPE_VERIFY, null);

            AddTableData(tables, "WorkflowStepOption", workflowStepOptions, CrudOperationType.Insert);
        }

        private async Task GenerateWorkflowInstanceActionHistoryTable(Dictionary<string, TableData> tables, vm.Proposal proposal)
        {
            var workflowInstanceActionHistory = await _predictiveWorkflowInstanceHistoryService.CreateNextStepWorkflowInstanceHistoryAsync(proposal);
            
            AddTableData(tables, "WorkflowInstanceActionHistory", workflowInstanceActionHistory, CrudOperationType.Insert);
        }

        private async Task GenerateEmailNotificationTable(Dictionary<string, TableData> tables, vm.Proposal proposal)
        {
            // Generate email notification data based on your requirements
            var emailNotifications = new List<SSMWorkflow.API.DataAccess.Models.EmailNotification>();
            // ... populate email notifications
            
            AddTableData(tables, "EmailNotification", emailNotifications, CrudOperationType.Insert);
        }
        private void AddTableData(Dictionary<string, TableData> tables, string tableName, object data, CrudOperationType operation)
        {
            if (!tables.ContainsKey(tableName))
            {
                tables[tableName] = new TableData
                {
                    TableName = tableName,
                    Records = new List<RecordEntry>()
                };
            }

            tables[tableName].Records.Add(new RecordEntry
            {
                Operation = operation,
                Data = data
            });
        }
    }
}
