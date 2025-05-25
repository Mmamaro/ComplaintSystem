using Azure.Core;
using ComplaintSystem.Data;
using ComplaintSystem.Models;
using Dapper;
using Microsoft.Extensions.Options;
using Serilog;

namespace ComplaintSystem.Repositories
{

    #region [ Interface ]

    public interface IUser
    {
        public Task<bool> AddUser(User payload);
        public Task<bool> DeleteUser(Guid id);
        public Task<bool> UpdateUser(Guid id, UserUpdate payload);
        public Task<List<User>> GetAllUsers();
        public Task<List<string>> GetAllUserEmails();
        public Task<User> GetUserById(Guid id);
        public Task<User> GetUserByEmail(string email);
        public Task<User> GetUserByTwoFAKeyAsync(string encryptedText);
        public Task<bool> UpdateMFAfields(UpdateMfaFieldsModel request);
        public Task<bool> UpdatesByAdmin(Guid Id, UpdatesByAdmin payload);
        public Task<bool> updateFirstSignIn(string email);
        public Task<bool> updatePassword(LoginModel payload);
        //public Task<UpdateResult> UpdateActiveStatus(string id, UpdateActiveStatusModel status);
        public Task<User?> Login(LoginModel request);

    } 

    #endregion

    public class UserRepo : IUser
    {

        #region [ Constructor ]
        private readonly IDapperContext _dbContext;

        public UserRepo(IDapperContext dBContext)
        {
            _dbContext = dBContext;
        }

        #endregion

        #region [ Add User ]

        public async Task<bool> AddUser(User payload)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("firstname", payload.Firstname);
                parameters.Add("lastname", payload.Lastname);
                parameters.Add("Email", payload.Email.ToLower());
                parameters.Add("Role", payload.Role.ToLower());
                parameters.Add("isActive", payload.IsActive);
                parameters.Add("DepartmentId", payload.DepartmentId);
                parameters.Add("password", payload.Password);
                parameters.Add("isFirstSignin", payload.isFirstSignIn);
                parameters.Add("isMFAverified", payload.isMFAVerified);

                var command = @"INSERT INTO Users(FirstName, LastName, Email, Role, isActive, DepartmentId, Password, isFirstSignIn, isMFA_verified)
                                VALUES(@FirstName, @LastName, @Email, @Role, @isActive, @DepartmentId, @Password, @isFirstSignin, @isMFAverified)";

                return await _dbContext.ExecuteCommand(command, parameters);


            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the user repo while trying to add user");
                throw;

            }
        }

        #endregion

        #region [ Delete User ]

        public async Task<bool> DeleteUser(Guid id)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("id", id);

                var command = @"DELETE FROM Users WHERE Id = @id";

                return await _dbContext.ExecuteCommand(command, parameters);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the user repo while trying to delete user");
                throw;
            }
        }

        #endregion

        #region [ Get All Users ]

        public async Task<List<User>> GetAllUsers()
        {
            try
            {
                var query = @"SELECT u.Id,FirstName,LastName,Email,Role,Password,isActive,isFirstSignIn,isMFA_verified,twoFAKey,QRCode,manualCode,u.DepartmentId,d.Name as DepartmentName
                              FROM Users as u
                              INNER JOIN Departments as d
                              ON u.DepartmentId = d.Id";

                var data = await _dbContext.Query<User>(query);

                return data;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the user repo while trying to get all users");
                throw;
            }
        }

        #endregion

        #region [ Get User By Id ]

        public async Task<User> GetUserById(Guid id)
        {
            try
            {
                var parameter = new DynamicParameters();
                parameter.Add("id", id);

                var query = @"SELECT u.Id,FirstName,LastName,Email,Role,Password,isActive,isFirstSignIn,isMFA_verified,twoFAKey,QRCode,manualCode,u.DepartmentId,d.Name as DepartmentName
                              FROM Users as u
                              INNER JOIN Departments as d
                              ON u.DepartmentId = d.Id
                              WHERE u.Id = @id";

                var data = await _dbContext.QuerySingleRecord<User>(query, parameter);

                return data;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the user repo while trying to get user by id: {ex}");
                throw;
            }
        }
        #endregion


        #region [ Get user by two factor key ]
        public async Task<User> GetUserByTwoFAKeyAsync(string encryptedText)
        {
            try
            {

                var parameter = new DynamicParameters();
                parameter.Add("twoFAKey", encryptedText);

                var query = @"SELECT u.Id,FirstName,LastName,Email,Role,Password,isActive,isFirstSignIn,isMFA_verified,twoFAKey,QRCode,manualCode,u.DepartmentId,d.Name as DepartmentName
                              FROM Users as u
                              INNER JOIN Departments as d
                              ON u.DepartmentId = d.Id
                              WHERE twoFAKey = @twoFAKey";

                var data = await _dbContext.QuerySingleRecord<User>(query, parameter);

                return data;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the user repo while trying check the two factor key");
                throw;
            }
        }
        #endregion

        #region [ Get User By Email ]

        public async Task<User> GetUserByEmail(string email)
        {
            try
            {
                var parameter = new DynamicParameters();
                parameter.Add("email", email);

                var query = @"SELECT u.Id,FirstName,LastName,Email,Role,Password,isActive,isFirstSignIn,isMFA_verified,twoFAKey,QRCode,manualCode,u.DepartmentId,d.Name as DepartmentName
                              FROM Users as u
                              INNER JOIN Departments as d
                              ON u.DepartmentId = d.Id 
                              WHERE Email = @email";

                var data = await _dbContext.QuerySingleRecord<User>(query, parameter);

                return data;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the user repo while trying to get user by email");
                throw;
            }
        }
        #endregion

        #region [ Update User ]
        public async Task<bool> UpdateUser(Guid id, UserUpdate payload)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("id", id);

                var command = @"UPDATE Users SET ";

                if (payload.Firstname != null)
                {
                    parameters.Add("firstname", payload.Firstname);
                    command += @"FirstName = @firstname, ";
                }

                if (payload.Lastname != null)
                {
                    parameters.Add("lastname", payload.Lastname);
                    command += @"LastName = @lastname, ";
                }

                if (payload.Email != null)
                {
                    parameters.Add("email", payload.Email);
                    command += @"Email = @email, ";
                }


                if (command.EndsWith(", "))
                {
                    command = command.Substring(0, command.Length - 2);
                }

                command += " WHERE Id = @id";

                return await _dbContext.ExecuteCommand(command, parameters);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the user repo while trying to update user");
                throw;
            }
        }
        #endregion

        #region [ Update 2FA Fields ]
        public async Task<bool> UpdateMFAfields(UpdateMfaFieldsModel request)
        {
            try
            {

                var parameters = new DynamicParameters();
                parameters.Add("qrCode", request.QrCodeUrl); 
                parameters.Add("manualCode", request.ManualEntryCode);
                parameters.Add("MFAKey", request.MFAKey);
                parameters.Add("isFirstSignIn", request.isFirstSignIn);
                parameters.Add("isMFAVerified", request.isMFAVerified);
                parameters.Add("id", request.userId);

                var command = @"UPDATE Users SET isFirstSignIn = @isFirstSignIn,isMFA_verified = @isMFAVerified ,twoFAKey = @MFAKey,
                                QRCode = @qrCode,manualCode = @manualCode WHERE Id = @id";

                return await _dbContext.ExecuteCommand(command,parameters);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the user repo while trying to update user");
                throw;
            }
        }
        #endregion

        #region [ Update First Sign In ]
        public async Task<bool> updateFirstSignIn(string email)
        {
            try
            {

                var parameters = new DynamicParameters();
                parameters.Add("email", email);
                parameters.Add("isFirstSignIn", false);
                parameters.Add("isMFAVerified", true);

                var command = @"UPDATE Users SET isFirstSignIn = @isFirstSignIn,isMFA_verified = @isMFAVerified WHERE Email = @email";

                return await _dbContext.ExecuteCommand(command, parameters);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the repo while trying to update mfa fields in the database");
                throw;
            }
        }
        #endregion

        #region [ Log In ]
        public async Task<User?> Login(LoginModel payload)
        {
            try
            {
                var parameter = new DynamicParameters();
                parameter.Add("email", payload.Email);

                var query = "SELECT * FROM Users WHERE Email = @email";

                var user = await _dbContext.QuerySingleRecord<User>(query, parameter);


                if (user == null || user.IsActive == false)
                {
                    return null;
                }

                var isValid = BCrypt.Net.BCrypt.Verify(payload.Password, user.Password);

                if (!isValid)
                {
                    return null;
                }

                return user;


            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the repo while trying to log in user");
                throw;
            }
        }
        #endregion

        #region [ Update Password ]
        public async Task<bool> updatePassword(LoginModel payload)
        {
            try
            {

                var parameters = new DynamicParameters();
                parameters.Add("password", BCrypt.Net.BCrypt.HashPassword(payload.Password));
                parameters.Add("email", payload.Email);

                var command = @"UPDATE Users SET Password = @password WHERE Email = @email";

                return await _dbContext.ExecuteCommand(command, parameters);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the repo while trying to update password");
                throw;
            }
        }
        #endregion

        #region [ Updates that can be done by an admin only ]
        public async Task<bool> UpdatesByAdmin(Guid id, UpdatesByAdmin payload)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("id", id);

                var command = @"UPDATE Users SET ";

                if (payload.DepartmentId != null)
                {
                    parameters.Add("departmentId", payload.DepartmentId);
                    command += @"DepartmentId = @departmentId, ";
                }

                if (payload.isActive.HasValue)
                {
                    parameters.Add("isActive", payload.isActive);
                    command += @"isActive = @isActive, ";
                }

                if (payload.Role != null)
                {
                    parameters.Add("role", payload.Role.ToLower());
                    command += @"Role = @role, ";
                }


                if (command.EndsWith(", "))
                {
                    command = command.Substring(0, command.Length - 2);
                }

                command += " WHERE Id = @id";

                return await _dbContext.ExecuteCommand(command, parameters);
            }
            catch (Exception ex)
            {
                Log.Error("Error in the user repo in the 'UpdatesByAdmin' method");
                throw;
            }
        }
        #endregion


        #region [ Get emails ]
        public async Task<List<string>> GetAllUserEmails()
        {
            try
            {
                var query = "SELECT [Email] FROM [ComplaintSys_Db].[dbo].[Users]";

                var data = await _dbContext.Query<string>(query);

                return data;
            }
            catch (Exception)
            {

                throw;
            }
        } 
        #endregion


    }
}