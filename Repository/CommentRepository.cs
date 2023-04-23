using Microsoft.Data.SqlClient;
using WorkLog.Constants;
using WorkLog.Utils;

namespace WorkLog.Repository
{
    public class CommentRepository
    {
        private static string ConnectionString = DbConstant.WorkLogDBConnectionString;

        public CommentRepository() { }

        public async Task<int> GetUnreadCommentsCount(string userEmail, long groupId = -1, string to_email = null, string logDate = null)
        {
            int count = 0;
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    string query = "SELECT Count(*) as cnt FROM UnreadComments INNER JOIN Comments on (CommentId = Comments.Id) WHERE UserEmail = @userEmail AND GroupId = @groupId";

                    if(to_email != null)
                        query += " AND To_Email = @toEmail";

                    if (logDate != null)
                        query += " AND Log_Date BETWEEN @fromDate AND @toDate";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userEmail", userEmail);
                        command.Parameters.AddWithValue("@groupId", groupId);
                        if (to_email != null)
                            command.Parameters.AddWithValue("@toEmail", to_email);
                        if (logDate != null)
                        {
                            DateTime log_date = DateTime.Parse(logDate);
                            command.Parameters.AddWithValue("@fromDate", new DateTimeUtils().startTimeOfDate(log_date).ToString());
                            command.Parameters.AddWithValue("@toDate", new DateTimeUtils().endTimeOfDate(log_date).ToString());
                        }

                        var reader = await command.ExecuteReaderAsync();
                        if (reader.Read())
                            count = int.Parse(reader["cnt"].ToString());

                        if (!reader.IsClosed)
                            await reader.CloseAsync();

                        if (connection.State != System.Data.ConnectionState.Closed)
                            await connection.CloseAsync();

                        return count;
                    }
                }
            }
            catch (Exception ex){
                return 0;
            }
        }
    }
}
