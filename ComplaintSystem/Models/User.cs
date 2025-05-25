using System.ComponentModel.DataAnnotations;

namespace ComplaintSystem.Models
{

    public class User
    {
        public Guid Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        [EmailAddress] public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public bool isFirstSignIn { get; set; }
        public bool isMFAVerified { get; set; }
        public string? twoFAKey { get; set; }
        public string? QRCode { get; set; }
        public string? ManualCode { get; set; }
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; }

    }

    public record RegisterModel
    {
        [Required(ErrorMessage = "First Name is required")]
        public string Firstname { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string Lastname { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        [Required(ErrorMessage = "DepartmentId is required")]
        public Guid DepartmentId { get; set; }

    }

    public record UserUpdate
    {
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Email { get; set; }

    }

    public record UpdatesByAdmin
    {

        public string? Role { get; set; }
        public bool? isActive { get; set; }
        public Guid? DepartmentId { get; set; }

    }

    public record LoginModel
    {
        [EmailAddress] public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public record ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; init; }
    }
}
