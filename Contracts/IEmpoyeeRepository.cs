using Entities.Models;
using Shared.RequestFeatures;

namespace Contracts
{
    public interface IEmpoyeeRepository
    {
        Task<PagedList<Employee>> GetEmployeesAsync(Guid companyId,EmployeeParameters employeeParameters, bool trackChanges);
        Task<Employee> GetEmployeeAsync(Guid companyId,Guid employeeId, bool trackChanges);
        void CreateEmployee(Guid companyId, Employee employee);
        void DeleteEmployee(Employee employee); 
        void UpdateEmployee(Employee employee);
    }
}
