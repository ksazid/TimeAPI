using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class EmployeeLeaveLogRepository : RepositoryBase, IEmployeeLeaveLogRepository
    {
        public EmployeeLeaveLogRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public void Add(EmployeeLeaveLog entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.employee_leave_log
                            (id, emp_leave_id, emp_id, leave_type, start_date, end_date, no_of_days, ondate, from_user, to_user, created_date, createdby)
                    VALUES (@id, @emp_leave_id, @emp_id, @leave_type, @start_date, @end_date, @no_of_days, @ondate, @from_user, @to_user, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public EmployeeLeaveLog Find(string key)
        {
            return QuerySingleOrDefault<EmployeeLeaveLog>(
                sql: "SELECT * FROM dbo.employee_leave_log WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public dynamic FetchEmployeeLeaveLogHistoryEmpID(string key)
        {
            return Query<dynamic>(
                sql: @"SELECT 
                        leave_status.leave_status_name, employee_leave_log.* from employee_leave_log 
                        INNER JOIN employee_leave on employee_leave_log.emp_leave_id = employee_leave.id
                        INNER JOIN leave_status on employee_leave.leave_status_id = leave_status.id
                        WHERE employee_leave_log.emp_id = @key
                    AND employee_leave_log.is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<EmployeeLeaveLog> All()
        {
            return Query<EmployeeLeaveLog>(
                sql: "SELECT * FROM dbo.employee_leave_log where is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.employee_leave_log
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(EmployeeLeaveLog entity)
        {
            Execute(
                sql: @"UPDATE dbo.employee_leave_log
                           SET 
                            emp_leave_id = @emp_leave_id, 
                            emp_id = @emp_id, 
                            leave_type = @leave_type, 
                            start_date = @start_date, 
                            end_date = @end_date, 
                            no_of_days = @no_of_days, 
                            ondate = @ondate,
                            from_user = @from_user, 
                            to_user = @to_user,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }

        public void UpdateApprovedByID(EmployeeLeaveLog entity)
        {
            Execute(
                sql: @"UPDATE dbo.employee_leave_log
                           SET 
                            is_approved = @is_approved, 
                            approve_start_date = @approve_start_date, 
                            approve_end_date = @approve_end_date, 
                            approved_days = @approved_days, 
                            ondate_approved = @ondate_approved, 
                            approver_notes = @approver_notes, 
                            leave_status_id = @leave_status_id,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }

    }
}