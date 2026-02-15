using Microsoft.Data.SqlClient;

namespace Telemedicine.API.Data
{
    public static class DatabaseMigrator
    {
        public static void Migrate(string connectionString)
        {
            var migrationFile = Path.Combine(AppContext.BaseDirectory, "Data", "Migration_Phase15_Profile.sql");
            
            if (!File.Exists(migrationFile))
            {

                 migrationFile = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Migration_Phase15_Profile.sql");
            }

            if (!File.Exists(migrationFile))
            {
                Console.WriteLine($"Migration file not found at: {migrationFile}");
                return;
            }

            var sql = File.ReadAllText(migrationFile);

            using var conn = new SqlConnection(connectionString);
            conn.Open();

            var commands = sql.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var cmdText in commands)
            {
                if (string.IsNullOrWhiteSpace(cmdText)) continue;
                using var cmd = new SqlCommand(cmdText, conn);
                cmd.ExecuteNonQuery();
            }
            
            Console.WriteLine("Migration executed successfully.");
        }
    }
}
