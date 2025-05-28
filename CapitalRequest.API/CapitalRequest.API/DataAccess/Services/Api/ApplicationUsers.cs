using AutoMapper;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Models;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;
using ApplicationUser = CapitalRequest.API.Models.ApplicationUser;

namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface IApplicationUsers
    {
        Task<ApplicationUser> Get(string userId);
    }

    public class ApplicationUsers : IApplicationUsers
    {
        private readonly CapitalRequestSettings _capitalRequestSettings;
        private readonly IMapper _mapper;

        public ApplicationUsers (
            IOptionsMonitor <CapitalRequestSettings> capitalRequestSettings,
            IMapper mapper)
        {
            _capitalRequestSettings = capitalRequestSettings.CurrentValue;
            _mapper = mapper;
        }
        

        public async Task<ApplicationUser> Get(string userId)
        {
            try
            {
                var applicationUser = new ApplicationUser();
                var debug =  _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("ApplicationUser")
                    .AppendPathSegment($"{userId}");

                    //https://localhost:44310/v1/CapitalRequest/ApplicationUser/ejones08
                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("ApplicationUser")
                    .AppendPathSegment($"{userId}")
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<ApplicationUser>(responseObject);

                if (results != null)
                {
                    applicationUser = results;
                }

                return _mapper.Map<ApplicationUser>(applicationUser);
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get request to CapitalRequest. {exceptionResponse}");
            }
        }

        
    }
}
