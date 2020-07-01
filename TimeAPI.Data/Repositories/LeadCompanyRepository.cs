using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class LeadCompanyRepository : RepositoryBase, ILeadCompanyRepository
    {
        public LeadCompanyRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(LeadCompany entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.lead_company
                            (id, org_id, company_name, comapany_desc, created_date, createdby)
                    VALUES (@id, @org_id, @company_name, @comapany_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public LeadCompany Find(string key)
        {
            return QuerySingleOrDefault<LeadCompany>(
                sql: "SELECT * FROM dbo.lead_company WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<LeadCompany> All()
        {
            return Query<LeadCompany>(
                sql: "SELECT * FROM dbo.lead_company where is_deleted = 0"
            );
        }

        public IEnumerable<LeadCompany> LeadCompanyByOrgID(string key)
        {
            return Query<LeadCompany>(
                sql: "SELECT * FROM dbo.lead_company where is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.lead_company
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(LeadCompany entity)
        {
            Execute(
                sql: @"UPDATE dbo.lead_company
                           SET 
                            org_id = @org_id, 
                            company_name = @company_name,
                            comapany_desc = @comapany_desc,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }

    }
}