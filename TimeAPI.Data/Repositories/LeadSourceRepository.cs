using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class LeadSourceRepository : RepositoryBase, ILeadSourceRepository
    {
        public LeadSourceRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(LeadSource entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.lead_source
                            (id, org_id, lead_source, lead_source_desc, created_date, createdby)
                    VALUES (@id, @org_id, @lead_source, @lead_source_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public LeadSource Find(string key)
        {
            return QuerySingleOrDefault<LeadSource>(
                sql: "SELECT * FROM dbo.lead_source WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<LeadSource> All()
        {
            return Query<LeadSource>(
                sql: "SELECT * FROM dbo.lead_source where is_deleted = 0"
            );
        }

        public IEnumerable<LeadSource> LeadSourceByOrgID(string key)
        {
            return Query<LeadSource>(
                sql: "SELECT * FROM dbo.lead_source where is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.lead_source
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(LeadSource entity)
        {
            Execute(
                sql: @"UPDATE dbo.lead_source
                           SET 
                            org_id = @org_id, 
                            lead_source = @lead_source, 
                            lead_source_desc = @lead_source_desc, 
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}