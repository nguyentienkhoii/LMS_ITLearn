using System.ComponentModel.DataAnnotations;

namespace BaiGiang.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
      
    }
}
