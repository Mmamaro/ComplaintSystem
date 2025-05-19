using ComplaintSystem.Data;
using ComplaintSystem.Models;
using Dapper;
using Serilog;

namespace ComplaintSystem.Repositories
{
    public interface IDepartment
    {
        public Task<List<Department>> GetAllDepartments();
        public Task<Department> GetDepartmentById(Guid id);
        public Task<Department> GetDepartmentByName(string name);
        public Task<bool> DeleteDepartment(Guid id);
        public Task<bool> UpdateDepartment(Guid id, AddDepartment payload);
        public Task<bool> AddDepartment(AddDepartment payload);
    }


    public class DepartmentRepo : IDepartment
    {

        private readonly IDapperContext _context;

        public DepartmentRepo(IDapperContext context)
        {
            _context = context;
        }

        public async Task<bool> AddDepartment(AddDepartment payload)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("name", payload.Name.ToLower());

                string command = @"INSERT INTO Departments(Name) VALUES(@name)";

                return await _context.ExecuteCommand(command, parameters);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Department Repo while trying to add a department");
                throw;
            }
        }

        public async Task<bool> DeleteDepartment(Guid id)
        {
            try
            {

                var parameters = new DynamicParameters();
                parameters.Add("id", id);

                string command = @"DELETE FROM Departments WHERE Id = @id";

                return await _context.ExecuteCommand(command, parameters);


            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Department Repo while trying to delete a department");
                throw;
            }

        }

        public async Task<List<Department>> GetAllDepartments()
        {
            try
            {
                string query = @"SELECT * FROM Departments";

                var data = await _context.Query<Department>(query);

                return data;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Department Repo while trying to get all department");
                throw;
            }

        }

        public async Task<Department> GetDepartmentById(Guid id)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("id", id);


                string query = @"SELECT * FROM Departments WHERE Id = @id";

                var data = await _context.QuerySingleRecord<Department>(query, parameters);

                return data;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Department Repo while trying to a department by id");
                throw;
            }

        }

        public async Task<Department> GetDepartmentByName(string name)
        {
            try
            {

                var parameters = new DynamicParameters();
                parameters.Add("name", name.ToLower());


                string query = @"SELECT * FROM Departments WHERE Name = @name";

                var data = await _context.QuerySingleRecord<Department>(query, parameters);

                return data;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Department Repo while trying to a department by name");
                throw;
            }

        }

        public async Task<bool> UpdateDepartment(Guid id, AddDepartment payload)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("id", id);
                parameters.Add("name", payload.Name.ToLower());

                string command = @"UPDATE Departments SET Name = @name WHERE Id = @id";

                return await _context.ExecuteCommand(command, parameters);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Department Repo while trying to update a department");
                throw;
            }

        }
    }
}
