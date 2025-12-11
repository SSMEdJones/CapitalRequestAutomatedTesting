using AutoMapper;
using CapitalRequest.API.DataAccess.ConfigurationSettings;
using CapitalRequest.API.DataAccess.Models;
using CapitalRequest.API.Models;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NLog.Filters;
using Proposal = CapitalRequest.API.Models.Proposal;
using vm = CapitalRequest.API.Models;

namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface IProposals
    {
        Task<Proposal> Get(int id);
        Task<List<Proposal>> GetAll(ProposalSearchFilter filter);
        Task Delete(int id);
    }

    public class Proposals : IProposals
    {
        private readonly CapitalRequestSettings _capitalRequestSettings;
        private readonly IMapper _mapper;

        public Proposals(
            IOptionsMonitor<CapitalRequestSettings> capitalRequestSettings,
            IMapper mapper)
        {
            _capitalRequestSettings = capitalRequestSettings.CurrentValue;
            _mapper = mapper;
        }

        public async Task<API.Models.Proposal> Get(int id)
        {
            try
            {
                var response = await _capitalRequestSettings.BaseApiUrl
                    .AppendPathSegment("Proposal")
                    .AppendPathSegment($"{id}")
                    .GetJsonAsync<Response<dynamic>>();

                var json = JsonConvert.SerializeObject(response.Result);

                var settings = new JsonSerializerSettings
                {
                    Error = (sender, args) =>
                    {
                        // Optional: log or debug
                        args.ErrorContext.Handled = true;
                    }
                };

                var proposal = JsonConvert.DeserializeObject<vm.Proposal>(json, settings);
                return proposal;

            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get request to CapitalRequest. {exceptionResponse}");
            }
        }

        public async Task<List<Proposal>> GetAll(ProposalSearchFilter filter)
        {
            try
            {
                var proposals = new List<Proposal>();

                var response = await _capitalRequestSettings.BaseApiUrl
                     .AppendPathSegment("Proposal")
                     .SetQueryParam("ProjectName", filter.ProjectName)
                     .SetQueryParam("CapitalFundingYear", filter.CapitalFundingYear)
                     .SetQueryParam("Region", filter.Region)
                     .SetQueryParam("SegmentId", filter.SegmentId)
                     .SetQueryParam("CapitalPool", filter.CapitalPool)
                     .SetQueryParam("CapitalPoolIdentifiers", filter.CapitalPoolIdentifiers)
                     .SetQueryParam("OverrideWorkflow", filter.OverrideWorkflow)
                     .SetQueryParam("UserId", filter.UserId)
                     .SetQueryParam("Overridden", filter.Overridden)
                     .SetQueryParam("OverriddenBy", filter.OverriddenBy)
                     .GetJsonAsync<Response<dynamic>>();

                var responseObject = JsonConvert.SerializeObject(response.Result);

                var settings = new JsonSerializerSettings
                {
                    Error = (sender, args) =>
                    {
                        // Optional: log or debug
                        args.ErrorContext.Handled = true;
                    }
                };

                var results = JsonConvert.DeserializeObject<List<Proposal>>(responseObject, settings);

                if (results != null)
                {
                    foreach (var result in results)
                    {
                        proposals.Add(result);
                    }
                }

                return proposals;
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send get all request to CapitalRequest. {exceptionResponse}");
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                await _capitalRequestSettings.BaseApiUrl
                        .AppendPathSegment("Proposal")
                        .AppendPathSegment($"{id}")
                        .DeleteAsync();
            }
            catch (FlurlHttpException ex)
            {
                var exceptionResponse = await ex.GetResponseStringAsync();
                throw new Exception($"Failed attempting to send delete all request to CapitalRequest. {exceptionResponse}");
            }
        }
    }
}
