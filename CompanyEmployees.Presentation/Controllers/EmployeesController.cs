using CompanyEmployees.Presentation.ActionFilters;
using Entities.LinkModels;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DTOs.Employee;
using Shared.RequestFeatures;
using System.Text.Json;

namespace CompanyEmployees.Presentation.Controllers
{
    [Route("api/companies/{companyId}/employees")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IServiceManager _service;
        public EmployeesController(IServiceManager service) => _service = service;

        [HttpGet]
        [ServiceFilter(typeof(ValidateMediaTypeAttribute))]
        [HttpHead]
        public async Task<IActionResult> GetEmployees(Guid companyId,
            [FromQuery] EmployeeParameters employeeParameters)
        {
            var linkParams = new LinkParameters(employeeParameters, HttpContext);

            var result = await _service.EmployeeService.GetEmployeesAsync(companyId, 
                linkParams, false);
            
            Response.Headers.Add("X-Pagination", 
                JsonSerializer.Serialize(result.metaData));
            return result.linkResponse.HasLinks? Ok(result.linkResponse.LinkedEntities) :
                Ok(result.linkResponse.ShapedEntities);
        }

        [HttpGet("{id:guid}",Name = "GetEmployeeForCompany")]
        public async Task<IActionResult> GetEmployeeForCompany(Guid companyId, Guid id)
        {
            var employee = await _service.EmployeeService.GetEmployeeAsync(companyId, id, false);
            return Ok(employee);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDTO employee)
        {
            var createdEmployee = await _service.EmployeeService.CreateEmployeeForCompanyAsync(companyId, employee, false);
            return CreatedAtRoute("GetEmployeeForCompany",
                new {companyId, id = createdEmployee.Id}, createdEmployee);
        }
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid id)
        {
            await _service.EmployeeService.DeleteEmployeeForCompanyAsync(companyId, id, false);
            return NoContent();
        }

        [HttpPut("{id:guid}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompanyAsync(Guid companyId,Guid id,
            [FromBody] EmployeeForUpdateDTO employeeForUpdate)
        {
            await _service.EmployeeService.UpdateEmployeeForCompanyAsync(companyId, id, employeeForUpdate, false, true);
            return NoContent();
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> PartiallUpdateEmployeeForCompany(Guid companyId,Guid id,
            [FromBody]JsonPatchDocument<EmployeeForUpdateDTO> patchDoc)
        {
            if (patchDoc is null)
            {
                return BadRequest($"{nameof(patchDoc)} object sent from client is null.");
            }

            var result = await _service.EmployeeService.GetEmployeeForPatchAsync(companyId, id, false, true);
            patchDoc.ApplyTo(result.employeeToPatch);

            await _service.EmployeeService.SaveChangesForPatchAsync(result.employeeToPatch, result.employee);
            return NoContent();
        }
    }
}
