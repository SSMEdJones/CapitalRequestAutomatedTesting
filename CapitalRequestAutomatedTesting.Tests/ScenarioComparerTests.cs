#nullable disable
using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Enums;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
using CapitalRequestAutomatedTesting.UI.Services;
using CapitalRequestAutomatedTesting.UI.Services.Actual;
using CapitalRequestAutomatedTesting.UI.Services.Predictive;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Diagnostics;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.Tests
{
    public class ScenarioComparer : IntegrationTestBase
    {
        private readonly IScenarioComparer _service;
        private readonly ICapitalRequestServices _capitalRequestServices;
        private readonly ISSMWorkflowServices _ssmWorkflowServices;
        private readonly IPredictiveScenarioService _predictiveScenarioService;
        private readonly IActualScenarioService _actualScenarioService;
        private readonly IPredictiveWorkflowInstanceHistoryService _predictiveWorkflowInstanceHistoryService;
        private readonly IMapper _mapper;

        public ScenarioComparer()
        {
            _service = _provider.GetRequiredService<IScenarioComparer>();
            _capitalRequestServices = _provider.GetRequiredService<ICapitalRequestServices>();
            _ssmWorkflowServices = _provider.GetRequiredService<ISSMWorkflowServices>();
            _predictiveScenarioService = _provider.GetRequiredService<IPredictiveScenarioService>();
            _actualScenarioService = _provider.GetRequiredService<IActualScenarioService>();
            _predictiveWorkflowInstanceHistoryService = _provider.GetRequiredService<IPredictiveWorkflowInstanceHistoryService>();
            _mapper = _provider.GetRequiredService<IMapper>();
        }

        [Fact]
        public async Task CompareData_WithValidData_ReturnsScenario4ComparisonResult()
        {

            // Arrange
            int proposalId = 2953;
            var proposal = await _capitalRequestServices.GetProposal(proposalId);

            proposal.WorkflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                .Where(x => !x.IsComplete)
                .FirstOrDefault();

            var scenario = new ScenarioDetailsViewModel
            {
                ScenarioId = "SCN004",
                ProposalId = proposalId,
                SubmitUserId = "pshumw"
            };
            scenario.PredictiveData = await _predictiveScenarioService.GenerateScenarioDataAsync(scenario);
            scenario.ActualData = await _actualScenarioService.GenerateScenarioDataAsync(scenario);

            var predictive = scenario.PredictiveData;
            var actual = scenario.ActualData;

            //var scenarioComparisonResult = _service.CompareData(predictive, actual);

            var result = _service.CompareData(predictive, actual);

            AnalyzeDifferences(result);
            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CompareData_WithValidData_ReturnsScenario3ComparisonResult()
        {

            // Arrange
            var proposalId = 2954;
            var proposal = await _capitalRequestServices.GetProposal(proposalId);

            var workflowStep = (await _ssmWorkflowServices.GetAllWorkFlowSteps(proposal.WorkflowId))
                .Where(x => x.IsComplete)
                .FirstOrDefault();

            var isTestMode = workflowStep != null ? true: false;    

            //var reviewerGroup = "Finance";
            //var reviewerGroup = "Purchasing";
            var reviewerGroup = "VP Ops";
            //var reviewerGroup = "VP Finance";
            var reviewerName = "Pam Shumway";

            var userId = (await _capitalRequestServices.GetAllApplicationUsers(new ApplicationUserSearchFilter
            {
                FullName = reviewerName
            }))
            .FirstOrDefault()?.UserId;

            var reviewerGroupId = (await _capitalRequestServices.GetAllReviewerGroups(new ReviewerGroupSearchFilter()))
                .FirstOrDefault(x => x.ReviewerType == Constants.REVIEW_TYPE_REVIEW && x.Name == reviewerGroup)?.Id ?? 0;

            var reviewer = (await _capitalRequestServices.GetAllReviewers(new ReviewerSearchFilter
            {
                UserId = userId,
                SegmentId = proposal.SegmentId,
                RegionId = proposal.Region,
                ReviewerGroupId = reviewerGroupId
            }))
            .FirstOrDefault();
               
            var reviewerId= reviewer?.Id ?? 0;

            var scenario = new ScenarioDetailsViewModel
            {
                ProposalId = proposalId,
                ScenarioId = "SCN003",
                PartialViewName = "_VerifyRequest",
                DisplayText = "Verify Request",
                RequestingGroupId = 0,
                ReplyingGroupId = 0,
                VerifyingGroupId = reviewerGroupId,
                ReviewerId = reviewerId,
                RequestedInformation = string.Empty,
                ReturnedInformation = string.Empty,
                RequestedInfoId = 0,
                PredictiveCompletionStep = 0,
                CanExecuteActualSteps = true,
                VerifyAndSendToVPFinance = false,
                ExecutionDurationMinutes = 1000000,
                IsTestMode = isTestMode
            };


            // Act
            scenario.PredictiveData = await _predictiveScenarioService.GenerateScenarioDataAsync(scenario);
            scenario.ActualData = await _actualScenarioService.GenerateScenarioDataAsync(scenario);

            var predictive = scenario.PredictiveData;
            var actual = scenario.ActualData;

            var result = _service.CompareData(predictive, actual);

            //// Verify that the EmailNotification table is processed
            //var emailTable = result.DifferingTables.FirstOrDefault(t => t.TableName == "EmailNotification");
            //Assert.NotNull(emailTable);

            //// This should NOT be empty after the fix
            //Assert.True(emailTable.OperationGroups?.Any() == true,
            //    "OperationGroups should be populated even when operation types differ between datasets");

            //Debug.WriteLine($"EmailNotification OperationGroups count: {emailTable.OperationGroups?.Count ?? 0}");
            //foreach (var group in emailTable.OperationGroups ?? new List<OperationGroupDifference>())
            //{
            //    Debug.WriteLine($"Operation: {group.Operation}, Records: {group.Records?.Count ?? 0}");
            //}

            AnalyzeDifferences(result);
            // Assert
            Assert.NotNull(result);
        }

        private async Task<Dictionary<string, TableData>> GeneratePredictiveTables(ScenarioDetailsViewModel scenario)
        {
            var tables = new Dictionary<string, TableData>();
            var proposal = _mapper.Map<vm.Proposal>(await _capitalRequestServices.GetProposal(scenario.ProposalId));
            proposal.ReviewerId = scenario.ReviewerId.Value;
            proposal.Reviewer = await _capitalRequestServices.GetReviewer(proposal.ReviewerId.Value);
            proposal.ReviewerGroupId = (int)scenario.VerifyingGroupId;


            // Generate WorkflowInstanceActionHistory table  
            await GenerateWorkflowInstanceActionHistoryTable(tables, proposal);

            // Add more table generation methods as needed

            return tables;
        }

        private async Task GenerateWorkflowInstanceActionHistoryTable(Dictionary<string, TableData> tables, vm.Proposal proposal)
        {
            var workflowInstanceActionHistory = await _predictiveWorkflowInstanceHistoryService.CreateNextStepWorkflowInstanceHistoryAsync(proposal);

            AddTableData(tables, "WorkflowInstanceActionHistory", workflowInstanceActionHistory, CrudOperationType.Insert);
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

        private void AnalyzeDifferences(ScenarioComparisonResult result)
        {
            Debug.WriteLine("=== DIFFERENCE ANALYSIS ===");

            if (!result.TablesOnlyInPredictive.Any() &&
                !result.TablesOnlyInActual.Any() &&
                !result.DifferingTables.Any())
            {
                Debug.WriteLine("✅ PERFECT MATCH: No differences found!");
                return;
            }

            if (result.TablesOnlyInPredictive.Any())
                Debug.WriteLine($"⚠️ Tables only in Predictive: {string.Join(", ", result.TablesOnlyInPredictive)}");

            if (result.TablesOnlyInActual.Any())
                Debug.WriteLine($"⚠️ Tables only in Actual: {string.Join(", ", result.TablesOnlyInActual)}");

            foreach (var table in result.DifferingTables)
            {
                Debug.WriteLine($"\n📊 Table '{table.TableName}' has differences:");

                // Count total field differences for quick overview
                var fieldDiffCount = (table.OperationGroups ?? new List<OperationGroupDifference>())
                    .SelectMany(og => og.Records)
                    .SelectMany(r => r.FieldDifferences)
                    .Count();

                Debug.WriteLine($"   - {fieldDiffCount} field differences");
                Debug.WriteLine($"   - {table.OnlyInPredictive.Count} records only in Predictive");
                Debug.WriteLine($"   - {table.OnlyInActual.Count} records only in Actual");

                // 🆕 Show detailed field differences if any exist
                if (fieldDiffCount > 0)
                {
                    Debug.WriteLine($"\n   🔍 FIELD DIFFERENCES for '{table.TableName}':");

                    foreach (var opGroup in table.OperationGroups ?? new List<OperationGroupDifference>())
                    {
                        if (opGroup.Records.Any(r => r.FieldDifferences.Any()))
                        {
                            Debug.WriteLine($"      Operation: {opGroup.Operation}");

                            foreach (var recordDiff in opGroup.Records)
                            {
                                if (recordDiff.FieldDifferences.Any())
                                {
                                    Debug.WriteLine($"         RowKey: {recordDiff.RowKey}");

                                    foreach (var fieldDiff in recordDiff.FieldDifferences)
                                    {
                                        Debug.WriteLine($"            Field '{fieldDiff.FieldName}':");
                                        Debug.WriteLine($"               Predictive: [{fieldDiff.PredictiveValue ?? "NULL"}]");
                                        Debug.WriteLine($"               Actual:     [{fieldDiff.ActualValue ?? "NULL"}]");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private ScenarioDataViewModel FilterActualData(ScenarioDataViewModel actualData, ScenarioDataViewModel predictiveData)
        {
            var filteredActual = new ScenarioDataViewModel
            {
                ScenarioId = actualData.ScenarioId,
                ActualExecutionDuration = actualData.ActualExecutionDuration,
                ActualExecutionDurationMinutes = actualData.ActualExecutionDurationMinutes,
                PredictiveMethods = actualData.PredictiveMethods,
                IsOriginalData = actualData.IsOriginalData
            };

            // Get predictive table names for filtering
            var predictiveTableNames = predictiveData.Tables.Keys.ToHashSet();

            // Filter tables: only keep tables that exist in predictive data
            foreach (var kvp in actualData.Tables.Where(t => predictiveTableNames.Contains(t.Key)))
            {
                var tableName = kvp.Key;
                var originalTable = kvp.Value;

                var filteredTable = new TableData
                {
                    TableName = originalTable.TableName,
                    Operation = originalTable.Operation,
                    Records = new List<RecordEntry>()
                };

                // Filter records: remove records where Data is null
                foreach (var record in originalTable.Records.Where(r => r.Data != null))
                {
                    // Additional filtering for list data
                    if (record.Data is IEnumerable<object> list && !(record.Data is string))
                    {
                        var filteredList = list.Where(item => item != null).ToList();
                        if (filteredList.Any()) // Only add if there are non-null items
                        {
                            filteredTable.Records.Add(new RecordEntry
                            {
                                Operation = record.Operation,
                                Data = filteredList,
                                IsOriginal = record.IsOriginal
                            });
                        }
                    }
                    else
                    {
                        // For non-list data, add as-is (already filtered for non-null)
                        filteredTable.Records.Add(record);
                    }
                }

                // Only add table if it has records
                if (filteredTable.Records.Any())
                {
                    filteredActual.Tables[tableName] = filteredTable;
                }
            }

            return filteredActual;
        }
    }
}
