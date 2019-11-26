using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class SocialRepository : RepositoryBase, ISocialRepository
    {

        public SocialRepository(IDbTransaction transaction) : base(transaction)
        {

        }

        public void Add(Social entity)
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

        public Social Find(string key)
        {
            return QuerySingleOrDefault<Social>(
                sql: "SELECT * FROM [dbo].[employee] WHERE id = @key",
                param: new { key }
            );
        }


        public IEnumerable<Social> FindByEmpID(string role)
        {
            return Query<Social>(
                sql: "SELECT * FROM [dbo].[employee] WHERE role = @role",
                param: new { role }
            );
        }

        public IEnumerable<Social> All()
        {
            return Query<Social>(
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

        public void Update(Social entity)
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
