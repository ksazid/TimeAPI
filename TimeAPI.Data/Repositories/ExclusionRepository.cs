using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class ExclusionRepository : RepositoryBase, IExclusionRepository
    {
        public ExclusionRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(Exclusion entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.project_exclusion
                            (id, org_id, exclusion_name, exclusion_desc, created_date, createdby)
                    VALUES (@id, @org_id, @exclusion_name, @exclusion_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Exclusion Find(string key)
        {
            return QuerySingleOrDefault<Exclusion>(
                sql: "SELECT * FROM dbo.project_exclusion WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<Exclusion> All()
        {
            return Query<Exclusion>(
                sql: "SELECT * FROM dbo.project_exclusion where is_deleted = 0"
            );
        }

        public IEnumerable<Exclusion> ExclusionByOrgID(string key)
        {
            return Query<Exclusion>(
                sql: "SELECT * FROM dbo.project_exclusion where is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.project_exclusion
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Exclusion entity)
        {
            Execute(
                sql: @"UPDATE dbo.project_exclusion
                           SET 
                            exclusion_name = @exclusion_name, 
                            exclusion_desc = @exclusion_desc,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}