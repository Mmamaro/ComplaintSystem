using ComplaintSystem.Helper;
using ComplaintSystem.Helpers;
using ComplaintSystem.Models;
using ComplaintSystem.Models.DTOs;
using ComplaintSystem.Repositories;
using ComplaintSystem.Service;
using Google.Authenticator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;
using static QRCoder.PayloadGenerator;

namespace ComplaintSystem.Controllers
{
    #region [ Constructor ]
    [Authorize]
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IUser _userRepo;
        private readonly EmailService _emailService;
        private readonly MFAService _mfaService;
        private readonly TokenHelper _tokensHelper;
        private readonly TokenService _tokenService;
        private readonly IConfiguration _config;
        private readonly IRefreshToken _refreshTokenRepo;
        private readonly PasswordHelper _passwordHelper;

        public AuthenticationController(ILogger<AuthenticationController> logger, IUser userRepo, EmailService emailService,
            MFAService mfaService, TokenHelper tokensHelper, IConfiguration config,
            IRefreshToken refreshTokenRepo, PasswordHelper passwordHelper, TokenService tokenService)
        {
            _emailService = emailService;
            _logger = logger;
            _userRepo = userRepo;
            _mfaService = mfaService;
            _config = config;
            _refreshTokenRepo = refreshTokenRepo;
            _passwordHelper = passwordHelper;
            _tokensHelper = tokensHelper;
            _tokenService = tokenService;
        }
        #endregion


        #region [ Normal Login ]
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult> NormalLogin(LoginModel request)
        {
            try
            {

                var user = await _userRepo.Login(request);

                if (user == null)
                {
                    return Unauthorized(new { Message = "Bad Credentials" });
                }

                var token = _tokenService.GenerateMfaToken(user);

                var creds = new normalLoginCreds
                {
                    Token = token,
                    isFirstSignIn = user.isFirstSignIn,
                    isMFA_verified = user.isMFAVerified,
                };

                return Ok(creds);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the controller while trying to log in");
                return StatusCode(500, new { Message = "Encountered an error" });
            }
        }

        #endregion

        #region [ 2FA Setup ]
        [HttpGet("mfa-setup")]
        public async Task<ActionResult> TwoFASetup(string email)
        {
            try
            {
                var tokenType = User.Claims.FirstOrDefault(c => c.Type == "TokenType")?.Value;
                var tokenUserEmail = User?.FindFirstValue(ClaimTypes.Email)?.ToString();

                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new { Message = "Provide Email" });
                }

                if (email.ToLower() != tokenUserEmail || tokenType != "mfa-token")
                {
                    return Unauthorized(new { Message = "Unauthorized User/Invalid token" });
                }

                var loginResponse = await _userRepo.GetUserByEmail(email);

                if (loginResponse == null)
                {
                    _logger.LogError($"User {email} does not exist or invalid credentials provided");
                    return Unauthorized(new { Message = "Bad Credentials" });
                }

                // Setup MFA for the user
                var twoFAConfig = await _mfaService.MFASetup(email);

                UpdateMfaFieldsModel mfaFieldsModel = new UpdateMfaFieldsModel()
                {
                    isFirstSignIn = true,
                    isMFAVerified = false,
                    ManualEntryCode = twoFAConfig.ManualEntryCode,
                    userId = loginResponse.Id,
                    MFAKey = twoFAConfig.MFAKey,
                    QrCodeUrl = twoFAConfig.QrCodeUrl,
                };

                var mfaFieldUpdate = await _userRepo.UpdateMFAfields(mfaFieldsModel);

                if (mfaFieldUpdate == null)
                {
                    _logger.LogError("Could not update mfa fields");
                    return BadRequest(new { Message = "Could set up MFA" });
                }

                var user = await _userRepo.GetUserByEmail(loginResponse.Email);

                var response = new
                {
                    ManualCode = user.ManualCode,
                    QrCode = user.QRCode
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                Log.Error($"Error in the controller while trying to set up mfa: {ex.Message}");
                return StatusCode(500, new { Message = "Encountered an error" });
            }
        }
        #endregion

        #region [ 2FA Login ]
        [HttpPost("mfa-login")]
        public async Task<ActionResult> TwoFALogin(TwoFALoginModel request)
        {
            try
            {
                var tokenType = User.Claims.FirstOrDefault(c => c.Type == "TokenType")?.Value;
                var tokenUserEmail = User?.FindFirstValue(ClaimTypes.Email)?.ToString();

                if (request.Email != tokenUserEmail || tokenType != "mfa-token")
                {
                    return Unauthorized(new { Message = "Unauthorized User/Invalid token" });
                }

                var user = await _userRepo.GetUserByEmail(request.Email);

                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                byte[] encryptedBytes = Convert.FromBase64String(user.twoFAKey);
                string decryptedSecret = await _mfaService.DecryptString(encryptedBytes);

                TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                var isValid = tfa.ValidateTwoFactorPIN(decryptedSecret, request.Code);

                if (!isValid)
                {
                    return Unauthorized(new { Message = "Bad Credentials" });
                }

                if (user.isFirstSignIn == true && user.isMFAVerified == false)
                {
                    var isUpdated = await _userRepo.updateFirstSignIn(user.Email);

                    if (isUpdated == null)
                    {
                        return BadRequest(new { Messaage = "Could not update mfa fields" });
                    }
                }

                var (token, refreshToken) = await _tokensHelper.GenerateTokens(user);

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
                {
                    return BadRequest(new { Messaage = "Could not generate tokens" });
                }

                return Ok(new { AccessToken = token, RefreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the controller while trying to log in with MFA");
                return StatusCode(500, new { Message = "Encountered an error" });
            }
        }
        #endregion

        #region [ Refresh Token ]
        [AllowAnonymous]
        [HttpPost("refreshtoken")]
        public async Task<ActionResult> RefreshToken(RefreshTokenRequest payload)
        {
            try
            {
                var refreshToken = await _refreshTokenRepo.GetRefreshTokenByRefreshToken(payload.RefreshToken);

                var user = await _userRepo.GetUserByEmail(payload.Email);

                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                if (refreshToken == null || refreshToken.ExpiresOn < DateTime.Now || refreshToken.UserId != user.Id)
                {
                    return Unauthorized(new { Message = "Invalid refresh token, please log in" });
                }

                var accessToken = _tokenService.GenerateAccessToken(user);


                if (string.IsNullOrEmpty(accessToken))
                {
                    return BadRequest("Could not generate access token");
                }

                return Ok(new { AccessToken = accessToken });


            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while trying to get a new access token");
                return StatusCode(500, new { Message = "Encountered an error" });
            }
        }
        #endregion

        #region [ Forgot Password ]

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPassword payload)
        {
            try
            {
                var email = payload.Email;

                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new { Message = "Enter email" });
                }

                var user = await _userRepo.GetUserByEmail(email);

                if (user == null)
                {
                    return NotFound();
                }

                var token = _tokenService.GenerateChangePasswordToken(user);

                string baseUrl = _config.GetSection("FrontEndBaseUrl").Value!;
                var url = $"{baseUrl}/auth/change-password?token={token}&email={user.Email}";
                var recipients = user.Email;
                var subject = "Forgot Password For Connect Genesys Billing Platform";
                var path = $"./Templates/ForgotPasswordTemplate.html";
                var template = System.IO.File.ReadAllText(path).Replace("\n", "");

                template = template.Replace("{{Name}}", user.Firstname)
                           .Replace("{{ResetLink}}", url);

                _emailService.SendTemplateEmail(recipients, subject, template);

                return Accepted(new { Message = "Email sent" });

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the forgot password endpoint");
                return StatusCode(500, new { Message = "Encountered an error" });
            }
        }
        #endregion

        #region [ Change Password ]
        [HttpPost("change-password")]
        public async Task<ActionResult> ChangePassword(LoginModel request)
        {
            try
            {
                var tokenType = User.Claims.FirstOrDefault(c => c.Type == "TokenType")?.Value;
                var tokenUserId = Guid.Parse(User?.FindFirstValue(ClaimTypes.Sid)!);
                var user = await _userRepo.GetUserByEmail(request.Email);

                if (!_passwordHelper.IsPasswordValid(request.Password))
                {
                    return BadRequest(new
                    {
                        Message = "Your password must be 8 characters long or more, " +
                        "contain at least 1 number and special character"
                    });
                }

                if (user == null)
                {
                    return NotFound(new { Message = "User does not exist" });
                }

                if (tokenType != "change-password-token")
                {
                    return Unauthorized(new { Message = "Bad Token" });
                }

                if (tokenUserId != user.Id)
                {
                    return Unauthorized(new { Message = "Unauthorized User" });
                }

                var isUpdated = await _userRepo.updatePassword(request);

                if (isUpdated == null)
                {
                    return BadRequest(new { Message = "Could not change password" });
                }

                var token = _tokenService.GenerateChangePasswordToken(user);

                string baseUrl = _config.GetSection("FrontEndBaseUrl").Value!;
                var url = $"{baseUrl}/auth/change-password?token={token}&email={user.Email}";
                var recipients = user.Email;
                var subject = "Change Password For Connect Genesys Billing Platform";
                var path = $"./Templates/ChangedPasswordTemplate.html";
                var template = System.IO.File.ReadAllText(path).Replace("\n", "");

                template = template.Replace("{{Name}}", user.Firstname)
                           .Replace("{{ResetLink}}", url);

                _emailService.SendTemplateEmail(recipients, subject, template);

                return Accepted(new { Messsage = "Password has been changed" });

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the change password end point");
                return StatusCode(500, new { Message = "Encountered an error" });
            }
        }
        #endregion
    }
}

//TEST AUTHENTICATION AND USER CRUD