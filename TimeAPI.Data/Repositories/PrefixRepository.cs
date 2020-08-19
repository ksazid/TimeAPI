using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class PrefixRepository : RepositoryBase, IPrefixRepository
    {
        public PrefixRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(Prefix entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.organization_prefix
                            (id, org_id, prefix_name, prefix_for, is_manual_allowed, created_date, createdby)
                    VALUES (@id, @org_id, @prefix_name, @prefix_for, @is_manual_allowed, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Prefix Find(string key)
        {
            return QuerySingleOrDefault<Prefix>(
                sql: "SELECT * FROM dbo.organization_prefix WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<Prefix> All()
        {
            return Query<Prefix>(
                sql: "SELECT * FROM dbo.organization_prefix where is_deleted = 0"
            );
        }

        public IEnumerable<Prefix> PrefixByOrgID(string key)
        {
            return Query<Prefix>(
                sql: "SELECT * FROM dbo.organization_prefix where is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.organization_prefix
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Prefix entity)
        {
            Execute(
                sql: @"UPDATE dbo.organization_prefix
                           SET 
                            org_id = @org_id, 
                            prefix_name = @prefix_name,
                            prefix_for = @prefix_for, 
                            is_manual_allowed = @is_manual_allowed,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}