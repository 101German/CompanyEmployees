using AutoMapper;
using Contracts;
using Entities.DataTransferObfects;
using Entities.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> GetEmployees(Guid companyId)
        {

            var company =await _repository.Company.GetCompanyAsync(companyId, trackChanges:false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the  database.");
                return NotFound();
            }
            var employeesFromDB =await _repository.Employee.GetEmployeesAsync(companyId, trackChanges:false);
            var employeesDTO = _mapper.Map<IEnumerable<EmployeeDTO>>(employeesFromDB);
            return Ok(employeesDTO);

        }
        [HttpGet("{id}", Name = "GetEmployeeForCompany")]
        public async Task<IActionResult> GetEmployeeForCompany(Guid companyId, Guid id)
        {
            var company =await  _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
            return NotFound();
            }
            var employeeDb =await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges:false);
            if (employeeDb == null)
            {
                _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
                return NotFound();
            }
            var employee = _mapper.Map<EmployeeDTO>(employeeDb);
            return Ok(employee);
        }
        [HttpPost]
        public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDTO employee)
        {
            if (employee == null)
            {
                _logger.LogError("EmployeeForCreationDto object sent from client is null.");
                return BadRequest("EmployeeForCreationDto object is null");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the EmployeeForCrationDTO object");
                ModelState.AddModelError("Name", "Invalid Name.Try again");
                return UnprocessableEntity(ModelState);
            }

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
        public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId,Guid id)
        {
            var company =await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
            if(company == null)
            {
                _logger.LogInfo($"Company with id:{companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeForCompany =await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges: false);
            if(employeeForCompany == null)
            {
                _logger.LogInfo($"Employee with id:{id} doesn't exist in the database.");
                return NotFound();
            }

            _repository.Employee.DeleteEmployee(employeeForCompany);
            await _repository.SaveAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId,Guid id,[FromBody] EmployeeForUpdateDTO employee)
        {
            if(employee == null)
            {
                _logger.LogError("EmployeeForUpdateDTO object sent from client is null");
                return BadRequest("EmployeeForUpdateDTO object is null");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the EmployeeForUpdateDto object");
                return UnprocessableEntity(ModelState);
            }

            var company =await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
            if(company == null)
            {
                _logger.LogInfo($"Company with id:{companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeEntity =await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges: true);
            if(employeeEntity == null)
            {
                _logger.LogInfo($"Employee with id:{id} doesn-t exist in the database");
                return NotFound();
            }

            _mapper.Map(employee, employeeEntity);
            await _repository.SaveAsync();

            return NoContent();
        }
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateForCompany(Guid companyId,Guid id,[FromBody] JsonPatchDocument<EmployeeForUpdateDTO> patchDoc)
        {
            if(patchDoc == null)
            {
                _logger.LogError("patchDoc object sent from client is null");
                return BadRequest("pathDoc object is null");
            }

            var company =await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
            if(company == null)
            {
                _logger.LogInfo($"Company with id:{companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeEntity =await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges:true);
            if (employeeEntity == null)
            {
                _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
                return NotFound();
            }

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
