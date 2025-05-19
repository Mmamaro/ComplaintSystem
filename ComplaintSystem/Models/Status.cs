using System.ComponentModel.DataAnnotations;

namespace ComplaintSystem.Models
{
    public class Status
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class AddStatus
    {
        [Required(ErrorMessage = "Status Name is required")]
        public string Name { get; set; }
    }
}
