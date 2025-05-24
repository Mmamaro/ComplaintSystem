using System.ComponentModel.DataAnnotations;

namespace ComplaintSystem.Models
{
    public class Complaint
    {
        public Guid? Id { get; set; }
        public Guid ReporterId { get; set; }
        public Guid AccusedId { get; set; }
        public string ComplaintDescription { get; set; }
        public Guid StatusId { get; set; }
        public string  ResolutionNote { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }

    public class ComplaintResponse
    {
        public Guid? Id { get; set; }
        public string Reporter { get; set; }
        public string Accused { get; set; }
        public string ComplaintDescription { get; set; }
        public string Status { get; set; }
        public string ResolutionNote { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }

    public record AddComplaint
    {
        [EmailAddress]
        [Required(ErrorMessage = "Accused Email Id is Required")]
        public string Accused { get; set; }

        [Required(ErrorMessage = "StatusId is Required")]
        public Guid StatusId { get; set; }

        [Required(ErrorMessage = "Complaint is Required")]
        public string ComplaintDescription { get; set; }
    }

    public record UserUpdateComplaint
    {
        [EmailAddress]
        [Required(ErrorMessage = "Accused Email Id is Required")]
        public string Accused { get; set; }

        [Required(ErrorMessage = "Complaint Message is Required")]
        public string ComplaintDescription { get; set; }
    }

    public record ManagerUpdateComplaint
    {
        [Required(ErrorMessage = "Status Id is Required")]
        public Guid StatusId { get; set; }

        [Required(ErrorMessage = "Resolution Note Required")]
        public string ResolutionNote { get; set; }
    }

    public record ComplaintFilters
    {
        [Required(ErrorMessage = "StartDate Id is Required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "EndDate Id is Required")]
        public DateTime EndDate { get; set; }
        public string? Reporter { get; set; }
        public string? Accused { get; set; }
        public string? Status { get; set; }
    }
}
