using Shared.DTOs.Employee;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Company
{
    public abstract record CompanyForManipulationDTO
    {
        [Required(ErrorMessage = "Company name is a required field.")]
        public string? Name { get; init; }
        [Required(ErrorMessage = "Company address is a required field.")]
        public string? Address { get; init; }
        [Required(ErrorMessage = "Company country is a required field.")]
        public string? Country { get; init; }
        public IEnumerable<EmployeeForCreationDTO>? Employees { get; init; }
    }
}
