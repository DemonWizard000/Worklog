using Microsoft.Data.SqlClient;
using WorkLog.Constants;
using WorkLog.Models;

namespace WorkLog.Repository
{
    public class TenantRepository
    {
        private static string ConnectionString = DbConstant.WorkLogDBConnectionString;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;
        
        public TenantRepository(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Tenant> GetTenantByDomain(string domain)
        {
            Tenant tenant = null;
            long tenantId;
            try {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("SELECT * FROM Tenants WHERE URL = @domain AND IsReady = 1", connection))
                    {
                        command.Parameters.AddWithValue("@domain", domain);
                        var reader = await command.ExecuteReaderAsync();

                        if (reader.Read())
                        {
                            tenant = new Tenant()
                            {
                                id = long.Parse(reader["Id"].ToString()),
                                URL = reader["URL"].ToString(),
                                CustomCSSURL = reader["CustomCSSURL"].ToString(),
                                CustomSettingURL = reader["CustomSettingURL"].ToString(),
                                PageName = reader["PageName"].ToString(),
                                Name = reader["Name"].ToString(),
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"].ToString(),
                                NavigationLabels = reader["NavigationLabels"].ToString(),
                                DefaultQuestions = reader["DefaultQuestions"].ToString(),
                                How_To_Page_Content = reader["How_To_Page_Content"].ToString(),
                                About_Page_Content = reader["About_Page_Content"].ToString(),
                                Home_Page_Content = reader["Home_Page_Content"].ToString(),
                            };
                        }

                        else
                            tenant = null;

                        if (!reader.IsClosed)
                            await reader.CloseAsync();

                        if (connection.State != System.Data.ConnectionState.Closed)
                            await connection.CloseAsync();

                        return tenant;
                    }
                }
            } catch(Exception ex)
            {
                return null;
            }
        }
    }
}