using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class TimesheetProjectCategoryRepository : RepositoryBase, ITimesheetProjectCategoryRepository
    {
        public TimesheetProjectCategoryRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(TimesheetProjectCategory entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.timesheet_x_project_category
                                  (id,  groupid, project_category_type, project_or_comp_id, project_or_comp_name, project_or_comp_type, created_date, createdby)
                           VALUES (@id, @groupid, @project_category_type, @project_or_comp_id, @project_or_comp_name, @project_or_comp_type, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public async Task< TimesheetProjectCategory> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<TimesheetProjectCategory>(
                sql: "SELECT * FROM dbo.timesheet_x_project_category WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_x_project_category
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public async Task RemoveByGroupID(string GroupID)
        {
           await ExecuteAsync(
                sql: @"UPDATE dbo.timesheet_x_project_category
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE groupid = @GroupID",
                param: new { GroupID }
            );
        }

        public void Update(TimesheetProjectCategory entity)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_x_project_category
                   SET
                    groupid = @groupid,
                    project_category_type = @project_category_type,
                    project_or_comp_id = @project_or_comp_id,
                    project_or_comp_name = @project_or_comp_name,
                    project_or_comp_type = @project_or_comp_type,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public async Task<IEnumerable<TimesheetProjectCategory>> All()
        {
            return await  QueryAsync<TimesheetProjectCategory>(
                sql: "SELECT * FROM [dbo].[timesheet_x_project_category] where is_deleted = 0"
            );
        }
    }
}