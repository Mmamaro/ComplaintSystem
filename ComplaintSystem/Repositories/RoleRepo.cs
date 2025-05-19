using ComplaintSystem.Data;
using ComplaintSystem.Models;
using Dapper;
using Serilog;

namespace ComplaintSystem.Repositories
{
    public interface IRole
    {
        public Task<List<Role>> GetAllRoles();
        public Task<Role> GetRoleById(Guid id);
        public Task<Role> GetRoleByName(string name);
        public Task<bool> DeleteRole(Guid id);
        public Task<bool> UpdateRole(Guid id, AddRole payload);
        public Task<bool> AddRole(AddRole payload);
    }


    public class RoleRepo : IRole
    {

        private readonly IDapperContext _context;

        public RoleRepo(IDapperContext context)
        {
            _context = context;
        }

        public async Task<bool> AddRole(AddRole payload)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("name", payload.Name.ToLower());

                string command = @"INSERT INTO Roles(Name) VALUES(@name)";

                return await _context.ExecuteCommand(command, parameters);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Role Repo while trying to add a department");
                throw;
            }
        }

        public async Task<bool> DeleteRole(Guid id)
        {
            try
            {

                var parameters = new DynamicParameters();
                parameters.Add("id", id);

                string command = @"DELETE FROM Roles WHERE Id = @id";

                return await _context.ExecuteCommand(command, parameters);


            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Role Repo while trying to delete a department");
                throw;
            }

        }

        public async Task<List<Role>> GetAllRoles()
        {
            try
            {
                string query = @"SELECT * FROM Roles";

                var data = await _context.Query<Role>(query);

                return data;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Role Repo while trying to get all department");
                throw;
            }

        }

        public async Task<Role> GetRoleById(Guid id)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("id", id);


                string query = @"SELECT * FROM Roles WHERE Id = @id";

                var data = await _context.QuerySingleRecord<Role>(query, parameters);

                return data;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Role Repo while trying to a department by id");
                throw;
            }

        }

        public async Task<Role> GetRoleByName(string name)
        {
            try
            {

                var parameters = new DynamicParameters();
                parameters.Add("name", name.ToLower());


                string query = @"SELECT * FROM Roles WHERE Name = @name";

                var data = await _context.QuerySingleRecord<Role>(query, parameters);

                return data;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Role Repo while trying to a department by name");
                throw;
            }

        }

        public async Task<bool> UpdateRole(Guid id, AddRole payload)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("id", id);
                parameters.Add("name", payload.Name.ToLower());

                string command = @"UPDATE Roles SET Name = @name WHERE Id = @id";

                return await _context.ExecuteCommand(command, parameters);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Role Repo while trying to update a department");
                throw;
            }

        }
    }
}
