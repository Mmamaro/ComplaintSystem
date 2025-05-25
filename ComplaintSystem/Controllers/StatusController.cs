using ComplaintSystem.Models;
using ComplaintSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ComplaintSystem.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("api/statuses")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly IStatus _statusRepo;

        public StatusController(IStatus statusRepo)
        {
            _statusRepo = statusRepo;
        }


        [HttpPost]
        public async Task<ActionResult> AddStatus(AddStatus payload)
        {
            try
            {
                var status = await _statusRepo.GetStatusByName(payload.Name);

                if (status != null)
                {
                    return BadRequest(new {Message = "This status already exists"});
                }

                var isAdded = await _statusRepo.AddStatus(payload);

                if (!isAdded)
                {
                    return BadRequest(new { Message = "Could not add status" });
                }

                return Created();

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Status Controller while trying to add a status");
                return StatusCode(500, new {Message = "Encountered an error"});
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetAllStatuses()
        {
            try
            {
                var statuses = await _statusRepo.GetAllStatuses();

                return Ok(statuses);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Departmeent Controller while trying to get all statuses");
                return StatusCode(500, new { Message = "Encountered an error" }); ;
            }
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult> GetAllStatuses(Guid id)
        {
            try
            {
                var status = await _statusRepo.GetStatusById(id);

                if (status == null)
                {
                    return NotFound(new {Message = "Status not found"});
                }

                return Ok(status);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Status Controller while trying to get status by id");
                return StatusCode(500, new { Message = "Encountered an error" }); ;
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateStatus(Guid id, AddStatus payload)
        {
            try
            {
                var statusExists = await _statusRepo.GetStatusById(id);

                if (statusExists == null)
                {
                    return NotFound(new { Message = "Status not found" });
                }

                var status = await _statusRepo.GetStatusByName(payload.Name);

                if (status != null)
                {
                    return BadRequest(new { Message = "This status name already exists" });
                }

                var isUpdated = await _statusRepo.UpdateStatus(id, payload);

                if (!isUpdated)
                {
                    return BadRequest(new { Message = "Could not update status" });
                }

                return Ok();

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Status Controller while trying to updaate a status");
                return StatusCode(500, new { Message = "Encountered an error" }); ;
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteStatus(Guid id)
        {
            try
            {
                var statusExists = await _statusRepo.GetStatusById(id);

                if (statusExists == null)
                {
                    return NotFound(new { Message = "Status not found" });
                }

                var isDeleted = await _statusRepo.DeleteStatus(id);

                if (!isDeleted)
                {
                    return BadRequest(new { Message = "Could not update status" });
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Status Controller while trying to delete a status");
                return StatusCode(500, new { Message = "Encountered an error" }); ;
            }
        }

    }
}
