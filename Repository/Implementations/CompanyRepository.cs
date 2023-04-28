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
	public class CompanyRepository : RepositoryBase<Company>, ICompanyRepository
	{
		public CompanyRepository(RepositoryContext repositoryContext) : base(repositoryContext)
		{
		}

		public void CreateCompany(Company company) {
			
			Create(company);
		}

		public IEnumerable<Company> GetAllCompanies(bool trackChanges)
		{
			return FindAll(trackChanges)
				.OrderBy(c => c.Name)
				.ToList();
		}

		public Company GetCompany(Guid id, bool trackChanges)
		{
			return FindByCondition(c => c.Id.Equals(id), trackChanges).SingleOrDefault();
		}
	}
}
