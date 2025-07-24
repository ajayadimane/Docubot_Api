using DocuBot_Api.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;

namespace DocuBot_Api.Models.Helpers
{
    public class KvalRepository
    {
        private readonly string _connectionString;

        public KvalRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("myconn");
        }

        public async Task<List<Dictionary<string, string>>> GetKvalDetailsByLfidAsync(int lfid)
        {
            var results = new List<Dictionary<string, string>>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("SELECT K1, Kval1 FROM kval WHERE lfid = @Lfid", connection))
                {
                    command.Parameters.Add(new SqlParameter("@Lfid", SqlDbType.Int) { Value = lfid });

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var row = new Dictionary<string, string>
                        {
                            { "K1", reader["K1"].ToString() },
                            { "Kval1", reader["Kval1"].ToString() }
                        };
                            results.Add(row);
                        }
                    }
                }
            }

            return results;
        }
        public async Task<List<Dictionary<string, string>>> GetCleanedTableDataByLfidAsync(int lfid)
        {
            var result = new List<Dictionary<string, string>>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Define columns to extract
                string columns = "col1, col2, col3, col4, col5, col6, col7";

                // Fetch data from specified columns based on lfid
                using (var command = new SqlCommand($"SELECT {columns} FROM tabledata WHERE lfid = @Lfid order by id", connection))
                {
                    command.Parameters.Add(new SqlParameter("@Lfid", SqlDbType.Int) { Value = lfid });

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Read the first row to use as keys
                        List<string> keys = null;
                        if (await reader.ReadAsync())
                        {
                            keys = new List<string>();
                            for (int i = 0; i < 7; i++)
                            {
                                keys.Add(reader[$"col{i + 1}"].ToString().Trim());
                            }
                        }

                        // Read the remaining rows and use keys to populate dictionaries
                        while (await reader.ReadAsync())
                        {
                            var data = new Dictionary<string, string>();

                            for (int i = 0; i < 7; i++)
                            {
                                var value = reader[$"col{i + 1}"].ToString().Trim();

                                // Remove spaces and newline characters from values
                                var cleanedValue = value.Replace(" ", "").Replace("\n", "").Replace("\r", "");

                                if (keys != null && i < keys.Count)
                                {
                                    data[keys[i]] = cleanedValue;
                                }
                            }

                            result.Add(data);
                        }
                    }
                }
            }

            return result;
        }


        public async Task<string> GetDocumentDetailsByLJIDAsync(int ljid)
        {
            var results = new List<Dictionary<string, string>>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("SELECT docname, id, status FROM LoadedFiles WHERE ljid = @Ljid", connection))
                {
                    command.Parameters.Add(new SqlParameter("@Ljid", SqlDbType.Int) { Value = ljid });

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var row = new Dictionary<string, string>
                    {
                        { "docname", reader["docname"].ToString() },
                        { "id", reader["id"].ToString() },
                        { "status", reader["status"].ToString() }
                    };
                            results.Add(row);
                        }
                    }
                }
            }

            return JsonConvert.SerializeObject(results);
        }





    }
}
