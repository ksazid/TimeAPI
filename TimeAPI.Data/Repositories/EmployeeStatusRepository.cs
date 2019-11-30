using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class EmployeeStatusRepository : RepositoryBase, IEmployeeStatusRepository
    {

        public EmployeeStatusRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(EmployeeStatus entity)
        {

            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.employee_status
                                  (id, employee_status_name, employee_status_desc, created_date, createdby)
                           VALUES (@id, @employee_status_name, @employee_status_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public EmployeeStatus Find(string key)
        {
            return QuerySingleOrDefault<EmployeeStatus>(
                sql: "SELECT * FROM dbo.employee_status WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.employee_status
                   SET
                       modified_date = @modified_date, modifiedby = @modifiedby, is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(EmployeeStatus entity)
        {
            Execute(
                sql: @"UPDATE dbo.employee_status
                   SET 
                    employee_status_name = @employee_status_name, 
                    employee_status_desc = @employee_status_desc, 
                    modified_date = @modified_date, 
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<EmployeeStatus> All()
        {
            return Query<EmployeeStatus>(
                sql: "SELECT * FROM [dbo].[employee_status] where is_deleted = 0"
            );
        }

    }
}
