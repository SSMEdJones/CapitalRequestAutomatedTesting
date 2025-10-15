using AutoMapper;
using CapitalRequestAutomatedTesting.UI.Models;
using dto = CapitalRequest.API.DataAccess.Models;
using vm = CapitalRequest.API.Models;

namespace CapitalRequestAutomatedTesting.UI.Services.Predictive;

public interface IPredictiveAttachmentService
{
    Task<List<dto.Attachment>> CreateAttachmentsAsync(string lookupKey, vm.Proposal proposal, List<FileUploadData> fileUploads);

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


    public async Task<List<dto.Attachment>> CreateAttachmentsAsync(string lookupKey, vm.Proposal proposal, List<FileUploadData> files)
    {

        var attachments = new List<dto.Attachment>();
        if (files == null)
        {
            return attachments;
        }

        var uploadDirectory = Path.Combine(_appConfigurationService.GetAppKeyValueByKey("CapitalRequest", lookupKey).LookupValue, proposal.Id.ToString());

        foreach (var file in files)
        {
            var filePathStr = Path.Combine(uploadDirectory, file.FileName);

            if (lookupKey == Constants.UPLOAD_DIRECTORY_ATTACHMENTS)
            {    
                if (files.Count > 0)
                {
                    var attachment = _mapper.Map<dto.Attachment>(proposal);
                    attachment.FileName = file.FileName;
                    attachments.Add(attachment);
                }
            }
           
        }

        return attachments;
    }
}