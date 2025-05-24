using ComplaintSystem.Models;
using ComplaintSystem.Repositories;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ComplaintSystem.Service
{
    public class TokenService
    {
        #region [ Constructor ]
        private readonly IUser _userRepo;
        private readonly ILogger<TokenService> _logger;
        private readonly IConfiguration _config;

        public TokenService(IUser userRepo, ILogger<TokenService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _userRepo = userRepo;
        } 
        #endregion


        #region [ Generate Change Password Token ]
        public string? GenerateChangePasswordToken(User? user)
        {
            try
            {
                if (user == null)
                {
                    return null;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]!);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Sid, user.Id.ToString()),
                    new Claim("TokenType", "change-password-token")
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.Now.AddHours(1),
                    Issuer = _config["Jwt:Issuer"],
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the token service while trying to generate change password token");
                throw;
            }
        } 
        #endregion

        #region [ Generate Access Token ]

        public string? GenerateAccessToken(User user)
        {
            try
            {
                //Create JWT token handler and get secret key
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]!);

                //Prepare list of user claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Sid, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("TokenType", "access-token")
                };

                // Create a token Descriptor
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    IssuedAt = DateTime.UtcNow,
                    Issuer = _config["Jwt:Issuer"]!,
                    Expires = DateTime.Now.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return tokenString;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the token service while trying to generate an access token");
                throw;
            }
        }
        #endregion

        #region [ MFA Token ]
        public string? GenerateMfaToken(User? user)
        {
            try
            {
                if (user == null)
                {
                    return null;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]!);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Sid, user.Id.ToString()),
                    new Claim("TokenType", "mfa-token")
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = user.isFirstSignIn == true ? DateTime.Now.AddMinutes(20) : DateTime.Now.AddMinutes(10),
                    Issuer = _config["Jwt:Issuer"]!,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the token service while trying to generate change password token");
                throw;
            }
        }
        #endregion

        #region [ Refresh Token ]
        public string GenerateRefreshToken()
        {
            try
            {
                return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while trying to generate a refresh token");
                throw;
            }
        } 
        #endregion

    }


}
