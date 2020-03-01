using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class ProjectStatusRepository : RepositoryBase, IProjectStatusRepository
    {
        public ProjectStatusRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(ProjectStatus entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.project_status
                                  (id, org_id, project_status_name, project_status_desc, created_date, createdby)
                           VALUES (@id, @org_id, @project_status_name, @project_status_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public ProjectStatus Find(string key)
        {
            return QuerySingleOrDefault<ProjectStatus>(
                sql: "SELECT * FROM dbo.project_status WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.project_status
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(ProjectStatus entity)
        {
            Execute(
                sql: @"UPDATE dbo.project_status
                   SET
                    org_id = @org_id,
                    project_status_name = @project_status_name,
                    project_status_desc = @project_status_desc,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<ProjectStatus> All()
        {
            return Query<ProjectStatus>(
                sql: "SELECT * FROM [dbo].[project_status] where is_deleted = 0"
            );
        }

        public IEnumerable<ProjectStatus> GetProjectStatusByOrgID(string key)
        {
            return Query<ProjectStatus>(
                sql: @"SELECT * FROM [dbo].[project_status]
                        WHERE is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }
    }
}