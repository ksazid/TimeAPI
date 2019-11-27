using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;
using TimeAPI.Domain.Repositories;


namespace TimeAPI.Data.Repositories
{
    public class DepartmentRepository : RepositoryBase, IDepartmentRepository
    {

        public DepartmentRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(Department entity)
        {

            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.department
                                  (id, org_id, depart_lead_empid, dep_name, alias, created_date, createdby)
                           VALUES (@id, @org_id, @depart_lead_empid, @dep_name, @alias, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Department Find(string key)  
        {
            return QuerySingleOrDefault<Department>(
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

        public void Update(Department entity)
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

        public IEnumerable<Department> All()
        {
            return Query<Department>(
                sql: "SELECT * FROM [dbo].[department] where is_deleted = 0"
            );
        }

        public Department FindByDepartmentName(string dep_name)
        {
            return QuerySingleOrDefault<Department>(
                sql: "SELECT * FROM [dbo].[department] WHERE dep_name = @dep_name and is_deleted = 0",
                param: new { dep_name }
            );
        }

        public Department FindByDepartmentAlias(string alias)
        {
            return QuerySingleOrDefault<Department>(
                sql: "SELECT * FROM [dbo].[department] WHERE alias = @alias and is_deleted = 0",
                param: new { alias }
            );
        }

        public IEnumerable<Department> FindDepartmentByOrgID(string org_id)
        {
            return Query<Department>(
                sql: "SELECT * FROM [dbo].[department] WHERE org_id = @org_id and is_deleted = 0",
                param: new { org_id }
            );
        }

        public IEnumerable<DepartmentResultSet> FindAllDepLeadByOrgID(string org_id)
        {
            return Query<DepartmentResultSet>(
                sql: @"select e.full_name, e.email, e.designation, d.dep_name from employee e
                        join department d on e.id = d.depart_lead_empid
                        where d.org_id = @org_id and d.is_deleted = 0",
                param: new { org_id }
            );
        }

        public DepartmentResultSet FindDepLeadByDepID(string DepID)
        {
            return QuerySingleOrDefault<DepartmentResultSet>(
                sql: @"select e.full_name, e.email, e.designation, d.dep_name from employee e
                        join department d on e.id = d.depart_lead_empid
                        where d.id = @DepID  and d.is_deleted = 0",
                param: new { DepID }
            );
        }

    }
}
