using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class ProjectTypeRepository : RepositoryBase, IProjectTypeRepository
    {
        public ProjectTypeRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(ProjectType entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.project_type
                                  (id, org_id, type_name, type_desc, created_date, createdby)
                           VALUES (@id, @org_id, @type_name, @type_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }
                                                         
        public ProjectType Find(string key)
        {
            return QuerySingleOrDefault<ProjectType>(   
                sql: "SELECT * FROM dbo.project_type WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public IEnumerable<ProjectType> FindByOrgID(string key)
        {
            return Query<ProjectType>(
                sql: "SELECT * FROM dbo.project_type WHERE is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.project_type
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void RemoveByOrgID(string key)
        {
            Execute(
                sql: @"UPDATE dbo.project_type
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(ProjectType entity)
        {
            Execute(
                sql: @"UPDATE dbo.project_type
                   SET
                    org_id = @org_id,
                    type_name = @type_name,
                    type_desc = @type_desc,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<ProjectType> All()
        {
            return Query<ProjectType>(
                sql: "SELECT * FROM dbo.project_type where is_deleted = 0"
            );
        }
    }
}