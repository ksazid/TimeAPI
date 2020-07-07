using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class EmployeeAppTrackedRepository : RepositoryBase, IEmployeeAppTrackedRepository
    {
        public EmployeeAppTrackedRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public void Add(EmployeeAppTracked entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.employee_app_tracked
                                   (id, emp_app_usage_id, emp_id, app_name, app_category_name, time_spend, icon, is_productive, is_unproductive, is_neutral, created_date, createdby)
                           VALUES (@id, @emp_app_usage_id, @emp_id, @app_name, @app_category_name, @time_spend, @icon, @is_productive, @is_unproductive, @is_neutral, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public EmployeeAppTracked Find(string key)
        {
            return QuerySingleOrDefault<EmployeeAppTracked>(
                sql: "SELECT * FROM dbo.employee_app_tracked WHERE id = @key and  is_deleted = 0",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.employee_app_tracked
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(EmployeeAppTracked entity)
        {
            Execute(
                sql: @"UPDATE dbo.employee_app_tracked
                   SET
                        emp_app_usage_id = @emp_app_usage_id,
                        emp_id = @emp_id,
                        app_name = @app_name, 
                        app_category_name = @app_category_name,
                        time_spend = @time_spend, 
                        icon = @icon,
                        is_productive = @is_productive, 
                        is_unproductive = @is_unproductive,
                        is_neutral = @is_neutral, 
                        modified_date = @modified_date,
                        modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<EmployeeAppTracked> All()
        {
            return Query<EmployeeAppTracked>(
                sql: "SELECT * FROM dbo.employee_app_tracked where is_deleted = 0"
            );
        }

        public EmployeeAppTracked FindEmployeeAppTrackedEmpID(string key)
        {
            return QuerySingleOrDefault<EmployeeAppTracked>(
                sql: "SELECT * FROM dbo.employee_app_tracked WHERE emp_id = @key and  is_deleted = 0",
                param: new { key }
            );
        }
    }
}