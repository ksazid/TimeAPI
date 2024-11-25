﻿using System.Collections.Generic;
using System.Data;
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
                                  (id, org_id, employee_status_name, employee_status_desc, created_date, createdby)
                           VALUES (@id, @org_id, @employee_status_name, @employee_status_desc, @created_date, @createdby);
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
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(EmployeeStatus entity)
        {
            Execute(
                sql: @"UPDATE dbo.employee_status
                   SET
                    org_id =@org_id,
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

        public IEnumerable<EmployeeStatus> GetEmployeeStatusByOrgID(string key)
        {
            return Query<EmployeeStatus>(
                sql: @"SELECT * FROM dbo.employee_status
                        WHERE org_id='default' AND is_deleted = 0
                        UNION 
                        SELECT * FROM dbo.employee_status
                        WHERE org_id= @key
                        AND is_deleted = 0  
                        ORDER BY employee_status_name ASC",
                param: new { key }
            );
        }
    }
}