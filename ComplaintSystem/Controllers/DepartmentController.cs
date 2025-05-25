using ComplaintSystem.Models;
using ComplaintSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ComplaintSystem.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("api/departments")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartment _departmentRepo;

        public DepartmentController(IDepartment departmentRepo)
        {
            _departmentRepo = departmentRepo;
        }


        [HttpPost]
        public async Task<ActionResult> AddDepartment(AddDepartment payload)
        {
            try
            {

                var department = await _departmentRepo.GetDepartmentByName(payload.Name);

                if (department != null)
                {
                    return BadRequest(new {Message = "This department already exists"});
                }

                var isAdded = await _departmentRepo.AddDepartment(payload);

                if (!isAdded)
                {
                    return BadRequest(new { Message = "Could not add departement" });
                }

                return Created();

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Department Controller while trying to add a department");
                return StatusCode(500, new {Message = "Encountered an error"});
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetAllDepartments()
        {
            try
            {
                var departments = await _departmentRepo.GetAllDepartments();

                return Ok(departments);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Departmeent Controller while trying to get all departments");
                return StatusCode(500, new { Message = "Encountered an error" }); ;
            }
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult> GetAllDepartments(Guid id)
        {
            try
            {
                var department = await _departmentRepo.GetDepartmentById(id);

                if (department == null)
                {
                    return NotFound(new {Message = "Department not found"});
                }

                return Ok(department);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Department Controller while trying to get department by id");
                return StatusCode(500, new { Message = "Encountered an error" }); ;
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateDepartment(Guid id, AddDepartment payload)
        {
            try
            {
                var departmentExists = await _departmentRepo.GetDepartmentById(id);

                if (departmentExists == null)
                {
                    return NotFound(new { Message = "Department not found" });
                }

                var department = await _departmentRepo.GetDepartmentByName(payload.Name);

                if (department != null)
                {
                    return BadRequest(new { Message = "This department name already exists" });
                }

                var isUpdated = await _departmentRepo.UpdateDepartment(id, payload);

                if (!isUpdated)
                {
                    return BadRequest(new { Message = "Could not update departemnt" });
                }

                return Ok();

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Department Controller while trying to updaate a department");
                return StatusCode(500, new { Message = "Encountered an error" }); ;
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDepartment(Guid id)
        {
            try
            {
                var departmentExists = await _departmentRepo.GetDepartmentById(id);

                if (departmentExists == null)
                {
                    return NotFound(new { Message = "Department not found" });
                }

                var isDeleted = await _departmentRepo.DeleteDepartment(id);

                if (!isDeleted)
                {
                    return BadRequest(new { Message = "Could not update departemnt" });
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Department Controller while trying to delete a department");
                return StatusCode(500, new { Message = "Encountered an error" }); ;
            }
        }

    }
}
