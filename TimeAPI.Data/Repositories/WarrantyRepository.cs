using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class WarrantyRepository : RepositoryBase, IWarrantyRepository
    {
        public WarrantyRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(Warranty entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.project_warrenty
                            (id, org_id, warranty_name, warranty_desc, created_date, createdby)
                    VALUES (@id, @org_id, @warranty_name, @warranty_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Warranty Find(string key)
        {
            return QuerySingleOrDefault<Warranty>(
                sql: "SELECT * FROM dbo.project_warrenty WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<Warranty> All()
        {
            return Query<Warranty>(
                sql: "SELECT * FROM dbo.project_warrenty where is_deleted = 0"
            );
        }

        public IEnumerable<Warranty> WarrantyByOrgID(string key)
        {
            return Query<Warranty>(
                sql: "SELECT * FROM dbo.project_warrenty where is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.project_warrenty
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Warranty entity)
        {
            Execute(
                sql: @"UPDATE dbo.project_warrenty
                           SET 
                           warranty_name = @warranty_name, 
                            warranty_desc = @warranty_desc,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}