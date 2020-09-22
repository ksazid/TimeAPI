using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
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

        public async Task<EmployeeLeave> Find(string key)
        {
            return await  QuerySingleOrDefaultAsync<EmployeeLeave>(
                sql: "SELECT * FROM dbo.employee_leave WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public async Task<dynamic> FetchEmployeeLeaveOrgID(string key)
        {
            return await QueryAsync<dynamic>(
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

        public async Task<dynamic> FetchEmployeeLeaveID(string key)
        {
            return await QueryAsync<dynamic>(
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
        }

        public async Task<dynamic> FetchEmployeeLeaveEmpID(string key)
        {
            return await QueryAsync<dynamic>(
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
        }

        public async Task<dynamic> FetchEmployeeLeaveHistoryEmpID(string key)
        {
            return await QueryAsync<dynamic>(
                sql: @"SELECT 
                            (leave_start_date) AS leave_start_date,
                            (leave_end_date) AS leave_end_date,
                            dbo.employee_leave.emp_id,  
                            dbo.leave_status.leave_status_name,
                            dbo.leave_type.leave_type_name,
                            dbo.leave_setup.leave_name
                            FROM dbo.employee_leave
                            LEFT JOIN dbo.leave_setup on dbo.employee_leave.leave_setup_id = dbo.leave_setup.id
                            LEFT JOIN dbo.leave_type on dbo.leave_setup.leave_type_id = dbo.leave_type.id
                            LEFT JOIN dbo.employee on dbo.employee_leave.emp_id =  dbo.employee.id
                            LEFT JOIN dbo.leave_status on dbo.employee_leave.leave_status_id =  dbo.leave_status.id
                        WHERE dbo.employee_leave.emp_id = @key AND dbo.leave_status.leave_status_name <> 'Declined'
                        AND dbo.employee_leave.is_deleted = 0 AND dbo.leave_setup.is_deleted = 0
                        group by  (leave_start_date),  (leave_end_date), dbo.employee_leave.emp_id, 
                        dbo.leave_status.leave_status_name,    
                        dbo.leave_type.leave_type_name, dbo.leave_setup.leave_name",
                param: new { key }
            );
        }

        public async Task<dynamic> GetDaysOfMonth(string startdate, string enddate)
        {
            return await QueryAsync<dynamic>(
                sql: @";WITH n(n) AS
                            (
                              SELECT TOP (DATEDIFF(MONTH, CAST(@startdate AS SMALLDATETIME), CAST(@enddate AS SMALLDATETIME))+1) ROW_NUMBER() OVER 
                              (ORDER BY [object_id])-1 FROM sys.all_objects
                            ),
                            x(n,fd,ld) AS 
                            (
                              SELECT n.n, DATEADD(MONTH, n.n, m.m), DATEADD(MONTH, n.n+1, m.m)
                              FROM n, (SELECT DATEADD(DAY, 1-DAY(CAST(@startdate AS SMALLDATETIME)), CAST(@startdate AS SMALLDATETIME))) AS m(m)
                            )
                            SELECT [Month] = DATENAME(MONTH, fd), [Days] = DATEDIFF(DAY, fd, ld) 
                              - CASE WHEN CAST(@startdate AS SMALLDATETIME) > fd THEN (DATEDIFF(DAY, fd, CAST(@startdate AS SMALLDATETIME))) ELSE 0 END
                              - CASE WHEN CAST(@enddate AS SMALLDATETIME) < ld THEN (DATEDIFF(DAY, CAST(@enddate AS SMALLDATETIME), ld)-1) ELSE 0 END
                              FROM x;",
                param: new { startdate, enddate }
            );
        }

        public async Task<dynamic> FetchEmployeeLeaveHistoryOrgID(string key)
        {
            return await QueryAsync<dynamic>(
                sql: @"SELECT 
		                    SUM(CAST(leave_days AS int)) as leave_days,
		                    MONTH(ondate_applied) AS month,
			                FORMAT(CAST(approve_start_date AS DATE), 'MM/dd/yyyy', 'EN-US') AS leave_start_date,
			                FORMAT(CAST(approve_end_date AS DATE), 'MM/dd/yyyy', 'EN-US') AS leave_end_date,
		                    dbo.employee_leave.emp_id,  
		                    dbo.employee.full_name,
		                    dbo.leave_status.id as leave_status_id,
		                    dbo.leave_status.leave_status_name,
		                    dbo.leave_type.leave_type_name,
		                    dbo.leave_setup.leave_name
	                    FROM dbo.employee_leave
	                    LEFT JOIN dbo.leave_setup on dbo.employee_leave.leave_setup_id = dbo.leave_setup.id
	                    LEFT JOIN dbo.leave_type on dbo.leave_setup.leave_type_id = dbo.leave_type.id
	                    LEFT JOIN dbo.employee on dbo.employee_leave.emp_id =  dbo.employee.id
	                    LEFT JOIN dbo.leave_status on dbo.employee_leave.leave_status_id =  dbo.leave_status.id
                    WHERE dbo.employee_leave.org_id = @key
                    AND dbo.employee_leave.is_deleted = 0 AND dbo.leave_setup.is_deleted = 0
                    group by MONTH(ondate_applied), dbo.employee_leave.emp_id, 
                    dbo.leave_status.leave_status_name, dbo.leave_status.id, dbo.employee.full_name,
                    dbo.leave_type.leave_type_name, dbo.leave_setup.leave_name,
	                FORMAT(CAST(approve_start_date AS DATE), 'MM/dd/yyyy', 'EN-US'),
	                FORMAT(CAST(approve_end_date AS DATE), 'MM/dd/yyyy', 'EN-US') ",
                param: new { key }
            );
        }

        public async Task<dynamic> FetchEmployeeLeaveHistoryApproverID(string key)
        {
            return await QueryAsync<dynamic>(
                sql: @"SELECT 
		                    SUM(CAST(leave_days AS int)) as leave_days,
		                    MONTH(ondate_applied) AS month,
                            FORMAT(CAST(approve_start_date AS DATE), 'MM/dd/yyyy', 'EN-US') AS leave_start_date,
			                FORMAT(CAST(approve_end_date AS DATE), 'MM/dd/yyyy', 'EN-US') AS leave_end_date,
		                    dbo.employee_leave.emp_id,  
		                    dbo.employee.full_name,
		                    dbo.leave_status.id as leave_status_id,
		                    dbo.leave_status.leave_status_name,
		                    dbo.leave_type.leave_type_name,
		                    dbo.leave_setup.leave_name
	                    FROM dbo.employee_leave
	                    LEFT JOIN dbo.leave_setup on dbo.employee_leave.leave_setup_id = dbo.leave_setup.id
	                    LEFT JOIN dbo.leave_type on dbo.leave_setup.leave_type_id = dbo.leave_type.id
	                    LEFT JOIN dbo.employee on dbo.employee_leave.emp_id =  dbo.employee.id
	                    LEFT JOIN dbo.leave_status on dbo.employee_leave.leave_status_id =  dbo.leave_status.id
                    WHERE dbo.employee_leave.approver_emp_id = @key
                    AND dbo.employee_leave.is_deleted = 0 AND dbo.leave_setup.is_deleted = 0
                    group by MONTH(ondate_applied), dbo.employee_leave.emp_id, 
                    dbo.leave_status.leave_status_name, dbo.leave_status.id, dbo.employee.full_name,
                    dbo.leave_type.leave_type_name, dbo.leave_setup.leave_name,    
                    FORMAT(CAST(approve_start_date AS DATE), 'MM/dd/yyyy', 'EN-US'),
			        FORMAT(CAST(approve_end_date AS DATE), 'MM/dd/yyyy', 'EN-US')",
                param: new { key }
            );
        }

        public async Task<IEnumerable<EmployeeLeave>> All()
        {
            return await QueryAsync<EmployeeLeave>(
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

        public async Task UpdateApprovedByID(EmployeeLeave entity)
        {
            await ExecuteAsync(
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