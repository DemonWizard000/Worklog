using Microsoft.Data.SqlClient;
using System.Reflection.Metadata.Ecma335;
using WorkLog.Models;
using WorkLog.Constants;

namespace WorkLog.Repository
{
    public class ChannelRepository
    {
        private static string ConnectionString = DbConstant.WorkLogDBConnectionString;
        public ChannelRepository()
        {
        }


        public async Task<Channel> GetChannelById(long channelId)
        {
            Channel channel = null;
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("SELECT * FROM Channels WHERE Id = @channelId", connection))
                    {
                        command.Parameters.AddWithValue("@channelId", channelId);
                        var reader = await command.ExecuteReaderAsync();

                        if (reader.Read())
                        {
                            channel = new Channel()
                            {
                                Id = long.Parse(reader["Id"].ToString()),
                                Name = reader["Name"].ToString()
                            };
                        }

                        if (!reader.IsClosed)
                            await reader.CloseAsync();

                        if (connection.State != System.Data.ConnectionState.Closed)
                            await connection.CloseAsync();

                        return channel;
                    }
                }
            } catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<Group>> GetGroupsByUserEmailAndChannelId(long channelId, string userEmail, int state = 1)
        {
            List<Group> groups = new List<Group>();
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("SELECT * FROM GroupUsers INNER JOIN Groups on (GroupId = Groups.Id) WHERE UserEmail= @userEmail AND State = @state AND ChannelId = @channelId", connection))
                    {
                        command.Parameters.AddWithValue("@userEmail", userEmail);
                        command.Parameters.AddWithValue("@state", state);
                        command.Parameters.AddWithValue("@channelId", channelId);

                        var reader = await command.ExecuteReaderAsync();

                        while (reader.Read())
                        {
                            Group nGroup = new Group()
                            {
                                Name= reader["Name"].ToString(),
                                Id = long.Parse(reader["GroupId"].ToString()),
                                Manager_email = reader["Manager_email"].ToString()
                            };
                            groups.Add(nGroup);
                        }

                        if (!reader.IsClosed)
                            await reader.CloseAsync();

                        if (connection.State != System.Data.ConnectionState.Closed)
                            await connection.CloseAsync();

                        return groups;
                    }
                }
            } catch(Exception ex)
            {
                return groups;
            }
            
        }

        public async Task<List<ChannelGroupDTO>> GetOwnedChannelsList(string userEmail)
        {
            List<ChannelGroupDTO> channelGroupDTOs = new List<ChannelGroupDTO>();

            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("SELECT * FROM Channels WHERE Manager_email = @managerEmail", connection))
                    {
                        command.Parameters.AddWithValue("@managerEmail", userEmail);
                        var reader = await command.ExecuteReaderAsync();

                        while (reader.Read())
                        {
                            ChannelGroupDTO channelGroupDTO = new ChannelGroupDTO()
                            {
                                ChannelId = long.Parse(reader["Id"].ToString()),
                                ChannelName = reader["Name"].ToString(),
                                ChannelDescription = reader["Description"].ToString(),
                                ChannelManager = reader["Manager_email"].ToString(),
                                Groups = await GetGroupsByUserEmailAndChannelId(long.Parse(reader["Id"].ToString()), userEmail),
                            };

                            channelGroupDTOs.Add(channelGroupDTO);
                        }

                        if (!reader.IsClosed)
                            await reader.CloseAsync();

                        if (connection.State != System.Data.ConnectionState.Closed)
                            await connection.CloseAsync();

                        return channelGroupDTOs;
                    }
                }
            }
            catch (Exception ex)
            {
                return channelGroupDTOs;
            }
        }

        public async Task<List<ChannelGroupDTO>> GetJoinedChannelList(string userEmail)
        {
            List<ChannelGroupDTO> channelGroupDTOs = new List<ChannelGroupDTO>();

            channelGroupDTOs.Add(new ChannelGroupDTO()
            {
                ChannelId = -1,
                ChannelName = "Default",
                Groups = await GetGroupsByUserEmailAndChannelId(-1, userEmail)
            }) ;

            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("SELECT * FROM ChannelUsers INNER JOIN Channels on (ChannelId = Channels.Id) WHERE UserEmail= @userEmail AND State = 1", connection))
                    {
                        command.Parameters.AddWithValue("@userEmail", userEmail);
                        var reader = await command.ExecuteReaderAsync();

                        while(reader.Read())
                        {
                            ChannelGroupDTO channelGroupDTO = new ChannelGroupDTO() 
                            {
                                ChannelId = long.Parse(reader["ChannelId"].ToString()),
                                ChannelName = reader["Name"].ToString(),
                                ChannelDescription = reader["Description"].ToString(),
                                ChannelManager = reader["Manager_email"].ToString(),
                                Groups = await GetGroupsByUserEmailAndChannelId(long.Parse(reader["ChannelId"].ToString()), userEmail),
                            };
                            
                            channelGroupDTOs.Add(channelGroupDTO);
                        }

                        if (!reader.IsClosed)
                            await reader.CloseAsync();

                        if (connection.State != System.Data.ConnectionState.Closed)
                            await connection.CloseAsync();

                        return channelGroupDTOs;
                    }                
                }
            }
            catch (Exception ex)
            {
                return channelGroupDTOs;
            }
        }

        public async Task<List<Channel>> GetAvailableChannelLists(string userEmail)
        {
            List<Channel> channels = new List<Channel>();

            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("SELECT * FROM ChannelUsers INNER JOIN Channels on (ChannelId = Channels.Id) WHERE UserEmail= @userEmail AND State = 1", connection))
                    {
                        command.Parameters.AddWithValue("@userEmail", userEmail);
                        var reader = await command.ExecuteReaderAsync();

                        while (reader.Read())
                        {
                            channels.Add(new Channel()
                            {
                                Id = long.Parse(reader["ChannelId"].ToString()),
                                Name = reader["Name"].ToString(),
                                Manager_email = reader["Manager_email"].ToString()
                            });
                        }

                        if (!reader.IsClosed)
                            await reader.CloseAsync();

                        if (connection.State != System.Data.ConnectionState.Closed)
                            await connection.CloseAsync();

                        return channels;
                    }
                }
            }
            catch (Exception ex)
            {
                return channels;
            }
        }
        public async Task<List<Group>> GetAvailableGroupLists(string userEmail)
        {
            List<Group> groups = new List<Group>();

            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("SELECT * FROM GroupUsers INNER JOIN Groups on (GroupId = Groups.Id) WHERE UserEmail = @userEmail AND State = 1", connection))
                    {
                        command.Parameters.AddWithValue("@userEmail", userEmail);
                        var reader = await command.ExecuteReaderAsync();

                        while (reader.Read())
                        {
                            groups.Add(new Group()
                            {
                                Id = long.Parse(reader["GroupId"].ToString()),
                                Name = reader["Name"].ToString(),
                                Manager_email = reader["Manager_email"].ToString()
                            });
                        }

                        if (!reader.IsClosed)
                            await reader.CloseAsync();

                        if (connection.State != System.Data.ConnectionState.Closed)
                            await connection.CloseAsync();

                        return groups;
                    }
                }
            }
            catch (Exception ex)
            {
                return groups;
            }
        }

        public async Task<bool> GroupInvitationExist(string userEmail)
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("SELECT * FROM GroupUsers INNER JOIN Groups on (GroupId = Groups.Id) WHERE UserEmail = @userEmail AND State = 0", connection))
                    {
                        command.Parameters.AddWithValue("@userEmail", userEmail);
                        var reader = await command.ExecuteReaderAsync();

                        if (reader.Read())
                        {
                            if (!reader.IsClosed)
                                await reader.CloseAsync();

                            if (connection.State != System.Data.ConnectionState.Closed)
                                await connection.CloseAsync();

                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<bool> ChannelInvitationExist(string userEmail)
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("SELECT * FROM ChannelUsers INNER JOIN Channels on (ChannelId = Channels.Id) WHERE UserEmail = @userEmail AND State = 0", connection))
                    {
                        command.Parameters.AddWithValue("@userEmail", userEmail);
                        var reader = await command.ExecuteReaderAsync();

                        if (reader.Read())
                        {
                            if (!reader.IsClosed)
                                await reader.CloseAsync();

                            if (connection.State != System.Data.ConnectionState.Closed)
                                await connection.CloseAsync();

                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
