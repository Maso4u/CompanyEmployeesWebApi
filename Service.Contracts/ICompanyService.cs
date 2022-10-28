using Shared.DTOs.Company;

namespace Service.Contracts
{
    public interface ICompanyService
    {
        Task<IEnumerable<CompanyDTO>> GetAllCompaniesAsync(bool trackChanges);
        Task<CompanyDTO> GetCompanyAsync(Guid id,bool trackChanges);
        Task<IEnumerable<CompanyDTO>> GetByIdAsync(IEnumerable<Guid> ids, bool trackChanges);

        Task<CompanyDTO> CreateCompanyAsync(CompanyForCreationDTO company);
        Task<(IEnumerable<CompanyDTO> companies, string ids)> CreateCompanyCollectionAsync
            (IEnumerable<CompanyForCreationDTO> companyCollection);
        Task DeleteCompanyAsync(Guid id,bool trackChanges);
        Task UpdateCompanyAsync(Guid id,CompanyForUpdateDTO company,bool trackChanges);
    }
}
