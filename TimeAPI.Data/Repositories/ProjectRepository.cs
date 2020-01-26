using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
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
                                  (id, user_id, org_id, project_name, project_desc, start_date, end_date, completed_date, project_status, is_private, is_public, created_date, createdby)
                           VALUES (@id, @user_id, @org_id, @project_name, @project_desc, @start_date, @end_date, @completed_date, @project_status, @is_private, @is_public, @created_date, @createdby);
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
                    start_date = @start_date, 
                    end_date = @end_date, 
                    completed_date = @completed_date, 
                    project_status = @project_status, 
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
    }
}
