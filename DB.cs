using System.Data;
using Microsoft.Data.SqlClient;

namespace SimpleHMS
{
    // DB - Database class for .NET 9
    // This class handles all database operations
    // It has 2 simple methods: GetData (for SELECT) and SetData (for INSERT/UPDATE/DELETE)
    public class DB
    {
        // Connection string - tells the program how to connect to SQL Server
        // Update "Data Source" to match your SQL Server name
        static string conn = "Data Source=JAVITHA00\\SQLEXPRESS;Initial Catalog=SimpleHospitalDB;Integrated Security=True";
        
        // Explanation of connection string parts:
        // - Data Source=localhost : Your SQL Server location (change if needed)
        // - Initial Catalog=SimpleHospitalDB : Database name
        // - Integrated Security=True : Use Windows authentication (no username/password needed)
        // - TrustServerCertificate=True : Required for .NET 9 to trust SQL Server certificate

        // GetData() - Used for SELECT queries (getting data from database)
        // Returns: DataTable containing the query results
        // Example: DataTable dt = DB.GetData("SELECT * FROM Patients");
        public static DataTable GetData(string query)
        {
            // Create a new DataTable to store the results
            DataTable dt = new DataTable();
            
            // using statement ensures connection is closed automatically
            using (SqlConnection con = new SqlConnection(conn))
            {
                // SqlDataAdapter executes the query and fills the DataTable
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.Fill(dt); // Fill the DataTable with query results
            }
            
            // Return the DataTable with all the data
            return dt;
        }

        // SetData() - Used for INSERT, UPDATE, DELETE queries (changing data in database)
        // Returns: Number of rows affected (1 = success, 0 = failed)
        // Example: int result = DB.SetData("INSERT INTO Patients (Name, Age) VALUES ('John', 30)");
        public static int SetData(string query)
        {
            // using statement ensures connection is closed automatically
            using (SqlConnection con = new SqlConnection(conn))
            {
                // Create a command object with the query
                SqlCommand cmd = new SqlCommand(query, con);
                
                // Open the database connection
                con.Open();
                
                // ExecuteNonQuery() runs the query and returns number of rows affected
                int rows = cmd.ExecuteNonQuery();
                
                // Close the database connection
                con.Close();
                
                // Return the number of rows affected
                return rows;
            }
        }
    }
}

