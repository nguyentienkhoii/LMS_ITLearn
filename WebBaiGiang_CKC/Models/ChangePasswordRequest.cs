using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang_CKC.Models
{
    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [MinLength(6)]
        [MaxLength(100)]
        public string NewPassword { get; set; }

        [Required]
        [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; }
    }
}
