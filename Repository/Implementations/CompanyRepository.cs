using Contracts.Interfaces;
using Entities;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
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

		public void DeleteCompany(Company company)
		{
			Delete(company);
		}

		async public Task<IEnumerable<Company>> GetAllCompaniesAsync(bool trackChanges)
		{
			return await FindAll(trackChanges)
				.OrderBy(c => c.Name)
				.ToListAsync();
		}

		async public Task<IEnumerable<Company>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges)
		{
			return await FindByCondition(x => ids.Contains(x.Id), trackChanges).ToListAsync();
		}

		async public Task<Company> GetCompanyAsync(Guid id, bool trackChanges)
		{
			return await FindByCondition(c => c.Id.Equals(id), trackChanges).SingleOrDefaultAsync();
		}
	}
}
