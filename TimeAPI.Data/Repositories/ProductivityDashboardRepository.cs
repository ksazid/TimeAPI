﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class ProductivityDashboardRepository : RepositoryBase, IProductivityDashboardRepository
    {
        public ProductivityDashboardRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        #region

        public void Add(ProductivityDashboard entity)
        {
            Execute(
                sql: @"",
                param: entity
            );
        }

        public void Update(ProductivityDashboard entity)
        {
            Execute(
                sql: @"",
                param: entity
            );
        }

        public IEnumerable<ProductivityDashboard> All()
        {
            return Query<ProductivityDashboard>(
                sql: ""
            );
        }

        public ProductivityDashboard Find(string key)
        {
            return QuerySingleOrDefault<ProductivityDashboard>(
                sql: "",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: "",
                param: new { key }
            );
        }

        #endregion

        public dynamic EmployeeProductivityPerDateByEmpIDAndDate(string EmpID, string StartDate, string EndDate)
        {
            return Query<dynamic>(
                 sql: @"SELECT
                            emp_id,
                            FORMAT(CAST(check_in AS DATETIME2), N'hh:mm tt') AS check_in,
                            ISNULL(FORMAT(CAST(check_out AS DATETIME2), N'hh:mm tt'), '-') AS check_out,
                            ISNULL(break_hours, '00:00') AS break_hours,
                            CONVERT(VARCHAR(5), DATEADD (MINUTE, (DATEDIFF(MINUTE, check_in, check_out)), 0), 114) AS total_hrs,
                            ISNULL(CONVERT(VARCHAR(5), DATEADD(MINUTE,(DATEDIFF(MINUTE, ISNULL(break_hours , '00:00'), 
	                        CONVERT(VARCHAR(5), DATEADD(MINUTE, (DATEDIFF(MINUTE, check_in, check_out)), 0), 114))), 0 ), 114), '00:00') AS final_total_hours,
                            FORMAT(CAST(ondate AS DATE), 'MM/dd/yyyy', 'EN-US') AS ondate,
	                        ISNULL(activityCount, 0) +  ISNULL(adminactivityCount, 0) as activity_count,
 	                        ISNULL(CONVERT(VARCHAR(5),  ISNULL(DATEADD(s, ((DATEPART(hh, activity_hours) * 3600 ) + (DATEPART(mi, activity_hours) * 60 ) + DATEPART(ss, activity_hours)), 0), 0) +
								                        ISNULL(DATEADD(s, (( DATEPART(hh, adminactivity_hours) * 3600 ) + (DATEPART(mi, adminactivity_hours) * 60 ) + DATEPART(ss, adminactivity_hours)),0), 0)
	                        , 114), '00:00')  AS time_spend_activity,
                          (DATEDIFF(minute, 0,  ISNULL(DATEADD(s, ((DATEPART(hh, activity_hours) * 3600 ) + (DATEPART(mi, activity_hours) * 60 ) + DATEPART(ss, activity_hours)), 0), 0) +
                          
                           ISNULL(DATEADD(s, ((DATEPART(hh, adminactivity_hours) * 3600 ) + (DATEPART(mi, adminactivity_hours) * 60 ) + DATEPART(ss, adminactivity_hours)),0), 0))) 
                           * 100 /  (DATEDIFF(minute, 0,  ISNULL(DATEADD(s, ((DATEPART(hh, check_out) * 3600 ) + (DATEPART(mi, check_out) * 60 ) + DATEPART(ss, check_out)), 0), 0) -
                           ISNULL(DATEADD(s, ((DATEPART(hh, check_in) * 3600 ) + (DATEPART(mi, check_in) * 60 ) + DATEPART(ss, check_in)),0), 0)))  as productivity_ratio
 
                        FROM (

                            SELECT
                                timesheet.empid AS emp_id,
                                timesheet.groupid AS groupid,
                                timesheetFirst.check_in AS check_in,
                                timesheetLast.check_out AS check_out,
                                timesheetbreak.break_hrs AS break_hours,
                                timesheet.ondate,
                                timesheetactivity.activityCount,
                                timesheetactivity.activity_hours,
                                timesheetadministrative.adminactivityCount,
                                timesheetadministrative.adminactivity_hours
                            FROM timesheet
                            INNER JOIN (SELECT
                                timesheet.id,
                                timesheet.groupid,
                                ISNULL(FORMAT(CAST(check_in AS datetime2), N'hh:mm tt'), '00:00') AS check_in,
                                FORMAT(CAST(ondate AS date), 'MM/dd/yyyy', 'EN-US') AS ondate,
                                empid
                            FROM timesheet
                            WHERE FORMAT(CAST(ondate AS datetime2), N'dd-MMM-yyyy HH:mm:ss', 'EN-US') IN (SELECT
                                MIN(FORMAT(CAST(ondate AS datetime2), N'dd-MMM-yyyy HH:mm:ss', 'EN-US'))
                            FROM timesheet
                            WHERE FORMAT(CAST(ondate AS date), 'MM/dd/yyyy', 'EN-US')
                            BETWEEN FORMAT(CAST(@StartDate AS date), 'MM/dd/yyyy', 'EN-US')
                            AND FORMAT(CAST(@EndDate AS date), 'MM/dd/yyyy', 'EN-US')
                            AND empid = @EmpID
                            AND timesheet.is_deleted = 0
                            GROUP BY FORMAT(CAST(ondate AS date), 'MM/dd/yyyy', 'EN-US'))) AS timesheetFirst
                                ON dbo.timesheet.groupid = timesheetFirst.groupid
                            LEFT JOIN (SELECT
                                timesheet.id,
                                timesheet.groupid,
                                ISNULL(FORMAT(CAST(check_out AS datetime2), N'hh:mm tt'), NULL) AS check_out,
                                FORMAT(CAST(ondate AS date), 'MM/dd/yyyy', 'EN-US') AS ondate,
                                empid
                            FROM timesheet
                            WHERE ondate IN (SELECT
                                MAX(ondate)
                            FROM timesheet
                            WHERE FORMAT(CAST(ondate AS date), 'MM/dd/yyyy', 'EN-US')
                            BETWEEN FORMAT(CAST(@StartDate AS date), 'MM/dd/yyyy', 'EN-US')
                            AND FORMAT(CAST(@EndDate AS date), 'MM/dd/yyyy', 'EN-US')
                            AND empid = @EmpID
                            AND timesheet.is_deleted = 0
                            GROUP BY FORMAT(CAST(ondate AS date), 'MM/dd/yyyy', 'EN-US'))) AS timesheetLast
                                ON timesheetFirst.ondate = timesheetLast.ondate
                            LEFT JOIN (SELECT
                                timesheet.empid,
                                FORMAT(CAST(timesheet.ondate AS date), 'MM/dd/yyyy', 'EN-US') AS ondate,
                                CONVERT(varchar(5), DATEADD(MINUTE, SUM(DATEDIFF(MINUTE, timesheet_break.break_in, timesheet_break.break_out)), 0), 114) AS break_hrs
                            FROM timesheet_break
                            LEFT JOIN timesheet
                                ON timesheet_break.groupid = timesheet.groupid
                            WHERE FORMAT(CAST(timesheet.ondate AS date), 'MM/dd/yyyy', 'EN-US')
                            BETWEEN FORMAT(CAST(@StartDate AS date), 'MM/dd/yyyy', 'EN-US')
                            AND FORMAT(CAST(@EndDate AS date), 'MM/dd/yyyy', 'EN-US')
                            AND timesheet.empid = @EmpID
                            AND timesheet_break.is_deleted = 0
                            GROUP BY FORMAT(CAST(timesheet.ondate AS date), 'MM/dd/yyyy', 'EN-US'),
                                        timesheet.empid) AS timesheetbreak
                                ON timesheetFirst.ondate = timesheetbreak.ondate
                            LEFT JOIN (SELECT
                                COUNT(*) AS activityCount,
                                FORMAT(CAST(timesheet_activity.ondate AS date), 'MM/dd/yyyy', 'EN-US') AS ondate,
                                CONVERT(varchar(5), DATEADD(MINUTE, SUM(DATEDIFF(MINUTE, timesheet_activity.start_time, timesheet_activity.end_time)), 0), 114) AS activity_hours
                            FROM timesheet_activity
                            LEFT JOIN timesheet
                                ON timesheet_activity.groupid = timesheet.groupid
                            WHERE FORMAT(CAST(timesheet.ondate AS date), 'MM/dd/yyyy', 'EN-US')
                            BETWEEN FORMAT(CAST(@StartDate AS date), 'MM/dd/yyyy', 'EN-US')
                            AND FORMAT(CAST(@EndDate AS date), 'MM/dd/yyyy', 'EN-US')
                            AND timesheet.empid = @EmpID
                            AND timesheet_activity.is_deleted = 0
                            GROUP BY FORMAT(CAST(timesheet_activity.ondate AS date), 'MM/dd/yyyy', 'EN-US')) AS timesheetactivity
                                ON timesheetFirst.ondate = timesheetactivity.ondate
                            LEFT JOIN (SELECT
                                COUNT(*) AS adminactivityCount,
                                FORMAT(CAST(timesheet_administrative_activity.ondate AS date), 'MM/dd/yyyy', 'EN-US') AS ondate,
                                CONVERT(varchar(5), DATEADD(MINUTE, SUM(DATEDIFF(MINUTE, timesheet_administrative_activity.start_time, timesheet_administrative_activity.end_time)), 0), 114) AS adminactivity_hours
                            FROM timesheet_administrative_activity
                            LEFT JOIN timesheet
                                ON timesheet_administrative_activity.groupid = timesheet.groupid
                            WHERE FORMAT(CAST(timesheet_administrative_activity.ondate AS date), 'MM/dd/yyyy', 'EN-US')
                            BETWEEN FORMAT(CAST(@StartDate AS date), 'MM/dd/yyyy', 'EN-US')
                            AND FORMAT(CAST(@EndDate AS date), 'MM/dd/yyyy', 'EN-US')
                            AND timesheet.empid = @EmpID
                            AND timesheet_administrative_activity.is_deleted = 0
                            GROUP BY FORMAT(CAST(timesheet_administrative_activity.ondate AS date), 'MM/dd/yyyy', 'EN-US')) AS timesheetadministrative
                                ON timesheetFirst.ondate = timesheetadministrative.ondate
                            WHERE timesheet.empid = @EmpID
                            AND FORMAT(CAST(timesheet.ondate AS date), 'MM/dd/yyyy', 'EN-US')
                            BETWEEN FORMAT(CAST(@StartDate AS date), 'MM/dd/yyyy', 'EN-US')
                            AND FORMAT(CAST(@EndDate AS date), 'MM/dd/yyyy', 'EN-US')
                            AND timesheet.is_deleted = 0) X
 
                            ORDER BY  FORMAT(CAST(ondate AS datetime2), N'dd-MMM-yyyy HH:mm:ss', 'EN-US') ASC",
                 param: new { EmpID, StartDate, EndDate }
             );
        }

        public dynamic DesktopEmployeeProductivityPerDateByEmpIDAndDate(string EmpID, string StartDate, string EndDate)
        {
            return Query<dynamic>(
                 sql: @"SELECT
                              emp_id,
                              FORMAT(CAST(check_in AS DATETIME2), N'hh:mm tt') AS check_in,
                              ISNULL(FORMAT(CAST(check_out AS DATETIME2), N'hh:mm tt'), '-') AS check_out,
                              ISNULL(private_time, '00:00') AS private_time,
							  CONVERT(VARCHAR(5), Dateadd (minute, (Datediff(minute, check_in, check_out)), 0), 114) AS total_hrs ,
							  ISNULL(productive_time, '00:00') AS productive_time,
                              ISNULL(productive_ratio, '0') AS productive_ratio,
                              ISNULL(CONVERT(varchar(5), DATEADD(minute, (DATEDIFF(minute, ISNULL(private_time, '00:00'), 
                                    CONVERT(VARCHAR(5), Dateadd (minute, (Datediff(minute, check_in, check_out)), 0), 114))), 0), 114), '00:00') 
                                    AS final_total_hours,
                              FORMAT(CAST(ondate AS DATE), 'MM/dd/yyyy', 'EN-US') as ondate 
                            FROM
                              (
                                SELECT
                                  timesheet_desk.empid as emp_id,
                                  timesheet_desk.groupid as groupid,
                                  timesheet_desk.check_in as check_in,
                                  timesheetLast.check_out as check_out,
                                  timesheetPrivate.privatetime as private_time,
                                  timesheetproductive.productivetime as productive_time,
                                  timesheetproductivePercent.ratio as productive_ratio,
                                  timesheet_desk.ondate 
                                FROM
                                  timesheet_desk 
                                  INNER JOIN
                                    (
                                      SELECT
                                        timesheet_desk.id,
                                        timesheet_desk.groupid,
                                        ISNULL(FORMAT(CAST(check_in AS DATETIME2), N'hh:mm tt'), '00:00') AS check_in,
                                        FORMAT(CAST(ondate AS DATE), 'MM/dd/yyyy', 'EN-US') AS ondate,
                                        empid 
                                      FROM
                                        timesheet_desk 
                                      WHERE
                                        FORMAT(CAST(ondate AS DATETIME2), N'dd-MMM-yyyy HH:mm:ss', 'EN-US') IN 
                                        (
                                          SELECT
                                            Min(FORMAT(CAST(ondate AS DATETIME2), N'dd-MMM-yyyy HH:mm:ss' , 'EN-US')) 
                                          FROM
                                            timesheet_desk 
                                          WHERE
                                            FORMAT(CAST(ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
                                            BETWEEN FORMAT(CAST(@StartDate AS DATE), 'MM/dd/yyyy', 'EN-US') 
                                            AND FORMAT(CAST(@EndDate AS DATE), 'MM/dd/yyyy', 'EN-US') 
                                            AND empid = @EmpID 
                                          GROUP BY
                                            FORMAT(CAST(ondate AS DATE), 'MM/dd/yyyy', 'EN-US' ) 
                                        )
                                    )
                                    AS timesheetFirst 
                                    ON dbo.timesheet_desk.groupid = timesheetFirst.groupid 
                                  LEFT JOIN
                                    (
                                      SELECT
                                        timesheet_desk.id,
                                        timesheet_desk.groupid,
                                        ISNULL(FORMAT(CAST(check_out AS DATETIME2), N'hh:mm tt'), null) AS check_out,
                                        FORMAT(CAST(ondate AS DATE), 'MM/dd/yyyy', 'EN-US') AS ondate,
                                        empid 
                                      FROM
                                        timesheet_desk 
                                      WHERE
                                        ondate IN 
                                        (
                                          SELECT
                                            Max(ondate) 
                                          FROM
                                            timesheet_desk 
                                          WHERE
                                            FORMAT(CAST(ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
                                            BETWEEN FORMAT(CAST(@StartDate AS DATE), 'MM/dd/yyyy', 'EN-US')
                                            AND FORMAT(CAST(@EndDate AS DATE), 'MM/dd/yyyy', 'EN-US') 
                                            AND empid = @EmpID 
                                          GROUP BY
                                            FORMAT(CAST(ondate AS DATE), 'MM/dd/yyyy' , 'EN-US') 
                                        )
                                    )
                                    AS timesheetLast 
                                    ON timesheetFirst.ondate = timesheetLast.ondate 
                                  LEFT JOIN
                                    (
                                      SELECT
                                        FORMAT(CAST(ondate AS DATE), 'MM/dd/yyyy', 'EN-US') AS ondate,
                                        CONVERT(VARCHAR(5), DATEADD(SS, SUM(DATEDIFF(SECOND, 0, DATEADD(DAY, 0, '00:' + employee_app_tracked.time_spend))), 0), 114) as privatetime 
                                      FROM
                                        employee_app_usage 
                                        INNER JOIN
                                          employee_app_tracked 
                                          on employee_app_usage.id = employee_app_tracked.emp_app_usage_id 
                                      WHERE
                                        FORMAT(CAST(employee_app_usage.ondate AS DATE), 'MM/dd/yyyy', 'EN-US') 
                                        BETWEEN FORMAT(CAST(@StartDate AS DATE), 'MM/dd/yyyy', 'EN-US') 
                                        AND FORMAT(CAST(@EndDate AS DATE), 'MM/dd/yyyy', 'EN-US') 
                                        AND employee_app_usage.emp_id = @EmpID 
                                        AND employee_app_tracked.app_name <> ' ' 
                                        AND employee_app_tracked.app_name = 'Private Time' 
                                      GROUP BY
                                        FORMAT(CAST(ondate AS DATE), 'MM/dd/yyyy' , 'EN-US') 
                                    )
                                    as timesheetPrivate 
                                    ON timesheetFirst.ondate = timesheetPrivate.ondate 
                                  LEFT JOIN
                                    (
                                      SELECT
                                        FORMAT(CAST(employee_app_usage.ondate AS DATE), 'MM/dd/yyyy', 'EN-US') ondate,
                                        CONVERT(VARCHAR(5), DATEADD(SS, SUM(DATEDIFF(SECOND, 0, DATEADD(DAY, 0, '00:' + employee_app_tracked.time_spend))), 0), 114) as productivetime 
                                      FROM
                                        employee_app_usage 
                                        INNER JOIN
                                          employee_app_tracked 
                                          on employee_app_usage.id = employee_app_tracked.emp_app_usage_id 
                                      WHERE
                                        FORMAT(CAST(employee_app_usage.ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
                                        BETWEEN FORMAT( CAST(@StartDate AS DATE), 'MM/dd/yyyy', 'EN-US') 
                                        AND FORMAT(CAST(@EndDate AS DATE), 'MM/dd/yyyy', 'EN-US') 
                                        AND employee_app_usage.emp_id = @EmpID 
                                        AND employee_app_tracked.app_name <> ' ' 
                                        AND employee_app_tracked.is_productive = 1 
                                      GROUP BY
                                        FORMAT(CAST(employee_app_usage.ondate AS DATE), 'MM/dd/yyyy' , 'EN-US') 
                                    )
                                    as timesheetproductive 
                                    ON timesheetFirst.ondate = timesheetproductive.ondate 
                                  LEFT JOIN
                                    (
                                        SELECT 
                                            FORMAT(CAST(xx.created_date AS DATE), 'MM/dd/yyyy', 'EN-US') AS ondate,
	                                        (timesheetproductivePercent.TOTALCOUNT) * 100 / (SUM(DATEDIFF(minute, 0, '00:' + xx.time_spend))) as ratio

                                        FROM
                                        employee_app_tracked xx
                                        INNER JOIN
                                        (
	                                        SELECT
	                                        FORMAT(CAST(created_date AS DATE), 'MM/dd/yyyy' , 'EN-US') AS ONDATE,
	                                        SUM(DATEDIFF(minute, 0, '00:' + employee_app_tracked.time_spend)) AS TOTALCOUNT
                                        FROM
		                                        employee_app_tracked 
		                                        WHERE
		                                        FORMAT(CAST(created_date AS DATE), 'MM/dd/yyyy', 'EN-US')
		                                        BETWEEN FORMAT(CAST(@StartDate AS DATE), 'MM/dd/yyyy', 'EN-US')
		                                        AND FORMAT(CAST(@EndDate AS DATE), 'MM/dd/yyyy', 'EN-US') 
		                                        AND emp_id = @EmpID   
		                                        AND employee_app_tracked.app_name <> ' '
		                                        AND employee_app_tracked.app_name <> 'unknown' 
		                                        AND employee_app_tracked.is_productive  = 1 
		                                        GROUP BY
		                                        FORMAT(CAST(created_date AS DATE), 'MM/dd/yyyy' , 'EN-US') 
                                        )
                                        AS timesheetproductivePercent ON FORMAT(CAST(xx.created_date AS DATE), 'MM/dd/yyyy', 'EN-US') 
                                                    = FORMAT(CAST(timesheetproductivePercent.ONDATE  AS DATE), 'MM/dd/yyyy', 'EN-US') 

                                        WHERE FORMAT(CAST(xx.created_date AS DATE), 'MM/dd/yyyy', 'EN-US')
	                                        BETWEEN FORMAT(CAST(@StartDate AS DATE), 'MM/dd/yyyy', 'EN-US')
	                                        AND FORMAT(CAST(@EndDate AS DATE), 'MM/dd/yyyy', 'EN-US') 
	                                        AND emp_id = @EmpID   
	                                        AND xx.app_name <> ' '
	                                        AND xx.app_name <> 'unknown' 
	                                        GROUP BY FORMAT(CAST(xx.created_date AS DATE), 'MM/dd/yyyy' , 'EN-US'), 
                                                     timesheetproductivePercent.TOTALCOUNT

                                    )
                                    as timesheetproductivePercent 
                                    ON timesheetFirst.ondate = timesheetproductivePercent.ondate 
                                WHERE
                                  timesheet_desk.empid = @EmpID 
                                  AND FORMAT(CAST(timesheet_desk.ondate AS DATE), 'MM/dd/yyyy', 'EN-US') 
	                              BETWEEN FORMAT(CAST(@StartDate AS DATE), 'MM/dd/yyyy', 'EN-US') 
	                              AND FORMAT(CAST(@EndDate AS DATE), 'MM/dd/yyyy', 'EN-US') 
                                  AND timesheet_desk.is_deleted = 0 
                              ) X 
                            ORDER BY
                              FORMAT(CAST(ondate AS datetime2), N'dd-MMM-yyyy HH:mm:ss', 'EN-US') ASC",
                 param: new { EmpID, StartDate, EndDate }
             );
        }

        public dynamic EmployeeProductivityTimeFrequencyByEmpIDAndDate(string EmpID, string StartDate, string EndDate)
        {
            List<EmployeeProductivityTime> employeeProductivityTimes = new List<EmployeeProductivityTime>();
            employeeProductivityTimes.AddRange(EmployeeProductivityTime(EmpID, StartDate, EndDate));

            for (int i = 0; i < employeeProductivityTimes.Count; i++)
            {
                List<EmployeeProductivityTrackedTime> EmployeeProductivityTrackedTime = new List<EmployeeProductivityTrackedTime>();
                EmployeeProductivityTrackedTime.AddRange(EmployeeProductivityTimeGraphFrequencyByUsageID(employeeProductivityTimes[i].id));
                employeeProductivityTimes[i].employeeProductivityTrackedTimes = EmployeeProductivityTrackedTime;

                employeeProductivityTimes[i].productive_percent = 20;

            }

            return employeeProductivityTimes;
        }

        public IEnumerable<EmployeeProductivityTrackedTime> EmployeeProductivityTimeGraphFrequencyByUsageID(string UsageID)
        {
            return Query<EmployeeProductivityTrackedTime>(
                           sql: @"SELECT
                                    employee_app_tracked.id,
                                    FORMAT(CAST(created_date AS DATE), 'MM/dd/yyyy', 'EN-US') AS ondate,
                                    app_name,
                                    time_spend,
                                    emp_id 
                                    FROM
                                    employee_app_tracked 
                                    WHERE
                                    created_date IN 
                                    (
                                        SELECT
                                        Max(created_date)
                                        FROM
                                        employee_app_tracked 
                                        WHERE
		                                 emp_app_usage_id = @UsageID
                                        GROUP BY
                                        FORMAT(CAST(created_date AS DATE), 'MM/dd/yyyy' , 'EN-US') 
                                    )
	                                and app_name <> ' '
	                                and app_name <> 'unknown'
	                                 ORDER BY
                                     FORMAT(CAST(created_date AS datetime2), N'dd-MMM-yyyy HH:mm:ss', 'EN-US') ASC",
                           param: new { UsageID }
                        );
        }

        private IEnumerable<EmployeeProductivityTime> EmployeeProductivityTime(string EmpID, string StartDate, string EndDate)
        {
            return Query<EmployeeProductivityTime>(
                    sql: @"SELECT
                                    employee_app_usage.id,
                                    employee_app_usage.start_time,
                                    employee_app_usage.end_time,
                                    FORMAT(CAST(ondate AS DATE), 'MM/dd/yyyy', 'EN-US') AS ondate,
                                    emp_id 
                                    FROM
                                    employee_app_usage 
                                    WHERE
                                    id IN 
                                    (
                                     SELECT
                                        Max(id) 
                                        FROM
                                        employee_app_usage 
                                        WHERE
                                        FORMAT(CAST(ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
                                        BETWEEN FORMAT(CAST(@StartDate AS DATE), 'MM/dd/yyyy', 'EN-US')
                                        AND FORMAT(CAST(@EndDate AS DATE), 'MM/dd/yyyy', 'EN-US') 
                                        AND emp_id = @EmpID 
                                        GROUP BY
                                        id 
                                    )
	                                ORDER BY
                                    FORMAT(CAST(ondate AS datetime2), N'dd-MMM-yyyy HH:mm:ss', 'EN-US') ASC",
                            param: new { EmpID, StartDate, EndDate }
                         );
        }

        public dynamic EmployeeAppTrackedByEmpIDAndDate(string EmpID, string StartDate, string EndDate)
        {
            return Query<dynamic>(
                 sql: @"SELECT
                            employee_app_tracked.id,
                            employee_app_tracked.app_name,
                            employee_app_tracked.app_category_name,
                            employee_app_tracked.time_spend,
                            employee_app_tracked.is_productive,
                            employee_app_tracked.is_unproductive,
                            employee_app_tracked.is_neutral,
                            employee_app_tracked.is_neutral,
                            employee_app_tracked.icon
	 
                        FROM
                            employee_app_tracked
                        inner join 
        
                            (
                                SELECT
			                        employee_app_tracked.id
				                    FROM employee_app_tracked
				                    WHERE
					                    FORMAT(CAST(created_date AS DATE), 'MM/dd/yyyy', 'EN-US') 
						                    BETWEEN FORMAT(CAST(@StartDate AS DATE), 'MM/dd/yyyy', 'EN-US') 
					                    AND FORMAT(CAST(@EndDate AS DATE), 'MM/dd/yyyy', 'EN-US')
					                    AND employee_app_tracked.app_name <> ' ' 
					                    AND employee_app_tracked.app_name <> 'Private Time' 
					                    AND employee_app_tracked.app_name <> 'unknown' 
					                    AND employee_app_tracked.app_name <> 'Idle' 
					                    AND emp_id = @EmpID
					                    GROUP BY ID
                            ) as employee_app_trackedSub
		                    on employee_app_tracked.id = employee_app_trackedSub.id",
                 param: new { EmpID, StartDate, EndDate }
             );
        }
        
    }

    public class EmployeeProductivityTime
    {
        public string id { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string productive_percent { get; set; }
        public string ondate { get; set; }
        public string emp_id { get; set; }
        public List<EmployeeProductivityTrackedTime> employeeProductivityTrackedTimes { get; set; }
    }
    public class EmployeeProductivityTrackedTime
    {
        public string app_name { get; set; }
        public string time_spend { get; set; }
    }
}