namespace Contracts
{
    public interface IRepositoryManager
    {
        ICompanyRepository Company { get;}
        IEmpoyeeRepository Empoyee { get; }
        Task SaveAsync();
    }
}
