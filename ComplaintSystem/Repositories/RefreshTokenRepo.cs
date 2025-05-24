
using ComplaintSystem.Data;
using ComplaintSystem.Models;
using Dapper;
using Serilog;
using static QRCoder.PayloadGenerator;

namespace ComplaintSystem.Repositories
{
    #region [ Interface ]
    public interface IRefreshToken
    {
        Task<bool> AddRefreshToken(RefreshToken refreshToken);
        Task<RefreshToken> GetRefreshTokenByUserId(Guid id);
        Task<RefreshToken> GetRefreshTokenByRefreshToken(string token);
        Task<bool> DeleteRefreshToken(Guid? id);

    } 
    #endregion

    public class RefreshTokenRepo : IRefreshToken
    {
        #region [ Constructor ]
        private readonly IDapperContext _dbContext;
        public RefreshTokenRepo(IDapperContext dapperContext)
        {
            _dbContext = dapperContext;
        }
        #endregion


        #region [ Add Refresh Token ]

        public async Task<bool> AddRefreshToken(RefreshToken payload)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("Token", payload.Token);
                parameters.Add("UserId", payload.UserId);
                parameters.Add("ExpiresOn", payload.ExpiresOn);

                var command = @"INSERT INTO RefreshTokens(Token, UserId, ExpiresOn)
                                VALUES(@Token, @UserId, @ExpiresOn)";

                return await _dbContext.ExecuteCommand(command, parameters);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the refresh token  repo while trying to add a refresh token to the db");
                throw;
            }
        }
        #endregion

        #region [ Delete Refresh Token ]
        public async Task<bool> DeleteRefreshToken(Guid? id)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("id", id);

                var command = @"DELETE FROM RefreshTokens WHERE Id = @id";

                return await _dbContext.ExecuteCommand(command,parameters);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the refresh token repo while trying to delete a refresh token");
                throw;
            }
        }
        #endregion

        #region [ Get refresh Token By User Id ]
        public async Task<RefreshToken> GetRefreshTokenByUserId(Guid id)
        {
            try
            {
                var parameter = new DynamicParameters();
                parameter.Add("id", id);

                var query = "SELECT * FROM RefreshTokens WHERE UserId = @id";

                var data = await _dbContext.QuerySingleRecord<RefreshToken>(query, parameter);

                return data;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the refresh token repo while trying to get a refresh token by user id");
                throw;
            }
        }
        #endregion

        #region [ Get Refresh Token by Token]
        public async Task<RefreshToken> GetRefreshTokenByRefreshToken(string token)
        {
            try
            {
                var parameter = new DynamicParameters();
                parameter.Add("refreshToken", token);

                var query = "SELECT * FROM RefreshTokens WHERE Token = @refreshToken";

                var data = await _dbContext.QuerySingleRecord<RefreshToken>(query, parameter);

                return data;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the refresh token repo while trying to get a refresh token by refresh token");
                throw;
            }
        } 
        #endregion
    }
}
