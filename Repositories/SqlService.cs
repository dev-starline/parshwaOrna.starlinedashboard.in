using Microsoft.Data.SqlClient;
using System.Data;

namespace SL_Bullion.Repositories
{
    public class SqlService
    {
        private readonly string _connectionString;

        public SqlService(IConfiguration configuration)
        {
            _connectionString = configuration["ConnectionStrings:DefaultConnection"];
        }

        public async Task<List<Dictionary<string, object>>> executeReaderList(string storedProcedure, SqlParameter[] parameters = null)
        {
            var resultList = new List<Dictionary<string, object>>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand(storedProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync()) // Read the data asynchronously
                            {
                                var row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string columnName = reader.GetName(i);
                                    object value = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
                                    row[columnName] = value;
                                }
                                resultList.Add(row);
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return resultList;

        }
    }
}
