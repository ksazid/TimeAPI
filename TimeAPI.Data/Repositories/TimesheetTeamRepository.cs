using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;
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

        public TimesheetTeam Find(string key)
        {
            return QuerySingleOrDefault<TimesheetTeam>(
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

        public void RemoveByGroupID(string GroupID)
        {
            Execute(
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

        public IEnumerable<TimesheetTeam> All()
        {
            return Query<TimesheetTeam>(
                sql: "SELECT * FROM [dbo].[timesheet_x_team] where is_deleted = 0"
            );
        }

    }
}
