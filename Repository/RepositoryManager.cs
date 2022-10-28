using Contracts;

namespace Repository
{
    public class RepositoryManager : IRepositoryManager
    {
        private readonly RepositoryContext _repositoryContext;
        private readonly Lazy<ICompanyRepository> _companyRepository;
        private readonly Lazy<IEmpoyeeRepository> _empoyeeRepository;

        public RepositoryManager(RepositoryContext repositoryContext)
        {
            _repositoryContext = repositoryContext;
            _companyRepository = new Lazy<ICompanyRepository>(()=>new CompanyRepository(repositoryContext));
            _empoyeeRepository = new Lazy<IEmpoyeeRepository>(()=>new EmployeeRepository(repositoryContext));
        }

        public ICompanyRepository Company => _companyRepository.Value;

        public IEmpoyeeRepository Empoyee => _empoyeeRepository.Value;

        public async Task SaveAsync()=> await _repositoryContext.SaveChangesAsync();
    }
}
