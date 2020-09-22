using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class EmployeeAppUsageRepository : RepositoryBase, IEmployeeAppUsageRepository
    {
        public EmployeeAppUsageRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public void Add(EmployeeAppUsage entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.employee_app_usage
                                   (id, org_id, emp_id, start_time, end_time, idle_time, ondate, created_date, createdby)
                           VALUES (@id, @org_id, @emp_id, @start_time, @end_time, @idle_time, @ondate, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public async Task<EmployeeAppUsage> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<EmployeeAppUsage>(
                sql: "SELECT * FROM dbo.employee_app_usage WHERE id = @key and  is_deleted = 0",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.employee_app_usage
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(EmployeeAppUsage entity)
        {
            Execute(
                sql: @"UPDATE dbo.employee_app_usage
                   SET
                       org_id = @org_id, 
                        emp_id = @emp_id,
                        start_time = @start_time, 
                        end_time = @end_time, 
                        idle_time = @idle_time, 
                        modified_date = @modified_date,
                        modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public async Task<IEnumerable<EmployeeAppUsage>> All()
        {
            return await QueryAsync<EmployeeAppUsage>(
                sql: "SELECT * FROM dbo.employee_app_usage where is_deleted = 0"
            );
        }

        public async Task<EmployeeAppUsage> FindEmployeeAppUsageEmpID(string key)
        {
            return await QuerySingleOrDefaultAsync<EmployeeAppUsage>(
                sql: "SELECT * FROM dbo.employee_app_usage WHERE emp_id = @key and  is_deleted = 0",
                param: new { key }
            );
        }
    }
}