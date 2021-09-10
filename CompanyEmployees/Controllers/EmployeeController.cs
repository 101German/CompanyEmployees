using AutoMapper;
using CompanyEmployees.ActionFilters;
using Contracts;
using Entities.DataTransferObfects;
using Entities.Models;
using Entities.RequestFeaters;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        public async Task<IActionResult> GetEmployees(Guid companyId,[FromQuery] EmployeeParameters employeeParameters)
        {
            if (!employeeParameters.ValidAgeRange)
            {
                return BadRequest("Max age can't be less then min age");
            }

            var company =await _repository.Company.GetCompanyAsync(companyId, trackChanges:false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the  database.");
                return NotFound();
            }
            var employeesFromDB =await _repository.Employee.GetEmployeesAsync(companyId,employeeParameters, trackChanges:false);

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(employeesFromDB.MetaData));

            var employeesDTO = _mapper.Map<IEnumerable<EmployeeDTO>>(employeesFromDB);
            return Ok(employeesDTO);
        }

        [HttpGet("{id}", Name = "GetEmployeeForCompany")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public  IActionResult GetEmployeeForCompany(Guid companyId, Guid id)
        {

            var employeeDb = HttpContext.Items["employee"] as Employee;
            var employee = _mapper.Map<EmployeeDTO>(employeeDb);
            return Ok(employee);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDTO employee)
        {
            var company =await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the   database.");
                 return NotFound();
            }
            var employeeEntity = _mapper.Map<Employee>(employee);

            _repository.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
            await _repository.SaveAsync();
            
           var employeeToReturn = _mapper.Map<EmployeeDTO>(employeeEntity);
            return CreatedAtRoute("GetEmployeeForCompany", new { companyId,id = employeeToReturn.Id }, employeeToReturn);
        }

        [HttpDelete("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId,Guid id)
        {
           
            var employeeForCompany =HttpContext.Items["employee"] as Employee;
           
            _repository.Employee.DeleteEmployee(employeeForCompany);
            await _repository.SaveAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId,Guid id,[FromBody] EmployeeForUpdateDTO employee)
        {

            var employeeEntity = HttpContext.Items["employee"] as Employee;
            

            _mapper.Map(employee, employeeEntity);
            await _repository.SaveAsync();

            return NoContent();
        }

        [HttpPatch("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> PartiallyUpdateForCompany(Guid companyId,Guid id,[FromBody] JsonPatchDocument<EmployeeForUpdateDTO> patchDoc)
        {

            var employeeEntity = HttpContext.Items["employee"] as Employee;
            
            var employeeToPatch = _mapper.Map<EmployeeForUpdateDTO>(employeeEntity);

            patchDoc.ApplyTo(employeeToPatch,ModelState);

            TryValidateModel(employeeToPatch);

            if (!ModelState.IsValid)
            {

                _logger.LogError("Invalid model state for the patch document");
                return UnprocessableEntity(ModelState);

            }

            _mapper.Map(employeeToPatch, employeeEntity);

           await _repository.SaveAsync();

            return NoContent();
        }
    }
}
