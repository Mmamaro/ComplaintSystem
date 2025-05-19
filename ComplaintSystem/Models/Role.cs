using System.ComponentModel.DataAnnotations;

namespace ComplaintSystem.Models
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class AddRole
    {
        [Required(ErrorMessage = "Role Name is required")]
        public string Name { get; set; }
    }
}
