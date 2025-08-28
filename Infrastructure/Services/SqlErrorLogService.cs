using Infrastructure.Middleware;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Infrastructure.Services
{
    public class SqlErrorLogService : IInfrastructureErrorLogService
    {
        private readonly string _connectionString;

        private readonly IConfiguration _configuration;
        
        public SqlErrorLogService(IConfiguration configuration)
        {
            _configuration = configuration;
            // Get environment suffix from machine environment variable (same as in Program.cs)
            var sqlEnv = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEB_SQL_ENV", EnvironmentVariableTarget.Machine))
                ? "DEV"
                : Environment.GetEnvironmentVariable("WEB_SQL_ENV", EnvironmentVariableTarget.Machine);

            // Use the same connection string pattern as your other services
            _connectionString = _configuration.GetConnectionString($"NLog_{sqlEnv}");

            if (string.IsNullOrEmpty(_connectionString))
            {
                // Fall back to NLog connection string
                _connectionString = _configuration.GetConnectionString("NLog_DEV");
            }
        }

        public async Task<int> CreateErrorLogAsync(ExceptionDetail detail)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = new SqlCommand(@"
        INSERT INTO LogEntries 
(Logged, Level, Message, Logger, ExceptionType, ExceptionMessage, StackTrace,
 Url, UserName, ScenarioId, RollbackStatus, FormData, Endpoint, ResponseBody,
 InvokedService, InvokedMethod, InvokedParameters)
VALUES (@logged, @level, @message, @logger, @exceptionType, @exceptionMessage, @stackTrace,
            @url, @userName, @scenarioId, @rollbackStatus, @formData, @endpoint, @responseBody,
            @invokedService, @invokedMethod, @invokedParameters);
;
        SELECT CAST(SCOPE_IDENTITY() AS INT);", connection);

            // Strongly-type key parameters to match schema
            cmd.Parameters.Add("@logged", SqlDbType.DateTime).Value = detail.Logged;
            cmd.Parameters.Add("@level", SqlDbType.VarChar, 50).Value = (object?)detail.Level ?? DBNull.Value;
            cmd.Parameters.Add("@message", SqlDbType.VarChar, -1).Value = (object?)detail.Message ?? DBNull.Value;
            cmd.Parameters.Add("@logger", SqlDbType.VarChar, 250).Value = (object?)detail.Logger ?? DBNull.Value;
            cmd.Parameters.Add("@exceptionType", SqlDbType.VarChar, 250).Value = (object?)detail.ExceptionType ?? DBNull.Value;
            cmd.Parameters.Add("@exceptionMessage", SqlDbType.VarChar, -1).Value = (object?)detail.ExceptionMessage ?? DBNull.Value;
            cmd.Parameters.Add("@stackTrace", SqlDbType.VarChar, -1).Value = (object?)detail.StackTrace ?? DBNull.Value;

            cmd.Parameters.Add("@url", SqlDbType.VarChar, 500).Value = (object?)detail.Url ?? DBNull.Value;
            cmd.Parameters.Add("@userName", SqlDbType.VarChar, 50).Value = (object?)detail.UserName ?? DBNull.Value;

            cmd.Parameters.Add("@scenarioId", SqlDbType.VarChar, 100).Value = (object?)detail.ScenarioId ?? DBNull.Value;
            cmd.Parameters.Add("@rollbackStatus", SqlDbType.VarChar, 50).Value = (object?)detail.RollbackStatus ?? DBNull.Value;
            cmd.Parameters.Add("@formData", SqlDbType.VarChar, -1).Value = (object?)detail.FormData ?? DBNull.Value;
            cmd.Parameters.Add("@endpoint", SqlDbType.VarChar, 255).Value = (object?)detail.Endpoint ?? DBNull.Value;
            cmd.Parameters.Add("@responseBody", SqlDbType.VarChar, -1).Value = (object?)detail.ResponseBody ?? DBNull.Value;
            cmd.Parameters.Add("@invokedService", SqlDbType.VarChar, 100).Value = (object?)detail.InvokedService ?? DBNull.Value;
            cmd.Parameters.Add("@invokedMethod", SqlDbType.VarChar, 100).Value = (object?)detail.InvokedMethod ?? DBNull.Value;
            cmd.Parameters.Add("@invokedParameters", SqlDbType.VarChar, 500).Value = (object?)detail.InvokedParameters ?? DBNull.Value;

            var id = (int)await cmd.ExecuteScalarAsync();
            return id;
        }

    }

}
