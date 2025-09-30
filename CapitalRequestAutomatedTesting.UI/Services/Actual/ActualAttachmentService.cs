using AutoMapper;
using CapitalRequestAutomatedTesting.Data.Services;
using CapitalRequestAutomatedTesting.UI.Models;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive;

public interface IActualAttachmentService
{
    Task<List<dto.Attachment>> GetAttachments(vm.Proposal proposal);
}

public class ActualAttachmentService : IActualAttachmentService
{
    private ICapitalRequestServices _capitalRequestServices;
    private readonly IMapper _mapper;

    public ActualAttachmentService(
        ICapitalRequestServices capitalRequestServices,
        IMapper mapper)
    {
        _capitalRequestServices = capitalRequestServices;
        _mapper = mapper;
    }

    public async Task<List<dto.Attachment>> GetAttachments(vm.Proposal proposal)
    {

        var attachments = (await _capitalRequestServices.GetAllAttachments(new dto.AttachmentSearchFilter
        {
            ProposalId = proposal.Id,
            ProvidedInfoId = proposal.ProvidedInfoId
        }))
        .Select(x => _mapper.Map<dto.Attachment>(x))
        .ToList();
        
        if (attachments == null || attachments.Count == 0)
        {
            return new List<dto.Attachment>();
        }

        return attachments;
    }


}
