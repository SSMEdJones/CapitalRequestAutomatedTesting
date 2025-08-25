using AutoMapper;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Models;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Enums;
using CapitalRequestAutomatedTesting.UI.Helpers;
using CapitalRequestAutomatedTesting.UI.Models;
using CapitalRequestAutomatedTesting.UI.Services.Predictive;
using CapitalRequestAutomatedTesting.UI.Utilities;
using Scriban;
using SSMWorkflow.API.Models;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Actual;

public interface IActualFileService
{
    void DownloadFile(string fileName, int id, UploadFileType uploadFileType);
}

public class ActualFileService : IActualFileService
{

    private readonly ISSMWorkflowServices _ssmWorkflowServices;
    private readonly ICapitalRequestServices _capitalRequestServices;
    private readonly IUserContextService _userContextService;
    private readonly IPredictiveEmailNotificationService _predictiveEmailNotificationService;
    private readonly IMapper _mapper;

    public ActualFileService(
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

    public void DownloadFile(string fileName, int id, UploadFileType uploadFileType)
    {
        if (fileName == null)
        {
            return;
        }
        //TODO Implement file download logic here
        return;
    }

    
}
