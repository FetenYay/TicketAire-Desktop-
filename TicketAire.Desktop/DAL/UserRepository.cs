using System;
using System.Data;
using Microsoft.Data.SqlClient;
using TicketAire.Desktop.DAL.Models;

namespace TicketAire.Desktop.DAL
{
    public class UserRepository
    {
        public User Authenticate(string enterpriseId, string accessKey)
        {
            const string sql = @"
                SELECT TOP 1 id, enterprise_id, first_name, last_name, email, role
                  FROM users
                 WHERE enterprise_id = @eid
                   AND access_key    = @key";

            using var cn = Database.GetConnection();
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.Add("@eid", SqlDbType.VarChar, 50).Value = enterpriseId;
            cmd.Parameters.Add("@key", SqlDbType.VarChar, 255).Value = accessKey;
            cn.Open();

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return new User
            {
                Id = reader.GetInt32(0),
                EnterpriseId = reader.GetString(1),
                FirstName = reader.GetString(2),
                LastName = reader.GetString(3),
                Email = reader.GetString(4),
                Role = reader.GetString(5)
            };
        }
    }
}
