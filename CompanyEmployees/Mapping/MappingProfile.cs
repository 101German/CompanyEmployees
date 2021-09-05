using AutoMapper;
using Entities.DataTransferObfects;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyEmployees.Mapping
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<Company, CompanyDTO>() .ForMember(c => c.FullAddress, 
                opt => opt.MapFrom(x => string.Join(' ', x.Address, x.Country)));
            CreateMap<Employee, EmployeeDTO>();
            CreateMap<CompanyForCreationDTO, Company>();
            CreateMap<EmployeeForCreationDTO, Employee>();
        }
    }
}
