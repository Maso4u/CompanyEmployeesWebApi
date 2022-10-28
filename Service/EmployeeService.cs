using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.LinkModels;
using Entities.Models;
using Service.Contracts;
using Shared.DTOs.Employee;
using Shared.RequestFeatures;
using System.Dynamic;

namespace Service
{
    internal sealed class EmployeeService : IEmployeeService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly IEmployeeLinks _employeeLinks;

        public EmployeeService(IRepositoryManager repository, 
            ILoggerManager logger, IMapper mapper,IEmployeeLinks employeeLinks)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _employeeLinks = employeeLinks;
        }

        public async Task<EmployeeDTO> CreateEmployeeForCompanyAsync(Guid companyId, EmployeeForCreationDTO employeeForCreation, bool trackChanges)
        {
            await CheckIfCompanyExists(companyId,trackChanges);
            var employee = _mapper.Map<Employee>(employeeForCreation);
            _repository.Empoyee.CreateEmployee(companyId, employee);
            await _repository.SaveAsync();

            var createdEmployee = _mapper.Map<EmployeeDTO>(employee);

            return createdEmployee;
        }

        public async Task DeleteEmployeeForCompanyAsync(Guid companyId, Guid employeeId, bool trackChanges)
        {
            await CheckIfCompanyExists(companyId, trackChanges);
            var employee = await GetEmployeeAndCheckIfItExists(companyId, employeeId, trackChanges);
            _repository.Empoyee.DeleteEmployee(employee);
            await _repository.SaveAsync();
        }
        public async Task<EmployeeDTO> GetEmployeeAsync(Guid companyId, Guid employeeId, bool trackChanges)
        {
            await CheckIfCompanyExists(companyId, trackChanges);

            var employee = await GetEmployeeAndCheckIfItExists(companyId, employeeId, trackChanges);
            var employeeDto = _mapper.Map<EmployeeDTO>(employee);
            return employeeDto;
        }


        public async Task<(EmployeeForUpdateDTO employeeToPatch, Employee employee)> 
            GetEmployeeForPatchAsync(Guid companyId, Guid id, bool compTrackChanges, bool empTrackChanges)
        {
            await CheckIfCompanyExists(companyId, compTrackChanges);

            var employee = await GetEmployeeAndCheckIfItExists(companyId, id, empTrackChanges);
            var employeeDTO = _mapper.Map<EmployeeForUpdateDTO>(employee);
            return (employeeDTO, employee);
        }

        public async Task<(LinkResponse linkResponse, MetaData metaData)> 
            GetEmployeesAsync(Guid companyId, LinkParameters linkParameters,bool trackChanges)
        {
            if (!linkParameters.EmployeeParameters.ValidAgeRange)
            {
                throw new MaxAgeRangeBadRequestException();
            }

            await CheckIfCompanyExists(companyId, trackChanges);

            var employeesWithMetaData = await _repository.Empoyee
                .GetEmployeesAsync(companyId,linkParameters.EmployeeParameters,trackChanges);

            var employeesDTO = _mapper.Map<IEnumerable<EmployeeDTO>>(employeesWithMetaData);
            var links = _employeeLinks.TryGenerateLinks(employeesDTO,linkParameters.EmployeeParameters.Fields,
                companyId,linkParameters.Context);

            return (links,employeesWithMetaData.MetaData);
        }

        public async Task SaveChangesForPatchAsync(EmployeeForUpdateDTO employeeToPatch, Employee employee)
        {
            _mapper.Map(employeeToPatch, employee);
            await _repository.SaveAsync();
        }

        public async Task UpdateEmployeeForCompanyAsync(Guid companyId, 
            Guid id, EmployeeForUpdateDTO employeeForUpdate, bool compTrackChanges, bool empTrackChanges)
        {
            await CheckIfCompanyExists(companyId, compTrackChanges);
            var employee = await GetEmployeeAndCheckIfItExists(companyId, id, empTrackChanges);
            _mapper.Map(employeeForUpdate,employee);
            await _repository.SaveAsync();
        }
        
        private async Task<Employee> GetEmployeeAndCheckIfItExists(Guid companyId, Guid employeeId, bool trackChanges)
        {
            var employee = await _repository.Empoyee.GetEmployeeAsync(companyId, employeeId, trackChanges);
            if (employee is null)
            {
                throw new EmployeeNotFoundException(employeeId);
            }

            return employee;
        }
        private async Task CheckIfCompanyExists(Guid companyId, bool trackChanges)
        {
            var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges);
            if (company is null)
            {
                throw new CompanyNotFoundException(companyId);
            }
        }
    }
}
