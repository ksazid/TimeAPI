using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class EmployeeLeaveRepository : RepositoryBase, IEmployeeLeaveRepository
    {
        public EmployeeLeaveRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(EmployeeLeave entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.employee_leave
                            (id, org_id, emp_id, leave_setup_id, leave_start_date, leave_end_date, leave_days, ondate_applied, approver_emp_id, is_approved, 
                            approve_start_date, approve_end_date, approved_days, ondate_approved, emp_notes, approver_notes, leave_status_id, created_date, createdby)
                    VALUES (@id, @org_id, @emp_id, @leave_setup_id, @leave_start_date, @leave_end_date, @leave_days, @ondate_applied, @approver_emp_id, @is_approved, 
                            @approve_start_date, @approve_end_date, @approved_days, @ondate_approved, @emp_notes, @approver_notes, @leave_status_id, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public EmployeeLeave Find(string key)
        {
            return QuerySingleOrDefault<EmployeeLeave>(
                sql: "SELECT * FROM dbo.employee_leave WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public dynamic FetchEmployeeLeaveOrgID(string key)
        {
            return Query<dynamic>(
                sql: @"	SELECT 
	                        dbo.employee_leave.id as employee_leave_id, 
		                    dbo.employee_leave.org_id, dbo.employee_leave.emp_id, 
		                    dbo.employee_leave.leave_setup_id as leave_setup_id,
	                        dbo.employee_leave.leave_start_date, dbo.employee_leave.leave_end_date, dbo.employee_leave.leave_days, 
	                        dbo.employee_leave.ondate_applied, dbo.employee_leave.approver_emp_id,
		                    dbo.employee_leave.is_approved, 
	                        dbo.employee_leave.approve_start_date, 
		                    dbo.employee_leave.approve_end_date, dbo.employee_leave.approved_days, 
	                        dbo.employee_leave.ondate_approved, dbo.employee_leave.emp_notes, dbo.employee_leave.approver_notes, 
	                        dbo.leave_setup.leave_name, dbo.leave_type.leave_type_name, dbo.employee.full_name, 
		                    dbo.leave_status.id as leave_status_id,
	                        dbo.leave_status.leave_status_name 
	                        FROM dbo.employee_leave
	                        LEFT JOIN dbo.leave_setup on dbo.employee_leave.leave_setup_id = dbo.leave_setup.id
	                        LEFT JOIN dbo.leave_type on dbo.leave_setup.leave_type_id = dbo.leave_type.id
	                        LEFT JOIN dbo.employee on dbo.employee_leave.emp_id =  dbo.employee.id
	                        LEFT JOIN dbo.leave_status on dbo.employee_leave.leave_status_id =  dbo.leave_status.id
	                        WHERE dbo.employee_leave.org_id = @key
	                        and dbo.employee_leave.is_deleted = 0",
                param: new { key }
            );
        }


        public dynamic FetchEmployeeLeaveID(string key)
        {
          var Xresult =  Query<dynamic>(
                sql: @"SELECT 
	                        dbo.employee_leave.id as employee_leave_id, 
		                    dbo.employee_leave.org_id, dbo.employee_leave.emp_id, 
		                    dbo.employee_leave.leave_setup_id as leave_setup_id,
	                        dbo.employee_leave.leave_start_date, dbo.employee_leave.leave_end_date, dbo.employee_leave.leave_days, 
	                        dbo.employee_leave.ondate_applied, dbo.employee_leave.approver_emp_id,
		                    dbo.employee_leave.is_approved, 
	                        dbo.employee_leave.approve_start_date, 
		                    dbo.employee_leave.approve_end_date, dbo.employee_leave.approved_days, 
	                        dbo.employee_leave.ondate_approved, dbo.employee_leave.emp_notes, dbo.employee_leave.approver_notes, 
	                        dbo.leave_setup.leave_name, dbo.leave_type.leave_type_name, dbo.employee.full_name, 
		                    dbo.leave_status.id as leave_status_id,
	                    dbo.leave_status.leave_status_name 
	                    FROM dbo.employee_leave
	                        LEFT JOIN dbo.leave_setup on dbo.employee_leave.leave_setup_id = dbo.leave_setup.id
	                        LEFT JOIN dbo.leave_type on dbo.leave_setup.leave_type_id = dbo.leave_type.id
	                        LEFT JOIN dbo.employee on dbo.employee_leave.emp_id =  dbo.employee.id
	                        LEFT JOIN dbo.leave_status on dbo.employee_leave.leave_status_id =  dbo.leave_status.id
                        WHERE dbo.employee_leave.id = @key and dbo.employee_leave.is_deleted = 0",
                param: new { key }
            );

            return Xresult;
        }

        public dynamic FetchEmployeeLeaveEmpID(string key)
        {
            var Xresult =  Query<dynamic>(
                sql: @"SELECT 
	                        dbo.employee_leave.id as employee_leave_id, 
		                    dbo.employee_leave.org_id, dbo.employee_leave.emp_id, 
		                    dbo.employee_leave.leave_setup_id as leave_setup_id,
	                        dbo.employee_leave.leave_start_date, dbo.employee_leave.leave_end_date, dbo.employee_leave.leave_days, 
	                        dbo.employee_leave.ondate_applied, dbo.employee_leave.approver_emp_id,
		                    dbo.employee_leave.is_approved, 
	                        dbo.employee_leave.approve_start_date, 
		                    dbo.employee_leave.approve_end_date, dbo.employee_leave.approved_days, 
	                        dbo.employee_leave.ondate_approved, dbo.employee_leave.emp_notes, dbo.employee_leave.approver_notes, 
	                        dbo.leave_setup.leave_name, dbo.leave_type.leave_type_name, dbo.employee.full_name, 
		                    dbo.leave_status.id as leave_status_id,
	                        dbo.leave_status.leave_status_name 
	                    FROM dbo.employee_leave
	                    LEFT JOIN dbo.leave_setup on dbo.employee_leave.leave_setup_id = dbo.leave_setup.id
	                    LEFT JOIN dbo.leave_type on dbo.leave_setup.leave_type_id = dbo.leave_type.id
	                    LEFT JOIN dbo.employee on dbo.employee_leave.emp_id =  dbo.employee.id
	                    LEFT JOIN dbo.leave_status on dbo.employee_leave.leave_status_id =  dbo.leave_status.id
                    WHERE dbo.employee_leave.emp_id = @key
	                and dbo.employee_leave.is_deleted = 0",
                param: new { key }
            );

            return Xresult;
        }

        public IEnumerable<EmployeeLeave> All()
        {
            return Query<EmployeeLeave>(
                sql: "SELECT * FROM dbo.employee_leave where is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.employee_leave
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(EmployeeLeave entity)
        {
            Execute(
                sql: @"UPDATE dbo.employee_leave
                           SET 
                            org_id = @org_id, 
                            leave_setup_id = @leave_setup_id, 
                            leave_start_date = @leave_start_date, 
                            leave_end_date = @leave_end_date, 
                            leave_days = @leave_days, 
                            ondate_applied = @ondate_applied, 
                            approver_emp_id = @approver_emp_id, 
                            is_approved = @is_approved, 
                            approve_start_date = @approve_start_date, 
                            approve_end_date = @approve_end_date, 
                            approved_days = @approved_days, 
                            ondate_approved = @ondate_approved, 
                            emp_notes = @emp_notes, 
                            approver_notes = @approver_notes, 
                            leave_status_id = @leave_status_id,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }

        public void UpdateApprovedByID(EmployeeLeave entity)
        {
            Execute(
                sql: @"UPDATE dbo.employee_leave
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