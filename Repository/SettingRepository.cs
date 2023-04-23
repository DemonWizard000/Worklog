using Microsoft.Data.SqlClient;
using WorkLog.Constants;
using WorkLog.Models;

namespace WorkLog.Repository
{
    public class SettingRepository
    {
        private static string ConnectionString = DbConstant.WorkLogDBConnectionString;

        public async Task<Settings> GetSettingByEmail(string UserEmail)
        {
            Settings setting = null;
            try {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("SELECT * FROM Settings WHERE UserEmail = @userEmail", connection))
                    {
                        command.Parameters.AddWithValue("@userEmail", UserEmail);
                        var reader = await command.ExecuteReaderAsync();

                        if (reader.Read())
                        {
                            setting = new Settings()
                            {
                                Id = long.Parse(reader["Id"].ToString()),
                                UserEmail = reader["UserEmail"].ToString(),
                                Min_Freq = int.Parse(reader["Min_Freq"].ToString()),
                                WeightFactor = int.Parse(reader["WeightFactor"].ToString())
                            };
                        }

                        else
                            setting = null;

                        if (!reader.IsClosed)
                            await reader.CloseAsync();

                        if (connection.State != System.Data.ConnectionState.Closed)
                            await connection.CloseAsync();

                        return setting;
                    }
                }
            } catch(Exception ex)
            {
                return null;
            }
        }
    }
}