using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class TimesheetBreakRepository : RepositoryBase, ITimesheetBreakRepository
    {
        public TimesheetBreakRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(TimesheetBreak entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.timesheet_break
                                  (id, orgid, empid, groupid, ondate, break_in, break_out, is_breakout, total_hrs, created_date, createdby, is_deleted)
                           VALUES (@id, @orgid, @empid, @groupid, @ondate, @break_in, @break_out, @is_breakout, @total_hrs, @created_date, @createdby, @is_deleted);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public TimesheetBreak Find(string key)
        {
            return QuerySingleOrDefault<TimesheetBreak>(
                sql: "SELECT * FROM dbo.timesheet_break WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public TimesheetBreak FindTimeSheetBreakByEmpID(string empid, string groupid)
        {
            return QuerySingleOrDefault<TimesheetBreak>(
                sql: "SELECT top 1 * FROM dbo.timesheet_break WHERE is_deleted = 0 and empid = @empid and groupid = @groupid AND is_breakout = 0",
                param: new { empid, groupid }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_break
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void RemoveByGroupID(string GroupID)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_break
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE groupid = @GroupID",
                param: new { GroupID }
            );
        }

        public void Update(TimesheetBreak entity)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_break
                   SET
                    empid = @empid, 
                    groupid = @groupid, 
                    ondate = @ondate, 
                    break_in = @break_in, 
                    break_out = @break_out, 
                    is_breakout = @is_breakout, 
                    total_hrs = @total_hrs,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<TimesheetBreak> All()
        {
            return Query<TimesheetBreak>(
                sql: "SELECT * FROM dbo.timesheet_break where is_deleted = 0"
            );
        }

        public IEnumerable<string> GetAllEmpByGroupID(string GroupID)
        {
            return Query<string>(
           sql: @"SELECT full_name FROM employee 
                INNER JOIN timesheet_break on employee.id = timesheet_break.empid
                WHERE timesheet_break.groupid = @GroupID",
                param: new { GroupID }
            );
        }

        public dynamic GetAllTimesheetBreakByOrgID(string OrgID)
        {
            return Query<dynamic>(
           sql: "SELECT * FROM dbo.timesheet_break where is_deleted = 0 AND orgid= @OrgID",
                param: new { OrgID }
            );
        }

        public void BreakOutByEmpIDAndGrpID(TimesheetBreak entity)
        {
            Execute(
                 sql: @"UPDATE dbo.timesheet_break
                   SET
                    break_out = @break_out, 
                    is_breakout = @is_breakout, 
                    total_hrs = @total_hrs,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE empid = @empid AND groupid = @groupid AND is_breakout = 0",
                 param: entity
             );
        }

        public IEnumerable<TimesheetBreak> FindLastTimeSheetBreakByEmpIDAndGrpID(string empid, string groupid)
        {
            return Query<TimesheetBreak>(
                sql: "SELECT * FROM dbo.timesheet_break WHERE is_deleted = 0 and empid = @empid and groupid = @groupid AND is_breakout=0",
                param: new { empid, groupid }
            );
        }

    }
}