using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class ProjectTagsRepository : RepositoryBase, IProjectTagsRepository
    {
        public ProjectTagsRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(ProjectTags entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.project_tags
                            (id, project_id, unit_id, tags, created_date, createdby)
                    VALUES (@id, @project_id, @unit_id, @tags, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public async Task<ProjectTags> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<ProjectTags>(
                sql: "SELECT * FROM dbo.project_tags WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public async Task<IEnumerable<ProjectTags>> GetProjectTagsByUnitID(string key)
        {
            return await QueryAsync<ProjectTags>(
                sql: "SELECT * FROM dbo.project_tags where is_deleted = 0 and unit_id = @key",
                param: new { key }
            );
        }

        public async Task<IEnumerable<ProjectTags>> GetProjectTagsByProjectID(string key)
        {
            return await QueryAsync<ProjectTags>(
                sql: "SELECT * FROM dbo.project_tags where is_deleted = 0 and project_id = @key",
                param: new { key }
            );
        }
                          
        public async Task<IEnumerable<ProjectTags>> All()
        {
            return await QueryAsync<ProjectTags>(
                sql: "SELECT * FROM dbo.project_tags where is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.project_tags
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public async Task RemoveByUnitID(string key)
        {
           await ExecuteAsync(
                sql: @"UPDATE dbo.project_tags
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE unit_id = @key",
                param: new { key }
            );
        }

        public void Update(ProjectTags entity)
        {
            Execute(
                sql: @"UPDATE dbo.project_tags
                           SET 
                            project_id = @project_id, 
                            unit_id = @unit_id,
                            tags =  @tags,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}