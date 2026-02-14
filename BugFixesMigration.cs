using Microsoft.Data.SqlClient;

namespace Telemedicine.API
{
    public static class BugFixesMigration
    {
        public static async Task Run(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");

            var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Procedures_BugFixes.sql");
            if (!File.Exists(scriptPath)) 
            {
                Console.WriteLine("BugFixes script not found.");
                return;
            }

            var script = await File.ReadAllTextAsync(scriptPath);
            
            // Split by GO
            var commands = script.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            foreach (var commandText in commands)
            {
                if (string.IsNullOrWhiteSpace(commandText)) continue;
                using var cmd = new SqlCommand(commandText, conn);
                try 
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing script block: {ex.Message}");
                }
            }
        }
    }
}
