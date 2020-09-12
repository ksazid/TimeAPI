using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
                                  (id, empid, task_name, task_desc, priority_id, status_id, assigned_empid, due_date, unit, qty, is_local_activity, created_date, createdby, is_approver, is_approver_id)
                           VALUES (@id, @empid, @task_name, @task_desc, @priority_id, @status_id, @assigned_empid, @due_date, @unit, @qty, @is_local_activity, @created_date, @createdby, @is_approver, @is_approver_id);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public async Task<Tasks> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<Tasks>(
                sql: @"SELECT dbo.project_activity_x_task.project_id as project_id, dbo.project_activity_x_task.activity_id as activtity_id, dbo.task.* 
						FROM dbo.task
						LEFT JOIN dbo.project_activity_x_task on dbo.task.id = dbo.project_activity_x_task.task_id
						WHERE dbo.task.is_deleted = 0 and dbo.task.id = @key",
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
					unit = @unit, 
					qty = @qty, 
					is_local_activity = @is_local_activity,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby,
                    is_approver = @is_approver,
                    is_approver_id = @is_approver_id
                    WHERE id =  @id",
                param: entity
            );
        }

        public async Task<IEnumerable<Tasks>> All()
        {
            return await QueryAsync<Tasks>(
                sql: "SELECT * FROM [dbo].[task] where is_deleted = 0"
            );
        }

        public async Task<dynamic> FindByTaskDetailsByEmpID(string key)
        {
            return await QueryAsync<dynamic>(
                   sql: @"SELECT
								DISTINCT(task.task_name),
								task.id,
								CASE 
								WHEN project.project_name IS NULL AND project_quotation.project_name IS NULL AND cost_project.project_name IS NULL and lead_deal.deal_name IS NULL then null
								WHEN project.project_name IS NULL AND project_quotation.project_name IS NULL AND cost_project.project_name IS NULL  then lead_deal.deal_name 
								WHEN lead_deal.deal_name IS NULL AND project_quotation.project_name IS NULL AND cost_project.project_name IS NULL  then project.project_name
								WHEN lead_deal.deal_name IS NULL AND project.project_name IS NULL AND cost_project.project_name IS NULL  then project_quotation.project_name
								WHEN lead_deal.deal_name IS NULL AND project.project_name IS NULL AND project_quotation.project_name IS NULL  then cost_project.project_name
								ELSE null
								END AS project_name,
								CASE 
								WHEN project.project_prefix IS NULL AND project_quotation.quotation_prefix IS NULL AND cost_project.project_prefix IS NULL  then lead_deal.deal_prefix 
								WHEN lead_deal.deal_prefix IS NULL AND project_quotation.quotation_prefix IS NULL AND cost_project.project_prefix IS NULL  then project.project_prefix
								WHEN lead_deal.deal_prefix IS NULL AND project.project_prefix IS NULL AND cost_project.project_prefix IS NULL  then project_quotation.quotation_prefix
								WHEN lead_deal.deal_prefix IS NULL AND project.project_prefix IS NULL AND project_quotation.quotation_prefix IS NULL  then cost_project.project_prefix
								ELSE null
								END AS project_prefix,
								CASE 
								WHEN project.project_prefix IS NULL AND project_quotation.quotation_prefix IS NULL AND cost_project.project_prefix IS NULL and lead_deal.deal_prefix is null then null
								WHEN project.project_prefix IS NULL AND project_quotation.quotation_prefix IS NULL AND cost_project.project_prefix IS NULL then 'Lead'
								WHEN lead_deal.deal_prefix IS NULL AND project_quotation.quotation_prefix IS NULL AND cost_project.project_prefix IS NULL  then 'Project'
								WHEN lead_deal.deal_prefix IS NULL AND project.project_prefix IS NULL AND cost_project.project_prefix IS NULL  then 'Quotation'
								WHEN lead_deal.deal_prefix IS NULL AND project.project_prefix IS NULL AND project_quotation.quotation_prefix IS NULL  then 'Estimation'
								ELSE null
								END AS project_type,
								task.is_local_activity,
								employee.id as empid,
								task.task_desc,
								priority.priority_name as priority,
								status.id as status_id,
								status.status_name as status_name,
								task.assigned_empid as assigned_to,
								e.full_name as assigned_to_name,
								task.is_approver as is_approver,
								task.is_approver_id as is_approver_id,
								et.full_name as approver_name,
								FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
								task.created_date
								FROM[dbo].[task] WITH (NOLOCK)
									INNER JOIN employee on task.empid = employee.id
									LEFT JOIN project_activity_x_task on task.id = project_activity_x_task.task_id
									LEFT JOIN project on project_activity_x_task.project_id = project.id
									LEFT JOIN lead on project_activity_x_task.project_id = lead.id
									LEFT JOIN lead_deal on lead.id = lead_deal.lead_id
									LEFT JOIN cost_project on project_activity_x_task.project_id = cost_project.id
									LEFT JOIN project_quotation on project_activity_x_task.project_id = project_quotation.id
									LEFT JOIN employee e on task.assigned_empid = e.id
									LEFT JOIN employee et on task.is_approver_id = et.id
									LEFT JOIN priority on task.priority_id = priority.id
									LEFT JOIN status on status.id = task.status_id
							WHERE task.is_deleted = 0 
							AND task.empid =@key
 
	
						UNION

						SELECT
								DISTINCT(task.task_name),
								task.id,
								CASE 
								WHEN project.project_name IS NULL AND project_quotation.project_name IS NULL AND cost_project.project_name IS NULL and lead_deal.deal_name IS NULL then null
								WHEN project.project_name IS NULL AND project_quotation.project_name IS NULL AND cost_project.project_name IS NULL  then lead_deal.deal_name 
								WHEN lead_deal.deal_name IS NULL AND project_quotation.project_name IS NULL AND cost_project.project_name IS NULL  then project.project_name
								WHEN lead_deal.deal_name IS NULL AND project.project_name IS NULL AND cost_project.project_name IS NULL  then project_quotation.project_name
								WHEN lead_deal.deal_name IS NULL AND project.project_name IS NULL AND project_quotation.project_name IS NULL  then cost_project.project_name
								ELSE null
								END AS project_name,
								CASE 
								WHEN project.project_prefix IS NULL AND project_quotation.quotation_prefix IS NULL AND cost_project.project_prefix IS NULL  then lead_deal.deal_prefix 
								WHEN lead_deal.deal_prefix IS NULL AND project_quotation.quotation_prefix IS NULL AND cost_project.project_prefix IS NULL  then project.project_prefix
								WHEN lead_deal.deal_prefix IS NULL AND project.project_prefix IS NULL AND cost_project.project_prefix IS NULL  then project_quotation.quotation_prefix
								WHEN lead_deal.deal_prefix IS NULL AND project.project_prefix IS NULL AND project_quotation.quotation_prefix IS NULL  then cost_project.project_prefix
								ELSE null
								END AS project_prefix,
								CASE 
								WHEN project.project_prefix IS NULL AND project_quotation.quotation_prefix IS NULL AND cost_project.project_prefix IS NULL and lead_deal.deal_prefix is null then null
								WHEN project.project_prefix IS NULL AND project_quotation.quotation_prefix IS NULL AND cost_project.project_prefix IS NULL then 'Lead'
								WHEN lead_deal.deal_prefix IS NULL AND project_quotation.quotation_prefix IS NULL AND cost_project.project_prefix IS NULL  then 'Project'
								WHEN lead_deal.deal_prefix IS NULL AND project.project_prefix IS NULL AND cost_project.project_prefix IS NULL  then 'Quotation'
								WHEN lead_deal.deal_prefix IS NULL AND project.project_prefix IS NULL AND project_quotation.quotation_prefix IS NULL  then 'Estimation'
								ELSE null
								END AS project_type,
								task.is_local_activity,
								employee.id as empid,
								task.task_desc,
								priority.priority_name as priority,
								status.id as status_id,
								status.status_name as status_name,
								task.assigned_empid as assigned_to,
								e.full_name as assigned_to_name,
								task.is_approver as is_approver,
								task.is_approver_id as is_approver_id,
								et.full_name as approver_name,
								FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
								task.created_date
								FROM [dbo].[task] WITH (NOLOCK)
									INNER JOIN employee on task.empid = employee.id
									LEFT JOIN project_activity_x_task on task.id = project_activity_x_task.task_id
									LEFT JOIN project on project_activity_x_task.project_id = project.id
									LEFT JOIN lead on project_activity_x_task.project_id = lead.id
									LEFT JOIN lead_deal on lead.id = lead_deal.lead_id
									LEFT JOIN cost_project on project_activity_x_task.project_id = cost_project.id
									LEFT JOIN project_quotation on project_activity_x_task.project_id = project_quotation.id
									LEFT JOIN employee e on task.assigned_empid = e.id
									LEFT JOIN employee et on task.is_approver_id = et.id
									LEFT JOIN priority on task.priority_id = priority.id
									LEFT JOIN status on status.id = task.status_id
							WHERE task.is_deleted = 0 
							AND task.assigned_empid =@key",
                param: new { key }
               );
        }

		public async Task UpdateTaskStatus(Tasks entity)
        {
           await ExecuteAsync(
                sql: @"UPDATE dbo.task
                   SET
                    status_id = @status_id,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id =  @id",
                param: entity
            );
        }

        public async Task<RootEmployeeTask> GetAllTaskByEmpID(string key, string date)
        {
            RootEmployeeTask rootEmployeeTask = new RootEmployeeTask();

            var _employeeTasks = await QueryAsync<EmployeeTasks>(
                         sql: @"SELECT
									DISTINCT(task.task_name),
									ISNULL(project.project_name, 'NA') as project_name,
									ISNULL(project_activity.activity_name, 'NA') as milestone_name,
									project.id as project_id,
									project_activity.id as milestone_id,
									task.id,
									task.is_local_activity,
									employee.id as empid,
									task.task_desc,
									priority.priority_name as priority,
									status.id as status_id,
									status.status_name as status_name,
									task.assigned_empid as assigned_to,
									e.full_name as assigned_to_name,
									task.is_approver as is_approver,
									task.is_approver_id as is_approver_id,
									et.full_name as approver_name,
									FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
									task.created_date
									FROM[dbo].[task] WITH (NOLOCK)
										LEFT JOIN employee on task.empid = employee.id
										left join task_team_members on [dbo].[task].id = task_team_members.task_id
										LEFT JOIN project_activity_x_task on [dbo].[task].id = project_activity_x_task.task_id
										LEFT JOIN project_activity on project_activity_x_task.activity_id = project_activity.id
										LEFT JOIN project on project_activity_x_task.project_id = project.id
										LEFT JOIN employee e on task.assigned_empid = e.id
										LEFT JOIN employee et on task.is_approver_id = et.id
										LEFT JOIN priority on task.priority_id = priority.id
										LEFT JOIN status on status.id = task.status_id
								WHERE task.is_deleted = 0 and task_team_members.empid = @key
								AND status.status_name != 'Completed'
	
							UNION

							SELECT
									DISTINCT(task.task_name),
									ISNULL(project.project_name, 'NA') as project_name,
									ISNULL(project_activity.activity_name, 'NA') as milestone_name,
									project.id as project_id,
									project_activity.id as milestone_id,
									task.id,
									task.is_local_activity,
									employee.id as empid,
									task.task_desc,
									priority.priority_name as priority,
									status.id as status_id,
									status.status_name as status_name,
									task.assigned_empid as assigned_to,
									e.full_name as assigned_to_name,
									task.is_approver as is_approver,
									task.is_approver_id as is_approver_id,
									et.full_name as approver_name,
									FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
									task.created_date
									FROM [dbo].[task] WITH (NOLOCK)
										LEFT JOIN employee on task.empid = employee.id
										LEFT JOIN task_team_members on [dbo].[task].id = task_team_members.task_id
										LEFT JOIN project_activity_x_task on [dbo].[task].id = project_activity_x_task.task_id
										LEFT JOIN project_activity on project_activity_x_task.activity_id = project_activity.id
										LEFT JOIN project on project_activity_x_task.project_id = project.id
										LEFT JOIN employee e on task.assigned_empid = e.id
										LEFT JOIN employee et on task.is_approver_id = et.id
										LEFT JOIN priority on task.priority_id = priority.id
										LEFT JOIN status on status.id = task.status_id
								WHERE task.is_deleted = 0 
								AND task.empid =@key
								AND status.status_name != 'Completed'


							UNION 

							SELECT
									DISTINCT(sub_task.sub_task_name),
									ISNULL(project.project_name, 'NA') as project_name,
									ISNULL(project_activity.activity_name, 'NA') as milestone_name,
									project.id as project_id,
									project_activity.id as milestone_id,
									sub_task.id,
									task.is_local_activity,
									employee.id as empid,
									sub_task.sub_task_desc,
									priority.priority_name as priority,
									status.id as status_id,
									status.status_name as status_name,
									sub_task.lead_id as assigned_to,
									e.full_name as assigned_to_name,
									sub_task.is_approver as is_approver,
									sub_task.is_approver_id as is_approver_id,
									et.full_name as approver_name,
									FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
									task.created_date
									FROM dbo.sub_task WITH (NOLOCK)
										LEFT JOIN task on sub_task.task_id =  task.id 
										LEFT JOIN employee on task.empid = employee.id
										LEFT JOIN task_team_members on [dbo].[task].id = task_team_members.task_id
										LEFT JOIN project_activity_x_task on [dbo].[task].id = project_activity_x_task.task_id
										LEFT JOIN project_activity on project_activity_x_task.activity_id = project_activity.id
										LEFT JOIN project on project_activity_x_task.project_id = project.id
										LEFT JOIN employee e on sub_task.lead_id = e.id
										LEFT JOIN employee et on sub_task.is_approver_id = et.id
										LEFT JOIN priority on sub_task.priority_id = priority.id
										LEFT JOIN status on status.id = sub_task.status_id
								WHERE task.is_deleted = 0 
								AND task_team_members.empid =@key
								AND status.status_name != 'Completed'",
                            param: new { key }
                     );

            var _employeeAssignedTasks = await QueryAsync<EmployeeTasks>(
                   sql: @"SELECT
                                DISTINCT(task.task_name),
								ISNULL(project.project_name, 'NA') as project_name,
								ISNULL(project_activity.activity_name, 'NA') as milestone_name,
								project.id as project_id,
                                project_activity.id as milestone_id,
                                task.id,
								task.is_local_activity,
	                            employee.id as empid,
	                            task.task_desc,
	                            priority.priority_name as priority,
	                            status.id as status_id,
	                            status.status_name as status_name,
		                        task.assigned_empid as assigned_to,
	                            e.full_name as assigned_to_name,
                                task.is_approver as is_approver,
                                task.is_approver_id as is_approver_id,
		                        et.full_name as approver_name,
	                            FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US'),
	                            task.created_date
	                           FROM [dbo].[task] WITH (NOLOCK)
			                        LEFT JOIN employee on task.empid = employee.id
									LEFT JOIN task_team_members on [dbo].[task].id = task_team_members.task_id
									LEFT JOIN project_activity_x_task on [dbo].[task].id = project_activity_x_task.task_id
									LEFT JOIN project_activity on project_activity_x_task.activity_id = project_activity.id
									LEFT JOIN project on project_activity_x_task.project_id = project.id
			                        LEFT JOIN employee e on task.assigned_empid = e.id
			                        LEFT JOIN employee et on task.is_approver_id = et.id
			                        LEFT JOIN priority on task.priority_id = priority.id
			                        LEFT JOIN status on status.id = task.status_id
                            WHERE task.is_deleted = 0 
	                        AND task_team_members.empid =@key
							AND status.status_name != 'Completed'


						UNION 


						SELECT
                                DISTINCT(sub_task.sub_task_name),
								ISNULL(project.project_name, 'NA') as project_name,
								ISNULL(project_activity.activity_name, 'NA') as milestone_name,
								project.id as project_id,
                                project_activity.id as milestone_id,
                                sub_task.id,
								task.is_local_activity,
	                            employee.id as empid,
	                            sub_task.sub_task_desc,
	                            priority.priority_name as priority,
	                            status.id as status_id,
	                            status.status_name as status_name,
		                        sub_task.lead_id as assigned_to,
	                            e.full_name as assigned_to_name,
                                sub_task.is_approver as is_approver,
                                sub_task.is_approver_id as is_approver_id,
		                        et.full_name as approver_name,
	                            FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US'),
	                            task.created_date
	                           FROM [dbo].[sub_task] WITH (NOLOCK)
			                        LEFT JOIN task on sub_task.task_id = task.id
			                        LEFT JOIN employee on task.empid = employee.id
									LEFT JOIN task_team_members on [dbo].[sub_task].id = task_team_members.task_id
									LEFT JOIN project_activity_x_task on [dbo].[task].id = project_activity_x_task.task_id
									LEFT JOIN project_activity on project_activity_x_task.activity_id = project_activity.id
									LEFT JOIN project on project_activity_x_task.project_id = project.id
			                        LEFT JOIN employee e on sub_task.lead_id = e.id
			                        LEFT JOIN employee et on sub_task.is_approver_id = et.id
			                        LEFT JOIN priority on sub_task.priority_id = priority.id
			                        LEFT JOIN status on status.id = sub_task.status_id
                            WHERE task.is_deleted = 0 
	                        AND task_team_members.empid =@key
							AND status.status_name != 'Completed'",
                      param: new { key }
               );

            var _overdueTasks = await QueryAsync<EmployeeTasks>(
                    sql: @"SELECT
                                DISTINCT(task.task_name),
								ISNULL(project.project_name, 'NA') as project_name,
								ISNULL(project_activity.activity_name, 'NA') as milestone_name,
								project.id as project_id,
                                project_activity.id as milestone_id,
                                task.id,
								task.is_local_activity,
	                            employee.id as empid,
	                            task.task_desc,
	                            priority.priority_name as priority,
	                            status.id as status_id,
	                            status.status_name as status_name,
		                        task.assigned_empid as assigned_to,
	                            e.full_name as assigned_to_name,
                                task.is_approver as is_approver,
                                task.is_approver_id as is_approver_id,
		                        et.full_name as approver_name,
                                FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
	                            task.created_date
	                            FROM[dbo].[task] WITH (NOLOCK)
			                        LEFT JOIN employee on task.empid = employee.id
									left join task_team_members on [dbo].[task].id = task_team_members.task_id
									LEFT JOIN project_activity_x_task on [dbo].[task].id = project_activity_x_task.task_id
									LEFT JOIN project_activity on project_activity_x_task.activity_id = project_activity.id
									LEFT JOIN project on project_activity_x_task.project_id = project.id
			                        LEFT JOIN employee e on task.assigned_empid = e.id
			                        LEFT JOIN employee et on task.is_approver_id = et.id
			                        LEFT JOIN priority on task.priority_id = priority.id
			                        LEFT JOIN status on status.id = task.status_id
                            WHERE task.is_deleted = 0 and employee.id =@key
							AND FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') < FORMAT(CAST(@date  AS DATE), 'd', 'EN-US')
							AND status_name != 'Completed'

					  UNION

	                    SELECT
                                DISTINCT(task.task_name),
								ISNULL(project.project_name, 'NA') as project_name,
								ISNULL(project_activity.activity_name, 'NA') as milestone_name,
								project.id as project_id,
                                project_activity.id as milestone_id,
                                task.id,
								task.is_local_activity,
	                            employee.id as empid,
	                            task.task_desc,
	                            priority.priority_name as priority,
	                            status.id as status_id,
	                            status.status_name as status_name,
		                        task.assigned_empid as assigned_to,
	                            e.full_name as assigned_to_name,
                                task.is_approver as is_approver,
                                task.is_approver_id as is_approver_id,
		                        et.full_name as approver_name,
	                            FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
	                            task.created_date
	                           FROM [dbo].[task] WITH (NOLOCK)
			                        left JOIN employee on task.empid = employee.id
									left join task_team_members on [dbo].[task].id = task_team_members.task_id
									LEFT JOIN project_activity_x_task on [dbo].[task].id = project_activity_x_task.task_id
									LEFT JOIN project_activity on project_activity_x_task.activity_id = project_activity.id
									LEFT JOIN project on project_activity_x_task.project_id = project.id
			                        LEFT JOIN employee e on task.assigned_empid = e.id
			                        LEFT JOIN employee et on task.is_approver_id = et.id
			                        LEFT JOIN priority on task.priority_id = priority.id
			                        LEFT JOIN status on status.id = task.status_id
                            WHERE task.is_deleted = 0 
	                        AND employee.id =@key
							AND FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') < FORMAT(CAST(@date  AS DATE), 'd', 'EN-US')
							AND status_name != 'Completed'

						UNION

						SELECT
                                DISTINCT(sub_task.sub_task_name),
								ISNULL(project.project_name, 'NA') as project_name,
								ISNULL(project_activity.activity_name, 'NA') as milestone_name,
								project.id as project_id,
                                project_activity.id as milestone_id,
                                sub_task.id,
								task.is_local_activity,
	                            employee.id as empid,
	                            sub_task.sub_task_desc,
	                            priority.priority_name as priority,
	                            status.id as status_id,
	                            status.status_name as status_name,
		                        sub_task.lead_id as assigned_to,
	                            e.full_name as assigned_to_name,
                                sub_task.is_approver as is_approver,
                                sub_task.is_approver_id as is_approver_id,
		                        et.full_name as approver_name,
	                            FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US'),
	                            task.created_date
	                           FROM [dbo].[sub_task] WITH (NOLOCK)
			                        LEFT JOIN task on sub_task.task_id = task.id
			                        LEFT JOIN employee on task.empid = employee.id
									LEFT JOIN task_team_members on [dbo].[sub_task].id = task_team_members.task_id
									LEFT JOIN project_activity_x_task on [dbo].[task].id = project_activity_x_task.task_id
									LEFT JOIN project_activity on project_activity_x_task.activity_id = project_activity.id
									LEFT JOIN project on project_activity_x_task.project_id = project.id
			                        LEFT JOIN employee e on sub_task.lead_id = e.id
			                        LEFT JOIN employee et on sub_task.is_approver_id = et.id
			                        LEFT JOIN priority on sub_task.priority_id = priority.id
			                        LEFT JOIN status on status.id = sub_task.status_id
                            WHERE task.is_deleted = 0 
	                        AND task_team_members.empid =@key
							AND status.status_name != 'Completed'",
                    param: new { key, date }
                );

            rootEmployeeTask.EmployeeTasks = _employeeTasks.OrderByDescending(x => x.due_date);
            rootEmployeeTask.AssignedEmployeeTasks = _employeeAssignedTasks.OrderByDescending(x => x.due_date);
            rootEmployeeTask.OverDueTasks = _overdueTasks.OrderByDescending(x => x.due_date);

            return rootEmployeeTask;
        }

        public async Task<RootEmployeeTask> GetAllTaskByOrgAndEmpID(string key, string EmpID)
        {
            RootEmployeeTask rootEmployeeTask = new RootEmployeeTask();

            var _employeeTasks = await QueryAsync<EmployeeTasks>(
                         sql: @"SELECT
                                    DISTINCT(task.task_name),
									project_activity.activity_name as milestone_name,
                                    task.id,
									task.is_local_activity,
	                                employee.id as empid,
	                                task.task_desc,
	                                priority.priority_name as priority,
	                                status.id as status_id,
	                                status.status_name as status_name,
		                            task.assigned_empid as assigned_to,
	                                e.full_name as assigned_to_name,
                                    task.is_approver as is_approver,
                                    task.is_approver_id as is_approver_id,
		                            et.full_name as approver_name,
	                                FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
	                                task.created_date
	                                FROM [dbo].[task] WITH (NOLOCK)
										
			                            INNER JOIN employee on task.empid = employee.id
										LEFT JOIN project_activity_x_task ON task.id = project_activity_x_task.task_id
										LEFT JOIN project_activity ON project_activity_x_task.activity_id = project_activity.id
			                            LEFT JOIN employee e on task.assigned_empid = e.id
			                            LEFT JOIN employee et on task.is_approver_id = et.id
			                            LEFT JOIN priority on task.priority_id = priority.id
			                            LEFT JOIN status on status.id = task.status_id
                                WHERE task.is_deleted = 0 and task.empid in (select employee.id from 
				                organization
				                inner join employee on organization.org_id = employee.org_id
				                where organization.org_id = @key)  
	
	                        UNION

	                        SELECT
                                        DISTINCT(task.task_name),
									project_activity.activity_name as milestone_name,
                                    task.id,
									task.is_local_activity,
	                                employee.id as empid,
	                                task.task_desc,
	                                priority.priority_name as priority,
	                                status.id as status_id,
	                                status.status_name as status_name,
		                            task.assigned_empid as assigned_to,
	                                e.full_name as assigned_to_name,
                                    task.is_approver as is_approver,
                                    task.is_approver_id as is_approver_id,
		                            et.full_name as approver_name,
	                                FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
	                                task.created_date
	                                FROM [dbo].[task] WITH (NOLOCK)
			                            INNER JOIN employee on task.empid = employee.id
										LEFT JOIN project_activity_x_task ON task.id = project_activity_x_task.task_id
										LEFT JOIN project_activity ON project_activity_x_task.activity_id = project_activity.id
			                            LEFT JOIN employee e on task.assigned_empid = e.id
			                            LEFT JOIN employee et on task.is_approver_id = et.id
			                            LEFT JOIN priority on task.priority_id = priority.id
			                            LEFT JOIN status on status.id = task.status_id
                                WHERE task.is_deleted = 0 
	                            AND task.assigned_empid in (select employee.id from 
				                organization
				                inner join employee on organization.org_id = employee.org_id
				                where organization.org_id = @key)",
                            param: new { key }
                     );

            var _employeeAssignedTasks = await QueryAsync<EmployeeTasks>(
                       sql: @"SELECT
									 DISTINCT(task.task_name),
										project_activity.activity_name as milestone_name,
									task.id,
									task.is_local_activity,
									employee.id as empid,
									task.task_desc,
									priority.priority_name as priority,
									status.id as status_id,
									status.status_name as status_name,
									task.assigned_empid as assigned_to,
									e.full_name as assigned_to_name,
									task.is_approver as is_approver,
									task.is_approver_id as is_approver_id,
									et.full_name as approver_name,
									FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
									task.created_date
								   FROM [dbo].[task] WITH (NOLOCK)
										INNER JOIN employee on task.empid = employee.id
										LEFT JOIN project_activity_x_task ON task.id = project_activity_x_task.task_id
										LEFT JOIN project_activity ON project_activity_x_task.activity_id = project_activity.id
										LEFT JOIN employee e on task.assigned_empid = e.id
										LEFT JOIN employee et on task.is_approver_id = et.id
										LEFT JOIN priority on task.priority_id = priority.id
										LEFT JOIN status on status.id = task.status_id
								WHERE task.is_deleted = 0 
								AND task.assigned_empid =@EmpID",
                          param: new { EmpID }
                    );


            var _overdueTasks = await QueryAsync<EmployeeTasks>(
                    sql: @"SELECT
								DISTINCT(task.task_name),
								project_activity.activity_name as milestone_name,
								task.id,
								task.is_local_activity,
								employee.id as empid,
								task.task_desc,
								priority.priority_name as priority,
								status.id as status_id,
								status.status_name as status_name,
								task.assigned_empid as assigned_to,
								e.full_name as assigned_to_name,
								task.is_approver as is_approver,
								task.is_approver_id as is_approver_id,
								et.full_name as approver_name,
								FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
								task.created_date
								FROM[dbo].[task] WITH (NOLOCK)
									INNER JOIN employee on task.empid = employee.id
										LEFT JOIN project_activity_x_task ON task.id = project_activity_x_task.task_id
										LEFT JOIN project_activity ON project_activity_x_task.activity_id = project_activity.id
									LEFT JOIN employee e on task.assigned_empid = e.id
									LEFT JOIN employee et on task.is_approver_id = et.id
									LEFT JOIN priority on task.priority_id = priority.id
									LEFT JOIN status on status.id = task.status_id
							WHERE task.is_deleted = 0 and task.empid in (select employee.id from 
							organization
							INNER JOIN employee on organization.org_id = employee.org_id
							WHERE organization.org_id = @key
							AND FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') < FORMAT(CAST(GETDATE()  AS DATE), 'd', 'EN-US')
							and status_name != 'Completed'
							)  
	
						UNION

						SELECT
								DISTINCT(task.task_name),
								project_activity.activity_name as milestone_name,
								task.id,
								task.is_local_activity,
								employee.id as empid,
								task.task_desc,
								priority.priority_name as priority,
								status.id as status_id,
								status.status_name as status_name,
								task.assigned_empid as assigned_to,
								e.full_name as assigned_to_name,
								task.is_approver as is_approver,
								task.is_approver_id as is_approver_id,
								et.full_name as approver_name,
								FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') as due_date,
								task.created_date
								FROM [dbo].[task] WITH (NOLOCK)
									INNER JOIN employee on task.empid = employee.id
									LEFT JOIN project_activity_x_task ON task.id = project_activity_x_task.task_id
									LEFT JOIN project_activity ON project_activity_x_task.activity_id = project_activity.id
									LEFT JOIN employee e on task.assigned_empid = e.id
									LEFT JOIN employee et on task.is_approver_id = et.id
									LEFT JOIN priority on task.priority_id = priority.id
									LEFT JOIN status on status.id = task.status_id
							WHERE task.is_deleted = 0 
							AND task.assigned_empid in (select employee.id from 
							organization
							INNER JOIN employee on organization.org_id = employee.org_id
							where organization.org_id = @key
							AND FORMAT(CAST(task.due_date  AS DATE), 'd', 'EN-US') < FORMAT(CAST(GETDATE()  AS DATE), 'd', 'EN-US')
							and status_name != 'Completed')",
                      param: new { key }
                    );

            rootEmployeeTask.EmployeeTasks = _employeeTasks;
            rootEmployeeTask.AssignedEmployeeTasks = _employeeAssignedTasks;
            rootEmployeeTask.OverDueTasks = _overdueTasks;

            return rootEmployeeTask;
        }
    }
}