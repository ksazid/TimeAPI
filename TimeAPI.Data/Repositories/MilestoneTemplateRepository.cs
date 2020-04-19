using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class MilestoneTemplateRepository : RepositoryBase, IMilestoneTemplateRepository
    {
        public MilestoneTemplateRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(MilestoneTemplate entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.milestone_template
                                  (id, org_id, template_name, milestone_name, milestone_desc, start_date, end_date, is_approve_req, approve_emp_id, created_date, createdby, is_deleted)
                           VALUES (@id, @org_id, @template_name, @milestone_name, @milestone_desc, @start_date, @end_date, @is_approve_req, @approve_emp_id, @created_date, @createdby, @is_deleted);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public MilestoneTemplate Find(string key)
        {
            return QuerySingleOrDefault<MilestoneTemplate>(
                sql: "SELECT * FROM dbo.milestone_template WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.milestone_template
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }
         
        public void Update(MilestoneTemplate entity)
        {
            Execute(
                sql: @"UPDATE dbo.milestone_template
                   SET
                    template_name = @template_name, 
                    milestone_name = @milestone_name, 
                    milestone_desc = @milestone_desc, 
                    start_date = @start_date, 
                    end_date = @end_date, 
                    is_approve_req = @is_approve_req, 
                    approve_emp_id = @approve_emp_id,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<MilestoneTemplate> All()
        {
            return Query<MilestoneTemplate>(
                sql: "SELECT * FROM dbo.milestone_template where is_deleted = 0"
            );
        }

        public IEnumerable<MilestoneTemplate> FindByOrgID(string org_id)
        {
            return Query<MilestoneTemplate>(
                sql: "SELECT * FROM dbo.milestone_template where is_deleted = 0 and org_id = @org_id",
                 param: new { org_id }
            );
        }
    }
}