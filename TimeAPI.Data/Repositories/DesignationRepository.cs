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
                    sql: @"INSERT INTO dbo.designation
                                  (id, dep_id, designation_name, alias, created_date, createdby)
                           VALUES (@id, @dep_id, @designation_name, @alias, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Designation Find(string key)
        {
            return QuerySingleOrDefault<Designation>(
                sql: "SELECT * FROM dbo.designation WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.designation
                   SET
                       modified_date = @modified_date, modifiedby = @modifiedby, is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Designation entity)
        {
            Execute(
                sql: @"UPDATE dbo.designation
                   SET 
                    id = @id, 
                    dep_id = @dep_id,
                    designation_name = @designation_name, 
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
                sql: "SELECT * FROM [dbo].[designation] where is_deleted = 0"
            );
        }

        public Designation FindByDesignationName(string dep_name)
        {
            return QuerySingleOrDefault<Designation>(
                sql: "SELECT * FROM [dbo].[designation] WHERE designation_name = @designation_name and is_deleted = 0",
                param: new { dep_name }
            );
        }

        public Designation FindByDesignationAlias(string alias)
        {
            return QuerySingleOrDefault<Designation>(
                sql: "SELECT * FROM [dbo].[designation] WHERE alias = @alias and is_deleted = 0",
                param: new { alias }
            );
        }

        public IEnumerable<Designation> FindDesignationByDeptID(string dep_id)
        {
            return Query<Designation>(
                sql: "SELECT * FROM [dbo].[designation] WHERE dep_id = @dep_id and is_deleted = 0",
                param: new { dep_id }
            );
        }

        public dynamic FetchGridDataByDesignationByDeptOrgID(string key)
        {
            return Query<dynamic>(
                   sql: @"SELECT 
	                        designation.id,
	                        department.dep_name,
	                        designation.designation_name,
	                        designation.alias
	                        from designation
	                        inner join department on designation.dep_id = department.id
                        WHERE department.org_id = @key",
                      param: new { key }
               );
        }

    }
}
