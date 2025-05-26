using AutoMapper;
using CapitalRequestAPI.Data.Database.Context;
using CapitalRequestAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface IProvidedInfos
    {
        Task<ProvidedInfo> Get(int id);
        Task<IEnumerable<ProvidedInfo>> GetAll(ProvidedInfoSearchFilter filter);

    }
    public class ProvidedInfos : IProvidedInfos
    {
        private readonly CapitalRequestContext _context;
        private readonly IMapper _mapper;
        public ProvidedInfos(CapitalRequestContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ProvidedInfo> Get(int id)
        {
            var ProvidedInfo = await _context.ProvidedInfo
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            return _mapper.Map<ProvidedInfo>(ProvidedInfo);
        }

        public async Task<IEnumerable<ProvidedInfo>> GetAll(ProvidedInfoSearchFilter filter)
        {
            var query = _context.ProvidedInfo.AsNoTracking().AsQueryable();

            if (filter.RequestedInfoId.HasValue)
                query = query.Where(x => x.RequestedInfoId == filter.RequestedInfoId.Value);


            if (filter.ReviewerGroupId.HasValue)
                query = query.Where(x => x.ReviewerGroupId == filter.ReviewerGroupId.Value);

            if (filter.ReviewerId.HasValue)
                query = query.Where(x => x.ReviewerId == filter.ReviewerId.Value);

            var ProvidedInfos = await query.ToListAsync();

            return ProvidedInfos.Select(x => _mapper.Map<ProvidedInfo>(x));
        }

    }

}
