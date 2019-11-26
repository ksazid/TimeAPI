﻿using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
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
                                           (id, org_id, depart_lead_empid, dep_name, alias, created_date, createdby, modified_date, modifiedby, is_deleted)
                           VALUES (@id, @org_id, @depart_lead_empid, @dep_name, @alias, @created_date, @createdby, @modified_date, @modifiedby, @is_deleted);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Department Find(string key)
        {
            return QuerySingleOrDefault<Department>(
                sql: "SELECT * FROM dbo.department WHERE id = @key",
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
                    created_date = @created_date, 
                    createdby = @createdby, 
                    modified_date = @modified_date, 
                    modifiedby = @modifiedby, 
                    is_deleted = @is_deleted
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<Department> All()
        {
            return Query<Department>(
                sql: "SELECT * FROM [dbo].[department]"
            );
        }

        public Department FindByDepartmentName(string dep_name)
        {
            return QuerySingleOrDefault<Department>(
                sql: "SELECT * FROM [dbo].[department] WHERE dep_name = @dep_name",
                param: new { dep_name }
            );
        }

        public Department FindByDepartmentAlias(string alias)
        {
            return QuerySingleOrDefault<Department>(
                sql: "SELECT * FROM [dbo].[department] WHERE alias = @alias",
                param: new { alias }
            );
        }

        public IEnumerable<Department> FindDepartmentByOrgID(string org_id)
        {
            return Query<Department>(
                sql: "SELECT * FROM [dbo].[department] WHERE alias = @org_id",
                param: new { org_id }
            );
        }
    }
}
