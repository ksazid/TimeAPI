using System.Collections.Generic;
using System.Data;
using System.Linq;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class CostProjectTaskRepository : RepositoryBase, ICostProjectTaskRepository
    {
        public CostProjectTaskRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(CostProjectTask entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.cost_task
                                  (id, project_id, is_selected, milestone_id, task_name, unit, qty, created_date, createdby)
                           VALUES (@id, @project_id, @is_selected, @milestone_id, @task_name, @unit, @qty, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public CostProjectTask Find(string key)
        {
            return QuerySingleOrDefault<CostProjectTask>(
                sql: @"SELECT dbo.project_activity_x_task.project_id as project_id, dbo.project_activity_x_task.activity_id as activtity_id, dbo.cost_task.* 
						FROM dbo.cost_task
						LEFT JOIN dbo.project_activity_x_task on dbo.cost_task.id = dbo.project_activity_x_task.task_id
						WHERE dbo.cost_task.is_deleted = 0 and dbo.cost_task.id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.cost_task
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(CostProjectTask entity)
        {
            Execute(
                sql: @"UPDATE dbo.cost_task
                   SET
                    project_id = @project_id,
					is_selected  = @is_selected, 
					milestone_id = @milestone_id, 
					task_name = @task_name, 
					unit = @unit, 
					qty = @qty,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby,
                    WHERE id =  @id",
                param: entity
            );
        }

        public IEnumerable<CostProjectTask> All()
        {
            return Query<CostProjectTask>(
                sql: "SELECT * FROM [dbo].[cost_task] where is_deleted = 0"
            );
        }

        public IEnumerable<CostProjectTask> GetAllStaticMilestoneTasksByMilestoneID(string MilestoneID)
        {
            return Query<CostProjectTask>(
                sql: "SELECT * FROM [dbo].[static_tasks] where is_deleted = 0 and  milestone_id = @MilestoneID",
                 param: new { MilestoneID }
            );
        }

        public IEnumerable<CostProjectTask> GetAllMilestoneTasksByMilestoneID(string MilestoneID)
        {
            return Query<CostProjectTask>(
                sql: "SELECT * FROM [dbo].[cost_task] where is_deleted = 0 and  milestone_id = @MilestoneID",
                 param: new { MilestoneID }
            );
        }

        //public dynamic FindByTaskDetailsByEmpID(string key)
        //{
        //    return Query<dynamic>(
        //           sql: @"SELECT
        //                        DISTINCT(task.task_name),
        //                        task.id,
        //                     employee.id as empid,
        //                     task.task_desc,
        //                     priority.priority_name as priority,
        //                     status.id as status_id,
        //                     status.status_name as status_name,
        //                  task.assigned_empid as assigned_to,
        //                     e.full_name as assigned_to_name,
        //                        task.is_approver as is_approver,
        //                        task.is_approver_id as is_approver_id,
        //                  et.full_name as approver_name,
        //                     FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
        //                     task.created_date
        //                     FROM[dbo].[task] WITH (NOLOCK)
        //                   INNER JOIN employee on task.empid = employee.id
        //                   LEFT JOIN employee e on task.assigned_empid = e.id
        //                   LEFT JOIN employee et on task.is_approver_id = et.id
        //                   LEFT JOIN priority on task.priority_id = priority.id
        //                   LEFT JOIN status on status.id = task.status_id
        //                    WHERE task.is_deleted = 0 and task.empid =@key 

        //           UNION

        //             SELECT
        //                         DISTINCT(task.task_name),
        //                        task.id,
        //                     employee.id as empid,
        //                     task.task_desc,
        //                     priority.priority_name as priority,
        //                     status.id as status_id,
        //                     status.status_name as status_name,
        //                  task.assigned_empid as assigned_to,
        //                     e.full_name as assigned_to_name,
        //                        task.is_approver as is_approver,
        //                        task.is_approver_id as is_approver_id,
        //                  et.full_name as approver_name,
        //                     FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
        //                     task.created_date
        //                    FROM [dbo].[task] WITH (NOLOCK)
        //                   INNER JOIN employee on task.empid = employee.id
        //                   LEFT JOIN employee e on task.assigned_empid = e.id
        //                   LEFT JOIN employee et on task.is_approver_id = et.id
        //                   LEFT JOIN priority on task.priority_id = priority.id
        //                   LEFT JOIN status on status.id = task.status_id
        //                    WHERE task.is_deleted = 0 
        //                 AND task.assigned_empid =@key",
        //        param: new { key }
        //       );
        //}

        //public void UpdateTaskStatus(CostProjectTask entity)
        //{
        //    Execute(
        //        sql: @"UPDATE dbo.cost_task
        //           SET
        //            status_id = @status_id,
        //            modified_date = @modified_date,
        //            modifiedby = @modifiedby
        //            WHERE id =  @id",
        //        param: entity
        //    );
        //}

        //     public RootEmployeeTask GetAllTaskByEmpID(string key,  string date)
        //     {
        //         RootEmployeeTask rootEmployeeTask = new RootEmployeeTask();

        //         var _employeeTasks = Query<EmployeeTasks>(
        //                      sql: @"SELECT
        //                             DISTINCT(task.task_name),
        //					ISNULL(project.project_name, 'NA') as project_name,
        //					ISNULL(project_activity.activity_name, 'NA') as milestone_name,
        //                             project.id as project_id,
        //                             project_activity.id as milestone_id,
        //                             task.id,
        //                          employee.id as empid,
        //                          task.task_desc,
        //                          priority.priority_name as priority,
        //                          status.id as status_id,
        //                          status.status_name as status_name,
        //                       task.assigned_empid as assigned_to,
        //                          e.full_name as assigned_to_name,
        //                             task.is_approver as is_approver,
        //                             task.is_approver_id as is_approver_id,
        //                       et.full_name as approver_name,
        //                             FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
        //                          task.created_date
        //                          FROM[dbo].[task] WITH (NOLOCK)
        //                        INNER JOIN employee on task.empid = employee.id
        //						LEFT JOIN project_activity_x_task on [dbo].[task].id = project_activity_x_task.task_id
        //						LEFT JOIN project_activity on project_activity_x_task.activity_id = project_activity.id
        //						LEFT JOIN project on project_activity_x_task.project_id = project.id
        //                        LEFT JOIN employee e on task.assigned_empid = e.id
        //                        LEFT JOIN employee et on task.is_approver_id = et.id
        //                        LEFT JOIN priority on task.priority_id = priority.id
        //                        LEFT JOIN status on status.id = task.status_id
        //                         WHERE task.is_deleted = 0 and task.empid =@key 

        //                UNION

        //                  SELECT
        //                             DISTINCT(task.task_name),
        //					ISNULL(project.project_name, 'NA') as project_name,
        //					ISNULL(project_activity.activity_name, 'NA') as milestone_name,
        //					project.id as project_id,
        //					project_activity.id as milestone_id,
        //                             task.id,
        //                          employee.id as empid,
        //                          task.task_desc,
        //                          priority.priority_name as priority,
        //                          status.id as status_id,
        //                          status.status_name as status_name,
        //                       task.assigned_empid as assigned_to,
        //                          e.full_name as assigned_to_name,
        //                             task.is_approver as is_approver,
        //                             task.is_approver_id as is_approver_id,
        //                       et.full_name as approver_name,
        //                          FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
        //                          task.created_date
        //                         FROM [dbo].[task] WITH (NOLOCK)
        //                        INNER JOIN employee on task.empid = employee.id
        //						LEFT JOIN project_activity_x_task on [dbo].[task].id = project_activity_x_task.task_id
        //						LEFT JOIN project_activity on project_activity_x_task.activity_id = project_activity.id
        //						LEFT JOIN project on project_activity_x_task.project_id = project.id
        //                        LEFT JOIN employee e on task.assigned_empid = e.id
        //                        LEFT JOIN employee et on task.is_approver_id = et.id
        //                        LEFT JOIN priority on task.priority_id = priority.id
        //                        LEFT JOIN status on status.id = task.status_id
        //                         WHERE task.is_deleted = 0 
        //                      AND task.assigned_empid =@key",
        //                         param: new { key }
        //                  );

        //         var _employeeAssignedTasks = Query<EmployeeTasks>(
        //                sql: @"SELECT
        //                             DISTINCT(task.task_name),
        //					ISNULL(project.project_name, 'NA') as project_name,
        //					ISNULL(project_activity.activity_name, 'NA') as milestone_name,
        //					project.id as project_id,
        //                             project_activity.id as milestone_id,
        //                             task.id,
        //                          employee.id as empid,
        //                          task.task_desc,
        //                          priority.priority_name as priority,
        //                          status.id as status_id,
        //                          status.status_name as status_name,
        //                       task.assigned_empid as assigned_to,
        //                          e.full_name as assigned_to_name,
        //                             task.is_approver as is_approver,
        //                             task.is_approver_id as is_approver_id,
        //                       et.full_name as approver_name,
        //                          FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US'),
        //                          task.created_date
        //                         FROM [dbo].[task] WITH (NOLOCK)
        //                        INNER JOIN employee on task.empid = employee.id
        //						LEFT JOIN project_activity_x_task on [dbo].[task].id = project_activity_x_task.task_id
        //						LEFT JOIN project_activity on project_activity_x_task.activity_id = project_activity.id
        //						LEFT JOIN project on project_activity_x_task.project_id = project.id
        //                        LEFT JOIN employee e on task.assigned_empid = e.id
        //                        LEFT JOIN employee et on task.is_approver_id = et.id
        //                        LEFT JOIN priority on task.priority_id = priority.id
        //                        LEFT JOIN status on status.id = task.status_id
        //                         WHERE task.is_deleted = 0 
        //                      AND task.assigned_empid =@key",
        //                   param: new { key }
        //            );

        //var _overdueTasks = Query<EmployeeTasks>(
        //		sql: @"SELECT
        //                             DISTINCT(task.task_name),
        //					ISNULL(project.project_name, 'NA') as project_name,
        //					ISNULL(project_activity.activity_name, 'NA') as milestone_name,
        //					project.id as project_id,
        //                             project_activity.id as milestone_id,
        //                             task.id,
        //                          employee.id as empid,
        //                          task.task_desc,
        //                          priority.priority_name as priority,
        //                          status.id as status_id,
        //                          status.status_name as status_name,
        //                       task.assigned_empid as assigned_to,
        //                          e.full_name as assigned_to_name,
        //                             task.is_approver as is_approver,
        //                             task.is_approver_id as is_approver_id,
        //                       et.full_name as approver_name,
        //                             FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
        //                          task.created_date
        //                          FROM[dbo].[task] WITH (NOLOCK)
        //                        INNER JOIN employee on task.empid = employee.id
        //						LEFT JOIN project_activity_x_task on [dbo].[task].id = project_activity_x_task.task_id
        //						LEFT JOIN project_activity on project_activity_x_task.activity_id = project_activity.id
        //						LEFT JOIN project on project_activity_x_task.project_id = project.id
        //                        LEFT JOIN employee e on task.assigned_empid = e.id
        //                        LEFT JOIN employee et on task.is_approver_id = et.id
        //                        LEFT JOIN priority on task.priority_id = priority.id
        //                        LEFT JOIN status on status.id = task.status_id
        //                         WHERE task.is_deleted = 0 and task.empid =@key 
        //				AND FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') > FORMAT(CAST(@date  AS DATE), 'd', 'EN-US')
        //				AND status_name != 'Completed'

        //		  UNION

        //                  SELECT
        //                             DISTINCT(task.task_name),
        //					ISNULL(project.project_name, 'NA') as project_name,
        //					ISNULL(project_activity.activity_name, 'NA') as milestone_name,
        //					project.id as project_id,
        //                             project_activity.id as milestone_id,
        //                             task.id,
        //                          employee.id as empid,
        //                          task.task_desc,
        //                          priority.priority_name as priority,
        //                          status.id as status_id,
        //                          status.status_name as status_name,
        //                       task.assigned_empid as assigned_to,
        //                          e.full_name as assigned_to_name,
        //                             task.is_approver as is_approver,
        //                             task.is_approver_id as is_approver_id,
        //                       et.full_name as approver_name,
        //                          FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
        //                          task.created_date
        //                         FROM [dbo].[task] WITH (NOLOCK)
        //                        INNER JOIN employee on task.empid = employee.id
        //						LEFT JOIN project_activity_x_task on [dbo].[task].id = project_activity_x_task.task_id
        //						LEFT JOIN project_activity on project_activity_x_task.activity_id = project_activity.id
        //						LEFT JOIN project on project_activity_x_task.project_id = project.id
        //                        LEFT JOIN employee e on task.assigned_empid = e.id
        //                        LEFT JOIN employee et on task.is_approver_id = et.id
        //                        LEFT JOIN priority on task.priority_id = priority.id
        //                        LEFT JOIN status on status.id = task.status_id
        //                         WHERE task.is_deleted = 0 
        //                      AND task.assigned_empid =@key
        //				AND FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') > FORMAT(CAST(@date  AS DATE), 'd', 'EN-US')
        //				AND status_name != 'Completed'",
        //		param: new { key, date }
        //	);

        //rootEmployeeTask.EmployeeTasks = _employeeTasks;
        //rootEmployeeTask.AssignedEmployeeTasks = _employeeAssignedTasks;
        //rootEmployeeTask.OverDueTasks = _overdueTasks;

        //return rootEmployeeTask;
        //     }

        //     public RootEmployeeTask GetAllTaskByOrgAndEmpID(string key, string EmpID)
        //     {
        //         RootEmployeeTask rootEmployeeTask = new RootEmployeeTask();

        //         var _employeeTasks = Query<EmployeeTasks>(
        //                      sql: @"SELECT
        //                                 DISTINCT(task.task_name),
        //                                 task.id,
        //                              employee.id as empid,
        //                              task.task_desc,
        //                              priority.priority_name as priority,
        //                              status.id as status_id,
        //                              status.status_name as status_name,
        //                           task.assigned_empid as assigned_to,
        //                              e.full_name as assigned_to_name,
        //                                 task.is_approver as is_approver,
        //                                 task.is_approver_id as is_approver_id,
        //                           et.full_name as approver_name,
        //                              FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
        //                              task.created_date
        //                              FROM[dbo].[task] WITH (NOLOCK)
        //                            INNER JOIN employee on task.empid = employee.id
        //                            LEFT JOIN employee e on task.assigned_empid = e.id
        //                            LEFT JOIN employee et on task.is_approver_id = et.id
        //                            LEFT JOIN priority on task.priority_id = priority.id
        //                            LEFT JOIN status on status.id = task.status_id
        //                             WHERE task.is_deleted = 0 and task.empid in (select employee.id from 
        //	                organization
        //	                inner join employee on organization.org_id = employee.org_id
        //	                where organization.org_id = @key)  

        //                      UNION

        //                      SELECT
        //                                     DISTINCT(task.task_name),
        //                                 task.id,
        //                              employee.id as empid,
        //                              task.task_desc,
        //                              priority.priority_name as priority,
        //                              status.id as status_id,
        //                              status.status_name as status_name,
        //                           task.assigned_empid as assigned_to,
        //                              e.full_name as assigned_to_name,
        //                                 task.is_approver as is_approver,
        //                                 task.is_approver_id as is_approver_id,
        //                           et.full_name as approver_name,
        //                              FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
        //                              task.created_date
        //                              FROM [dbo].[task] WITH (NOLOCK)
        //                            INNER JOIN employee on task.empid = employee.id
        //                            LEFT JOIN employee e on task.assigned_empid = e.id
        //                            LEFT JOIN employee et on task.is_approver_id = et.id
        //                            LEFT JOIN priority on task.priority_id = priority.id
        //                            LEFT JOIN status on status.id = task.status_id
        //                             WHERE task.is_deleted = 0 
        //                          AND task.assigned_empid in (select employee.id from 
        //	                organization
        //	                inner join employee on organization.org_id = employee.org_id
        //	                where organization.org_id = @key)",
        //                         param: new { key }
        //                  );

        //         var _employeeAssignedTasks = Query<EmployeeTasks>(
        //		   sql: @"SELECT
        //						 DISTINCT(task.task_name),
        //						task.id,
        //						employee.id as empid,
        //						task.task_desc,
        //						priority.priority_name as priority,
        //						status.id as status_id,
        //						status.status_name as status_name,
        //						task.assigned_empid as assigned_to,
        //						e.full_name as assigned_to_name,
        //						task.is_approver as is_approver,
        //						task.is_approver_id as is_approver_id,
        //						et.full_name as approver_name,
        //						FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
        //						task.created_date
        //					   FROM [dbo].[task] WITH (NOLOCK)
        //							INNER JOIN employee on task.empid = employee.id
        //							LEFT JOIN employee e on task.assigned_empid = e.id
        //							LEFT JOIN employee et on task.is_approver_id = et.id
        //							LEFT JOIN priority on task.priority_id = priority.id
        //							LEFT JOIN status on status.id = task.status_id
        //					WHERE task.is_deleted = 0 
        //					AND task.assigned_empid =@EmpID",
        //			  param: new { EmpID }
        //		);


        //         var _overdueTasks = Query<EmployeeTasks>(
        //		sql: @"SELECT
        //					DISTINCT(task.task_name),
        //					task.id,
        //					employee.id as empid,
        //					task.task_desc,
        //					priority.priority_name as priority,
        //					status.id as status_id,
        //					status.status_name as status_name,
        //					task.assigned_empid as assigned_to,
        //					e.full_name as assigned_to_name,
        //					task.is_approver as is_approver,
        //					task.is_approver_id as is_approver_id,
        //					et.full_name as approver_name,
        //					FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
        //					task.created_date
        //					FROM[dbo].[task] WITH (NOLOCK)
        //						INNER JOIN employee on task.empid = employee.id
        //						LEFT JOIN employee e on task.assigned_empid = e.id
        //						LEFT JOIN employee et on task.is_approver_id = et.id
        //						LEFT JOIN priority on task.priority_id = priority.id
        //						LEFT JOIN status on status.id = task.status_id
        //				WHERE task.is_deleted = 0 and task.empid in (select employee.id from 
        //				organization
        //				INNER JOIN employee on organization.org_id = employee.org_id
        //				WHERE organization.org_id = @key
        //				AND FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') < FORMAT(CAST(GETDATE()  AS DATE), 'd', 'EN-US')
        //				and status_name != 'Completed'
        //				)  

        //			UNION

        //			SELECT
        //						DISTINCT(task.task_name),
        //					task.id,
        //					employee.id as empid,
        //					task.task_desc,
        //					priority.priority_name as priority,
        //					status.id as status_id,
        //					status.status_name as status_name,
        //					task.assigned_empid as assigned_to,
        //					e.full_name as assigned_to_name,
        //					task.is_approver as is_approver,
        //					task.is_approver_id as is_approver_id,
        //					et.full_name as approver_name,
        //					FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
        //					task.created_date
        //					FROM [dbo].[task] WITH (NOLOCK)
        //						INNER JOIN employee on task.empid = employee.id
        //						LEFT JOIN employee e on task.assigned_empid = e.id
        //						LEFT JOIN employee et on task.is_approver_id = et.id
        //						LEFT JOIN priority on task.priority_id = priority.id
        //						LEFT JOIN status on status.id = task.status_id
        //				WHERE task.is_deleted = 0 
        //				AND task.assigned_empid in (select employee.id from 
        //				organization
        //				INNER JOIN employee on organization.org_id = employee.org_id
        //				where organization.org_id = @key
        //				AND FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') < FORMAT(CAST(GETDATE()  AS DATE), 'd', 'EN-US')
        //				and status_name != 'Completed')",
        //		  param: new { key }
        //		);

        //         rootEmployeeTask.EmployeeTasks = _employeeTasks;
        //         rootEmployeeTask.AssignedEmployeeTasks = _employeeAssignedTasks;
        //         rootEmployeeTask.OverDueTasks = _overdueTasks;


        //         return rootEmployeeTask;
        //     }
    }
}