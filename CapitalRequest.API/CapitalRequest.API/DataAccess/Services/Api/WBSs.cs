using AutoMapper;
using CapitalRequestAPI.Data.Database.Context;
using CapitalRequestAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface IWBSs
    {
        Task<Wbs> Get(int id);
        Task<IEnumerable<Wbs>> GetAll(WbsSearchFilter filter);

    }
    public class WBSs : IWBSs
    {
        private readonly CapitalRequestContext _context;
        private readonly IMapper _mapper;
        public WBSs(CapitalRequestContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Wbs> Get(int id)
        {

            var wbs = await _context.Wbs
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            return _mapper.Map<Wbs>(wbs);
        }

        public async Task<IEnumerable<Wbs>> GetAll(WbsSearchFilter filter)
        {
            var query = _context.Wbs.AsNoTracking().AsQueryable();


            if (!string.IsNullOrEmpty(filter.Wbsnumber))
                query = query.Where(x => x.Wbsnumber == filter.Wbsnumber);


            if (filter.ProposalId.HasValue)
                query = query.Where(x => x.ProposalId == filter.ProposalId.Value);

            if (filter.TypeOfProject.HasValue)
                query = query.Where(x => x.TypeOfProject == filter.TypeOfProject.Value);


            var wbs = await query.ToListAsync();

            return wbs.Select(x => _mapper.Map<Wbs>(x));
        }
    }

}
