using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class SuperadminOrganizationRepository : RepositoryBase, ISuperadminOrganizationRepository
    {
        public SuperadminOrganizationRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(SuperadminOrganization entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO superadmin_x_org
                                  (id, superadmin_empid, org_id, created_date, createdby)
                           VALUES (@id, @superadmin_empid, @org_id,  @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public SuperadminOrganization Find(string key)
        {
            return QuerySingleOrDefault<SuperadminOrganization>(
                sql: "SELECT * FROM superadmin_x_org WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE superadmin_x_org
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void RemoveByOrgID(string key)
        {
            Execute(
                sql: @"UPDATE superadmin_x_org
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(SuperadminOrganization entity)
        {
            Execute(
                sql: @"UPDATE superadmin_x_org
                   SET
                    superadmin_empid = @superadmin_empid, 
                    org_id =  @org_id,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<SuperadminOrganization> All()
        {
            return Query<SuperadminOrganization>(
                sql: "SELECT * FROM superadmin_x_org where is_deleted = 0"
            );
        }
    }
}