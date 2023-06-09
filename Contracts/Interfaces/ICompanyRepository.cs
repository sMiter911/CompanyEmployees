﻿using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Interfaces
{
	public interface ICompanyRepository
	{
        Task<IEnumerable<Company>> GetAllCompaniesAsync(bool trackChanges);
		Task<Company> GetCompanyAsync(Guid id, bool trackChanges);
		void CreateCompany(Company company);
		Task<IEnumerable<Company>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges);
		void DeleteCompany(Company company);
	}
}
