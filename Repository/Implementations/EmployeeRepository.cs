﻿using Contracts.Interfaces;
using Entities;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Implementations
{
	public class EmployeeRepository : RepositoryBase<Employee>, IEmployeeRepository
	{
		public EmployeeRepository(RepositoryContext repositoryContext) : base(repositoryContext)
		{

		}

		public Employee GetEmployee(Guid companyId, Guid id, bool trackChanges)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<Employee> GetEmployees(Guid companyId, bool trackChanges)
		{
			return FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges).OrderBy(e => e.Name);
		}
	}
}