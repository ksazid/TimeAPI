using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class TypeOfDesignRepository : RepositoryBase, ITypeOfDesignRepository
    {
        public TypeOfDesignRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public void Add(TypeOfDesign entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"
                    INSERT INTO dbo.design_type
                            (id, org_id, project_id, design_name, created_date, createdby)
                    VALUES (@id, @org_id, @project_id, @design_name, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public TypeOfDesign Find(string key)
        {
            return QuerySingleOrDefault<TypeOfDesign>(
                sql: "SELECT * FROM dbo.design_type WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<TypeOfDesign> FetchAllTypeOfDesignByProjectID(string key)
        {
            return Query<TypeOfDesign>(
                sql: "SELECT * FROM dbo.design_type WHERE is_deleted = 0 and project_id= @key",
                param: new { key }
            );
        }
        public IEnumerable<TypeOfDesign> FetchAllTypeOfDesignByOrgID(string key)
        {
            return Query<TypeOfDesign>(
                sql: "SELECT * FROM dbo.design_type WHERE is_deleted = 0 and org_id= @key",
                param: new { key }
            );
        }
        

        public IEnumerable<TypeOfDesign> All()
        {
            return Query<TypeOfDesign>(
                sql: "SELECT * FROM dbo.design_type where is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.design_type
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(TypeOfDesign entity)
        {
            Execute(
                sql: @"UPDATE dbo.design_type
                           SET 
                            project_id = @project_id,
                            design_name = @design_name,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE project_id = @project_id",
                param: entity
            );
        }
    }
}