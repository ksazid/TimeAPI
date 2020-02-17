using System.Collections.Generic;
using System.Data;
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
                                  (id, user_id, org_id, project_name, project_desc, project_prefix, start_date, end_date, completed_date, project_status_id, is_private, is_public, created_date, createdby)
                           VALUES (@id, @user_id, @org_id, @project_name, @project_desc, @project_prefix, @start_date, @end_date, @completed_date, @project_status_id, @is_private, @is_public, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Project Find(string key)
        {
            return QuerySingleOrDefault<Project>(
                sql: "SELECT * FROM dbo.project WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public Project FindAutoProjectPrefixByOrgID(string key, string date)
        {
            return QuerySingleOrDefault<Project>(
                sql: @"SELECT TOP 1 project_prefix 
                            FROM dbo.project 
                        WHERE 
                            CONTAINS(project_prefix, 'JOB')  
                            AND org_id = @key 
                            AND FORMAT(CAST(@date AS DATE), 'd', 'EN-US')
                            ORDER BY created_date DESC",
                param: new { key , date }
            );
        }

        public Project FindCustomProjectPrefixByOrgIDAndPrefix(string key, string project_prefix)
        {
            return QuerySingleOrDefault<Project>(
                sql: @"SELECT top 1 project_prefix    
                        FROM dbo.project  WHERE 
                     NOT CONTAINS(project_prefix, 'JOB')  AND project_prefix = @project_prefix
                        org_id = @key
                     ORDER BY created_date DESC",
                param: new { key, project_prefix  }
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

        public IEnumerable<Project> All()
        {
            return Query<Project>(
                sql: "SELECT * FROM [dbo].[project] where is_deleted = 0"
            );
        }

        public IEnumerable<dynamic> FetchAllProjectByOrgID(string key)
        {
            return Query<dynamic>(
                   sql: @"SELECT
                            project.id as project_id,
                            project.project_name,
                            project.project_prefix,
                            e_tl.full_name as project_owner,
                            e_tl.workemail,
                            project_status.project_status_name ,
		                    project.start_date,
                            project.end_date,
                            project.completed_date
                        FROM dbo.project WITH(NOLOCK)
                        LEFT JOIN employee e_tl ON dbo.project.user_id = e_tl.id
                        LEFT JOIN project_status  ON dbo.project.project_status_id = project_status.id
                        WHERE project.org_id = @key
                        AND project.is_deleted = 0
                        ORDER BY project.project_name ASC",
                      param: new { key }
               );
        }

        public void UpdateProjectStatusByID(Project entity)
        {
            Execute(
               sql: @"UPDATE dbo.project
                   SET
                    project_status_id = @project_status_id,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
               param: entity
           );
        }
    }
}