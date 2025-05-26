using AutoMapper;
using CapitalRequest.API.Models;
using CapitalRequestAPI.Data.Database.Context;
using CapitalRequestAPI.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapitalRequest.API.DataAccess.Services.Api
{
    public interface IProposals
    {
        Task<Proposal> Get(int id);
    }
    public class Proposals : IProposals
    {
        private readonly CapitalRequestContext _context;
        private readonly IMapper _mapper;
        public Proposals(CapitalRequestContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Proposal> Get(int id)
        {

            var proposal = await _context.Proposal
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            return _mapper.Map<Proposal>(proposal);
        }
    }

}
