using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class TypeOfUnitRepository : RepositoryBase, ITypeOfUnitRepository
    {
        public TypeOfUnitRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(TypeOfUnit entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.unit_type
                            (id, org_id, unit_name, created_date, createdby)
                    VALUES (@id, @org_id, @unit_name, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public TypeOfUnit Find(string key)
        {
            return QuerySingleOrDefault<TypeOfUnit>(
                sql: "SELECT * FROM dbo.unit_type WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<TypeOfUnit> All()
        {
            return Query<TypeOfUnit>(
                sql: "SELECT * FROM dbo.unit_type where is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.unit_type
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(TypeOfUnit entity)
        {
            Execute(
                sql: @"UPDATE dbo.unit_type
                           SET 
                            unit_name = @unit_name, 
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}