using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class EmployeeRoleRepository : RepositoryBase, IEmployeeRoleRepository
    {

        public EmployeeRoleRepository(IDbTransaction transaction) : base(transaction)
        { }

        //public void Add(EmployeeRole entity)
        //{

        //    entity.id = ExecuteScalar<string>(
        //            sql: @"INSERT INTO dbo.role
        //                          (id, org_id, role_name, role_desc, created_date, createdby)
        //                   VALUES (@id, @org_id, @role_name, @role_desc, @created_date, @createdby);
        //            SELECT SCOPE_IDENTITY()",
        //            param: entity
        //        );
        //}

        //public EmployeeRole Find(string key)
        //{
        //    return QuerySingleOrDefault<EmployeeRole>(
        //        sql: "SELECT * FROM dbo.role WHERE is_deleted = 0 and id = @key",
        //        param: new { key }
        //    );
        //}

        //public void Remove(string key)
        //{
        //    Execute(
        //        sql: @"UPDATE dbo.role
        //           SET
        //               modified_date = GETDATE(), is_deleted = 1
        //            WHERE id = @key",
        //        param: new { key }
        //    );
        //}

        //public void Update(EmployeeRole entity)
        //{
        //    Execute(
        //        sql: @"UPDATE dbo.role
        //           SET 
        //            org_id =@org_id,
        //            role_name = @role_name, 
        //            role_desc = @role_desc, 
        //            modified_date = @modified_date, 
        //            modifiedby = @modifiedby
        //            WHERE id = @id",
        //        param: entity
        //    );
        //}

        public IEnumerable<EmployeeRole> GetEmployeeRoles()
        {
            return Query<EmployeeRole>(
                sql: "SELECT * FROM [dbo].[AspNetRoles]"
            );
        }

    }
}
