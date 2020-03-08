using System.Collections.Generic;
using System.Data;
using System.Linq;
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
                                  (id, empid, task_name, task_desc, priority_id, status_id, assigned_empid, due_date, created_date, createdby, is_approver, is_approver_id)
                           VALUES (@id, @empid, @task_name, @task_desc, @priority_id, @status_id, @assigned_empid, @due_date, @created_date, @createdby, @is_approver, @is_approver_id);
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
                       modified_date = GETDATE(), is_deleted = 1
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
                    priority_id = @priority_id,
                    status_id = @status_id,
                    assigned_empid = @assigned_empid,
                    due_date = @due_date,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby,
                    is_approver = @is_approver,
                    is_approver_id = @is_approver_id
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

        public dynamic FindByTaskDetailsByEmpID(string key)
        {
            return Query<dynamic>(
                   sql: @"	SELECT
                            ROW_NUMBER() OVER (ORDER BY task.task_name) AS rowno,
                            task.id,
	                        task.task_name,
	                        task.task_desc,
	                        priority.priority_name,
	                        status.status_name,
		                    task.assigned_empid as assigned_to,
	                        e.full_name as assigned_to_name,
                            et.id as approver_id,
		                    et.full_name as approver_name,
	                        task.due_date,
	                        task.created_date
		                FROM[dbo].[task]
		                    INNER JOIN employee on task.empid = employee.id
			                LEFT JOIN employee e on task.assigned_empid = e.id
			                LEFT JOIN employee et on task.is_approver_id = et.id
			                LEFT JOIN priority on task.priority_id = priority.id
			                LEFT JOIN status on status.id = task.status_id
		                WHERE task.is_deleted = 0 and task.empid = @key",
                      param: new { key }
               );
        }

        public void UpdateTaskStatus(Tasks entity)
        {
            Execute(
                sql: @"UPDATE dbo.task
                   SET
                    status_id = @status_id,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id =  @id",
                param: entity
            );
        }

        public RootEmployeeTask GetAllTaskByEmpID(string key)
        {
            RootEmployeeTask rootEmployeeTask = new RootEmployeeTask();

            List<EmployeeTasks> employeeTasks = new List<EmployeeTasks>();

            var _employeeTasks = Query<EmployeeTasks>(
                     sql: @"SELECT
                               distinct(task.task_name),
                                task.id,
	                            employee.id as empid,
	                            task.task_name,
	                            task.task_desc,
	                            priority.priority_name as priority,
	                            status.id as status,
		                        task.assigned_empid as assigned_to,
	                            e.full_name as assigned_to_name,
                                et.id as approver_id,
		                        et.full_name as approver_name,
	                            task.due_date,
	                            task.created_date
	                            FROM[dbo].[task] WITH (NOLOCK)
			                        INNER JOIN employee on task.empid = employee.id
			                        LEFT JOIN employee e on task.assigned_empid = e.id
			                        LEFT JOIN employee et on task.is_approver_id = et.id
			                        LEFT JOIN priority on task.priority_id = priority.id
			                        LEFT JOIN status on status.id = task.status_id
                            WHERE task.is_deleted = 0 and task.empid =@key 
	
	                        UNION

	                        SELECT
                               distinct(task.task_name),
                                task.id,
	                            employee.id as empid,
	                            task.task_name,
	                            task.task_desc,
	                            priority.priority_name as priority,
	                            status.id as status,
		                        task.assigned_empid as assigned_to,
	                            e.full_name as assigned_to_name,
                                et.id as approver_id,
		                        et.full_name as approver_name,
	                            task.due_date,
	                            task.created_date
	                            FROM [dbo].[task]  WITH (NOLOCK)
			                        INNER JOIN employee on task.assigned_empid = employee.id
			                        LEFT JOIN employee e on task.is_approver_id = e.id
			                        LEFT JOIN priority on task.priority_id = priority.id
			                        LEFT JOIN status on status.id = task.status_id
                            WHERE task.is_deleted = 0 
	                        AND task.assigned_empid =@key ",
                        param: new { key }
                 );

            var _employeeAssignedTasks = Query<EmployeeTasks>(
                   sql: @"SELECT
                            distinct(task.task_name),
                                task.id,
	                            employee.id as empid,
	                            task.task_name,
	                            task.task_desc,
	                            priority.priority_name as priority,
	                            status.id as status,
		                        task.assigned_empid as assigned_to,
	                            employee.full_name as assigned_to_name,
                                et.id as approver_id,
		                        et.full_name as approver_name,
	                            task.due_date,
	                            task.created_date
	                        FROM [dbo].[task]  WITH (NOLOCK)
			                    INNER JOIN employee on task.assigned_empid = employee.id
			                    LEFT JOIN employee e on task.is_approver_id = e.id
			                    LEFT JOIN priority on task.priority_id = priority.id
			                    LEFT JOIN status on status.id = task.status_id
                        WHERE task.is_deleted = 0 
	                    AND task.assigned_empid =@key",
                      param: new { key }
               );
            rootEmployeeTask.EmployeeTasks = _employeeTasks;
            rootEmployeeTask.AssignedEmployeeTasks = _employeeAssignedTasks;

            return rootEmployeeTask;
        }
    }
}