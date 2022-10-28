using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Employee
{
    public abstract record EmployeeForManipulationDTO
    {
        [Required(ErrorMessage = "Employee name is a required field")]
        [MaxLength(30, ErrorMessage = "Max length for Name is 30 Characters.")]
        public string? Name { get; init; }
        [Range(18, int.MaxValue, ErrorMessage = "Age is a required and it cant be lower than 18")]
        public int? Age { get; init; }
        [Required(ErrorMessage = "Position is a required field.")]
        [MaxLength(20, ErrorMessage = "Max length for Position is 20 Characters.")]
        public string? Position { get; init; }
    };
}
