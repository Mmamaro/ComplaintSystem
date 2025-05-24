using System.ComponentModel.DataAnnotations;

namespace ComplaintSystem.Models.DTOs
{
    public class UserDTO
    {
        public Guid Id { get; set; }
        public required string Firstname { get; set; }
        public required string Lastname { get; set; }
        [EmailAddress] public required string Email { get; set; }
        public required string Role { get; set; }
        public string DepartmentName { get; set; }
        public required bool IsActive { get; set; }

    }
}
