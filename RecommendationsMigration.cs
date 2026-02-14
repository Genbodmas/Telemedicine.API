using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Telemedicine.API
{
    public class RecommendationsMigration
    {
        public static async Task Run(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var sqlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Procedures_Recommendations.sql");
            
            if (!File.Exists(sqlFilePath))
            {
                Console.WriteLine("Recommendations SQL file not found, skipping.");
                return;
            }

            var sql = await File.ReadAllTextAsync(sqlFilePath);
            var batches = sql.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            foreach (var batch in batches)
            {
                if (string.IsNullOrWhiteSpace(batch)) continue;
                using var cmd = new SqlCommand(batch, conn);
                await cmd.ExecuteNonQueryAsync();
            }

            Console.WriteLine("Recommendations Procedures executed successfully.");
        }
    }
}
