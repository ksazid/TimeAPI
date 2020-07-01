using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class LeadProjectRepository : RepositoryBase, ILeadProjectRepository
    {
        public LeadProjectRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public void Add(LeadProject entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.lead_project
                            (id, lead_id, project_prefix, project_name, design_type_id, project_type_id, packages_id, created_date, createdby)
                    VALUES (@id, @lead_id, @project_prefix, @project_name, @design_type_id, @project_type_id, @packages_id, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public LeadProject Find(string key)
        {
            return QuerySingleOrDefault<LeadProject>(
                sql: "SELECT * FROM dbo.lead_project WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<LeadProject> All()
        {
            return Query<LeadProject>(
                sql: "SELECT * FROM dbo.lead_project where is_deleted = 0"
            );
        }

        public IEnumerable<LeadProject> LeadProjectByOrgID(string key)
        {
            return Query<LeadProject>(
                sql: "SELECT * FROM dbo.lead_project where is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }

        public LeadProject LeadProjectByLeadID(string key)
        {
            return QuerySingleOrDefault<LeadProject>(
                sql: "SELECT * FROM dbo.lead_project where is_deleted = 0 and lead_id = @key",
                param: new { key }
            );
        }

        
        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.lead_project
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(LeadProject entity)
        {
            Execute(
                sql: @"UPDATE dbo.lead_project
                           SET 
                            lead_id = @lead_id, 
                            project_prefix = @project_prefix, 
                            project_name = @project_name, 
                            design_type_id = @design_type_id,
                            project_type_id = @project_type_id, 
                            packages_id = @packages_id,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }

    }
}