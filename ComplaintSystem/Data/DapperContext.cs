using Dapper;
using Microsoft.Data.SqlClient;
using Serilog;
using System.Data;

namespace ComplaintSystem.Data
{
    public interface IDapperContext
    {
        Task<List<T>> Query<T>(string sql);
        Task<T> QuerySingleRecord<T>(string sql, DynamicParameters parameters);
        Task<bool> ExecuteCommand(string sql, DynamicParameters parameters);
        Task<List<T>> QueryWithParams<T>(string sql);
    }

    public class DapperContext : IDapperContext
    {
        private readonly IConfiguration _config;

        public DapperContext(IConfiguration config)
        {
            _config = config;
        }

        public async Task<List<T>> Query<T>(string sql)
        {
            try
            {
                using IDbConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

                var data = await connection.QueryAsync<T>(sql);

                return data.ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Dapper Context while trying to query data");
                throw;
            }
        }

        public async Task<T> QuerySingleRecord<T>(string sql, DynamicParameters parameters)
        {
            try
            {
                using IDbConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

                var data = await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);

                return data;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Dapper Context while trying to query single record");
                throw;
            }
        }

        public async Task<List<T>> QueryWithParams<T>(string sql)
        {
            try
            {
                using IDbConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

                var data = await connection.QueryAsync<T>(sql);

                return data.ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Dapper Context while trying to query with parameters");
                throw;
            }
        }

        public async Task<bool> ExecuteCommand(string sql, DynamicParameters parameters)
        {
            try
            {
                using IDbConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

                return await connection.ExecuteAsync(sql, parameters) > 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Dapper Context while trying to execute a command");
                throw;
            }
        }
    }
}
