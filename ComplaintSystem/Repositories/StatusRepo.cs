using ComplaintSystem.Data;
using ComplaintSystem.Models;
using Dapper;
using Serilog;

namespace ComplaintSystem.Repositories
{
    public interface IStatus
    {
        public Task<List<Status>> GetAllStatuses();
        public Task<List<string>> GetAllStatusNames();
        public Task<Status> GetStatusById(Guid id);
        public Task<Status> GetStatusByName(string name);
        public Task<bool> DeleteStatus(Guid id);
        public Task<bool> UpdateStatus(Guid id, AddStatus payload);
        public Task<bool> AddStatus(AddStatus payload);
    }


    public class StatusRepo : IStatus
    {

        private readonly IDapperContext _context;

        public StatusRepo(IDapperContext context)
        {
            _context = context;
        }

        public async Task<bool> AddStatus(AddStatus payload)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("name", payload.Name.ToLower());

                string command = @"INSERT INTO Statuses(Name) VALUES(@name)";

                return await _context.ExecuteCommand(command, parameters);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Status Repo while trying to add a status");
                throw;
            }
        }

        public async Task<bool> DeleteStatus(Guid id)
        {
            try
            {

                var parameters = new DynamicParameters();
                parameters.Add("id", id);

                string command = @"DELETE FROM Statuses WHERE Id = @id";

                return await _context.ExecuteCommand(command, parameters);


            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Status Repo while trying to delete a status");
                throw;
            }

        }

        public async Task<List<Status>> GetAllStatuses()
        {
            try
            {
                string query = @"SELECT * FROM Statuses";

                var data = await _context.Query<Status>(query);

                return data;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Status Repo while trying to get all statuses");
                throw;
            }

        }

        public Task<List<string>> GetAllStatusNames()
        {
            try
            {
                string query = @"SELECT * FROM Statuses";

                var data = await _context.Query<Status>(query);

                return data;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Status Repo while trying to get all status names");
                throw;
            }
        }

        public async Task<Status> GetStatusById(Guid id)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("id", id);


                string query = @"SELECT * FROM Statuses WHERE Id = @id";

                var data = await _context.QuerySingleRecord<Status>(query, parameters);

                return data;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Status Repo while trying to a status by id");
                throw;
            }

        }

        public async Task<Status> GetStatusByName(string name)
        {
            try
            {

                var parameters = new DynamicParameters();
                parameters.Add("name", name.ToLower());


                string query = @"SELECT * FROM Statuses WHERE Name = @name";

                var data = await _context.QuerySingleRecord<Status>(query, parameters);

                return data;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Status Repo while trying to a status by name");
                throw;
            }

        }

        public async Task<bool> UpdateStatus(Guid id, AddStatus payload)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("id", id);
                parameters.Add("name", payload.Name.ToLower());

                string command = @"UPDATE Statuses SET Name = @name WHERE Id = @id";

                return await _context.ExecuteCommand(command, parameters);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Status Repo while trying to update a status");
                throw;
            }

        }
    }
}
