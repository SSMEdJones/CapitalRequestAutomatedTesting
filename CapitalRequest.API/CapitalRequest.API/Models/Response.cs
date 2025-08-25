using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CapitalRequest.API.Models
{
    public class Response<T>
    {
        private T _result;

        public Response()
        {

        }

        public Response(T result) : base()
        {
            Result = result;
        }

        public bool Success { get; set; }
        
        public Exception Exception { get; set; }
        public Dictionary<string, string> Errors { get; set; }
        public T Result
        {
            get => _result;
            set
            {
                if (value is JsonElement element)
                {
                    try
                    {
                        var json = element.GetRawText();
                        _result = JsonConvert.DeserializeObject<T>(json);
                    }
                    catch (Exception ex)
                    {
                        Exception = ex;
                        _result = default;
                    }
                }
                else
                {
                    _result = value;
                }
            }
        }
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
