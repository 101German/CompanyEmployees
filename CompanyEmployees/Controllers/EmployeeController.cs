using AutoMapper;
using Contracts;
using Entities.DataTransferObfects;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyEmployees.Controllers
{
    [Route("api/companies/{companyId}/employees")]
    [ApiController]
    public class EmployeeController:ControllerBase
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        public EmployeeController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetEmployees(Guid companyId)
        {
            var company = _repository.Company.GetCompany(companyId, trackChanges:false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the  database.");
                return NotFound();
            }
            var employeesFromDB = _repository.Employee.GetEmployees(companyId, trackChanges:false);
            var employeesDTO = _mapper.Map<IEnumerable<EmployeeDTO>>(employeesFromDB);
            return Ok(employeesDTO);

        }
    }
}
