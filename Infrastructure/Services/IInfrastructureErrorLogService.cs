using Infrastructure.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public interface IInfrastructureErrorLogService
    {
        Task<int> CreateErrorLogAsync(ExceptionDetail detail);
    }
}

