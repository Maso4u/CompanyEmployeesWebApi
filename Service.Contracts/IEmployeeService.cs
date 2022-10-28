using Entities.LinkModels;
using Entities.Models;
using Shared.DTOs.Employee;
using Shared.RequestFeatures;
using System.Dynamic;

namespace Service.Contracts
{
    public interface IEmployeeService
    {
        Task<(LinkResponse linkResponse, MetaData metaData)> GetEmployeesAsync(
            Guid companyId,LinkParameters linkParameters, bool trackChanges);
        Task<EmployeeDTO> GetEmployeeAsync(Guid companyId, Guid employeeId, bool trackChanges);
        Task<EmployeeDTO> CreateEmployeeForCompanyAsync(Guid companyId, EmployeeForCreationDTO employeeForCreation, bool trackChanges);

        Task DeleteEmployeeForCompanyAsync(Guid companyId, Guid employeeId, bool trackChanges);
        Task UpdateEmployeeForCompanyAsync(Guid companyId, Guid id, EmployeeForUpdateDTO employeeForUpdate, bool compTrackChanges, bool empTrackChanges);

        Task<(EmployeeForUpdateDTO employeeToPatch, Employee employee)> GetEmployeeForPatchAsync(
            Guid companyId, Guid id, bool compTrackChanges,bool empTrackChanges);

        Task SaveChangesForPatchAsync(EmployeeForUpdateDTO employeeToPatch, Employee employee);
    }
}
