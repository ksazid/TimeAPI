using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class TimesheetTeamRepository : RepositoryBase, ITimesheetTeamRepository
    {
        public TimesheetTeamRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(TimesheetTeam entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.timesheet_x_team
                                  (id,  groupid, teamid, created_date, createdby)
                           VALUES (@id, @groupid, @teamid, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public async Task<TimesheetTeam> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<TimesheetTeam>(
                sql: "SELECT * FROM dbo.timesheet_x_team WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_x_team
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public async Task RemoveByGroupID(string GroupID)
        {
            await ExecuteAsync(
                   sql: @"UPDATE dbo.timesheet_x_team
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE groupid = @GroupID",
                   param: new { GroupID }
               );
        }

        public void Update(TimesheetTeam entity)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_x_team
                   SET
                    groupid = @groupid,
                    teamid = @teamid,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public async Task<IEnumerable<TimesheetTeam>> All()
        {
            return await QueryAsync<TimesheetTeam>(
                sql: "SELECT * FROM [dbo].[timesheet_x_team] where is_deleted = 0"
            );
        }
    }
}