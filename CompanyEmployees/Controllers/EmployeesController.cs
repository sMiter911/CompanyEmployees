﻿using AutoMapper;
using CompanyEmployees.ActionFilters;
using Contracts.Interfaces;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using Entities.RequestFeatures;
using Newtonsoft.Json;
using static CompanyEmployees.Extensions.ServiceExtensions;

namespace CompanyEmployees.Controllers
{
	[Route("api/companies/{companyId}/employees")]
	[ApiController]
	public class EmployeesController : ControllerBase
	{
		private readonly IRepositoryManager _repository;
		private readonly ILoggerManager _logger;
		private readonly IMapper _mapper;
		private readonly IDataShaper<EmployeeDto> _dataShaper;

		public EmployeesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IDataShaper<EmployeeDto> dataShaper)
		{
			_repository = repository;
			_logger = logger;
			_mapper = mapper;
			_dataShaper = dataShaper;
		}

		[HttpGet]
		[ServiceFilter(typeof(ValidateMediaTypeAttribute))]
		public async Task<IActionResult> GetEmployeesFromCompany(Guid companyId, [FromQuery] EmployeeParameters employeeParameters)
		{
			if (!employeeParameters.ValidAgeRange)
			{
				return BadRequest("Max age cannot be less that min age");
			}

			var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
			if(company == null)
			{
				_logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
				return NotFound();
			}
			var employeesFromDb = await _repository.Employee.GetEmployeesAsync(companyId, employeeParameters, trackChanges: false);

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(employeesFromDb.MetaData));

            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);

			return Ok(_dataShaper.ShapeData(employeesDto, employeeParameters.Fields));
		}

		[HttpGet("{id}", Name = "GetEmployeeForCompany")]
		public async Task<IActionResult> GetEmployeeFromCompany(Guid companyId, Guid id)
		{
			var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
			if(company == null)
			{
				_logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
				return NotFound();
			}

			var employeeDb = await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges: false);
			if(employeeDb == null)
			{
				_logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
				return NotFound();
			}

			var employee = _mapper.Map<EmployeeDto>(employeeDb);

			return Ok(employee);
		}

		[HttpPost]
		[ServiceFilter(typeof(ValidationFilterAttribute))]
		public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto employee)
		{
			var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
			if(company == null)
			{
				_logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
				return NotFound();
			}

			var employeeIdentity = _mapper.Map<Employee>(employee);

			_repository.Employee.CreateEmployeeForCompany(companyId, employeeIdentity);
			await _repository.SaveAsync();

			var employeeToReturn = _mapper.Map<EmployeeDto>(employeeIdentity);

			return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id = employeeToReturn.Id }, employeeToReturn);
		}

		[HttpDelete("{id}")]
		[ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
		public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid id)
		{
			var employeeForCompany = HttpContext.Items["employee"] as Employee;

			_repository.Employee.DeleteEmployee(employeeForCompany);
			await _repository.SaveAsync();

			return NoContent();
		}

		[HttpPut("{id}")]
		[ServiceFilter(typeof(ValidationFilterAttribute))]
		[ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
		public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] EmployeeForUpdateDto employee) 
		{
			var employeeEntity = HttpContext.Items["employee"] as Employee;

			_mapper.Map(employee, employeeEntity);
			await _repository.SaveAsync();

			return NoContent();
		}

		[HttpPatch("{id}")]
		[ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
		public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDoc)
		{
			if(patchDoc == null)
			{
				_logger.LogError("patchDoc object sent from client is null.");
				return BadRequest("patchDoc object is null");
			}
		
			var employeeEntity = HttpContext.Items["employee"] as Employee;
		
			var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeEntity);

			patchDoc.ApplyTo(employeeToPatch, ModelState);

			TryValidateModel(employeeToPatch);

			if(!ModelState.IsValid)
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
