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
                    INSERT INTO [dbo].[employee] (id, user_id, deptid, full_name, first_name,last_name, alias, gender, emp_status, emp_type, imgurl, workemail, 
                                   emp_code, role, designation, dob, joined_date, phone, mobile, email, summary, created_date, createdby, modified_date, modifiedby,
                                   is_deleted, is_admin)
                    VALUES (@id, @user_id, @deptid, @full_name, @first_name, @last_name, @alias,  @gender, @emp_status, @emp_type, @imgurl, @workemail,  @emp_code,
                                  @role, @designation, @dob, @joined_date, @phone, @mobile, @email, @summary, @created_date, @createdby, @modified_date, @modifiedby,
                                  @is_deleted, @is_admin);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Employee Find(string key)
        {
            return QuerySingleOrDefault<Employee>(
                sql: "SELECT * FROM [dbo].[employee] WHERE id = @key",
                param: new { key }
            );
        }

        public Employee FindByEmpName(string full_name)
        {
            return QuerySingleOrDefault<Employee>(
                sql: "SELECT * FROM [dbo].[employee] WHERE full_name = @full_name",
                param: new { full_name }
            );
        }

        public IEnumerable<Employee> FindByOrgIDCode(string OrgID)
        {
            return Query<Employee>(
                sql: "SELECT * FROM [dbo].[employee] WHERE org_id = @OrgID",
                param: new { OrgID }
            );
        }
        public Employee FindByEmpCode(string emp_code)
        {
            return QuerySingleOrDefault<Employee>(
                sql: "SELECT * FROM [dbo].[employee] WHERE emp_code = @emp_code",
                param: new { emp_code }
            );
        }

        public IEnumerable<Employee> FindByRoleName(string role)
        {
            return Query<Employee>(
                sql: "SELECT * FROM [dbo].[employee] WHERE role = @role",
                param: new { role }
            );
        }

        public IEnumerable<Employee> All()
        {
            return Query<Employee>(
                sql: "SELECT * FROM [dbo].[employee]"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.employee
                   SET
                       modified_date = @modified_date, modifiedby = @modifiedby, is_deleted = 1, is_admin = @is_admin
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
                       gender = @gender, emp_status = @emp_status, emp_type = @emp_type, imgurl = @imgurl, workemail = @workemail, 
                       emp_code = @emp_code, role = @role, designation = @designationdob = @dob, joined_date = @joined_date, 
                       phone = @phone, mobile = @mobile, summary = @summary, created_date = @created_date, createdby = @createdby, 
                       modified_date = @modified_date, modifiedby = @modifiedby, is_deleted = @is_deleted, is_admin = @is_admin
                    WHERE id = @id",
                param: entity
            );
        }

    }
}
