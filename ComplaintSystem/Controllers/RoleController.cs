using ComplaintSystem.Models;
using ComplaintSystem.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ComplaintSystem.Controllers
{
    [Route("api/roles")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRole _roleRepo;

        public RoleController(IRole roleRepo)
        {
            _roleRepo = roleRepo;
        }


        [HttpPost]
        public async Task<ActionResult> AddRole(AddRole payload)
        {
            try
            {
                var role = await _roleRepo.GetRoleByName(payload.Name);

                if (role != null)
                {
                    return BadRequest(new {Message = "This role already exists"});
                }

                var isAdded = await _roleRepo.AddRole(payload);

                if (!isAdded)
                {
                    return BadRequest(new { Message = "Could not add departement" });
                }

                return Created();

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Role Controller while trying to add a role");
                return StatusCode(500, new {Message = "Encountered an error"});
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetAllRoles()
        {
            try
            {
                var roles = await _roleRepo.GetAllRoles();

                return Ok(roles);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Departmeent Controller while trying to get all roles");
                return StatusCode(500, new { Message = "Encountered an error" }); ;
            }
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult> GetAllRoles(Guid id)
        {
            try
            {
                var role = await _roleRepo.GetRoleById(id);

                if (role == null)
                {
                    return NotFound(new {Message = "Role not found"});
                }

                return Ok(role);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Role Controller while trying to get role by id");
                return StatusCode(500, new { Message = "Encountered an error" }); ;
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateRole(Guid id, AddRole payload)
        {
            try
            {
                var roleExists = await _roleRepo.GetRoleById(id);

                if (roleExists == null)
                {
                    return NotFound(new { Message = "Role not found" });
                }

                var role = await _roleRepo.GetRoleByName(payload.Name);

                if (role != null)
                {
                    return BadRequest(new { Message = "This role name already exists" });
                }

                var isUpdated = await _roleRepo.UpdateRole(id, payload);

                if (!isUpdated)
                {
                    return BadRequest(new { Message = "Could not update departemnt" });
                }

                return Ok();

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Role Controller while trying to updaate a role");
                return StatusCode(500, new { Message = "Encountered an error" }); ;
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRole(Guid id)
        {
            try
            {
                var roleExists = await _roleRepo.GetRoleById(id);

                if (roleExists == null)
                {
                    return NotFound(new { Message = "Role not found" });
                }

                var isDeleted = await _roleRepo.DeleteRole(id);

                if (!isDeleted)
                {
                    return BadRequest(new { Message = "Could not update departemnt" });
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Role Controller while trying to delete a role");
                return StatusCode(500, new { Message = "Encountered an error" }); ;
            }
        }

    }
}
