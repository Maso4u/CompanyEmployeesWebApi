using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Service.Contracts;
using Shared.DTOs.Company;

namespace Service
{
    internal sealed class CompanyService : ICompanyService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;

        public CompanyService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<CompanyDTO> CreateCompanyAsync(CompanyForCreationDTO companyDTO)
        {
            //map DTO to Entity
            var company = _mapper.Map<Company>(companyDTO);

            //Create Entity in the repository
            _repository.Company.CreateCompany(company);
            //save entity to Db
            await _repository.SaveAsync();

            //map the created entity back to a DTO
            var companyToReturn= _mapper.Map<CompanyDTO>(company);
            return companyToReturn;
        }

        public async Task<(IEnumerable<CompanyDTO> companies, string ids)> CreateCompanyCollectionAsync(IEnumerable<CompanyForCreationDTO> companyCollection)
        {
            if (companyCollection is null)
            {
                throw new CompanyCollectionBadRequest();
            }

            var companies = _mapper.Map<IEnumerable<Company>>(companyCollection);
            foreach (var company in companies)
            {
                _repository.Company.CreateCompany(company);
            }
            await _repository.SaveAsync();

            var companiesCreated = _mapper.Map<IEnumerable<CompanyDTO>>(companies);
            var ids = string.Join(',', companiesCreated.Select(c => c.Id));

            return (companiesCreated, ids); 
        }

        public async Task DeleteCompanyAsync(Guid id, bool trackChanges)
        {
            var company = await GetCompanyAndCheckIfItExists(id, trackChanges);
            _repository.Company.DeleteCompany(company);
            await _repository.SaveAsync();
        }

        public async Task<IEnumerable<CompanyDTO>> GetAllCompaniesAsync(bool trackChanges)
        {
            var companies = await _repository.Company.GetAllCompaniesAsync(trackChanges);
            var companiesDto = _mapper.Map<List<CompanyDTO>>(companies);
            return companiesDto;
        }

        public async Task<IEnumerable<CompanyDTO>> GetByIdAsync(IEnumerable<Guid> ids, bool trackChanges)
        {
            if (ids is null)
            {
                throw new IdParametersBadRequestException();
            }

            var companies = await _repository.Company.GetByIdAsync(ids, trackChanges);
            if (ids.Count() !=companies.Count())
            {
                throw new CollectionByIdsBadRequestException();
            }

            var companyToReturn = _mapper.Map<IEnumerable<CompanyDTO>>(companies);
            return companyToReturn;
        }

        public async Task<CompanyDTO> GetCompanyAsync(Guid id, bool trackChanges)
        {
            var company = await GetCompanyAndCheckIfItExists(id, trackChanges);
            var companyDto = _mapper.Map<CompanyDTO>(company);
            return companyDto;
        }

        public async Task UpdateCompanyAsync(Guid id, CompanyForUpdateDTO company, bool trackChanges)
        {
            var companyfound = await GetCompanyAndCheckIfItExists(id, trackChanges);
            _mapper.Map(company, companyfound);
            await _repository.SaveAsync();

        }

        private async Task<Company> GetCompanyAndCheckIfItExists(Guid id, bool trackChanges)
        {
            var company = await _repository.Company.GetCompanyAsync(id, trackChanges);
            if (company is null)
            {
                throw new CompanyNotFoundException(id);
            }
            return company;
        }
    }
}
