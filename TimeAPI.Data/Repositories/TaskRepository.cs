using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class TaskRepository : RepositoryBase, ITaskRepository
    {

        public TaskRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(Tasks entity)
        {

            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.task
                                  (id, empid, task_name, task_desc, priority, status, assigned_empid, due_date, created_date, createdby)
                           VALUES (@id, @empid, @task_name, @task_desc, @priority, @status, @assigned_empid, @due_date, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Tasks Find(string key)
        {
            return QuerySingleOrDefault<Tasks>(
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

        public void Update(Tasks entity)
        {
            Execute(
                sql: @"UPDATE dbo.task
                   SET 
                    empid = @empid,
                    task_name = @task_name, 
                    task_desc = @task_desc, 
                    priority = @priority,
                    status = @status, 
                    assigned_empid = @assigned_empid, 
                    due_date = @due_date,
                    modified_date = @modified_date, 
                    modifiedby = @modifiedby
                    WHERE id =  @id",
                param: entity
            );
        }

        public IEnumerable<Tasks> All()
        {
            return Query<Tasks>(
                sql: "SELECT * FROM [dbo].[task] where is_deleted = 0"
            );
        }

        public dynamic FindByTaskDetailsByID(string key)
        {
            return Query<dynamic>(
                   sql: @"	SELECT 
		                    task.task_name,
		                    task.task_desc,
		                    priority.priority_name,
		                    status.status_name,
		                    employee.full_name,
		                    task.due_date
		                    FROM[dbo].[task]
		                    inner join priority on task.priority = priority.id
		                    inner join employee on task.assigned_empid = employee.id
		                    inner join status on status.id = task.status
		                WHERE task.is_deleted = 0 and task.id = @key",
                      param: new { key }
               );
        }

    }
}
