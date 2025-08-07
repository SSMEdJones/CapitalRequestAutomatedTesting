using Microsoft.Data.SqlClient;
using System.Data;

namespace CapitalRequestAutomatedTesting.UI.Services
{
    public class AppKeyValues
    {
        public string LookupKey { get; set; } = string.Empty;
        public string LookupValue { get; set; } = string.Empty;
    }

    public interface IAppConfigurationService
    {
        AppKeyValues GetAppKeyValueByKey(string appName, string lookupKey);
        List<AppKeyValues> GetAppKeyValues(string appName);
    }

    public class AppConfigurationService : IAppConfigurationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AppConfigurationService> _logger;
        private readonly Dictionary<string, List<AppKeyValues>> _cache = new();
        private readonly string _connectionString;

        public AppConfigurationService(IConfiguration configuration, ILogger<AppConfigurationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            // Get environment suffix from machine environment variable (same as in Program.cs)
            var sqlEnv = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEB_SQL_ENV", EnvironmentVariableTarget.Machine))
                ? "DEV"
                : Environment.GetEnvironmentVariable("WEB_SQL_ENV", EnvironmentVariableTarget.Machine);
            
            // Construct connection string name with environment suffix
            _connectionString = _configuration.GetConnectionString($"AppConfig_{sqlEnv}");
            
            if (string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogWarning("AppConfig_{SqlEnv} connection string not found. Falling back to AppConfig_DEV", sqlEnv);
                _connectionString = _configuration.GetConnectionString("AppConfig_DEV");
            }
        }

        public AppKeyValues GetAppKeyValueByKey(string appName, string lookupKey)
        {
            if (!_cache.ContainsKey(appName))
            {
                _cache[appName] = GetAppKeyValues(appName);
            }

            return _cache[appName]
                .FirstOrDefault(x => x.LookupKey.Equals(lookupKey, StringComparison.OrdinalIgnoreCase)) 
                ?? new AppKeyValues { LookupKey = lookupKey };
        }

        public List<AppKeyValues> GetAppKeyValues(string appName)
        {
            if (_cache.ContainsKey(appName))
            {
                return _cache[appName];
            }

            var result = new List<AppKeyValues>();
            
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                using var command = new SqlCommand(
                    "SELECT [LookupKey], [LookupValue] FROM AppConfig_AppKeyValues WHERE App_Name = @AppName", 
                    connection);
                command.Parameters.Add("@AppName", SqlDbType.NVarChar).Value = appName;

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(new AppKeyValues
                    {
                        LookupKey = reader.GetString(0),
                        LookupValue = reader.GetString(1)
                    });
                }

                _cache[appName] = result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving app key values for {AppName} using connection {ConnectionName}", 
                    appName, $"AppConfig_{Environment.GetEnvironmentVariable("WEB_SQL_ENV", EnvironmentVariableTarget.Machine) ?? "DEV"}");
            }

            return result;
        }
    }
}