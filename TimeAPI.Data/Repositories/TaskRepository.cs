using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class TaskRepository : RepositoryBase, ITaskRepository
    {

        public TaskRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(Task entity)
        {

            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.task
                                  (id, task_name, task_desc, priority, status, assigned_empid, due_date, created_date, createdby)
                           VALUES (@id, @task_name, @task_desc, @priority, @status, @assigned_empid, @due_date, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Task Find(string key)
        {
            return QuerySingleOrDefault<Task>(
                sql: "SELECT * FROM dbo.task WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.task
                   SET
                       modified_date = @modified_date, modifiedby = @modifiedby, is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Task entity)
        {
            Execute(
                sql: @"UPDATE dbo.task
                   SET 
                    task_name = @task_name, 
                    task_desc = @task_desc, 
                    priority = @priority,
                    status = @status, 
                    assigned_empid = @assigned_empid, 
                    due_date = @due_date,
                    modified_date = @modified_date, 
                    modifiedby = @modifiedby, 
                    is_deleted = @is_deleted",
                param: entity
            );
        }

        public IEnumerable<Task> All()
        {
            return Query<Task>(
                sql: "SELECT * FROM [dbo].[task] where is_deleted = 0"
            );
        }

    }
}
