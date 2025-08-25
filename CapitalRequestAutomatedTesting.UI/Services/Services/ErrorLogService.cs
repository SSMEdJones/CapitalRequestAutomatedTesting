using Microsoft.Data.SqlClient;
using System.Data;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public interface IErrorLogService
    {
        Task<List<ErrorLogViewModel>> GetRecentErrorLogsAsync(int count = 100);
        Task<ErrorLogViewModel> GetErrorLogByIdAsync(int id);
        Task<int> GetErrorCountAsync();
        Task<List<ErrorLogViewModel>> SearchErrorLogsAsync(string query);
    }

    public class ErrorLogService : IErrorLogService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ErrorLogService(IConfiguration configuration)
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

        public async Task<List<ErrorLogViewModel>> GetRecentErrorLogsAsync(int count = 100)
        {
            var logs = new List<ErrorLogViewModel>();
            
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Adjusted query to include both ExceptionType and ExceptionMessage
                using var command = new SqlCommand(
                    @"SELECT TOP (@Count) Id, Logged, Level, Message, Logger, ExceptionType, ExceptionMessage, 
                      Url, UserName, StackTrace, ScenarioId, RollbackStatus
                      FROM LogEntries 
                      WHERE Level IN ('ERROR', 'FATAL') 
                      ORDER BY Id DESC", connection);

                command.Parameters.Add("@Count", SqlDbType.Int).Value = count;

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    logs.Add(new ErrorLogViewModel
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Logged = reader.GetDateTime(reader.GetOrdinal("Logged")),
                        Level = reader.GetString(reader.GetOrdinal("Level")),
                        Message = reader.GetString(reader.GetOrdinal("Message")),
                        Logger = reader.GetString(reader.GetOrdinal("Logger")),
                        ExceptionType = !reader.IsDBNull(reader.GetOrdinal("ExceptionType")) 
                            ? reader.GetString(reader.GetOrdinal("ExceptionType"))
                            : null,
                        ExceptionMessage = !reader.IsDBNull(reader.GetOrdinal("ExceptionMessage")) 
                            ? reader.GetString(reader.GetOrdinal("ExceptionMessage"))
                            : null,
                        // Combine ExceptionType and ExceptionMessage for backward compatibility
                        Exception = CombineException(
                            !reader.IsDBNull(reader.GetOrdinal("ExceptionType")) ? reader.GetString(reader.GetOrdinal("ExceptionType")) : null,
                            !reader.IsDBNull(reader.GetOrdinal("ExceptionMessage")) ? reader.GetString(reader.GetOrdinal("ExceptionMessage")) : null),
                        Url = !reader.IsDBNull(reader.GetOrdinal("Url"))
                            ? reader.GetString(reader.GetOrdinal("Url"))
                            : null,
                        UserName = !reader.IsDBNull(reader.GetOrdinal("UserName"))
                            ? reader.GetString(reader.GetOrdinal("UserName"))
                            : null,
                        StackTrace = !reader.IsDBNull(reader.GetOrdinal("StackTrace"))
                            ? reader.GetString(reader.GetOrdinal("StackTrace"))
                            : null,
                        ScenarioId = !reader.IsDBNull(reader.GetOrdinal("ScenarioId"))
                            ? reader.GetString(reader.GetOrdinal("ScenarioId"))
                            : null,
                        RollbackStatus = !reader.IsDBNull(reader.GetOrdinal("RollbackStatus"))
                            ? reader.GetString(reader.GetOrdinal("RollbackStatus"))
                            : null
                    });
                }
            }
            catch (Exception ex)
            {
                // Log the exception but don't throw from the error logger
                Console.WriteLine($"Error retrieving error logs: {ex.Message}");
            }
            
            return logs;
        }

        // Helper method to combine exception type and message for backward compatibility
        private string CombineException(string exceptionType, string exceptionMessage)
        {
            if (string.IsNullOrEmpty(exceptionType) && string.IsNullOrEmpty(exceptionMessage))
                return null;
            
            if (string.IsNullOrEmpty(exceptionType))
                return exceptionMessage;
                
            if (string.IsNullOrEmpty(exceptionMessage))
                return exceptionType;
                
            return $"{exceptionType}: {exceptionMessage}";
        }

        public async Task<ErrorLogViewModel> GetErrorLogByIdAsync(int id)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var cmd = new SqlCommand(
                    @"SELECT TOP 100 Id, Logged, Level, Message, Logger, ExceptionType, ExceptionMessage, 
                      Url, UserName, StackTrace, ScenarioId, RollbackStatus
                      FROM LogEntries 
                      WHERE Id = @Id", connection);

                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new ErrorLogViewModel
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Logged = reader.GetDateTime(reader.GetOrdinal("Logged")),
                        Level = reader.GetString(reader.GetOrdinal("Level")),
                        Logger = reader.GetString(reader.GetOrdinal("Logger")),
                        Message = reader.GetString(reader.GetOrdinal("Message")),
                        ExceptionType = !reader.IsDBNull(reader.GetOrdinal("ExceptionType")) 
                            ? reader.GetString(reader.GetOrdinal("ExceptionType"))
                            : null,
                        ExceptionMessage = !reader.IsDBNull(reader.GetOrdinal("ExceptionMessage")) 
                            ? reader.GetString(reader.GetOrdinal("ExceptionMessage"))
                            : null,
                        Exception = CombineException(
                            !reader.IsDBNull(reader.GetOrdinal("ExceptionType")) ? reader.GetString(reader.GetOrdinal("ExceptionType")) : null,
                            !reader.IsDBNull(reader.GetOrdinal("ExceptionMessage")) ? reader.GetString(reader.GetOrdinal("ExceptionMessage")) : null),
                        Url = !reader.IsDBNull(reader.GetOrdinal("Url"))
                            ? reader.GetString(reader.GetOrdinal("Url"))
                            : null,
                        UserName = !reader.IsDBNull(reader.GetOrdinal("UserName"))
                            ? reader.GetString(reader.GetOrdinal("UserName"))
                            : null,
                        StackTrace = !reader.IsDBNull(reader.GetOrdinal("StackTrace"))
                            ? reader.GetString(reader.GetOrdinal("StackTrace"))
                            : null,
                        ScenarioId = !reader.IsDBNull(reader.GetOrdinal("ScenarioId"))
                            ? reader.GetString(reader.GetOrdinal("ScenarioId"))
                            : null,
                        RollbackStatus = !reader.IsDBNull(reader.GetOrdinal("RollbackStatus"))
                            ? reader.GetString(reader.GetOrdinal("RollbackStatus"))
                            : null
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving error log: {ex.Message}");
            }
            
            return null;
        }

        public async Task<int> GetErrorCountAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(
                    "SELECT COUNT(*) FROM LogEntries WHERE Level IN ('ERROR', 'FATAL')", 
                    connection);

                return (int)await command.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting error count: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<ErrorLogViewModel>> SearchErrorLogsAsync(string query)
        {
            var logs = new List<ErrorLogViewModel>();
            
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(
                    @"SELECT TOP 100 Id, Logged, Level, Message, Logger, ExceptionType, ExceptionMessage, 
                      Url, UserName, StackTrace, ScenarioId, RollbackStatus
                      FROM LogEntries 
                      WHERE (Message LIKE @Query OR ExceptionType LIKE @Query OR ExceptionMessage LIKE @Query) 
                      AND Level IN ('ERROR', 'FATAL')
                      ORDER BY Logged DESC", connection);
                
                command.Parameters.Add("@Query", SqlDbType.NVarChar).Value = $"%{query}%";

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    logs.Add(new ErrorLogViewModel
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Logged = reader.GetDateTime(reader.GetOrdinal("Logged")),
                        Level = reader.GetString(reader.GetOrdinal("Level")),
                        Logger = reader.GetString(reader.GetOrdinal("Logger")),
                        Message = reader.GetString(reader.GetOrdinal("Message")),
                        ExceptionType = !reader.IsDBNull(reader.GetOrdinal("ExceptionType")) 
                            ? reader.GetString(reader.GetOrdinal("ExceptionType"))
                            : null,
                        ExceptionMessage = !reader.IsDBNull(reader.GetOrdinal("ExceptionMessage")) 
                            ? reader.GetString(reader.GetOrdinal("ExceptionMessage"))
                            : null,
                        Exception = CombineException(
                            !reader.IsDBNull(reader.GetOrdinal("ExceptionType")) ? reader.GetString(reader.GetOrdinal("ExceptionType")) : null,
                            !reader.IsDBNull(reader.GetOrdinal("ExceptionMessage")) ? reader.GetString(reader.GetOrdinal("ExceptionMessage")) : null),
                        Url = !reader.IsDBNull(reader.GetOrdinal("Url"))
                            ? reader.GetString(reader.GetOrdinal("Url"))
                            : null,
                        UserName = !reader.IsDBNull(reader.GetOrdinal("UserName"))
                            ? reader.GetString(reader.GetOrdinal("UserName"))
                            : null,
                        StackTrace = !reader.IsDBNull(reader.GetOrdinal("StackTrace"))
                            ? reader.GetString(reader.GetOrdinal("StackTrace"))
                            : null,
                        ScenarioId = !reader.IsDBNull(reader.GetOrdinal("ScenarioId"))
                            ? reader.GetString(reader.GetOrdinal("ScenarioId"))
                            : null,
                        RollbackStatus = !reader.IsDBNull(reader.GetOrdinal("RollbackStatus"))
                            ? reader.GetString(reader.GetOrdinal("RollbackStatus"))
                            : null
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching error logs: {ex.Message}");
            }
            
            return logs;
        }
    }
}