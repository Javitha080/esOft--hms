using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Configuration;

namespace SimpleHMS
{
    // Database helper for .NET 9
    public static class DB
    {
        private static readonly string conn = 
            "Data Source=localhost\\SQLEXPRESS;Initial Catalog=SimpleHospitalDB;Integrated Security=True;TrustServerCertificate=True;";
            
        // Get connection string
        public static string GetConnectionString()
        {
            return conn;
        }
        
        // Get SqlConnection object
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(conn);
        }

        // SELECT queries
        public static DataTable GetData(string query)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(conn))
            using (SqlDataAdapter da = new SqlDataAdapter(query, con))
            {
                da.Fill(dt);
            }

            return dt;
        }

        // INSERT / UPDATE / DELETE queries
        public static int SetData(string query)
        {
            using (SqlConnection con = new SqlConnection(conn))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }
        
        // Check if database connection is available
        public static bool IsConnected()
        {
            try
            {
                if (string.IsNullOrEmpty(conn))
                    return false;
                    
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
