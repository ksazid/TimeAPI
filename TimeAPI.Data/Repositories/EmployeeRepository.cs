﻿using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class EmployeeRepository : RepositoryBase, IEmployeeRepository
    {
        public EmployeeRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public void Add(Employee entity)
        {
            entity.id = ExecuteScalar<string>(
                        sql: @"
                    INSERT INTO [dbo].[employee] (id, org_id, user_id, deptid, full_name, first_name,last_name, alias, gender, emp_status_id, emp_type_id, imgurl_id, workemail,
                                   emp_code, role_id, designation_id, dob, joined_date, phone, mobile, email, summary, created_date, createdby, is_admin, is_superadmin, is_password_reset)
                    VALUES (@id, @org_id, @user_id, @deptid, @full_name, @first_name, @last_name, @alias,  @gender, @emp_status_id, @emp_type_id, @imgurl_id, @workemail, @emp_code,
                                  @role_id, @designation_id, @dob, @joined_date, @phone, @mobile, @email, @summary, @created_date, @createdby, @is_admin, @is_superadmin, @is_password_reset);
                    SELECT SCOPE_IDENTITY()",
                        param: entity
                    );
        }

        public async Task<Employee> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<Employee>(
                sql: "SELECT * FROM [dbo].[employee] WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public async Task<Employee> FindByEmpUserID(string key)
        {
            return await QuerySingleOrDefaultAsync<Employee>(
                sql: "SELECT * FROM [dbo].[employee] WHERE user_id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public async Task<IEnumerable<Employee>> All()
        {
            return await QueryAsync<Employee>(
                sql: "SELECT * FROM [dbo].[employee] where is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
             Execute(
                sql: @"UPDATE dbo.employee
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public async Task RemovePermanent(string key)
        {
           await ExecuteAsync(
                sql: @"DELETE  
                        dbo.employee 
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Employee entity)
        {
             Execute(
                sql: @"UPDATE dbo.employee
                   SET
                       deptid = @deptid, full_name = @full_name, first_name = @first_name, last_name = @last_name, alias = @alias,
                       gender = @gender, emp_status_id = @emp_status_id, emp_type_id = @emp_type_id, imgurl_id = @imgurl_id, workemail = @workemail,
                       emp_code = @emp_code, role_id = @role_id, designation_id = @designation_id, dob = @dob, joined_date = @joined_date,
                       phone = @phone, mobile = @mobile, summary = @summary, modified_date = @modified_date, modifiedby = @modifiedby,
                       is_admin = @is_admin
                    WHERE id = @id",
                param: entity
            );
        }

        public async Task SetEmpPasswordResetByUserID(string key)
        {
             await ExecuteAsync(
                sql: @"UPDATE dbo.employee
                   SET
                      is_password_reset = 1
                    WHERE user_id = @key",
                param: new { key }
            );
        }

        public async Task<Employee> FindByEmpName(string full_name)
        {
            return await QuerySingleOrDefaultAsync<Employee>(
                sql: "SELECT * FROM [dbo].[employee] WHERE full_name = @full_name and is_deleted = 0",
                param: new { full_name }
            );
        }

        public async Task<IEnumerable<Employee>> FindByOrgIDCode(string OrgID)
        {
            return await QueryAsync<Employee>(
                sql: "SELECT * FROM [dbo].[employee] WHERE org_id = @OrgID and is_deleted = 0",
                param: new { OrgID }
            );
        }

        public async Task<Employee> FindByEmpCode(string emp_code)
        {
            return await QuerySingleOrDefaultAsync<Employee>(
                sql: "SELECT * FROM [dbo].[employee] WHERE emp_code = @emp_code and is_deleted = 0",
                param: new { emp_code }
            );
        }

        public async Task<IEnumerable<Employee>> FindByRoleName(string role)
        {
            return await QueryAsync<Employee>(
                sql: "SELECT * FROM [dbo].[employee] WHERE role_id = @role and is_deleted = 0",
                param: new { role }
            );
        }

        public async Task<dynamic> FetchGridDataEmployeeByOrgID(string key)
        {
            return await QueryAsync<dynamic>(
                   sql: @"SELECT
                            ROW_NUMBER() OVER (ORDER BY employee.full_name) AS rowno,
	                        employee.id,
	                        employee.org_id,
	                        employee.full_name,
	                        employee.workemail,
	                        employee.emp_code,
	                        employee.phone,
	                        employee_status.employee_status_name,
	                        employee_type.employee_type_name,
	                        AspNetRoles.Name as role_name,
	                        department.dep_name,
                            department.id as department_id,
	                        designation.designation_name
                          FROM dbo.employee WITH(NOLOCK)
	                          LEFT JOIN employee_status ON employee.emp_status_id = employee_status.id
	                          LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
	                          LEFT JOIN AspNetRoles ON employee.role_id = AspNetRoles.id
	                          LEFT JOIN department ON employee.deptid = department.id
	                          LEFT JOIN designation ON employee.designation_id = designation.id
                          WHERE employee.org_id =  @key AND employee.is_deleted = 0
                          AND employee.is_superadmin = 0
                          ORDER BY employee.full_name ASC",
                   param: new { key }
               );
        }

        public async Task<dynamic> FindEmployeeListByDesignationID(string DesignationID)
        {
            return await QueryAsync<dynamic>(
                sql: @"SELECT  employee.id, employee.full_name FROM dbo.employee
                            INNER JOIN designation ON employee.designation_id = designation.id
                        WHERE  employee.is_deleted = 0
                        AND employee.is_superadmin = 0
                        AND designation.id = @DesignationID
                        ORDER BY employee.full_name ASC",
                param: new { DesignationID }
            );
        }

        public async Task<dynamic> FindEmployeeListByDepartmentID(string DepartmentID)
        {
            return await QueryAsync<dynamic>(
                sql: @"SELECT employee.id, employee.full_name FROM dbo.employee
                        INNER JOIN department ON employee.deptid = department.id
                        WHERE  employee.is_deleted = 0
                        AND employee.is_superadmin = 0
                        AND department.id = @DepartmentID
                        ORDER BY employee.full_name ASC",
                param: new { DepartmentID }
            );
        }

        public async Task<dynamic> FindEmpDepartDesignByEmpID(string key)
        {
            return await QuerySingleOrDefaultAsync<dynamic>(
                   sql: @"	SELECT
	                        employee.id,
	                        employee.full_name,
	                        employee.workemail,
	                        employee.emp_code,
	                        employee_type.employee_type_name,
	                        department.dep_name,
	                        designation.designation_name
	                    FROM dbo.employee WITH(NOLOCK)
	                LEFT JOIN employee_status ON employee.emp_status_id = employee_status.id
	                LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
	                LEFT JOIN AspNetRoles ON employee.role_id = AspNetRoles.id
	                LEFT JOIN department ON employee.deptid = department.id
	                LEFT JOIN designation ON employee.designation_id = designation.id
	                WHERE employee.id =  @key AND employee.is_deleted = 0
	                AND employee.is_superadmin = 0
	                ORDER BY employee.full_name ASC",
                      param: new { key }
               );
        }

        public async Task<dynamic> GetAllOutsourcedEmpByOrgID(string OrgID)
        {
            return await QueryAsync<dynamic>(
                sql: @"select employee.id, employee.full_name from employee
                        INNER JOIN employee_type on employee.emp_type_id = employee_type.id
                        WHERE UPPER(employee_type.employee_type_name) = 'OUTSOURCED'
                        AND employee.org_id = @OrgID
                        AND employee.is_deleted = 0",
                param: new { OrgID }
            );
        }

        public async Task<dynamic> GetAllFreelancerEmpByOrgID(string OrgID)
        {
            return await QueryAsync<dynamic>(
                sql: @"select employee.id, employee.full_name from employee
                        INNER JOIN employee_type on employee.emp_type_id = employee_type.id
                        WHERE UPPER(employee_type.employee_type_name) = 'FREELANCER'
                        AND employee.org_id = @OrgID
                        AND employee.is_deleted = 0",
                param: new { OrgID }
            );
        }

        public async Task<dynamic> FindEmpDepartDesignByTeamID(string key)
        {
            return await QueryAsync<dynamic>(
                   sql: @" SELECT
	                        employee.id,
	                        employee.full_name,
	                        employee.workemail,
	                        employee.emp_code,
	                        employee_type.employee_type_name,
	                        department.dep_name,
	                        designation.designation_name
	                    FROM dbo.employee WITH(NOLOCK)
	                        LEFT JOIN employee_status ON employee.emp_status_id = employee_status.id
	                        LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
	                        LEFT JOIN AspNetRoles ON employee.role_id = AspNetRoles.id
	                        LEFT JOIN department ON employee.deptid = department.id
	                        LEFT JOIN designation ON employee.designation_id = designation.id
	                        LEFT JOIN team_members ON employee.id = team_members.emp_id
	                        WHERE team_members.team_id =  @key and employee.is_deleted = 0
	                        AND employee.is_superadmin = 0
	                        ORDER BY employee.full_name ASC",
                      param: new { key }
               );
        }

        public async Task SetEmployeeInactiveByEmpID(string key)
        {
             await ExecuteAsync(
                sql: @"UPDATE dbo.employee
                   SET
                    is_inactive = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public async Task SetDelegateeAsAdminByEmpID(string key)
        {
             await ExecuteAsync(
                sql: @"UPDATE dbo.employee
                   SET
                   is_admin = 1,
                   modified_date = GETDATE()
                   WHERE id = @key",
                param: new { key }
            );
        }

        public async Task SetDelegateeAsSuperAdminByEmpID(string key)
        {
             await ExecuteAsync(
                sql: @"UPDATE dbo.employee
                   SET
                   is_superadmin = 1,
                   modified_date = GETDATE()
                   WHERE id = @key",
                param: new { key }
            );
        }

        public async Task RemoveSuperAdminRightByEmpID(string key)
        {
             await ExecuteAsync(
                sql: @"UPDATE dbo.employee
                   SET
                   is_superadmin = 0,
                   modified_date = GETDATE()
                   WHERE id = @key",
                param: new { key }
            );
        }

        public async Task RemoveAdminRightByEmpID(string key)
        {
             await ExecuteAsync(
                sql: @"UPDATE dbo.employee
                   SET
                   is_admin = 0,
                   modified_date = GETDATE()
                   WHERE id = @key",
                param: new { key }
            );
        }

        public async Task<int> RemoveEmployeeIfZeroActivity(string key)
        {
            return await QuerySingleOrDefaultAsync<int>(
              sql: @"SELECT				
                   COUNT(dbo.timesheet_activity.id) as CoutActivity
                FROM
                    dbo.timesheet_activity  with(nolock)
                    INNER JOIN project on project.id = [timesheet_activity].project_id
                    INNER JOIN project_activity on project_activity.id =[timesheet_activity].milestone_id
                    LEFT JOIN task on dbo.timesheet_activity.task_id = task.id
                    INNER JOIN timesheet on dbo.timesheet_activity.groupid = timesheet.groupid
                    INNER JOIN timesheet_x_project_category on timesheet_x_project_category.groupid = dbo.timesheet_activity.groupid
                    AND dbo.timesheet_activity.is_deleted = 0
                    AND timesheet.empid = @key",
              param: new { key }
          );
        }

        public async Task<dynamic> GetOrganizationScreenshotDetails(string key)
        {
            return await QueryAsync<dynamic>(
                   sql: @" 	SELECT
				                employee.id as empid,
				                employee.org_id,
				                employee.full_name,
				                employee.workemail,
				                employee.emp_code,
				                employee_type.employee_type_name,
				                department.dep_name,
				                designation.designation_name,
				                organization.org_name,
				                organization_setup.screenshot_time,
				                organization_setup.is_screenshot,
				                organization_setup.is_track_app,
				                organization_setup.track_app_time
			                FROM dbo.employee WITH(NOLOCK)
 				                INNER JOIN organization ON employee.org_id = organization.org_id
 				                LEFT JOIN organization_setup ON organization.org_id = organization_setup.org_id
				                LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
 				                LEFT JOIN department ON employee.deptid = department.id
				                LEFT JOIN designation ON employee.designation_id = designation.id
 				                WHERE dbo.employee.user_id =  @key and employee.is_deleted = 0
				                ORDER BY employee.full_name ASC",
                      param: new { key }
               );
        }

        //Task IEmployeeRepository.RemoveSuperAdminRightByEmpID(string key)
        //{
        //    throw new System.NotImplementedException();
        //}
    }
}