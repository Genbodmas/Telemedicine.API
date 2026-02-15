using Microsoft.Data.SqlClient;

namespace Telemedicine.API
{
    public static class Phase9Migration
    {
        public static async Task Run(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var scriptPath = Path.Combine(AppContext.BaseDirectory, "Data", "Procedures_Phase9.sql");
            if (!File.Exists(scriptPath))
            {
                // Fallback for development where file might be in source folder
                scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Procedures_Phase9.sql");
            }

            if (File.Exists(scriptPath))
            {
                var script = await File.ReadAllTextAsync(scriptPath);
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // Split script by GO
                var commands = script.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var command in commands)
                {
                    if (string.IsNullOrWhiteSpace(command)) continue;
                    using var cmd = new SqlCommand(command, connection);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
