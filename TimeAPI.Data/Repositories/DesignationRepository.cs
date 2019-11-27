using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class DesignationRepository : RepositoryBase, IDesignationRepositiory
    {

        public DesignationRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(Designation entity)
        {

            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.department
                                  (id, org_id, depart_lead_empid, dep_name, alias, created_date, createdby)
                           VALUES (@id, @org_id, @depart_lead_empid, @dep_name, @alias, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Designation Find(string key)
        {
            return QuerySingleOrDefault<Designation>(
                sql: "SELECT * FROM dbo.department WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.department
                   SET
                       modified_date = @modified_date, modifiedby = @modifiedby, is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Designation entity)
        {
            Execute(
                sql: @"UPDATE dbo.department
                   SET 
                    id = @id, 
                    org_id = @org_id,
                    depart_lead_empid = @depart_lead_empid, 
                    dep_name = @dep_name, 
                    alias = @alias, 
                    modified_date = @modified_date, 
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<Designation> All()
        {
            return Query<Designation>(
                sql: "SELECT * FROM [dbo].[department] where is_deleted = 0"
            );
        }

        public Designation FindByDesignationName(string dep_name)
        {
            return QuerySingleOrDefault<Designation>(
                sql: "SELECT * FROM [dbo].[department] WHERE dep_name = @dep_name and is_deleted = 0",
                param: new { dep_name }
            );
        }

        public Designation FindByDesignationAlias(string alias)
        {
            return QuerySingleOrDefault<Designation>(
                sql: "SELECT * FROM [dbo].[department] WHERE alias = @alias and is_deleted = 0",
                param: new { alias }
            );
        }

        public IEnumerable<Designation> FindDesignationByDeptID(string org_id)
        {
            return Query<Designation>(
                sql: "SELECT * FROM [dbo].[department] WHERE org_id = @org_id and is_deleted = 0",
                param: new { org_id }
            );
        }

    }
}
