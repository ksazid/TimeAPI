using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class LeadStatusRepository : RepositoryBase, ILeadStatusRepository
    {
        public LeadStatusRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(LeadStatus entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.lead_status
                            (id, org_id, lead_status, lead_status_desc, created_date, createdby)
                    VALUES (@id, @org_id, @lead_status, @lead_status_desc,  @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public LeadStatus Find(string key)
        {
            return QuerySingleOrDefault<LeadStatus>(
                sql: "SELECT * FROM dbo.lead_status WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<LeadStatus> All()
        {
            return Query<LeadStatus>(
                sql: "SELECT * FROM dbo.lead_status where is_deleted = 0"
            );
        }

        public IEnumerable<LeadStatus> LeadStatusByOrgID(string key)
        {
            return Query<LeadStatus>(
                sql: "SELECT * FROM dbo.lead_status where is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.lead_status
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(LeadStatus entity)
        {
            Execute(
                sql: @"UPDATE dbo.lead_status
                           SET 
                            org_id = @org_id, 
                             lead_status = @lead_status, 
                            lead_status_desc = @lead_status_desc,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }

    }
}