using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class UnitDescriptionRepository : RepositoryBase, IUnitDescriptionRepository
    {
        public UnitDescriptionRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(UnitDescription entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"
                    INSERT INTO dbo.unit_desc
                            (id, org_id, unit_name, is_checkbox, is_extra, created_date, createdby)
                    VALUES (@id, @org_id, @unit_name, @is_checkbox, @is_extra, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public async Task<UnitDescription> Find(string key)
        {
            return await  QuerySingleOrDefaultAsync<UnitDescription>(
                sql: "SELECT * FROM dbo.unit_desc WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public async Task<IEnumerable<UnitDescription>> All()
        {
            return await QueryAsync<UnitDescription>(
                sql: "SELECT * FROM dbo.unit_desc where is_deleted = 0"
            );
        }

        public async Task<IEnumerable<UnitDescription>> FetchAllUnitDescriptionByOrgID(string key)
        {
            return await QueryAsync<UnitDescription>(
                sql: "SELECT * FROM dbo.unit_desc WHERE is_deleted = 0 AND org_id = @key AND is_extra = 0 ORDER BY unit_name ASC",
                   param: new { key }
            );
        }

        public async Task<IEnumerable<UnitDescription>> FetchAllUnitDescriptionExtraByOrgID(string key)
        {
            return await QueryAsync<UnitDescription>(
                sql: "SELECT * FROM dbo.unit_desc WHERE is_deleted = 0 AND org_id = @key AND is_extra = 1 ORDER BY unit_name ASC",
                   param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.unit_desc
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(UnitDescription entity)
        {
            Execute(
                sql: @"UPDATE dbo.unit_desc
                           SET 
                            org_id = @org_id, 
                            unit_name = @unit_name, 
                            is_checkbox = @is_checkbox,
                            is_extra = @is_extra,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}