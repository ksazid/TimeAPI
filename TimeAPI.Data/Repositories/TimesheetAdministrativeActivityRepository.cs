using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class TimesheetAdministrativeActivityRepository : RepositoryBase, ITimesheetAdministrativeRepository
    {
        public TimesheetAdministrativeActivityRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(TimesheetAdministrative entity)
        {

            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.timesheet_administrative_activity
                                  (id, administrative_id, groupid, purpose, remarks, ondate, start_time, end_time, is_billable, created_date, createdby)
                           VALUES (@id, @administrative_id, @groupid, @purpose, @remarks, @ondate, @start_time, @end_time, @is_billable, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public TimesheetAdministrative Find(string key)
        {
            return QuerySingleOrDefault<TimesheetAdministrative>(
                sql: "SELECT * FROM dbo.timesheet_administrative_activity WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_administrative_activity
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void RemoveByGroupID(string GroupID)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_administrative_activity
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE groupid = @GroupID",
                param: new { GroupID }
            );
        }

        public void Update(TimesheetAdministrative entity)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_administrative_activity
                   SET 
                    administrative_id = @administrative_id,
                    groupid = @groupid,
                    purpose = @purpose, 
                    remarks = @remarks, 
                    ondate = @ondate, 
                    start_time = @start_time, 
                    end_time = @end_time,
                    is_billable = @is_billable,
                    modified_date = @modified_date, 
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<TimesheetAdministrative> All()
        {
            return Query<TimesheetAdministrative>(
                sql: "SELECT * FROM [dbo].[timesheet_administrative_activity] where is_deleted = 0"
            );
        }

        public dynamic GetTop10TimesheetAdminActivityOnGroupIDAndAdminID(string GroupID, string AdminID)
        {
            return Query<dynamic>(
                sql: @"SELECT TOP 10 
                                timesheet_administrative_activity.id, 
                                administrative.administrative_name as task_name, 
                                timesheet_administrative_activity.purpose AS subtask_name, 
                                FORMAT(CAST(timesheet_administrative_activity.start_time AS DATETIME2), N'hh:mm tt') AS start_time, 
                                FORMAT(CAST(timesheet_administrative_activity.end_time AS DATETIME2), N'hh:mm tt') AS end_time, 
                                timesheet_administrative_activity.total_hrs,
                                timesheet_administrative_activity.is_billable,
                            FORMAT( timesheet_administrative_activity.ondate, 'dd/MM/yyyy', 'en-US' ) AS ondate
                            FROM 
                                [dbo].[timesheet_administrative_activity] WITH (NOLOCK)
                            INNER JOIN administrative on timesheet_administrative_activity.administrative_id = administrative.id
                            WHERE timesheet_administrative_activity.groupid = @GroupID AND administrative.id = @AdminID
                            ORDER BY timesheet_administrative_activity.ondate DESC",
                param: new { GroupID, AdminID }
            );
        }

    }
}
