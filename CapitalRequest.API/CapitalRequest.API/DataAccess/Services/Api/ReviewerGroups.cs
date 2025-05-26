using AutoMapper;
using CapitalRequestAPI.Data.Database.Context;
using CapitalRequestAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface IReviewerGroups
    {
        Task<ReviewerGroup> Get(int id);
        Task<IEnumerable<ReviewerGroup>> GetAll(ReviewerGroupSearchFilter filter);

    }
    public class ReviewerGroups : IReviewerGroups
    {
        private readonly CapitalRequestContext _context;
        private readonly IMapper _mapper;
        public ReviewerGroups(CapitalRequestContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ReviewerGroup> Get(int id)
        {
            var ReviewerGroup = await _context.ReviewerGroup
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            return _mapper.Map<ReviewerGroup>(ReviewerGroup);
        }


        public async Task<IEnumerable<ReviewerGroup>> GetAll(ReviewerGroupSearchFilter filter)
        {
            var query = _context.ReviewerGroup.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(filter.Name))
                query = query.Where(x => x.Name == filter.Name);


            if (filter.EmailTemplateId.HasValue)
                query = query.Where(x => x.EmailTemplateId == filter.EmailTemplateId.Value);

            if (!string.IsNullOrEmpty(filter.ReviewerType))
                query = query.Where(x => x.ReviewerType == filter.ReviewerType);

            if (filter.AdminReviewer.HasValue)
                query = query.Where(x => x.AdminReviewer == filter.AdminReviewer.Value);

            var ReviewerGroups = await query.ToListAsync();

            return ReviewerGroups.Select(x => _mapper.Map<ReviewerGroup>(x));
        }

    }

}
