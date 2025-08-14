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

namespace CapitalRequestAutomatedTesting.UI.Services.Rollback;

public interface IRollbackFileService
{
    void DeleteFile(string lookupKey, vm.Proposal proposal, List<IFormFile> files);
}

public class RollbackFileService : IRollbackFileService
{

    private readonly ISSMWorkflowServices _ssmWorkflowServices;
    private readonly ICapitalRequestServices _capitalRequestServices;
    private readonly IUserContextService _userContextService;
    private readonly IMapper _mapper;

    public RollbackFileService(
        ISSMWorkflowServices ssmWorkflowServices,
        ICapitalRequestServices capitalRequestServices,
        IUserContextService userContextService,
        IMapper mapper)
    {
        _ssmWorkflowServices = ssmWorkflowServices;
        _capitalRequestServices = capitalRequestServices;
        _userContextService = userContextService;
        _mapper = mapper;
    }

    public void DeleteFile(string lookupKey, vm.Proposal proposal, List<IFormFile> files)
    {
        if (files == null)
        {
            return;
        }
        //TODO Implement file upload logic here
        return;
    }

    
}
