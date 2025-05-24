using ComplaintSystem.Models;
using ComplaintSystem.Repositories;
using ComplaintSystem.Service;

namespace ComplaintSystem.Helper
{
    public class TokenHelper
    {
        #region [ Constructor ]
        private readonly TokenService _tokenService;
        private readonly IRefreshToken _refreshTokenRepo;
        private readonly ILogger<TokenHelper> _logger;

        public TokenHelper(TokenService tokenService, IRefreshToken refreshTokenRepo, ILogger<TokenHelper> logger)
        {
            _refreshTokenRepo = refreshTokenRepo;
            _tokenService = tokenService;
            _logger = logger;
        }
        #endregion

        #region [ Generate Tokens ]
        public async Task<(string, string)> GenerateTokens(User user)
        {
            try
            {
                var token = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
                {
                    return (null, null);
                }

                var existingRefreshToken = await _refreshTokenRepo.GetRefreshTokenByUserId(user.Id);

                if (existingRefreshToken != null)
                {
                    await _refreshTokenRepo.DeleteRefreshToken(existingRefreshToken.Id);
                }

                var refreshTokenModel = new RefreshToken()
                {
                    Token = refreshToken,
                    ExpiresOn = DateTime.Now.AddDays(7),
                    UserId = user.Id
                };

                var isAdded = await _refreshTokenRepo.AddRefreshToken(refreshTokenModel);

                if (isAdded == null)
                {
                    return (null, null);
                }

                return (token, refreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Token Helper while trying to generate tokens");
                return (null, null);
            }
        } 
        #endregion
    }
}
