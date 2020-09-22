using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class ProjectDesignTypeRepository : RepositoryBase, IProjectDesignTypeRepository
    {
        public ProjectDesignTypeRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(ProjectDesignType entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.project_design_type
                            (id, project_id, unit_id, design_type_id, created_date, createdby)
                    VALUES (@id, @project_id, @unit_id, @design_type_id, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public async Task< ProjectDesignType> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<ProjectDesignType>(
                sql: "SELECT * FROM dbo.project_design_type WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public async Task<IEnumerable<ProjectDesignType>> GetProjectDesignTypeByUnitID(string key)
        {
            return await QueryAsync<ProjectDesignType>(
                sql: "SELECT * FROM dbo.project_design_type WHERE unit_id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public async Task<IEnumerable<ProjectDesignType>> GetProjectDesignTypeByUnitIDAndProjectTypeID(string key, string ProjectTypeID)
        {
            return await QueryAsync<ProjectDesignType>(
                sql: "SELECT * FROM dbo.project_design_type WHERE unit_id = @key and is_deleted = 0 and design_type_id = @ProjectTypeID",
                param: new { key, ProjectTypeID }
            );
        }

        public async Task<IEnumerable<ProjectDesignType>> GetProjectDesignTypeByProjectID(string key)
        {
            return await QueryAsync<ProjectDesignType>(
                sql: "SELECT * FROM dbo.project_design_type WHERE project_id = @key and is_deleted = 0",
                param: new { key }
            );
        }              

        public async Task<IEnumerable<ProjectDesignType>> All()
        {
            return await QueryAsync<ProjectDesignType>(
                sql: "SELECT * FROM dbo.project_design_type where is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.project_design_type
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public async Task RemoveByUnitID(string key)
        {
           await ExecuteAsync(
                sql: @"UPDATE dbo.project_design_type
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE unit_id = @key",
                param: new { key }
            );
        }

        public async Task RemoveByProjectID(string key)
        {
           await ExecuteAsync(
                sql: @"UPDATE dbo.project_design_type
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE project_id = @key",
                param: new { key }
            );
        }

        public void Update(ProjectDesignType entity)
        {
            Execute(
                sql: @"UPDATE dbo.project_design_type
                           SET 
                            project_id = @project_id, 
                            unit_id = @unit_id,
                            design_type_id = @design_type_id, 
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}