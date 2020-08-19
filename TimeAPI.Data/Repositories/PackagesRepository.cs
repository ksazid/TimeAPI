using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class PackagesRepository : RepositoryBase, IPackagesRepository
    {
        public PackagesRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(Packages entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.packages
                            (id, org_id, package_name, package_desc, created_date, createdby)
                    VALUES (@id, @org_id, @package_name, @package_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Packages Find(string key)
        {
            return QuerySingleOrDefault<Packages>(
                sql: "SELECT * FROM dbo.packages WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<Packages> FetchAllPackagesByOrgID(string key)
        {
            return Query<Packages>(
                sql: "SELECT * FROM dbo.packages WHERE org_id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<Packages> All()
        {
            return Query<Packages>(
                sql: "SELECT * FROM dbo.packages where is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.packages
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }
        public void Update(Packages entity)
        {
            Execute(
                sql: @"UPDATE dbo.packages
                           SET 
                            org_id = @org_id, 
                            package_name = @package_name,
                            package_desc = @package_desc,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}