using AutoMapper;
using CapitalRequest.API.Models;
using CapitalRequestAPI.Data.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface IRequestedInfos
    {
        Task<RequestedInfo> Get(int id);
        Task<IEnumerable<RequestedInfo>> GetAll(RequestedInfoSearchFilter filter);

    }
    public class RequestedInfos : IRequestedInfos
    {
        private readonly CapitalRequestContext _context;
        private readonly IMapper _mapper;
        public RequestedInfos(CapitalRequestContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<RequestedInfo> Get(int id)
        {
            var requestedInfo = await _context.RequestedInfo
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            return _mapper.Map<RequestedInfo>(requestedInfo);
        }

        public async Task<IEnumerable<RequestedInfo>> GetAll(RequestedInfoSearchFilter filter)
        {
            var query = _context.RequestedInfo.AsNoTracking().AsQueryable();

            if (filter.ProposalId.HasValue)
                query = query.Where(x => x.ProposalId == filter.ProposalId.Value);

            if (filter.RequestingReviewerGroupId.HasValue)
                query = query.Where(x => x.RequestingReviewerGroupId == filter.RequestingReviewerGroupId.Value);

            if (filter.RequestingReviewerId.HasValue)
                query = query.Where(x => x.RequestingReviewerId == filter.RequestingReviewerId.Value);

            if (filter.ReviewerGroupId.HasValue)
                query = query.Where(x => x.ReviewerGroupId == filter.ReviewerGroupId.Value);

            if (filter.WorkflowStepOptionId.HasValue && filter.WorkflowStepOptionId != Guid.Empty)
                query = query.Where(x => x.WorkflowStepOptionId == filter.WorkflowStepOptionId.Value);

            if (filter.IsOpen.HasValue)
                query = query.Where(x => x.IsOpen == filter.IsOpen.Value);

            var requestedInfos = await query.ToListAsync();

            return requestedInfos.Select(x => _mapper.Map<RequestedInfo>(x));
        }

    }

}
