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
                                  (id, groupid, project_id, milestone_id, milestone_name, task_id, task_name, remarks, ondate, start_time, end_time, total_hrs, is_billable, created_date, createdby)
                           VALUES (@id, @groupid, @project_id, @milestone_id, @milestone_name, @task_id, @task_name, @remarks, @ondate, @start_time, @end_time, @total_hrs, @is_billable, @created_date, @createdby);
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
                    project_id =@project_id, 
                    milestone_id = @milestone_id, 
                    milestone_name = @milestone_name,
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

        //public dynamic GetTimesheetActivityByGroupAndProjectID(string GroupID, string ProjectID, string Date)
        //{
        //    return Query<dynamic>(
        //        sql: @"SELECT        
        //       UPPER(dbo.timesheet_x_project_category.project_category_type) + ': '
        //     + dbo.project.project_name + ' (' 
        //     + ISNULL(NULLIF(FORMAT(CAST(eTime.check_in AS DATETIME2), N'hh:mm tt'), ' '), 'NA') + ' - '
        //     + ISNULL(NULLIF(FORMAT(CAST(eTime.check_out AS DATETIME2), N'hh:mm tt'), ' '), 'NA')  + ' | '  
        //     + ISNULL(NULLIF(eTime.total_hrs, ' '), 'NA') +  ' )' as timesheet, 
        //     dbo.project_activity.activity_name,
        //     dbo.timesheet_activity.task_name,
        //     dbo.timesheet_activity.remarks,
        //     FORMAT(CAST(dbo.timesheet_activity.start_time AS DATETIME2), N'hh:mm tt') as start_time ,
        //     FORMAT(CAST(dbo.timesheet_activity.end_time AS DATETIME2), N'hh:mm tt') as end_time,
        //     timesheet_activity.total_hrs,
        //     timesheet_activity.is_billable,
        //     FORMAT(dbo.timesheet_activity.ondate, 'dd-MM-yyyy', 'en-US') AS ondate
        //                FROM
        //                    [dbo].[timesheet_activity] WITH (NOLOCK)
        //                        INNER JOIN task on dbo.timesheet_activity.task_id = task.id
        //     INNER JOIN project_activity_x_task on project_activity_x_task.task_id = task.id 
        //     INNER JOIN project_activity on project_activity.id = project_activity_x_task.activity_id
        //     INNER JOIN project on project.id = project_activity_x_task.project_id 
        //     INNER JOIN (select top 1 * from dbo.timesheet   where groupid IN (SELECT groupid FROM timesheet_activity 
        //                    WHERE dbo.timesheet_activity.groupid = @GroupID)) eTime
        //     on eTime.groupid = dbo.timesheet_activity.groupid
        //     INNER JOIN timesheet_x_project_category on timesheet_x_project_category.groupid = dbo.timesheet_activity.groupid
        //                    WHERE dbo.timesheet_activity.groupid =@GroupID
        //        AND [dbo].[timesheet_activity].is_deleted  = 0
        //                    AND FORMAT(CAST(timesheet_activity.ondate AS DATE), 'd', 'EN-US') = FORMAT(CAST(@Date AS DATE), 'd', 'EN-US')
        //        AND [dbo].[project].id  = @ProjectID
        //    ORDER BY FORMAT(CAST([dbo].[timesheet_activity].end_time AS DATETIME2), N'hh:mm tt') DESC",
        //        param: new { GroupID, ProjectID, Date }
        //    );
        //}


        public dynamic GetTimesheetActivityByGroupAndProjectID(string GroupID, string ProjectID, string Date)
        {
            return Query<dynamic>(
                sql: @"SELECT    
		                UPPER(dbo.timesheet_x_project_category.project_category_type) + ': '
			                        + [dbo].[administrative].administrative_name + ' (' 
			                        + ISNULL(NULLIF(FORMAT(CAST(eTime.check_in AS DATETIME2), N'hh:mm tt'), ' '), 'NA') + ' - '
			                        + ISNULL(NULLIF(FORMAT(CAST(eTime.check_out AS DATETIME2), N'hh:mm tt'), ' '), 'NA')  + ' | '  
			                        + ISNULL(NULLIF(eTime.total_hrs, ' '), 'NA') +  ' )' as timesheet, 
			                        activity_name = [dbo].[administrative].administrative_name,
			                        task_name = dbo.timesheet_administrative_activity.purpose,
			                        remarks = dbo.timesheet_administrative_activity.remarks,
			                        FORMAT(CAST(dbo.timesheet_administrative_activity.start_time AS DATETIME2), N'hh:mm tt') as start_time ,
			                        FORMAT(CAST(dbo.timesheet_administrative_activity.end_time AS DATETIME2), N'hh:mm tt') as end_time,
			                        timesheet_administrative_activity.total_hrs,
			                        timesheet_administrative_activity.is_billable,
			                        FORMAT(dbo.timesheet_administrative_activity.ondate, 'dd-MM-yyyy', 'en-US') AS ondate 
                        FROM
                            dbo.timesheet_administrative_activity WITH (NOLOCK)
	                        LEFT JOIN (select top 1 * from dbo.timesheet   where groupid IN (SELECT groupid FROM timesheet_activity 
                            WHERE dbo.timesheet_activity.groupid = @GroupID)) eTime
	                        on eTime.groupid = dbo.timesheet_administrative_activity.groupid
	                        LEFT JOIN timesheet_x_project_category on timesheet_x_project_category.groupid = dbo.timesheet_administrative_activity.groupid
	                        LEFT JOIN (select top 1 * from dbo.timesheet_location  WHERE groupid IN (SELECT groupid FROM timesheet_activity 
                            WHERE dbo.timesheet_activity.groupid = @GroupID)) eTimeLocation
	                        ON eTimeLocation.groupid = dbo.timesheet_administrative_activity.groupid
	                        LEFT JOIN  [dbo].[administrative] on   [dbo].[timesheet_administrative_activity].administrative_id = [dbo].[administrative].id 
                            WHERE dbo.timesheet_administrative_activity.groupid =@GroupID
	                        AND [dbo].timesheet_administrative_activity.is_deleted  = 0
                            AND FORMAT(CAST(timesheet_administrative_activity.ondate AS DATE), 'd', 'EN-US') = FORMAT(CAST(GETDATE() AS DATE), 'd', 'EN-US')

                        UNION 

                        SELECT  
		                        UPPER(dbo.timesheet_x_project_category.project_category_type) + ': '
		                        + ISNULL(dbo.project.project_name, 'Activity') + ' (' 
		                        + ISNULL(NULLIF(FORMAT(CAST(eTime.check_in AS DATETIME2), N'hh:mm tt'), ' '), 'NA') + ' - '
		                        + ISNULL(NULLIF(FORMAT(CAST(eTime.check_out AS DATETIME2), N'hh:mm tt'), ' '), 'NA')  + ' | '  
		                        + ISNULL(NULLIF(eTime.total_hrs, ' '), 'NA') +  ' )' as timesheet, 
		                        activity_name =ISNULL( dbo.project_activity.activity_name, 'NA'),
		                        task_name = dbo.timesheet_activity.task_name,
		                        remarks= dbo.timesheet_activity.remarks,
		                        FORMAT(CAST(dbo.timesheet_activity.start_time AS DATETIME2), N'hh:mm tt') as start_time ,
		                        FORMAT(CAST(dbo.timesheet_activity.end_time AS DATETIME2), N'hh:mm tt') as end_time,
		                        timesheet_activity.total_hrs,
		                        timesheet_activity.is_billable,
		                        FORMAT(dbo.timesheet_activity.ondate, 'dd-MM-yyyy', 'en-US') AS ondate 
                                    FROM
                                        [dbo].[timesheet_activity] WITH (NOLOCK)
                                        LEFT JOIN task on dbo.timesheet_activity.task_id = task.id
				                        LEFT JOIN project_activity_x_task on project_activity_x_task.task_id = task.id 
				                        LEFT JOIN project_activity on project_activity.id = project_activity_x_task.activity_id
				                        LEFT JOIN project on project.id = project_activity_x_task.project_id 
				                        LEFT JOIN (select top 1 * from dbo.timesheet   where groupid IN (SELECT groupid FROM timesheet_activity 
                                        WHERE dbo.timesheet_activity.groupid = @GroupID)) eTime
				                        on eTime.groupid = dbo.timesheet_activity.groupid
				                        LEFT JOIN timesheet_x_project_category on timesheet_x_project_category.groupid = dbo.timesheet_activity.groupid
                                        WHERE dbo.timesheet_activity.groupid =@GroupID
				                        AND [dbo].[timesheet_activity].is_deleted  = 0
                                        AND FORMAT(CAST(timesheet_activity.ondate AS DATE), 'd', 'EN-US') = FORMAT(CAST(GETDATE() AS DATE), 'd', 'EN-US')",
                param: new { GroupID, ProjectID, Date }
            );
        }

        public dynamic GetTimesheetActivityByEmpID(string EmpID, string StartDate, string EndDate)
        {
            return Query<dynamic>(
                sql: @"SELECT				
				    UPPER(dbo.timesheet_x_project_category.project_category_type) + ': '
					+ dbo.project.project_name + ' (' 
					+ ISNULL(NULLIF(FORMAT(CAST(timesheet.check_in AS DATETIME2), N'hh:mm tt'), ' '), 'NA') + ' - '
					+ ISNULL(NULLIF(FORMAT(CAST(timesheet.check_out AS DATETIME2), N'hh:mm tt'), ' '), 'NA')  + ' | '  
					+ ISNULL(NULLIF(timesheet.total_hrs, ' '), 'NA') +  ' )' as timesheet, 
					dbo.project_activity.activity_name,
					dbo.timesheet_activity.task_name,
					dbo.timesheet_activity.remarks,
					FORMAT(CAST(dbo.timesheet_activity.start_time AS DATETIME2), N'hh:mm tt') as start_time ,
					FORMAT(CAST(dbo.timesheet_activity.end_time AS DATETIME2), N'hh:mm tt') as end_time,
					timesheet_activity.total_hrs,
					timesheet_activity.is_billable,
					FORMAT(dbo.timesheet_activity.ondate, 'dd-MM-yyyy', 'en-US') AS ondate
                FROM
                    dbo.timesheet_activity  with(nolock)
                    INNER JOIN task on dbo.timesheet_activity.task_id = task.id
					INNER JOIN project_activity_x_task on project_activity_x_task.task_id = task.id 
					INNER JOIN project_activity on project_activity.id = project_activity_x_task.activity_id
					INNER JOIN project on project.id = project_activity_x_task.project_id 
					INNER JOIN timesheet on dbo.timesheet_activity.groupid = timesheet.groupid
					INNER JOIN timesheet_x_project_category on timesheet_x_project_category.groupid = dbo.timesheet_activity.groupid
				    AND dbo.timesheet_activity.is_deleted  = 0
                    AND FORMAT(CAST(timesheet_activity.ondate AS DATE), 'd', 'EN-US')
					BETWEEN FORMAT(CAST(@StartDate AS DATE), 'd', 'EN-US') 
					AND FORMAT(CAST(@EndDate AS DATE), 'd', 'EN-US') AND timesheet.empid = @EmpID

				ORDER BY FORMAT(CAST(dbo.timesheet_activity.end_time AS DATETIME2), N'hh:mm tt') DESC",
                param: new { EmpID, StartDate, EndDate }
            );
        }
    }
}