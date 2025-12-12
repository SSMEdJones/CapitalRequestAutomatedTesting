using AutoMapper;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Models;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ApplicationUser = CapitalRequest.API.Models.ApplicationUser;

namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface IApplicationUsers
    {
        Task<ApplicationUser> Get(string userId);
        Task<List<ApplicationUser>> GetAll(ApplicationUserSearchFilter filter);
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

        public async Task<List<ApplicationUser>> GetAll(ApplicationUserSearchFilter filter)
        {
            try
            {
                var applicationUsers = new List<ApplicationUser>();

                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("ApplicationUser")
                    .SetQueryParams(new
                    {
                        filter.UserId,
                        filter.ApplicationRoleId,
                        filter.Email,
                        filter.ReportAccess,
                        filter.FullName,
                        filter.FirstName,
                        filter.LastName,

                    })
                    .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);
                var results = JsonConvert.DeserializeObject<List<ApplicationUser>>(responseObject);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        applicationUsers.Add(result);
                    }
                }

                return applicationUsers;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to CapitalRequest. {exceptionResponse}");
            }
        }

    }
}
