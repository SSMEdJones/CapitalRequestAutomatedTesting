using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapitalRequest.API.DataAccess.Models
{
    public class Response<T>
    {
        public Response()
        {

        }

        public Response(T result) : base()
        {
            Result = result;
        }

        public bool Success { get; set; }
        public T Result { get; set; }
        public Exception Exception { get; set; }
        public Dictionary<string, string> Errors { get; set; }
    }

    public class SuccessResponse<T> : Response<T>
    {
        private SuccessResponse() : base()
        {
            Success = true;
        }

        public SuccessResponse(T result) : this()
        {
            Result = result;
        }
    }
}
