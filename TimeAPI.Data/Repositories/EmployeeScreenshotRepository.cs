using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class EmployeeScreenshotRepository : RepositoryBase, IEmployeeScreenshotRepository
    {
        public EmployeeScreenshotRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public void Add(EmployeeScreenshot entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.employee_screenshot
                                   (id, org_id, emp_id, img_name, img_url, created_date, createdby)
                           VALUES (@id, @org_id, @emp_id, @img_name, @img_url, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public EmployeeScreenshot Find(string key)
        {
            return QuerySingleOrDefault<EmployeeScreenshot>(
                sql: "SELECT * FROM dbo.employee_screenshot WHERE id = @key and  is_deleted = 0",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.employee_screenshot
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(EmployeeScreenshot entity)
        {
            Execute(
                sql: @"UPDATE dbo.employee_screenshot
                   SET
                       org_id = @org_id, 
                        emp_id = @emp_id,
                        img_name = @img_name,
                        img_url = @img_url,
                        modified_date = @modified_date,
                        modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<EmployeeScreenshot> All()
        {
            return Query<EmployeeScreenshot>(
                sql: "SELECT * FROM dbo.employee_screenshot where is_deleted = 0"
            );
        }

        public EmployeeScreenshot FindByProfileEmpiID(string key)
        {
            return QuerySingleOrDefault<EmployeeScreenshot>(
                sql: "SELECT * FROM dbo.employee_screenshot WHERE emp_id = @key and  is_deleted = 0",
                param: new { key }
            );
        }
    }
}