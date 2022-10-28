using AutoMapper;
using Entities.Models;
using Shared.DTOs;
using Shared.DTOs.Company;
using Shared.DTOs.Employee;

namespace CompanyEmployees
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            //Company mappings
            CreateMap<Company, CompanyDTO>()
                .ForMember(c=>c.FullAddress,
                opt => opt.MapFrom(x => string.Join(' ', x.Address, x.Country)));
            CreateMap<CompanyForCreationDTO, Company>();
            CreateMap<CompanyForUpdateDTO, Company>();

            //Employee mappings
            CreateMap<Employee, EmployeeDTO>();
            CreateMap<EmployeeForCreationDTO,Employee>();
            CreateMap<EmployeeForUpdateDTO, Employee>().ReverseMap();

            //User
            CreateMap<UserForRegistrationDTO, User>();
        }

    }
}
