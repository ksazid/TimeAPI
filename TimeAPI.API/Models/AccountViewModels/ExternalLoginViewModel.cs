using System.ComponentModel.DataAnnotations;

namespace TimeAPI.API.Models.AccountViewModels
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
