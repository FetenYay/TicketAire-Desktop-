using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Data.SqlClient;
using TicketAire.Desktop.DAL.Models;

namespace TicketAire.Desktop.DAL
{
    public static class Database
    {
        private static readonly string _cnString =
           ConfigurationManager.ConnectionStrings["TicketAireDb"].ConnectionString;

        private static SqlConnection GetConn() => new SqlConnection(_cnString);

        public static SqlConnection GetConnection()
        {
            var cs = ConfigurationManager
                         .ConnectionStrings["TicketAireDb"]
                         .ConnectionString;
            return new SqlConnection(cs);
        }
        public static User AuthenticateUser(string enterpriseId, string accessKey)
        {
            const string sql = @"
              SELECT id, enterprise_id, first_name, last_name, email, role
                FROM users
               WHERE enterprise_id=@eid
                 AND access_key=@key";
            using var conn = GetConn();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@eid", enterpriseId);
            cmd.Parameters.AddWithValue("@key", accessKey);
            conn.Open();
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            return new User
            {
                Id = (int)r["id"],
                EnterpriseId = (string)r["enterprise_id"],
                FirstName = (string)r["first_name"],
                LastName = (string)r["last_name"],
                Email = (string)r["email"],
                Role = (string)r["role"]
            };
        }

        public static List<User> GetAllUsers(string search = "")
        {
            var list = new List<User>();
            var sql = search == ""
              ? "SELECT id, enterprise_id, first_name, last_name, email, role FROM users"
              : @"SELECT id, enterprise_id, first_name, last_name, email, role
                    FROM users
                   WHERE first_name LIKE @q OR last_name LIKE @q OR email LIKE @q";
            using var conn = GetConn();
            using var cmd = new SqlCommand(sql, conn);
            if (search != "") cmd.Parameters.AddWithValue("@q", $"%{search}%");
            conn.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new User
                {
                    Id = (int)r["id"],
                    EnterpriseId = (string)r["enterprise_id"],
                    FirstName = (string)r["first_name"],
                    LastName = (string)r["last_name"],
                    Email = (string)r["email"],
                    Role = (string)r["role"]
                });
            }
            return list;
        }

        public static void DeleteUser(int userId)
        {
            using var conn = GetConn();
            using var cmd = new SqlCommand("DELETE FROM users WHERE id=@id", conn);
            cmd.Parameters.AddWithValue("@id", userId);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public static List<Ticket> GetAllTickets(string search = "")
        {
            var list = new List<Ticket>();
            var sql = @"
                  SELECT t.id AS TicketId, t.title, t.status, t.priority, t.created_at, 
                         CONCAT(r.first_name,' ',r.last_name) AS RequesterName,
                         ISNULL(CONCAT(te.first_name,' ',te.last_name),'Unassigned') AS TechnicianName,
                         c.name as CategoryName
                    FROM tickets t
                    LEFT JOIN users r ON t.user_id = r.id
                    LEFT JOIN users te ON t.technician_id = te.id
                    LEFT JOIN categories c ON t.category_id = c.id"
              + (search == "" ? "" : " WHERE t.title LIKE @q OR r.first_name LIKE @q");
            using var conn = GetConn();
            using var cmd = new SqlCommand(sql, conn);
            if (search != "") cmd.Parameters.AddWithValue("@q", $"%{search}%");
            conn.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Ticket
                {
                    TicketId = (int)r["TicketId"],
                    Title = (string)r["title"],
                    Status = (string)r["status"],
                    Priority = (string)r["priority"],
                    CreatedDate = (DateTime)r["created_at"],
                    RequesterName = (string)r["RequesterName"],
                    TechnicianName = (string)r["TechnicianName"],
                    CategoryName = (string)r["CategoryName"]
                });
            }
            return list;
        }




        public static List<Ticket> GetAssignedTickets(int techId)
        {
            var list = new List<Ticket>();
            const string sql = @"
              SELECT id AS TicketId, title, status, priority, created_at
                FROM tickets
               WHERE technician_id = @tid";
            using var conn = GetConn();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tid", techId);
            conn.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Ticket
                {
                    TicketId = (int)r["TicketId"],
                    Title = (string)r["title"],
                    Status = (string)r["status"],
                    Priority = (string)r["priority"],
                    CreatedDate = (DateTime)r["created_at"]
                });
            }
            return list;
        }

        public static List<Ticket> GetMyTickets(int userId)
        {
            var list = new List<Ticket>();
            const string sql = @"
              SELECT t.id AS TicketId, t.title, t.status, t.priority, t.created_at,
                     ISNULL(CONCAT(te.first_name,' ',te.last_name),'Unassigned') AS TechnicianName
                FROM tickets t
                LEFT JOIN users te ON t.technician_id = te.id
               WHERE t.user_id = @uid";
            using var conn = GetConn();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Ticket
                {
                    TicketId = (int)r["TicketId"],
                    Title = (string)r["title"],
                    Status = (string)r["status"],
                    Priority = (string)r["priority"],
                    CreatedDate = (DateTime)r["created_at"],
                    TechnicianName = (string)r["TechnicianName"]
                });
            }
            return list;
        }

        public static List<Category> GetAllCategories()
        {
            var list = new List<Category>();
            const string sql = "SELECT id, name FROM categories";
            using var conn = GetConn();
            using var cmd = new SqlCommand(sql, conn);
            conn.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Category
                {
                    Id = (int)r["id"],
                    Name = (string)r["name"]
                });
            }
            return list;
        }

        public static void CreateTicket(int userId, int categoryId, string title, string desc, string priority)
        {
            const string sql = @"
              INSERT INTO tickets(user_id,category_id,title,description,priority)
                   VALUES(@uid,@cid,@t,@d,@p)";
            using var conn = GetConn();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@cid", categoryId);
            cmd.Parameters.AddWithValue("@t", title);
            cmd.Parameters.AddWithValue("@d", desc);
            cmd.Parameters.AddWithValue("@p", priority);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
        public static void CreateUser(string eid, string fn, string ln, string email, string role)
        {
            using var conn = GetConn();
            using var cmd = new SqlCommand(
                "INSERT INTO users(enterprise_id, first_name, last_name, email, role, access_key) " +
                "VALUES(@eid,@fn,@ln,@em,@r,'changeme')", conn);
            cmd.Parameters.AddWithValue("@eid", eid);
            cmd.Parameters.AddWithValue("@fn", fn);
            cmd.Parameters.AddWithValue("@ln", ln);
            cmd.Parameters.AddWithValue("@em", email);
            cmd.Parameters.AddWithValue("@r", role);
            conn.Open(); cmd.ExecuteNonQuery();
        }

        public static void UpdateUser(User u)
        {
            using var conn = GetConn();
            using var cmd = new SqlCommand(
                "UPDATE users SET enterprise_id=@eid, first_name=@fn, last_name=@ln, email=@em, role=@r WHERE id=@id", conn);
            cmd.Parameters.AddWithValue("@eid", u.EnterpriseId);
            cmd.Parameters.AddWithValue("@fn", u.FirstName);
            cmd.Parameters.AddWithValue("@ln", u.LastName);
            cmd.Parameters.AddWithValue("@em", u.Email);
            cmd.Parameters.AddWithValue("@r", u.Role);
            cmd.Parameters.AddWithValue("@id", u.Id);
            conn.Open(); cmd.ExecuteNonQuery();
        }
        public static Ticket GetTicketById(int id)
        {
            const string sql = @"
      SELECT t.id AS TicketId, t.title, t.status, t.priority, t.created_at, 
             CONCAT(r.first_name,' ',r.last_name) AS RequesterName,
             ISNULL(CONCAT(te.first_name,' ',te.last_name),'Unassigned') AS TechnicianName,
             c.name as CategoryName
      FROM tickets t
      LEFT JOIN users r ON t.user_id = r.id
      LEFT JOIN users te ON t.technician_id = te.id
      LEFT JOIN categories c ON t.category_id = c.id
      WHERE t.id = @id";

            using var conn = GetConn();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            using var r = cmd.ExecuteReader();
            if (r.Read())
            {
                return new Ticket
                {
                    TicketId = (int)r["TicketId"],
                    Title = (string)r["title"],
                    Status = (string)r["status"],
                    Priority = (string)r["priority"],
                    CreatedDate = (DateTime)r["created_at"],
                    RequesterName = (string)r["RequesterName"],
                    TechnicianName = (string)r["TechnicianName"],
                    CategoryName = (string)r["CategoryName"]
                };
            }
            
            return null;
        }
        public static void AddComment(int ticketId, int userId, string content, char isSolution)
        {
            const string sql = @"
      INSERT INTO comments(ticket_id, user_id, content, is_solution, created_at)
      VALUES(@tid, @uid, @ct, @sol, GETDATE())";

            using var conn = GetConn();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tid", ticketId);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@ct", content);
            cmd.Parameters.AddWithValue("@sol", isSolution);   
            conn.Open();
            cmd.ExecuteNonQuery();
        }
        public static void ResolveTicket(int ticketId)
        {
            
            const string sql1 = @"
              UPDATE tickets
                 SET status     = 'resolved',
                     updated_at = GETDATE()
               WHERE id = @tid";
            using (var conn = GetConn())
            using (var cmd = new SqlCommand(sql1, conn))
            {
                cmd.Parameters.AddWithValue("@tid", ticketId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            
            const string sql2 = @"
      INSERT INTO ticket_history(ticket_id, status, changed_at)
      VALUES(@tid, 'resolved', GETDATE())";
            using (var conn = GetConn())
            using (var cmd = new SqlCommand(sql2, conn))
            {
                cmd.Parameters.AddWithValue("@tid", ticketId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }




        public static List<Comment> GetCommentsForTicket(int ticketId)
{
        var list = new List<Comment>();
        const string sql = @"
          SELECT c.id, c.ticket_id, c.user_id, c.content, c.is_solution, c.created_at,
                 CONCAT(u.first_name,' ',u.last_name) AS UserName
          FROM comments c
          JOIN users u ON c.user_id = u.id
          WHERE c.ticket_id = @tid
          ORDER BY c.created_at";

        using var conn = GetConn();
        using var cmd  = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@tid", ticketId);
        conn.Open();
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
        list.Add(new Comment
            {
            Id         = (int)r["id"],
            TicketId   = ticketId,
            UserId     = (int)r["user_id"],
            Content    = (string)r["content"],
            IsSolution = (string)r["is_solution"] == "Y",
            CreatedAt  = (DateTime)r["created_at"],
            UserName   = (string)r["UserName"]
             });
        }
        return list;
        }

        public static List<TicketHistory> GetAllTicketHistory()
        {
            var list = new List<TicketHistory>();
            const string sql = @"
              SELECT ticket_id,
                     LAG(status) OVER (PARTITION BY ticket_id ORDER BY changed_at) AS OldStatus,
                     status               AS NewStatus,
                     changed_at           AS ChangedAt
              FROM ticket_history
              ORDER BY changed_at DESC";

            using var conn = GetConn();
            using var cmd = new SqlCommand(sql, conn);
            conn.Open();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new TicketHistory
                {
                    TicketId = (int)r["ticket_id"],
                    OldStatus = r["OldStatus"] as string ?? "(none)",
                    NewStatus = (string)r["NewStatus"],
                    ChangedAt = (DateTime)r["ChangedAt"]
                });
            }
            return list;
        }


    }
}
