using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Models
{
    public class ChangePassword
    {
        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Confirmation password does not match.")]
        public string ConfirmPassword { get; set; }
    }
}
