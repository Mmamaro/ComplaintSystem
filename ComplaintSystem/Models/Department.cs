using System.ComponentModel.DataAnnotations;

namespace ComplaintSystem.Models
{
    public class Department
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class AddDepartment
    {
        [Required(ErrorMessage = "Department Name is required")]
        public string Name { get; set; }
    }
}
