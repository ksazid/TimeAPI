using System.Collections.Generic;
using System.Data;
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
                                   emp_code, role_id, designation_id, dob, joined_date, phone, mobile, email, summary, created_date, createdby, is_admin, is_superadmin)
                    VALUES (@id, @org_id, @user_id, @deptid, @full_name, @first_name, @last_name, @alias,  @gender, @emp_status_id, @emp_type_id, @imgurl_id, @workemail, @emp_code,
                                  @role_id, @designation_id, @dob, @joined_date, @phone, @mobile, @email, @summary, @created_date, @createdby, @is_admin, @is_superadmin);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Employee Find(string key)
        {
            return QuerySingleOrDefault<Employee>(
                sql: "SELECT * FROM [dbo].[employee] WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<Employee> All()
        {
            return Query<Employee>(
                sql: "SELECT * FROM [dbo].[employee] where is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.employee
                   SET
                       modified_date = @modified_date, modifiedby = @modifiedby, is_deleted = 1
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

        public Employee FindByEmpName(string full_name)
        {
            return QuerySingleOrDefault<Employee>(
                sql: "SELECT * FROM [dbo].[employee] WHERE full_name = @full_name and is_deleted = 0",
                param: new { full_name }
            );
        }

        public IEnumerable<Employee> FindByOrgIDCode(string OrgID)
        {
            return Query<Employee>(
                sql: "SELECT * FROM [dbo].[employee] WHERE org_id = @OrgID and is_deleted = 0",
                param: new { OrgID }
            );
        }

        public Employee FindByEmpCode(string emp_code)
        {
            return QuerySingleOrDefault<Employee>(
                sql: "SELECT * FROM [dbo].[employee] WHERE emp_code = @emp_code and is_deleted = 0",
                param: new { emp_code }
            );
        }

        public IEnumerable<Employee> FindByRoleName(string role)
        {
            return Query<Employee>(
                sql: "SELECT * FROM [dbo].[employee] WHERE role_id = @role and is_deleted = 0",
                param: new { role }
            );
        }

        public dynamic FetchGridDataEmployeeByOrgID(string key)
        {
            return Query<dynamic>(
                   sql: @"SELECT 
	                        employee.id,
	                        employee.full_name,
	                        employee.workemail,
	                        employee.emp_code,
	                        employee_status.employee_status_name,
	                        employee_type.employee_type_name,
	                        AspNetRoles.Name as role_name,
	                        department.dep_name,
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



        public IEnumerable<Employee> FindEmployeeListByDesignationID(string DesignationID)
        {
            return Query<Employee>(
                sql: @"SELECT  * FROM dbo.employee
                            INNER JOIN designation ON employee.designation_id = designation.id
                        WHERE  employee.is_deleted = 0
                        AND employee.is_superadmin = 0
                        AND designation.id = @DesignationID
                        ORDER BY employee.full_name ASC",
                param: new { DesignationID }
            );
        }

        public IEnumerable<Employee> FindEmployeeListByDepartmentID(string DepartmentID)
        {
            return Query<Employee>(
                sql: @"SELECT  * FROM dbo.employee
                        INNER JOIN department ON employee.deptid = department.id
                        WHERE  employee.is_deleted = 0
                        AND employee.is_superadmin = 0 
                        AND department.id = @DepartmentID
                        ORDER BY employee.full_name ASC",
                param: new { DepartmentID }
            );
        }

    }
}
