using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class SpecifiationRepository : RepositoryBase, ISpecifiationRepository
    {
        public SpecifiationRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public void Add(Specifiation entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"
                    INSERT INTO dbo.specification
                            (id, org_id, specification_name, specification_qty, created_date, createdby)
                    VALUES (@id, @org_id, @specification_name, @specification_qty, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Specifiation Find(string key)
        {
            return QuerySingleOrDefault<Specifiation>(
                sql: "SELECT * FROM dbo.specification WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<Specifiation> All()
        {
            return Query<Specifiation>(
                sql: "SELECT * FROM dbo.specification where is_deleted = 0"
            );
        }

        public IEnumerable<Specifiation> FetchAllSpecifiationByOrgID(string key)
        {
            return Query<Specifiation>(
                sql: "SELECT * FROM dbo.specification where is_deleted = 0 and org_id =@key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.specification
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Specifiation entity)
        {
            Execute(
                sql: @"UPDATE dbo.specification
                           SET 
                            org_id = @org_id, 
                            specification_name = @specification_name, 
                            specification_qty = @specification_qty,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}