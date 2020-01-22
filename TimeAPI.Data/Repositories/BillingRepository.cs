using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class BillingRepository : RepositoryBase, IBillingRepository
    {
        public BillingRepository(IDbTransaction transaction) : base(transaction)
        { }
        public void Add(Billing entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.team
                                  (id, org_id, team_name, team_desc, team_by, team_department_id, team_lead_empid, created_date, createdby)
                           VALUES (@id, @org_id, @team_name,  @team_desc, @team_by, @team_department_id, @team_lead_empid, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }   
        public Billing Find(string key)
        {
            return QuerySingleOrDefault<Billing>(
                sql: "SELECT * FROM dbo.team WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }
        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.team
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }
        public void Update(Billing entity)
        {
            Execute(
                sql: @"UPDATE dbo.team
                   SET 
                    org_id = @org_id,
                    team_name = @team_name, 
                    team_desc = @team_desc, 
                    team_by = @team_by, 
                    team_department_id = @team_department_id, 
                    team_lead_empid = @team_lead_empid,
                    modified_date = @modified_date, 
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }
        public IEnumerable<Billing> All()
        {
            return Query<Billing>(
                sql: "SELECT * FROM [dbo].[team] where is_deleted = 0"
            );
        }
        public IEnumerable<Billing> FindBillingsByOrgID(string key)
        {
            return Query<Billing>(
                sql: "SELECT * FROM dbo.team WHERE is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }
    }
}
