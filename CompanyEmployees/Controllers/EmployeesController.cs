﻿using AutoMapper;
using Contracts.Interfaces;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace CompanyEmployees.Controllers
{
	[Route("api/companies/{companyId}/employees")]
	[ApiController]
	public class EmployeesController : ControllerBase
	{
		private readonly IRepositoryManager _repository;
		private readonly ILoggerManager _logger;
		private readonly IMapper _mapper;

		public EmployeesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
		{
			_repository = repository;
			_logger = logger;
			_mapper = mapper;
		}

		[HttpGet]
		public IActionResult GetEmployessFromCompany(Guid companyId)
		{
			var company = _repository.Company.GetCompany(companyId, trackChanges: false);
			if(company == null)
			{
				_logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
				return NotFound();
			}
			var employeesFromDb = _repository.Employee.GetEmployees(companyId, trackChanges: false);
			var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);
			return Ok(employeesDto);
		}

		[HttpGet("{id}", Name = "GetEmployeeForCompany")]
		public IActionResult GetEmployeeFroCompany(Guid companyId, Guid id)
		{
			var company = _repository.Company.GetCompany(companyId, trackChanges: false);
			if(company == null)
			{
				_logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
				return NotFound();
			}

			var employeeDb = _repository.Employee.GetEmployee(companyId, id, trackChanges: false);
			if(employeeDb == null)
			{
				_logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
				return NotFound();
			}

			var employee = _mapper.Map<EmployeeDto>(employeeDb);

			return Ok(employee);
		}

		[HttpPost]
		public IActionResult CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto employee)
		{
			if(employee == null)
			{
				_logger.LogError("EmployeeForCreationDto object sent from client is null.");
				return BadRequest("EmployeeForCreationDto object is null");
			}

			var company = _repository.Company.GetCompany(companyId, trackChanges: false);
			if(company == null)
			{
				_logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
				return NotFound();
			}

			var employeeIdentity = _mapper.Map<Employee>(employee);

			_repository.Employee.CreateEmployeeForCompany(companyId, employeeIdentity);
			_repository.Save();

			var employeeToReturn = _mapper.Map<EmployeeDto>(employeeIdentity);

			return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id = employeeToReturn.Id }, employeeToReturn);
		}
	}
}
