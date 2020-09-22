using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class AdminProductivityDashboardRepository : RepositoryBase, IAdminProductivityDashboardRepository
    {
        public AdminProductivityDashboardRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        #region

        public void Add(AdminProductivityDashboard entity)
        {
            Execute(
                sql: @"",
                param: entity
            );
        }

        public void Update(AdminProductivityDashboard entity)
        {
            Execute(
                sql: @"",
                param: entity
            );
        }

        public async Task<IEnumerable<AdminProductivityDashboard>> All()
        {
            return await QueryAsync<AdminProductivityDashboard>(
                sql: ""
            );
        }

        public async Task<AdminProductivityDashboard> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<AdminProductivityDashboard>(
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


        public async Task<dynamic> EmployeeProductivityPerDateByOrgIDAndDate(string OrgID, string StartDate, string EndDate)
        {
            EmployeeProductivity employeeProductivity = new EmployeeProductivity();
            List<EmployeeProductivity> employeeProductivities = new List<EmployeeProductivity>();
            List<Weekdays> weekdays = new List<Weekdays>();

            string avgCheckout = string.Empty;
            string avgCheckin = string.Empty;

            List<Employee> employees = (await EmployeeDetailByOrgID(OrgID)).ToList();
            weekdays.AddRange((await WorkingHoursFindByOrgID(OrgID)));
            for (int i = 0; i < employees.Count; i++)
            {
                employeeProductivities.AddRange((await EmployeeProductivity(employees[i].id, StartDate, EndDate)));
            }

            avgCheckin = AverageCheckinTime(employeeProductivities).ToString(@"hh:mm tt");

            if (employeeProductivities.Count == 1 && employeeProductivities.Where(d => d.check_out.Contains("-")).Any() == true)
                avgCheckout = "-";
            else
                avgCheckout = AverageCheckoutTime(employeeProductivities, weekdays).ToString(@"hh:mm tt");


            TimeSpan _break_hours = TimeSpan.FromMinutes(Convert.ToInt64(employeeProductivities.Where(d => d.break_hours != null)
                                                                                    .Sum(x => TimeSpan.Parse(x.break_hours).TotalMinutes)));

            TimeSpan _total_hrs = TimeSpan.FromMinutes(Convert.ToInt64(employeeProductivities.Where(d => d.total_hrs != null)
                                                                                    .Sum(x => TimeSpan.Parse(x.total_hrs).TotalMinutes)));

            TimeSpan _final_total_hours = TimeSpan.FromMinutes(Convert.ToInt64(employeeProductivities.Where(d => d.final_total_hours != null)
                                                                                    .Sum(x => TimeSpan.Parse(x.final_total_hours).TotalMinutes)));

            TimeSpan _time_spend_activity = TimeSpan.FromMinutes(Convert.ToInt64(employeeProductivities.Where(d => d.time_spend_activity != null)
                                                                                   .Sum(x => TimeSpan.Parse(x.time_spend_activity).TotalMinutes)));

            //var result = employeeProductivities.GroupBy(x => new { x.ondate })
            //         .Select(x => new
            //         {
            //             ID = x.Select(z => z.emp_id).Distinct().Count()
            //         }).ToList();

            var result = employeeProductivities.GroupBy(x => new { x.ondate })
               .Sum(x => x.Select(z => z.emp_id).Distinct().Count());

            TimeSpan _temp_break_hours = AverageHours(_break_hours, result);
            TimeSpan _temp_total_hrs = AverageHours(_total_hrs, result);
            TimeSpan _temp_final_total_hours = AverageHours(_final_total_hours, result);
            TimeSpan _temp_time_spend_activity = AverageHours(_time_spend_activity, result);

            //employeeProductivity.emp_id = employeeProductivities[0].emp_id;
            employeeProductivity.check_in = avgCheckin;
            employeeProductivity.check_out = avgCheckout;
            employeeProductivity.break_hours = string.Format("{0:00}:{1:00}", (int)_temp_break_hours.TotalHours, _temp_break_hours.Minutes);
            employeeProductivity.total_hrs = string.Format("{0:00}:{1:00}", (int)_temp_total_hrs.TotalHours, _temp_total_hrs.Minutes);
            employeeProductivity.final_total_hours = string.Format("{0:00}:{1:00}", (int)_temp_final_total_hours.TotalHours, _temp_final_total_hours.Minutes);
            employeeProductivity.time_spend_activity = string.Format("{0:00}:{1:00}", (int)_temp_time_spend_activity.TotalHours, _temp_time_spend_activity.Minutes);
            employeeProductivity.activity_count = employeeProductivities.Sum(x => Convert.ToDouble(x.activity_count)).ToString();

            int curr = 0;
            if ((int)_final_total_hours.TotalMinutes == 0)
            {
                curr = (int)TimeSpan.FromMinutes((DateTime.Now.TimeOfDay - Convert.ToDateTime(avgCheckin).TimeOfDay).TotalMinutes).TotalMinutes;
            }
            else
                curr = (int)_final_total_hours.TotalMinutes;

            float value = (int)_time_spend_activity.TotalMinutes * 100 / (int)curr;
            employeeProductivity.productivity_ratio = value.ToString();
            //employeeProductivity.productivity_ratio = employeeProductivities.Sum(x => Convert.ToDouble(x.productivity_ratio)).ToString();

            return employeeProductivity;

        }

        private static TimeSpan AverageHours(TimeSpan _total_hrs, int Count)
        {
            var temp_total_hrs = _total_hrs.TotalMinutes / (double)Count;
            TimeSpan _temp_total_hrs = new TimeSpan(0, 0, (int)temp_total_hrs, 0);
            return _temp_total_hrs;
        }

        public async Task<dynamic> ScreenshotByOrgIDAndDate(string OrgID, string StartDate, string EndDate)
        {
            return await QueryAsync<dynamic>(
                 sql: @"SELECT * FROM employee_screenshot
                        WHERE FORMAT(CAST(employee_screenshot.created_date AS date), 'MM/dd/yyyy', 'EN-US')
                    BETWEEN FORMAT(CAST(@StartDate AS DATE), 'MM/dd/yyyy', 'EN-US')
                    AND FORMAT(CAST(@EndDate AS DATE), 'MM/dd/yyyy', 'EN-US') 
                    AND employee_screenshot.org_id = @OrgID
                    AND employee_screenshot.is_deleted = 0
                    ORDER BY
                    FORMAT(CAST(created_date AS datetime2), N'dd-MMM-yyyy HH:mm:ss', 'EN-US') ASC",
                 param: new { OrgID, StartDate, EndDate }
             );
        }

        public async Task<dynamic> EmployeeProductivityTimeFrequencyByOrgIDAndDate(string OrgID, string StartDate, string EndDate)
        {
            double? temp = 0D, temp1 = 0D, tempx = 0D;
            DateTime avgCheckin = DateTime.Now, avgCheckout = DateTime.Now;
            TimeSpan TotalDeskTimeHrs = TimeSpan.Zero;
            TimeSpan TotalDeskTimeHrs1 = TimeSpan.Zero;

            RootEmployeeProductivityRatio rootEmployeeProductivityRatio = new RootEmployeeProductivityRatio();
            List<Weekdays> weekdays = new List<Weekdays>();

            List<EmployeeProductivityTime> employeeProductivityTimes = new List<EmployeeProductivityTime>();
            employeeProductivityTimes.AddRange((await EmployeeProductivityTime(OrgID, StartDate, EndDate)));

            List<EmployeeIdleTime> employeeIdleTime = new List<EmployeeIdleTime>();
            employeeIdleTime.AddRange((await EmployeeProductivityIdleTime(OrgID, StartDate, EndDate)));

            var EmpIDList = employeeProductivityTimes.Select(x => x.emp_id).Distinct().ToList();
            weekdays.AddRange((await WorkingHoursFindByOrgID(OrgID)));

            for (int i = 0; i < EmpIDList.Count; i++)
            {
                var data = (await FirstCheckInLastCheckout(EmpIDList[i], StartDate, EndDate)).ToList();

                if (data != null)
                {
                    for (int d = 0; d < data.Count; d++)
                    {
                        //CHECKIN
                        temp += (double)Convert.ToDateTime(data[d].checkin).Ticks / (double)EmpIDList.Count;

                        //CHECKOUT
                        if (!data[d].checkout.Contains("-"))
                            temp1 += (double)Convert.ToDateTime(data[d].checkout).Ticks / (double)EmpIDList.Count;
                        else
                        {
                            var CheckOut = weekdays.Where(x => x.day_name == Convert.ToDateTime(data[d].ondate).DayOfWeek.ToString()).FirstOrDefault();
                            if (CheckOut != null)
                            {
                                data[d].checkout = CheckOut.to_time;
                                temp1 += (double)Convert.ToDateTime(data[d].checkout).Ticks / (double)EmpIDList.Count;
                            }
                            else
                            {
                                data[d].checkout = "7:00 PM";
                                temp1 += (double)Convert.ToDateTime(data[d].checkout).Ticks / (double)EmpIDList.Count;
                            }
                        }

                        if (data[d].desk_time != null)
                        {
                            string[] Time = data[d].desk_time.ToString().Split(':');
                            TimeSpan _time_spend = new TimeSpan(0, Convert.ToInt32(Time[0]), Convert.ToInt32(Time[1]), 0);
                            tempx += _time_spend.TotalMinutes / (double)EmpIDList.Count;
                        }
                        else
                        {
                            DateTime timespan = DateTime.Parse(data[d].checkin);  //gives us a DateTime object
                            DateTime timespan2 = DateTime.Parse(data[d].checkout);

                            TimeSpan _time = timespan.TimeOfDay;  //returns a TimeSpan from the Time portion
                            TimeSpan _time2 = timespan2.TimeOfDay;
                            TimeSpan interval = (_time2 - _time);

                            if (interval.CompareTo(TimeSpan.Zero) < 0)
                                interval = new TimeSpan(Math.Abs(interval.Ticks));

                            string[] Time = interval.ToString().Split(':');
                            TimeSpan _time_spend = new TimeSpan(0, Convert.ToInt32(Time[0]), Convert.ToInt32(Time[1]), 0);
                            tempx += _time_spend.TotalMinutes / (double)EmpIDList.Count;
                        }
                    }
                }
            }

            if (temp != null && temp1 != null && tempx != null)
            {
                avgCheckin = new DateTime((long)temp);
                avgCheckout = new DateTime((long)temp1);
                TimeSpan _time_spendx = new TimeSpan(0, 0, (int)tempx, 0);

                employeeProductivityTimes = employeeProductivityTimes.Where(x => Convert.ToDateTime(x.start_time) > Convert.ToDateTime(avgCheckin.ToString(@"hh:mm tt"))).ToList();

                if (employeeProductivityTimes.Count > 0)
                {
                    employeeProductivityTimes[0].checkin = avgCheckin.ToString(@"hh:mm tt");
                    employeeProductivityTimes[0].checkout = avgCheckout.ToString(@"hh:mm tt");
                    employeeProductivityTimes[0].desk_time = string.Format("{0:00}:{1:00}", (int)_time_spendx.TotalHours, _time_spendx.Minutes);
                }
            }

            //employeeProductivityTimes = employeeProductivityTimes.Where(x => Convert.ToDateTime(x.start_time) > Convert.ToDateTime(employeeProductivityTimes[0].checkin)).ToList();


            for (int i = 0; i < employeeProductivityTimes.Count; i++)
            {
                List<EmployeeProductivityTrackedTime> EmployeeProductivityTrackedTime = new List<EmployeeProductivityTrackedTime>();
                List<EmployeeProductivityTrackedTime> EmployeeProductivityIdleTrackedTime = new List<EmployeeProductivityTrackedTime>();

                EmployeeProductivityTrackedTime.AddRange(await EmployeeProductivityTimeGraphFrequencyByUsageID(employeeProductivityTimes[i].id));
                employeeProductivityTimes[i].employeeProductivityTrackedTimes = EmployeeProductivityTrackedTime;


                EmployeeProductivityIdleTrackedTime.AddRange(await EmployeeProductivityIdleGraphFrequencyByUsageID(employeeProductivityTimes[i].id));
                employeeIdleTime[i].employeeProductivityTrackedTimes = EmployeeProductivityIdleTrackedTime;

                TimeSpan timeSpan = TimeSpan.ParseExact(employeeProductivityTimes[i].start_time, "c", null);
                TimeSpan timeSpan2 = TimeSpan.ParseExact(employeeProductivityTimes[i].end_time, "c", null);
                TimeSpan interval = (timeSpan - timeSpan2);

                if (interval.CompareTo(TimeSpan.Zero) < 0)
                    interval = new TimeSpan(Math.Abs(interval.Ticks));

                TimeSpan TotalWorkedHrs = TimeSpan.Zero;
                if (timeSpan != null && timeSpan2 != null)
                {
                    for (int x = 0; x < EmployeeProductivityTrackedTime.Count; x++)
                    {
                        string[] Time = EmployeeProductivityTrackedTime[x].time_spend.ToString().Split(':');
                        TimeSpan _time_spend = new TimeSpan(0, 0, Convert.ToInt32(Time[0]), Convert.ToInt32(Time[1]));
                        TotalWorkedHrs += _time_spend;
                    }

                    var ticks = (TotalWorkedHrs.TotalSeconds * 100 / (interval.TotalSeconds));
                    employeeProductivityTimes[i].productive_ratio = ticks.ToString();
                }

                TimeSpan IdleWorkedHrs = TimeSpan.Zero;
                if (timeSpan != null && timeSpan2 != null)
                {
                    for (int x = 0; x < EmployeeProductivityIdleTrackedTime.Count; x++)
                    {
                        string[] Time = EmployeeProductivityIdleTrackedTime[x].time_spend.ToString().Split(':');
                        TimeSpan _time_spend = new TimeSpan(0, 0, Convert.ToInt32(Time[0]), Convert.ToInt32(Time[1]));
                        IdleWorkedHrs += _time_spend;
                    }

                    var Idleticks = (IdleWorkedHrs.TotalSeconds * 100 / (interval.TotalSeconds));
                    employeeIdleTime[i].productive_ratio = Idleticks.ToString();
                }
            }

            rootEmployeeProductivityRatio.employeeProductivityTime = employeeProductivityTimes;
            rootEmployeeProductivityRatio.employeeIdleTime = employeeIdleTime;

            return rootEmployeeProductivityRatio;
        }

        #region PrivateMethods

        private async Task<IEnumerable<EmployeeProductivityTime>> EmployeeProductivityTime(string OrgID, string StartDate, string EndDate)
        {
            return await QueryAsync<EmployeeProductivityTime>(
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
                                        AND org_id = @OrgID 
                                        GROUP BY
                                        id 
                                    )
	                            ORDER BY
                                FORMAT(CAST(ondate AS datetime2), N'dd-MMM-yyyy HH:mm:ss', 'EN-US') ASC",
                        param: new { OrgID, StartDate, EndDate }
                        );
        }

        private async Task<IEnumerable<EmployeeIdleTime>> EmployeeProductivityIdleTime(string OrgID, string StartDate, string EndDate)
        {
            return await QueryAsync<EmployeeIdleTime>(
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
                                        AND org_id = @OrgID 
                                        GROUP BY
                                        id 
                                    )
	                                ORDER BY
                                    FORMAT(CAST(ondate AS datetime2), N'dd-MMM-yyyy HH:mm:ss', 'EN-US') ASC",
                            param: new { OrgID, StartDate, EndDate }
                         );
        }

        private async Task<IEnumerable<EmployeeProductivityTrackedTime>> EmployeeProductivityTimeGraphFrequencyByUsageID(string UsageID)
        {
            return await QueryAsync<EmployeeProductivityTrackedTime>(
                           sql: @"SELECT
                                    app_category_name as app_name,
                                    CONVERT(varchar(5), DATEADD(MINUTE, SUM(DATEDIFF(MINUTE, 0, time_spend)), 0), 114) AS time_spend,
                                    FORMAT(CAST(created_date AS datetime2), N'dd-MMM-yyyy HH:mm:ss', 'EN-US') ondate 
                                    FROM
                                    employee_app_tracked 
                                   WHERE
                                        emp_app_usage_id = @UsageID
                                        and app_category_name is not null 
                                        and app_name <> ' '
                                        and app_name <> 'unknown'
                                        and app_name <> 'Idle'
                                     GROUP BY 
                                     app_category_name, FORMAT(CAST(created_date AS datetime2), N'dd-MMM-yyyy HH:mm:ss', 'EN-US') 
	                                 ORDER BY
                                     FORMAT(CAST(created_date AS datetime2), N'dd-MMM-yyyy HH:mm:ss', 'EN-US') ASC",
                           param: new { UsageID }
                        );
        }

        private async Task<IEnumerable<EmployeeProductivityTrackedTime>> EmployeeProductivityIdleGraphFrequencyByUsageID(string UsageID)
        {
            return await QueryAsync<EmployeeProductivityTrackedTime>(
                           sql: @"SELECT
                                    app_name,
                                    CONVERT(varchar(5), DATEADD(MINUTE, SUM(DATEDIFF(MINUTE, 0, time_spend)), 0), 114) AS time_spend,
                                    FORMAT(CAST(created_date AS datetime2), N'dd-MMM-yyyy HH:mm:ss', 'EN-US') ondate 
                                    FROM
                                    employee_app_tracked 
                                   WHERE
		                                 emp_app_usage_id = @UsageID
                                         and app_name <> ' '
                                         and app_name <> 'unknown'
                                         and app_name = 'Idle'
                                     GROUP BY 
                                     app_name, 	FORMAT(CAST(created_date AS datetime2), N'dd-MMM-yyyy HH:mm:ss', 'EN-US')
	                                 ORDER BY
                                     FORMAT(CAST(created_date AS datetime2), N'dd-MMM-yyyy HH:mm:ss', 'EN-US') ASC",
                           param: new { UsageID }
                        );
        }

        private async Task<IEnumerable<EmployeeFirstCheckInLastCheckout>> FirstCheckInLastCheckout(string EmpID, string StartDate, string EndDate)
        {
            return await QueryAsync<EmployeeFirstCheckInLastCheckout>(
                 sql: @"SELECT
                            timesheet.empid AS emp_id,
                            FORMAT(CAST(timesheetFirst.check_in AS DATETIME2), N'hh:mm tt') AS checkin,
                            ISNULL(FORMAT(CAST(timesheetLast.check_out AS DATETIME2), N'hh:mm tt'), '-') AS checkout,
                            CONVERT(VARCHAR(5), DATEADD (MINUTE, (DATEDIFF(MINUTE, timesheetFirst.check_in, timesheetLast.check_out)), 0), 114) AS desk_time,
                            FORMAT(CAST(timesheet.ondate AS date), 'MM/dd/yyyy', 'EN-US') AS ondate
                                        
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
                                        WHERE timesheet.empid = @EmpID
                                        AND FORMAT(CAST(timesheet.ondate AS date), 'MM/dd/yyyy', 'EN-US')
                                        BETWEEN FORMAT(CAST(@StartDate AS date), 'MM/dd/yyyy', 'EN-US')
                                        AND FORMAT(CAST(@EndDate AS date), 'MM/dd/yyyy', 'EN-US')
                                        AND timesheet.is_deleted = 0",
                 param: new { EmpID, StartDate, EndDate }
             );
        }


        //EmployeeProductivityPerDateByEmpIDAndDate
        private async Task<IEnumerable<EmployeeProductivity>> EmployeeProductivity(string EmpID, string StartDate, string EndDate)
        {
            return await QueryAsync<EmployeeProductivity>(
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
                           * 100 /  (DATEDIFF(minute, 0,  ISNULL(DATEADD(s, ((DATEPART(hh, ISNULL(check_out , GETDATE())) * 3600 ) + (DATEPART(mi, ISNULL(check_out , GETDATE())) * 60 ) + DATEPART(ss, ISNULL(check_out , GETDATE()))), 0), 0) -
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

        private static DateTime AverageCheckinTime(List<EmployeeProductivity> employeeProductivities)
        {
            double temp = 0D;
            for (int i = 0; i < employeeProductivities.Count; i++)
            {
                temp += (double)Convert.ToDateTime(employeeProductivities[i].check_in).Ticks / (double)employeeProductivities.Count;
            }
            var avgCheckin = new DateTime((long)temp);
            return avgCheckin;
        }

        private static DateTime AverageCheckoutTime(List<EmployeeProductivity> employeeProductivities, List<Weekdays> weekdays)
        {
            double temp = 0D;
            for (int i = 0; i < employeeProductivities.Count; i++)
            {
                if (!employeeProductivities[i].check_out.Contains("-"))
                {
                    temp += (double)Convert.ToDateTime(employeeProductivities[i].check_out).Ticks / (double)employeeProductivities.Count;
                }
                else
                {
                    var CheckOut = weekdays.Where(x => x.day_name == Convert.ToDateTime(employeeProductivities[i].ondate).DayOfWeek.ToString()).FirstOrDefault();
                    if (CheckOut != null)
                        temp += (double)Convert.ToDateTime(CheckOut.to_time).Ticks / (double)employeeProductivities.Count;
                    else
                        temp += (double)Convert.ToDateTime("7:00 PM").Ticks / (double)employeeProductivities.Count;
                }
            }
            var avgCheckout = new DateTime((long)temp);
            return avgCheckout;
        }

        public async Task<IEnumerable<Employee>> EmployeeDetailByOrgID(string key)
        {
            return await QueryAsync<Employee>(
                sql: "SELECT * FROM [dbo].[employee] WHERE org_id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        private async Task<IEnumerable<Weekdays>> WorkingHoursFindByOrgID(string key)
        {
            return await QueryAsync<Weekdays>(
                sql: "SELECT * FROM dbo.weekdays WHERE is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }

        #endregion PrivateMethods

    }
}