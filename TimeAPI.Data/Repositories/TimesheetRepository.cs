using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class TimesheetRepository : RepositoryBase, ITimesheetRepository
    {
        public TimesheetRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(Timesheet entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.timesheet
                                  (id, empid, ondate, check_in, check_out, is_checkout, groupid, created_date, createdby)
                           VALUES (@id, @empid, @ondate, @check_in, @check_out, @is_checkout, @groupid, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Timesheet Find(string key)
        {
            return QuerySingleOrDefault<Timesheet>(
                sql: "SELECT * FROM dbo.timesheet WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public Timesheet FindTimeSheetByEmpID(string empid, string groupid)
        {
            return QuerySingleOrDefault<Timesheet>(
                sql: "SELECT * FROM dbo.timesheet WHERE is_deleted = 0 and empid = @empid and groupid = @groupid",
                param: new { empid, groupid }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void RemoveByGroupID(string GroupID)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE groupid = @GroupID",
                param: new { GroupID }
            );
        }

        public void Update(Timesheet entity)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet
                   SET
                    empid = @empid,
                    ondate = @ondate,
                    check_in = @check_in,
                    check_out = @check_out,
                    is_checkout = @is_checkout,
                    groupid = @groupid,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<Timesheet> All()
        {
            return Query<Timesheet>(
                sql: "SELECT * FROM [dbo].[timesheet] where is_deleted = 0"
            );
        }

        public void CheckOutByEmpID(Timesheet entity)
        {
            Execute(
                 sql: @"UPDATE dbo.timesheet
                   SET
                    check_out = @check_out,
                    is_checkout = @is_checkout,
                    total_hrs = @total_hrs,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE empid = @empid and groupid = @groupid",
                 param: entity
             );
        }
    }
}