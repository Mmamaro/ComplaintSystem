using ComplaintSystem.Models;
using ComplaintSystem.Models.DTOs;
using ComplaintSystem.Repositories;
using ComplaintSystem.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;

namespace ComplaintSystem.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUser _userRepo;
        private readonly MFAService _mfaService;
        private readonly IDepartment _departmentRepo;
        private readonly ILogger<UserController> _logger;

        public UserController(IUser userRepo, MFAService mfaService, IDepartment departmentRepo, ILogger<UserController> logger)
        {
            _userRepo = userRepo;
            _mfaService = mfaService;
            _departmentRepo = departmentRepo;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult> GetCurrentUser()
        {
            try
            {
                var tokenType = User.Claims.FirstOrDefault(c => c.Type == "TokenType")?.Value;
                var tokenUserId = Guid.Parse(User?.FindFirstValue(ClaimTypes.Sid)!);

                if (tokenType != "access-token")
                {
                    return Unauthorized(new { Message = "Bad token" });
                }

                var user = await _userRepo.GetUserById(tokenUserId);

                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                var userDTO = new UserDTO()
                {
                    DepartmentName = user.DepartmentName,
                    Email = user.Email,
                    Firstname = user.Firstname,
                    Id = user.Id,
                    IsActive = user.IsActive,
                    Lastname = user.Lastname,
                    Role = user.Role
                };

                return Ok(userDTO);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the User Controller while trying to get user by id");
                return StatusCode(500, new { Message = "Encountered an error" });
            }
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterModel payload)
        {
            try
            {
                var userExists = await _userRepo.GetUserByEmail(payload.Email);
                var deptExists = await _departmentRepo.GetDepartmentById(payload.DepartmentId);

                if (userExists != null)
                {
                    return Conflict(new {Message = "Email already exists" });
                }

                if (deptExists == null)
                {
                    return NotFound(new { Message = "Department does not exist" });
                }

                User user = new User()
                {
                    Email = payload.Email,
                    DepartmentId = payload.DepartmentId,
                    Firstname = payload.Firstname,
                    Lastname = payload.Lastname,
                    IsActive = true,
                    Role = "user",
                    Password = BCrypt.Net.BCrypt.HashPassword(payload.Password),
                    isFirstSignIn = true,
                    isMFAVerified = false
                };

                var isAdded = await _userRepo.AddUser(user);

                if (!isAdded)
                {
                    return BadRequest(new { Message = "Could not add user" });
                }

                return Ok(new {Message = "User registered successfully"});

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the User Controller whilee trying to register user");
                return StatusCode(500, new {Message = "Encountered an error"});
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> GetAllUsers()
        {
            try
            {
                var tokenType = User.Claims.FirstOrDefault(c => c.Type == "TokenType")?.Value;

                if (tokenType != "access-token")
                {
                    return Unauthorized(new { Message = "Bad token" });
                }

                var users = await _userRepo.GetAllUsers();

                var usersDTO = users.Select(x => new UserDTO
                {
                    DepartmentName = x.DepartmentName,
                    Email = x.Email,
                    Firstname = x.Firstname,
                    Id = x.Id,
                    IsActive = x.IsActive,
                    Lastname = x.Lastname,
                    Role = x.Role
                });

                return Ok(usersDTO);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the User Controller while trying to get all users");
                return StatusCode(500, new { Message = "Encountered an error" });
            }
        }

        [Authorize]
        [HttpGet("emails")]
        public async Task<ActionResult> GetEmails()
        {
            try
            {
                var tokenType = User.Claims.FirstOrDefault(c => c.Type == "TokenType")?.Value;

                if (tokenType != "access-token")
                {
                    return Unauthorized(new { Message = "Bad token" });
                }

                var users = await _userRepo.GetAllUserEmails();

                return Ok(users);

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the User Controller while trying to get all user emails");
                return StatusCode(500, new { Message = "Encountered an error" });
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetUserById(Guid id)
        {
            try
            {
                var tokenType = User.Claims.FirstOrDefault(c => c.Type == "TokenType")?.Value;

                if (tokenType != "access-token")
                {
                    return Unauthorized(new { Message = "Bad token" });
                }

                var user = await _userRepo.GetUserById(id);

                if (user == null)
                {
                    return NotFound(new {Message = "User not found"});
                }

                var userDTO = new UserDTO()
                { 
                    DepartmentName = user.DepartmentName,
                    Email = user.Email,
                    Firstname = user.Firstname,
                    Id = user.Id,
                    IsActive = user.IsActive,
                    Lastname = user.Lastname,
                    Role = user.Role
                };

                return Ok(userDTO); 

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the User Controller while trying to get user by id");
                return StatusCode(500, new { Message = "Encountered an error" });
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUser(Guid id, UserUpdate payload)
        {
            try
            {

                _logger.LogInformation("Hello World");

                var tokenType = User.Claims.FirstOrDefault(c => c.Type == "TokenType")?.Value;
                var tokenUserId = Guid.Parse(User?.FindFirstValue(ClaimTypes.Sid)?.ToString()!);
                var tokenUserEmail = User?.FindFirstValue(ClaimTypes.Email)?.ToString();

                if (tokenType != "access-token")
                {
                    return Unauthorized(new {Message = "Bad token"});
                }

                var user = await _userRepo.GetUserById(id);

                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                if (tokenUserId != user.Id) 
                {
                    return Unauthorized(new { Message = "Unauthorized user" });
                }

                var isUpdated = await _userRepo.UpdateUser(id, payload);

                if (!isUpdated)
                {
                    return BadRequest(new {Mwssage = "Could not update user"});
                }

                if (payload.Email != null)
                {
                    var mfa = await _mfaService.MFASetup(payload.Email);

                    var updateMFA = new UpdateMfaFieldsModel()
                    {
                        isFirstSignIn = true,
                        isMFAVerified = false,
                        ManualEntryCode = mfa.ManualEntryCode,
                        MFAKey = mfa.MFAKey,
                        QrCodeUrl = mfa.QrCodeUrl,
                        userId = user.Id
                    };

                    var isMfaUpdated = await _userRepo.UpdateMFAfields(updateMFA);

                    if (!isMfaUpdated)
                    {
                        return BadRequest(new {Message = "Could not update mfa fields"});
                    }
                }

                return Ok(new {Message = "User updated successfully"});

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in the User Controller ");
                return StatusCode(500, new { Message = "Encountered an error" });
            }
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            try
            {
                var user = await _userRepo.GetUserById(id);

                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                var isDeleted = await _userRepo.DeleteUser(id);

                if (!isDeleted)
                {
                    return BadRequest(new { Message = "Could not delete user" });
                }

                return Ok(new {Message = "User deleted successfully"});
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the User Controller while trying to delete user");
                return StatusCode(500, new { Message = "Encountered an error" });
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPatch("update-by-admin/{id}")]
        public async Task<ActionResult> UpdateByAdmin(Guid id, UpdatesByAdmin payload)
        {
            try
            {
                var validRoles = new List<string>() { "manager", "user", "admin" };

                if (!validRoles.Contains(payload.Role.ToLower()))
                {
                    return BadRequest(new {Message = "That role does not exist"});
                }

                var user = await _userRepo.GetUserById(id);

                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                var isUpdated = await _userRepo.UpdatesByAdmin(id, payload);

                if (!isUpdated)
                {
                    return BadRequest(new { Message = "Could not update user" });
                }

                return Ok(new { Message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the User Controller ");
                return StatusCode(500, new { Message = "Encountered an error" });
            }
        }

    }
}
