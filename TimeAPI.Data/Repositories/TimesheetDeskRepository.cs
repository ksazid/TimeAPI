using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class TimesheetDeskRepository : RepositoryBase, ITimesheetDeskRepository
    {
        public TimesheetDeskRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(TimesheetDesk entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.timesheet_desk
                                  (id, empid, ondate, check_in, check_out, is_checkout, groupid, created_date, createdby)
                           VALUES (@id, @empid, @ondate, @check_in, @check_out, @is_checkout, @groupid, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public TimesheetDesk Find(string key)
        {
            return QuerySingleOrDefault<TimesheetDesk>(
                sql: "SELECT * FROM dbo.timesheet_desk WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public TimesheetDesk FindTimeSheetDeskByEmpID(string empid, string groupid)
        {
            return QuerySingleOrDefault<TimesheetDesk>(
                sql: "SELECT * FROM dbo.timesheet_desk WHERE is_deleted = 0 and empid = @empid and groupid = @groupid",
                param: new { empid, groupid }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_desk
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void RemoveByGroupID(string GroupID)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_desk
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE groupid = @GroupID",
                param: new { GroupID }
            );
        }

        public void Update(TimesheetDesk entity)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_desk
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

        public IEnumerable<TimesheetDesk> All()
        {
            return Query<TimesheetDesk>(
                sql: "SELECT * FROM [dbo].[timesheet] where is_deleted = 0"
            );
        }

        public void CheckOutByEmpID(TimesheetDesk entity)
        {
            Execute(
                 sql: @"UPDATE dbo.timesheet_desk
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

        public dynamic GetAllTimesheetDeskByOrgID(string OrgID)
        {
            return Query<dynamic>(
           sql: "SELECT * FROM [dbo].[timesheet] where is_deleted = 0 AND timesheet.groupid = @OrgID",
                param: new { OrgID }
            );
        }

        public IEnumerable<string> GetAllEmpByGroupID(string OrgID)
        {
            return Query<string>(
           sql: @"SELECT full_name FROM employee 
                INNER JOIN timesheet on employee.id = timesheet.empid
                WHERE timesheet.groupid = @OrgID",
                param: new { OrgID }
            );
        }
    }
}