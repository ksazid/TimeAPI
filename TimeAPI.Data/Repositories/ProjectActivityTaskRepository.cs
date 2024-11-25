﻿using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class ProjectActivityTaskRepository : RepositoryBase, IProjectActivityTaskRepository
    {
        public ProjectActivityTaskRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(ProjectActivityTask entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.project_activity_x_task
                                  (id, project_id, activity_id, task_id, created_date, createdby)
                           VALUES (@id, @project_id, @activity_id, @task_id, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public async Task<ProjectActivityTask> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<ProjectActivityTask>(
                sql: "SELECT * FROM dbo.project_activity_x_task WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.project_activity_x_task
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE task_id = @key",
                param: new { key }
            );
        }

        public void Update(ProjectActivityTask entity)
        {
            Execute(
                sql: @"UPDATE dbo.project_activity_x_task
                   SET
                    project_id = @project_id,
                    activity_id = @activity_id,
                    task_id = @task_id,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public async Task<IEnumerable<ProjectActivityTask>> All()
        {
            return await QueryAsync<ProjectActivityTask>(
                sql: "SELECT * FROM [dbo].[project_activity_x_task] where is_deleted = 0"
            );
        }

        public async Task RemoveByProjectActivityID(string key)
        {
            await ExecuteAsync(
                      sql: @"UPDATE dbo.project_activity_x_task
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE activity_id = @key",
                param: new { key }
            );
        }

        public async Task RemoveByProjectID(string key)
        {
            await ExecuteAsync(
                  sql: @"UPDATE dbo.project_activity_x_task
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE project_id = @key",
                  param: new { key }
              );
        }

        public async Task<IEnumerable<dynamic>> GetAllTaskByActivityID(string key)
        {
            return await QueryAsync<dynamic>(
                   sql: @"SELECT
                            dbo.task.id,
							project_activity.activity_name,
                            dbo.task.task_name,
                            dbo.task.due_date,
							dbo.task.task_desc,
							priority.priority_name,
							status.status_name,
			                dbo.task.assigned_empid,
			                dbo.task.status_id
                        FROM dbo.task WITH(NOLOCK)
                        INNER JOIN  dbo.project_activity_x_task on task.id = dbo.project_activity_x_task.task_id
                        INNER JOIN  dbo.project_activity  on project_activity_x_task.activity_id = dbo.project_activity.id
                        LEFT JOIN priority on task.priority_id = priority.id
						LEFT JOIN status on task.status_id = status.id
                        WHERE dbo.project_activity_x_task.activity_id = @key
                        AND dbo.task.is_deleted = 0
                        ORDER BY dbo.task.task_name DESC",
                      param: new { key }
               );
        }

        public async Task<IEnumerable<dynamic>> GetAllTaskByProjectID(string key)
        {
            return await QueryAsync<dynamic>(
                   sql: @"SELECT 
                            dbo.task.id as task_id,
                            dbo.task.task_name,
                            FORMAT(CAST(dbo.task.due_date AS DATE), 'd', 'EN-US') as due_date,
                            dbo.project_activity.id as project_activity_id,
                            dbo.project_activity.activity_name,
                            task.assigned_empid as assigned_empid,
                            x.first_name as assignedto,
                            dbo.priority.priority_name,
                            dbo.status.status_name,
                            dbo.status.id as status_id
                        FROM dbo.task WITH (NOLOCK)
                            INNER JOIN dbo.project_activity_x_task on task.id = dbo.project_activity_x_task.task_id
                            INNER JOIN dbo.project_activity on [project_activity_x_task].activity_id = dbo.project_activity.id
                            INNER JOIN dbo.project on [project_activity_x_task].project_id = dbo.project.id
                            LEFT JOIN dbo.employee x on task.assigned_empid = x.id
                            LEFT JOIN dbo.priority on task.priority_id = dbo.priority.id
                            INNER JOIN dbo.status on task.status_id = dbo.status.id
                        WHERE project.id = @key
                            AND dbo.project.is_deleted = 0
                            AND dbo.project_activity.is_deleted = 0
                            AND dbo.task.is_deleted = 0
                        ORDER BY dbo.task.task_name ASC",
                      param: new { key }
               );
        }

        public async Task<IEnumerable<ProjectActivityTaskEntityViewModel>> GetAllTaskForAssignByProjectID(string key)
        {
            return await QueryAsync<ProjectActivityTaskEntityViewModel>(
                   sql: @"SELECT 
                            dbo.project_activity_x_task.id as id,
                            project.id as project_id,
                            dbo.project_activity.id as activity_id,
                            dbo.task.id as task_id,
                            dbo.task.task_name,
                            FORMAT(CAST(dbo.task.due_date AS DATE), 'd', 'EN-US') as due_date,
                            dbo.project_activity.activity_name,
                            task.assigned_empid as assigned_empid,
                            x.first_name as assignedto,
                            dbo.priority.priority_name,
                            dbo.status.status_name,
                            dbo.status.id as status_id
                        FROM dbo.task WITH (NOLOCK)
                            INNER JOIN dbo.project_activity_x_task on task.id = dbo.project_activity_x_task.task_id
                            INNER JOIN dbo.project_activity on [project_activity_x_task].activity_id = dbo.project_activity.id
                            INNER JOIN dbo.project on [project_activity_x_task].project_id = dbo.project.id
                            LEFT JOIN dbo.employee x on task.assigned_empid = x.id
                            LEFT JOIN dbo.priority on task.priority_id = dbo.priority.id
                            INNER JOIN dbo.status on task.status_id = dbo.status.id
                        WHERE project.id = @key
                            AND dbo.project.is_deleted = 0
                            AND dbo.project_activity.is_deleted = 0
                            AND dbo.task.is_deleted = 0
                        ORDER BY dbo.task.task_name ASC",
                      param: new { key }
               );
        }

        public async Task<IEnumerable<ProjectSubTaskEntityViewModel>> GetAllSubTaskByTaskID(string key)
        {
            return await QueryAsync<ProjectSubTaskEntityViewModel>(
                sql: @"SELECT 
                            dbo.sub_task.id as id,
                            project.id as project_id,
                            dbo.project_activity.id as activity_id,
                            dbo.task.id as task_id,
							dbo.project_activity.activity_name as milestone_name,
                            dbo.task.task_name as task_name,
                            dbo.sub_task.sub_task_name,
                            FORMAT(CAST(dbo.sub_task.due_date AS DATE), 'd', 'EN-US') as due_date,
                            sub_task.lead_id as lead_id,
                            x.first_name as lead_name,
                            dbo.priority.priority_name,
                            dbo.status.status_name,
                            dbo.status.id as status_id
                        FROM dbo.sub_task WITH (NOLOCK)
							LEFT JOIN dbo.task on sub_task.task_id = dbo.task.id
                            LEFT JOIN dbo.project_activity_x_task on task.id = dbo.project_activity_x_task.task_id
                            LEFT JOIN dbo.project_activity on [project_activity_x_task].activity_id = dbo.project_activity.id
                            LEFT JOIN dbo.project on [project_activity_x_task].project_id = dbo.project.id
                            LEFT JOIN dbo.employee x on sub_task.lead_id = x.id
                            LEFT JOIN dbo.priority on sub_task.priority_id = dbo.priority.id
                            LEFT JOIN dbo.status on sub_task.status_id = dbo.status.id
                        WHERE dbo.task.id = @key
                            AND dbo.project.is_deleted = 0
                            AND dbo.project_activity.is_deleted = 0
                            AND dbo.task.is_deleted = 0
                        ORDER BY dbo.task.task_name ASC",
                param: new { key }
            );
        }

        public async Task<dynamic> GetProjectActivityTaskRatioByProjectID(string key)
        {
            return await QueryAsync<dynamic>(
                   sql: @"SELECT dbo.status.status_name, count(*) * 100 / sum(count(*))  over() as ratio
                        FROM dbo.task WITH(NOLOCK)
                            INNER JOIN dbo.project_activity_x_task on dbo.task.id = dbo.project_activity_x_task.task_id
                            INNER JOIN dbo.project_activity on  dbo.project_activity_x_task.activity_id = dbo.project_activity.id
                            INNER JOIN dbo.project on dbo.project_activity_x_task.project_id = dbo.project.id
                            INNER JOIN dbo.status on dbo.task.status_id = dbo.status.id
                        WHERE dbo.project_activity.project_id = @key
                            AND dbo.project_activity.is_deleted = 0 
                            AND dbo.task.is_deleted = 0 
                        GROUP BY dbo.status.status_name",
                      param: new { key }
               );
        }
    }
}