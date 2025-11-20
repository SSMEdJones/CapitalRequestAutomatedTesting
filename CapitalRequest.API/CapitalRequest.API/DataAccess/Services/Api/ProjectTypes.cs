using AutoMapper;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.Models;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface IProjectTypes
    {
        Task<List<ProjectType>> GetAll();
    }

    public class ProjectTypes : IProjectTypes
    {
        private readonly CapitalRequestSettings _capitalRequestSettings;
        private readonly IMapper _mapper;

        public ProjectTypes(
            IOptionsMonitor<CapitalRequestSettings> capitalRequestSettings,
            IMapper mapper)
        {
            _capitalRequestSettings = capitalRequestSettings.CurrentValue;
            _mapper = mapper;
        }

        

        public async Task<List<ProjectType>> GetAll()
        {
            try
            {
                var projectTypes = new List<ProjectType>();

                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("ProjectType")                    
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<ProjectType>>(responseObject);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        projectTypes.Add(result);
                    }
                }

                return projectTypes;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to CapitalRequest. {exceptionResponse}");
            }
        }

        
        
    }
}
