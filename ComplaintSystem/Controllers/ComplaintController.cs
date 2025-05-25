using ComplaintSystem.Models;
using ComplaintSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ComplaintSystem.Controllers
{
    [Authorize]
    [Route("api/complaints")]
    [ApiController]
    public class ComplaintController : ControllerBase
    {
        private readonly IComplaint _complaints;
        private readonly ILogger<ComplaintController> _logger;
        private readonly IUser _userRepo;
        private readonly IStatus _statusRepo;

        public ComplaintController(IComplaint complaints, ILogger<ComplaintController> logger, IUser userRepo, IStatus statusRepo)
        {
            _complaints = complaints;
            _logger = logger;
            _userRepo = userRepo;
            _statusRepo = statusRepo;
        }

        [Authorize(Roles = "admin,manager")]
        [HttpGet]
        public async Task<ActionResult> GetAllComplaints()
        {
            try
            {
                var complaints = await _complaints.GetAllComplaints();

                return Ok(complaints);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in the ComplaintController in the GetAllComplaints method");
                return StatusCode(500, new {Message = "Encountered an error"});
            }
        }

        [Authorize(Roles = "admin,manager")]
        [HttpPost("filters")]
        public async Task<ActionResult> GetComplaintsByFilters(ComplaintFilters payload)
        {
            try
            {
                var data = await _complaints.GetComplaintsByFilters(payload);

                if (data == null)
                {
                    return NotFound(new {Message = "No data"});
                }

                return Ok(data);

            }
            catch (Exception ex)
            {
                _logger.LogError("Error in the ComplaintController in the GetComplaintsByFilters method");
                return StatusCode(500, new { Message = "Encountered an error" });
            }
        }

        [Authorize(Roles = "admin,manager")]
        [HttpGet("departmentid")]
        public async Task<ActionResult> GetComplaintsByManagerDeptId()
        {
            try
            {
                var tokenUserId = Guid.Parse(User?.FindFirstValue(ClaimTypes.Sid)!);

                var user = await _userRepo.GetUserById(tokenUserId);
                var data = await _complaints.GetComplaintsByManagerDeptId(user.DepartmentId);

                if (data == null)
                {
                    return NotFound(new { Message = "No data" });
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in the ComplaintController in the GetComplaintsByManagerDeptId method");
                return StatusCode(500, new { Message = "Encountered an error" });
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddComplaint(AddComplaint payload)
        {
            try
            {
                var tokenUserId = Guid.Parse(User?.FindFirstValue(ClaimTypes.Sid)?.ToString()!);
                var tokenUserEmail = User?.FindFirstValue(ClaimTypes.Email)?.ToString();

                var accused = await _userRepo.GetUserByEmail(payload.Accused);
                var reporter = await _userRepo.GetUserByEmail(tokenUserEmail);

                if (reporter == null)
                {
                    return BadRequest(new { Message = "Reporter does not exist" });
                }

                if (accused == null)
                {
                    return BadRequest(new { Message = "Accused does not exist" });
                }

                if (reporter.Email == accused.Email)
                {
                    return BadRequest(new { Message = "You cannot complain about yourself" });
                }

                var complaint = new Complaint()
                {
                     AccusedId = accused.Id,
                     ComplaintDescription = payload.ComplaintDescription,
                     StatusId = payload.StatusId,
                     CreatedOn = DateTime.Now,
                     UpdatedOn = DateTime.Now,
                     ReporterId = tokenUserId,
 
                };

                var isAdded = await _complaints.AddComplaint(complaint);

                if (!isAdded)
                {
                    return BadRequest(new { Message = "Could not add complaint" });
                }

                return Ok(new {Message = "Complaint added successfully"});

            }
            catch (Exception ex)
            {
                _logger.LogError("Error in the ComplaintController in the AddComplaint method");
                return StatusCode(500, new { Message = "Encountered an error" });
            }
        }

        [Authorize]
        [HttpPatch("update-by-user/{id}")]
        public async Task<ActionResult> UpdateComplaint(Guid id, UserUpdateComplaint payload)
        {
            try
            {
                var complaint = await _complaints.GetComplaintsById(id);
                var tokenUserEmail = User?.FindFirstValue(ClaimTypes.Email)?.ToString();

                if (complaint == null)
                {
                    return NotFound(new {Message = "complaint does not exist"});
                }

                var accused = await _userRepo.GetUserByEmail(payload.Accused);

                if (accused == null)
                {
                    return NotFound(new { Message = "Accused does not exist" });
                }

                if (tokenUserEmail != complaint.Reporter)
                {
                    return NotFound(new { Message = "UnAuthorized to update this complaint" });
                }

                var isUpdated = await _complaints.UpdateComplaint(id, accused.Id, payload.ComplaintDescription);

                if (!isUpdated)
                {
                    return BadRequest(new { Message = "Could not update complaint" });
                }

                return Ok(new { Message = "Complaint updated successfully" });

            }
            catch (Exception ex)
            {
                _logger.LogError("Error in the ComplaintController in the UpdateComplaint method");
                return StatusCode(500, new { Message = "Encountered an error" });
            }
        }

        [Authorize(Roles = "admin,manager")]
        [HttpPatch("update-by-manager/{id}")]
        public async Task<ActionResult> ManagerUpdateComplaint(Guid id, ManagerUpdateComplaint payload)
        {
            try
            {
                var complaint = await _complaints.GetComplaintsById(id);

                if (complaint == null)
                {
                    return NotFound(new { Message = "complaint does not exist" });
                }

                var status = await _statusRepo.GetStatusById(payload.StatusId);

                if (status == null)
                {
                    return NotFound(new { Message = "status does not exist" });
                }

                var isUpdated = await _complaints.ManagerUpdateComplaint(id, payload);

                if (!isUpdated)
                {
                    return BadRequest(new { Message = "Could not update complaint" });
                }

                return Ok(new { Message = "Complaint updated successfully" });

            }
            catch (Exception ex)
            {
                _logger.LogError("Error in the ComplaintController in the ManagerUpdateComplaint method");
                return StatusCode(500, new { Message = "Encountered an error" });
            }
        }
    }
}
