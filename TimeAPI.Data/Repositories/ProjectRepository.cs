using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class ProjectRepository : RepositoryBase, IProjectRepository
    {
        public ProjectRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(Project entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.project
                                  (id, user_id, org_id, package_id, project_type_id, project_name, project_desc, project_prefix, start_date, end_date, completed_date, project_status_id, is_private, is_public, created_date, createdby)
                           VALUES (@id, @user_id, @org_id, @package_id, @project_type_id, @project_name, @project_desc, @project_prefix, @start_date, @end_date, @completed_date, @project_status_id, @is_private, @is_public, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public async Task<Project> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<Project>(
                sql: "SELECT * FROM dbo.project WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public async Task<Project> FindAutoProjectPrefixByOrgID(string key, string date)
        {
            return await QuerySingleOrDefaultAsync<Project>(
                sql: @"SELECT TOP 1 project_prefix 
                            FROM dbo.project 
                        WHERE   
                            project_prefix like  '%JOB%' 
                            AND org_id = @key 
                            AND FORMAT(CAST(created_date AS DATE), 'd', 'EN-US') = FORMAT(CAST(@date AS DATE), 'd', 'EN-US')
                            AND dbo.project.is_deleted = 0
                            ORDER BY created_date DESC",
                param: new { key, date }
            );
        }

        public async Task<Project> FindAutoCostProjectPrefixByOrgID(string key, string date)
        {
            return await QuerySingleOrDefaultAsync<Project>(
                sql: @"SELECT TOP 1 project_prefix 
                            FROM dbo.cost_project 
                        WHERE   
                            project_prefix like  '%JOB%' 
                            AND org_id = @key 
                            AND FORMAT(CAST(created_date AS DATE), 'd', 'EN-US') = FORMAT(CAST(@date AS DATE), 'd', 'EN-US')
                            AND dbo.cost_project.is_deleted = 0
                            ORDER BY created_date DESC",
                param: new { key, date }
            );
        }

      

        public async Task<Project> FindCustomProjectPrefixByOrgIDAndPrefix(string key, string project_prefix)
        {
            return await QuerySingleOrDefaultAsync<Project>(
                sql: @"SELECT TOP 1 project_prefix    
                            FROM dbo.project  WHERE 
                            project_prefix = @project_prefix
                            AND org_id =@key
                            ORDER BY created_date DESC",
                param: new { key, project_prefix }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.project
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Project entity)
        {
            Execute(
                sql: @"UPDATE dbo.project
                   SET
                    user_id = @user_id,
                    package_id = @package_id,
                    project_type_id = @project_type_id,
                    org_id = @org_id,
                    project_name = @project_name,
                    project_desc = @project_desc,
                    project_prefix = @project_prefix,
                    start_date = @start_date,
                    end_date = @end_date,
                    completed_date = @completed_date,
                    project_status_id = @project_status_id,
                    is_private = @is_private,
                    is_public = @is_public,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public async Task<IEnumerable<Project>> All()
        {
            return await QueryAsync<Project>(
                sql: "SELECT * FROM [dbo].[project] where is_deleted = 0"
            );
        }

        public async Task<IEnumerable<dynamic>> FetchAllProjectByOrgID(string key)
        {
            //AND entity_contact.is_primary = 1
            return await QueryAsync<dynamic>(
                   sql: @"SELECT
                            project_type.id as project_type_id,
                            project_type.type_name as project_type_name,
                            project.id as project_id,
		                    dbo.customer.id,
                            project.project_name,
                            project.project_prefix,
		                    dbo.customer.cst_name,
		                    entity_contact.name,
		                    entity_contact.phone,
		                    entity_contact.email,
		                    entity_contact.is_primary,
                            e_tl.full_name as project_owner,
                            e_tl.workemail,
                            project_status.project_status_name ,
		                    project.start_date,
                            project.end_date,
                            project.completed_date,
                            CASE
                            WHEN FORMAT(CAST( project.modified_date AS DATETIME2), N'dd/MM/yyyy') IS NULL
	                        THEN FORMAT(CAST( project.created_date AS DATETIME2), N'dd/MM/yyyy')
                            ELSE FORMAT(CAST( project.created_date AS DATETIME2), N'dd/MM/yyyy') 
                        END AS ondate
                        FROM project  WITH(NOLOCK)
                        LEFT JOIN dbo.customer_x_project  ON project.id = dbo.customer_x_project.project_id
	                    LEFT JOIN dbo.customer ON dbo.customer_x_project.cst_id = dbo.customer.id
	                    LEFT JOIN dbo.entity_contact on dbo.customer.id = dbo.entity_contact.entity_id
	                    LEFT JOIN dbo.employee e_tl ON project.user_id = e_tl.id
                        LEFT JOIN project_status  ON project.project_status_id = project_status.id
                        LEFT JOIN project_type  ON project.project_type_id = project_type.id
                        WHERE
	                    project.org_id= @key
	                    AND project.is_deleted = 0
	                    ORDER BY project.project_name ASC",
                      param: new { key }
               );
        }

        public async Task UpdateProjectStatusByID(Project entity)
        {
            await ExecuteAsync(
                sql: @"UPDATE dbo.project
                   SET
                    project_status_id = @project_status_id,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public async Task<string> ProjectTaskCount(string entity)
        {
            return await QuerySingleOrDefaultAsync<string>(
                     sql: @"SELECT 
                            TaskCount = COUNT (task.id) FROM dbo.project
                            INNER JOIN project_activity_x_task on project.id = project_activity_x_task.project_id
                            INNER JOIN project_activity on project_activity_x_task.activity_id = project_activity.id
                            INNER JOIN task on task.id = project_activity_x_task.task_id
                        WHERE project.id = @entity
                            AND project.is_deleted = 0
                            AND project_activity.is_deleted = 0
                            AND task.is_deleted = 0",
               param: new { entity }
           );
        }

        public async Task<IEnumerable<dynamic>> FindAllProjectActivityByProjectID(string key)
        {
            return await QueryAsync<dynamic>(
                   sql: @"SELECT 
                            project_activity.project_id, 
                            project_activity.id as project_activity_id,
                            project_activity.activity_name,
                            project_activity.activity_desc, 
                            CASE WHEN project_activity.is_approve_req = 0 THEN 'False' ELSE 'True' END as is_approve_req, 
                            ISNULL(employee.full_name, 'NA') as approver, 
                            employee.id as approver_id,
                            CASE WHEN project_activity.is_approved = 0 THEN 'False' ELSE 'True' END as is_approved, 
                            status.status_name, 
                            project_activity.start_date, 
                            project_activity.end_date 
                        FROM project_activity 
                            INNER JOIN project on project_activity.project_id = project.id
                            LEFT JOIN employee on project_activity.approved_id = employee.id
                            LEFT JOIN status on project_activity.status_id = status.id
                        WHERE project.id = @key
                            AND project.is_deleted = 0
                            AND project_activity.is_deleted = 0
                            ORDER BY project_activity.activity_name ASC",
                      param: new { key }
               );
        }
         
        public async Task<string> GetLastAddedProjectPrefixByOrgID(string key)
        {
            return await QuerySingleOrDefaultAsync<string>(
                sql: @"SELECT TOP 1 project_prefix from dbo.project 
                        WHERE dbo.project.org_id = @key
                        AND dbo.project.is_deleted = 0
                        ORDER BY FORMAT(CAST(dbo.project .created_date AS DATETIME2), N'dd/MM/yyyy hh:mm tt') DESC",
                param: new { key }
            );
        }
    }
}