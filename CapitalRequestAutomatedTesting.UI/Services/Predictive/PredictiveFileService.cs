using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Models;
using CapitalRequestAutomatedTesting.Data;
using CapitalRequestAutomatedTesting.UI.Helpers;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.Utilities;
using Scriban;
using SSMWorkflow.API.Models;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive;

public interface IPredictiveFileService
{
    void UploadFile(string lookupKey, vm.Proposal proposal, List<IFormFile> files);
}

public class PredictiveFileService : IPredictiveFileService
{

    private readonly ISSMWorkflowServices _ssmWorkflowServices;
    private readonly ICapitalRequestServices _capitalRequestServices;
    private readonly IUserContextService _userContextService;
    private readonly IPredictiveEmailNotificationService _predictiveEmailNotificationService;
    private readonly IMapper _mapper;

    public PredictiveFileService(
        ISSMWorkflowServices ssmWorkflowServices,
        ICapitalRequestServices capitalRequestServices,
        IUserContextService userContextService,
        IPredictiveEmailNotificationService predictiveEmailNotificationService,
        IMapper mapper)
    {
        _ssmWorkflowServices = ssmWorkflowServices;
        _capitalRequestServices = capitalRequestServices;
        _userContextService = userContextService;
        _predictiveEmailNotificationService = predictiveEmailNotificationService;
        _mapper = mapper;
    }

    public void UploadFile(string lookupKey, vm.Proposal proposal, List<IFormFile> files)
    {
        if (files == null)
        {
            return;
        }
        //TODO Implement file upload logic here
        return;
    }

    
}
