using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class TaskTemplateRepository : RepositoryBase, ITaskTemplateRepository
    {
        public TaskTemplateRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(TaskTemplate entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.task_template
                                  (id, org_id, template_name, milestone_id, task_name, task_desc, priority_id, assignee_emp_id, due_date, is_approve_req, approve_emp_id, created_date, createdby, is_deleted)
                           VALUES (@id, @org_id, @template_name, @milestone_id, @task_name, @task_desc, @priority_id, @assignee_emp_id, @due_date, @is_approve_req, @approve_emp_id, @created_date, @createdby, @is_deleted);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public TaskTemplate Find(string key)
        {
            return QuerySingleOrDefault<TaskTemplate>(
                sql: "SELECT * FROM dbo.task_template WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.task_template
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(TaskTemplate entity)
        {
            Execute(
                sql: @"UPDATE dbo.task_template
                   SET
                    template_name = @template_name, 
                    milestone_id = @milestone_id, 
                    task_name = @task_name, 
                    task_desc = @task_desc, 
                    priority_id = @priority_id, 
                    assignee_emp_id = @assignee_emp_id, 
                    due_date = @due_date, 
                    is_approve_req = @is_approve_req, 
                    approve_emp_id = @approve_emp_id,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<TaskTemplate> All()
        {
            return Query<TaskTemplate>(
                sql: "SELECT * FROM dbo.task_template where is_deleted = 0"
            );
        }

        public IEnumerable<TaskTemplate> FindByOrgID(string org_id)
        {
            return Query<TaskTemplate>(
                sql: "SELECT * FROM dbo.task_template where is_deleted = 0 and org_id = @org_id",
                 param: new { org_id }
            );
        }
    }
}