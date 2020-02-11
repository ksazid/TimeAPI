using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class TimesheetActivityRepository : RepositoryBase, ITimesheetActivityRepository
    {
        public TimesheetActivityRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(TimesheetActivity entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.timesheet_activity
                                  (id, groupid, task_id, task_name, remarks, ondate, start_time, end_time, total_hrs, is_billable, created_date, createdby)
                           VALUES (@id, @groupid, @task_id, @task_name, @remarks, @ondate, @start_time, @end_time, @total_hrs, @is_billable, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public TimesheetActivity Find(string key)
        {
            return QuerySingleOrDefault<TimesheetActivity>(
                sql: "SELECT * FROM dbo.timesheet_activity WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        //public TimesheetActivity FindTimeSheetByEmpID(string empid, string groupid)
        //{
        //    return QuerySingleOrDefault<TimesheetActivity>(
        //        sql: "SELECT * FROM dbo.timesheet_activity WHERE is_deleted = 0 and empid = @empid and groupid = @groupid",
        //        param: new { empid, groupid }
        //    );
        //}

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_activity
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void RemoveByGroupID(string GroupID)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_activity
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE groupid = @GroupID",
                param: new { GroupID }
            );
        }

        public void Update(TimesheetActivity entity)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_activity
                   SET
                    groupid = @groupid,
                    task_id = @task_id,
                    task_name = @task_name,
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

        public IEnumerable<TimesheetActivity> All()
        {
            return Query<TimesheetActivity>(
                sql: "SELECT * FROM [dbo].[timesheet_activity] where is_deleted = 0"
            );
        }

        public dynamic GetTop10TimesheetActivityOnTaskID(string TaskID)
        {
            return Query<dynamic>(
                sql: @"SELECT TOP 10
                        timesheet_activity.id,
                        timesheet_activity.task_name AS task_name,
                        FORMAT(CAST(timesheet_activity.start_time AS DATETIME2), N'hh:mm tt') AS start_time,
                        FORMAT(CAST(timesheet_activity.end_time AS DATETIME2), N'hh:mm tt') AS end_time,
                        timesheet_activity.total_hrs,
                        timesheet_activity.is_billable,
                        FORMAT(timesheet_activity.ondate, 'dd/MM/yyyy', 'en-US') AS ondate
                FROM
                    [dbo].[timesheet_activity] WITH (NOLOCK)
                    INNER JOIN task on timesheet_activity.task_id = task.id
                    WHERE task.id = @TaskID
                    ORDER BY timesheet_activity.ondate DESC",
                param: new { TaskID }
            );
        }

        public dynamic GetTimesheetActivityByGroupAndProjectID(string GroupID, string ProjectID)
        {
            return Query<dynamic>(
                sql: @"SELECT 
                            DISTINCT(timesheet_activity.groupid), [dbo].[timesheet_x_project_category].project_category_type + ': ' 
		                            + [dbo].[project].project_name + ' ('
		                            + FORMAT(CAST([dbo].[timesheet].check_in AS DATETIME2), N'hh:mm tt')  + ' - '
		                            + FORMAT(CAST([dbo].[timesheet].check_out AS DATETIME2), N'hh:mm tt')  + ' | ' 
		                            + [dbo].[timesheet].total_hrs +  ' )' as timesheet, 
                            [dbo].[project_activity].activity_name, 
                            [dbo].[timesheet_activity].task_name,
                            [dbo].[timesheet_activity].remarks,
                            FORMAT(CAST([dbo].[timesheet_activity].start_time AS DATETIME2), N'hh:mm tt') as start_time ,
                            FORMAT(CAST([dbo].[timesheet_activity].end_time AS DATETIME2), N'hh:mm tt') as end_time
                            from timesheet_activity
                            INNER JOIN timesheet on timesheet_activity.groupid = timesheet.groupid
                            INNER JOIN [dbo].[project_activity_x_task] ON [dbo].[timesheet_activity].task_id = [dbo].[project_activity_x_task].task_id
                            INNER JOIN  [dbo].[project] on [dbo].[project_activity_x_task].project_id = project.id
                            INNER JOIN  [dbo].[project_activity] on [dbo].[project].id = [dbo].[project_activity].project_id
                            INNER JOIN [dbo].[timesheet_x_project_category] on [dbo].[project].id = [dbo].[timesheet_x_project_category].project_or_comp_id
                            where [dbo].[timesheet_activity].groupid = @GroupID
                            AND [dbo].[timesheet_activity].is_deleted  = 0
                            AND [dbo].[project].id  = @ProjectID
                            ORDER BY FORMAT(CAST([dbo].[timesheet_activity].end_time AS DATETIME2), N'hh:mm tt') ASC",
                param: new { GroupID, ProjectID }
            );
        }
    }
}