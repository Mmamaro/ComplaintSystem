using System.ComponentModel.DataAnnotations;

namespace ComplaintSystem.Models
{
    public class MFAModel
    {
        public string QrCodeUrl { get; set; }
        public string ManualEntryCode { get; set; }
        public string MFAKey { get; set; }
    }

    public class TwoFALoginModel
    {
        [EmailAddress] public string Email { get; set; }
        public string Code { get; set; }
    }

    public class UpdateMfaFieldsModel
    {
        public Guid userId { get; set; }
        public bool? isFirstSignIn { get; set; }
        public bool? isMFAVerified { get; set; }
        public string QrCodeUrl { get; set; }
        public string ManualEntryCode { get; set; }
        public string MFAKey { get; set; }
    }
}



