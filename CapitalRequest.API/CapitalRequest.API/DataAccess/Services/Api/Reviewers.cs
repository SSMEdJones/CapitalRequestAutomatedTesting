using AutoMapper;
using CapitalRequestAPI.Data.Database.Context;
using CapitalRequestAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface IReviewers
    {
        Task<Reviewer> Get(int id);
        Task<IEnumerable<Reviewer>> GetAll(ReviewerSearchFilter filter);

    }
    public class Reviewers : IReviewers
    {
        private readonly CapitalRequestContext _context;
        private readonly IMapper _mapper;
        public Reviewers(CapitalRequestContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Reviewer> Get(int id)
        {
            var Reviewer = await _context.Reviewer
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            return _mapper.Map<Reviewer>(Reviewer);
        }


        public async Task<IEnumerable<Reviewer>> GetAll(ReviewerSearchFilter filter)
        {
            var query = _context.Reviewer.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(filter.Email))
                query = query.Where(x => x.Email == filter.Email);


            if (filter.RegionId.HasValue)
                query = query.Where(x => x.RegionId == filter.RegionId.Value);

            if (filter.SegmentId.HasValue)
                query = query.Where(x => x.SegmentId == filter.SegmentId.Value);

            if (filter.ReviewerGroupId.HasValue)
                query = query.Where(x => x.ReviewerGroupId == filter.ReviewerGroupId.Value);

            if (filter.StepNumber.HasValue)
                query = query.Where(x => x.StepNumber == filter.StepNumber.Value);

            if (filter.IsVpOfOps.HasValue)
                query = query.Where(x => x.IsVpOfOps == filter.IsVpOfOps.Value);


            var reviewers = await query.ToListAsync();

            return reviewers.Select(x => _mapper.Map<Reviewer>(x));
        }

    }

}
