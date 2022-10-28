using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    public record UserForAuthenticationDTO
    {
        [Required(ErrorMessage = "User name is required")]
        public string? UserName { get; init; }
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; init; }
    }
}
