using AutoMapper;
using CapitalRequestAutomatedTesting.UI.Models;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive;

public interface IPredictiveAttachmentService
{
    List<dto.Attachment> CreateAttachments(string lookupKey, vm.Proposal proposal, List<IFormFile> files);
}

public class PredictiveAttachmentService : IPredictiveAttachmentService
{

    private readonly IAppConfigurationService _appConfigurationService;
    private readonly IMapper _mapper;

    public PredictiveAttachmentService(
        IAppConfigurationService appConfigurationService,
        IMapper mapper)
    {
        _appConfigurationService = appConfigurationService;
        _mapper = mapper;
    }

    public List<dto.Attachment> CreateAttachments(string lookupKey, vm.Proposal proposal, List<IFormFile> files)
    {
        var attachments = new List<dto.Attachment>();
        if (files == null)
        {
            return attachments;
        }

        var uploadDirectory = Path.Combine(_appConfigurationService.GetAppKeyValueByKey("CapitalRequest", lookupKey).LookupValue, proposal.Id.ToString());

        files.ForEach(x =>
        {
            var filePathStr = Path.Combine(uploadDirectory, x.FileName);

            if (lookupKey == Constants.UPLOAD_DIRECTORY_ATTACHMENTS)
            {
                if (proposal.AttachmentFiles != null && proposal.AttachmentFiles.Count > 0)
                {
                    var attachment = _mapper.Map<dto.Attachment>(proposal);
                    attachment.FileName = x.FileName;

                    attachments.Add(attachment);
                }
            }

        });

        return attachments;
    }


}
