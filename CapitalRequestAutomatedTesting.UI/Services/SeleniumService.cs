//using CapitalRequestAutomatedTesting.UI.ScenarioFramework;
//using OpenQA.Selenium.Chrome;

//namespace CapitalRequestAutomatedTesting.UI.Services
//{
//    public class SeleniumService
//    {
//        public async Task<SeleniumScenarioResult> RunScenarioAsync(string scenarioName, List<SeleniumScenarioStep> steps)
//        {
//            var result = new SeleniumScenarioResult { ScenarioName = scenarioName };
//            using var driver = new ChromeDriver(); // or inject via WebDriverFactory

//            try
//            {
//                foreach (var step in steps.OrderBy(s => s.StepNumber))
//                {
//                    try
//                    {
//                        var stepResult = await step.Action(driver);
//                        step.Result = stepResult;

//                        result.Steps.Add(new SeleniumStepResultDetail
//                        {
//                            Description = step.Description,
//                            Success = stepResult.Success,
//                            Message = stepResult.Message
//                        });

//                        if (!stepResult.Success)
//                            break;
//                    }
//                    catch (Exception ex)
//                    {
//                        step.Result = new SeleniumStepResult { Success = false, Message = ex.Message };
//                        result.Steps.Add(new SeleniumStepResultDetail
//                        {
//                            Description = step.Description,
//                            Success = false,
//                            Message = $"Exception thrown: {ex.Message}"
//                        });
//                        break;
//                    }
//                }
//            }
//            finally
//            {
//                driver.Quit();
//            }

//            return result;
//        }
//    }

//}
