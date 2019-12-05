using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class EmployeeTypeRepository : RepositoryBase, IEmployeeTypeRepository
    {

        public EmployeeTypeRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(EmployeeType entity)
        {

            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.employee_type
                                  (id, org_id, employee_type_name, employee_type_desc, created_date, createdby)
                           VALUES (@id, @org_id, @employee_type_name, @employee_type_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public EmployeeType Find(string key)
        {
            return QuerySingleOrDefault<EmployeeType>(
                sql: "SELECT * FROM dbo.employee_type WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.employee_type
                   SET
                       modified_date = @modified_date, modifiedby = @modifiedby, is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(EmployeeType entity)
        {
            Execute(
                sql: @"UPDATE dbo.employee_type
                   SET 
                    org_id = @org_id,
                    employee_type_name = @employee_type_name, 
                    employee_type_desc = @employee_type_desc, 
                    modified_date = @modified_date, 
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<EmployeeType> All()
        {
            return Query<EmployeeType>(
                sql: "SELECT * FROM [dbo].[employee_type] where is_deleted = 0"
            );
        }

    }
}
