using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class WorkforceRepository : RepositoryBase, IWorkforceRepository
    {
        public WorkforceRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public async void Add(Workforce entity)
        {
            await ExecuteScalarAsync<string>(
                  sql: @"",
                  param: entity
              ).ConfigureAwait(false);
        }

        public async Task<Workforce> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<Workforce>(
                sql: "",
                param: new { key }
            );
        }

        public async Task<IEnumerable<Workforce>> All()
        {
            return await QueryAsync<Workforce>(
                sql: ""
                );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"",
                param: new { key }
            );
        }

        public async void Update(Workforce entity)
        {
            await ExecuteAsync(
                 sql: @"",
                 param: entity
             );
        }

        public async Task<Workforce> WorkforceProductiveByDeptIDAndDate(string key, string date)
        {
            TimeSpan totaltime = new TimeSpan();
            Workforce workforce = new Workforce();
            string avgCheckout = string.Empty;
            int working_emp_count = 0;
            var _EmployeeWorkforce = await QueryAsync<EmployeeWorkforce>(
                    sql: @"SELECT
                            employee.org_id as org_id,
                            employee.id as emp_id,
                            employee.full_name,
                            employee.workemail,
                            department.dep_name as deptartment_name,
                            designation.designation_name
                        FROM employee  WITH (NOLOCK)
                            LEFT JOIN department ON  employee.deptid = department.id
                            LEFT JOIN designation ON  employee.designation_id = designation.id
                            WHERE department.id= @key 
                            AND department.is_deleted = 0
                            AND employee.is_deleted = 0
                        ORDER BY employee.full_name ASC",
                        param: new { key }
                    );

            var empList = _EmployeeWorkforce.ToList();
            for (int i = 0; i < empList.Count; i++)
            {
                List<EmployeeProductivity> employeeProductivities = new List<EmployeeProductivity>();
                employeeProductivities.AddRange(await EmployeeProductivity(empList[i].emp_id, date, date).ConfigureAwait(false));

                if (employeeProductivities.Count > 0)
                {
                    working_emp_count += employeeProductivities.Count();
                    if (employeeProductivities.Count == 1 && employeeProductivities.Where(d => d.check_out.Contains("-")).Any() == true)
                        avgCheckout = "-";
                    else
                        avgCheckout = Convert.ToDateTime(employeeProductivities[0].check_out).ToString(@"hh:mm tt");


                    TimeSpan _break_hours = TimeSpan.FromMinutes(Convert.ToInt64(employeeProductivities.Where(d => d.break_hours != null)
                                                                                            .Sum(x => TimeSpan.Parse(x.break_hours).TotalMinutes)));

                    TimeSpan _total_hrs = TimeSpan.FromMinutes(Convert.ToInt64(employeeProductivities.Where(d => d.total_hrs != null)
                                                                                            .Sum(x => TimeSpan.Parse(x.total_hrs).TotalMinutes)));

                    TimeSpan _final_total_hours = TimeSpan.FromMinutes(Convert.ToInt64(employeeProductivities.Where(d => d.final_total_hours != null)
                                                                                            .Sum(x => TimeSpan.Parse(x.final_total_hours).TotalMinutes)));

                    TimeSpan _time_spend_activity = TimeSpan.FromMinutes(Convert.ToInt64(employeeProductivities.Where(d => d.time_spend_activity != null)
                                                                                           .Sum(x => TimeSpan.Parse(x.time_spend_activity).TotalMinutes)));

                    totaltime += (_time_spend_activity);

                    empList[i].emp_id = employeeProductivities[0].emp_id;
                    empList[i].arrival_time = employeeProductivities[0].check_in;
                    empList[i].left_time = avgCheckout;
                    //empList[i].break_hours = string.Format("{0:00}:{1:00}", (int)_break_hours.TotalHours, _break_hours.Minutes);
                    empList[i].time_at_work = string.Format("{0:00}:{1:00}", (int)_total_hrs.TotalHours, _total_hrs.Minutes);
                    empList[i].desktop_time = string.Format("{0:00}:{1:00}", (int)_final_total_hours.TotalHours, _final_total_hours.Minutes);
                    empList[i].productive_time = string.Format("{0:00}:{1:00}", (int)_time_spend_activity.TotalHours, _time_spend_activity.Minutes);
                    //empList[i].late = employeeProductivities.Sum(x => Convert.ToDouble(x.activity_count)).ToString();

                    int curr = 0;
                    if ((int)_final_total_hours.TotalMinutes == 0)
                    {
                        curr = (int)TimeSpan.FromMinutes((DateTime.Now.TimeOfDay - Convert.ToDateTime(employeeProductivities[0].check_in).TimeOfDay).TotalMinutes).TotalMinutes;
                    }
                    else
                        curr = (int)_final_total_hours.TotalMinutes;

                    float value = (int)_time_spend_activity.TotalMinutes * 100 / (int)curr;
                    empList[i].productive_percent = value.ToString();
                }
            }


            workforce.team_members = _EmployeeWorkforce.Count().ToString();
            workforce.working = working_emp_count.ToString();
            workforce.total_productive_percent = empList.Sum(x => Convert.ToDouble(x.productive_percent)).ToString();
            workforce.total_productive_time = string.Format("{0:00}:{1:00}", (int)totaltime.TotalHours, totaltime.Minutes);

            workforce.EmployeeWorkforce = empList;

            return workforce;
        }

        public async Task<Workforce> WorkforceProductiveByOrgIDAndDate(string key, string date)
        {
            TimeSpan totaltime = new TimeSpan();
            Workforce workforce = new Workforce();
            string avgCheckout = string.Empty;
            int working_emp_count = 0;
            var _EmployeeWorkforce = await QueryAsync<EmployeeWorkforce>(
                    sql: @"SELECT
                            employee.org_id as org_id,
                            employee.id as emp_id,
                            employee.full_name,
                            employee.workemail,
                            department.dep_name as deptartment_name,
                            designation.designation_name
                        FROM employee  WITH (NOLOCK)
                            LEFT JOIN department ON  employee.deptid = department.id
                            LEFT JOIN designation ON  employee.designation_id = designation.id
                            WHERE employee.org_id= @key 
                            AND department.is_deleted = 0
                            AND employee.is_deleted = 0
                        ORDER BY employee.full_name ASC",
                        param: new { key }
                    );

            var empList = _EmployeeWorkforce.ToList();
            for (int i = 0; i < empList.Count; i++)
            {
                List<EmployeeProductivity> employeeProductivities = new List<EmployeeProductivity>();
                employeeProductivities.AddRange(await EmployeeProductivity(empList[i].emp_id, date, date).ConfigureAwait(false));

                if (employeeProductivities.Count > 0)
                {
                    working_emp_count += employeeProductivities.Count();
                    if (employeeProductivities.Count == 1 && employeeProductivities.Where(d => d.check_out.Contains("-")).Any() == true)
                        avgCheckout = "-";
                    else
                        avgCheckout = Convert.ToDateTime(employeeProductivities[0].check_out).ToString(@"hh:mm tt");


                    TimeSpan _break_hours = TimeSpan.FromMinutes(Convert.ToInt64(employeeProductivities.Where(d => d.break_hours != null)
                                                                                            .Sum(x => TimeSpan.Parse(x.break_hours).TotalMinutes)));

                    TimeSpan _total_hrs = TimeSpan.FromMinutes(Convert.ToInt64(employeeProductivities.Where(d => d.total_hrs != null)
                                                                                            .Sum(x => TimeSpan.Parse(x.total_hrs).TotalMinutes)));

                    TimeSpan _final_total_hours = TimeSpan.FromMinutes(Convert.ToInt64(employeeProductivities.Where(d => d.final_total_hours != null)
                                                                                            .Sum(x => TimeSpan.Parse(x.final_total_hours).TotalMinutes)));

                    TimeSpan _time_spend_activity = TimeSpan.FromMinutes(Convert.ToInt64(employeeProductivities.Where(d => d.time_spend_activity != null)
                                                                                           .Sum(x => TimeSpan.Parse(x.time_spend_activity).TotalMinutes)));

                    totaltime += (_time_spend_activity);

                    empList[i].emp_id = employeeProductivities[0].emp_id;
                    empList[i].arrival_time = employeeProductivities[0].check_in;
                    empList[i].left_time = avgCheckout;
                    //empList[i].break_hours = string.Format("{0:00}:{1:00}", (int)_break_hours.TotalHours, _break_hours.Minutes);
                    empList[i].time_at_work = string.Format("{0:00}:{1:00}", (int)_total_hrs.TotalHours, _total_hrs.Minutes);
                    empList[i].desktop_time = string.Format("{0:00}:{1:00}", (int)_final_total_hours.TotalHours, _final_total_hours.Minutes);
                    empList[i].productive_time = string.Format("{0:00}:{1:00}", (int)_time_spend_activity.TotalHours, _time_spend_activity.Minutes);
                    //empList[i].late = employeeProductivities.Sum(x => Convert.ToDouble(x.activity_count)).ToString();

                    int curr = 0;
                    if ((int)_final_total_hours.TotalMinutes == 0)
                    {
                        curr = (int)TimeSpan.FromMinutes((DateTime.Now.TimeOfDay - Convert.ToDateTime(employeeProductivities[0].check_in).TimeOfDay).TotalMinutes).TotalMinutes;
                    }
                    else
                        curr = (int)_final_total_hours.TotalMinutes;

                    float value = (int)_time_spend_activity.TotalMinutes * 100 / (int)curr;
                    empList[i].productive_percent = value.ToString();
                }
            }


            workforce.team_members = _EmployeeWorkforce.Count().ToString();
            workforce.working = working_emp_count.ToString();
            workforce.total_productive_percent = empList.Sum(x => Convert.ToDouble(x.productive_percent)).ToString();
            workforce.total_productive_time = string.Format("{0:00}:{1:00}", (int)totaltime.TotalHours, totaltime.Minutes);

            workforce.EmployeeWorkforce = empList;

            return workforce;
        }

        public async Task<Workforce> WorkforceProductiveByTeamIDAndDate(string key, string date)
        {
            TimeSpan totaltime = new TimeSpan();
            Workforce workforce = new Workforce();
            string avgCheckout = string.Empty;
            int working_emp_count = 0;
            var _EmployeeWorkforce = await QueryAsync<EmployeeWorkforce>(
                    sql: @"SELECT
                            employee.org_id as org_id,
                            employee.id as emp_id,
                            employee.full_name,
                            employee.workemail,
                            department.dep_name as deptartment_name,
                            designation.designation_name
                        FROM employee  WITH (NOLOCK)
                            LEFT JOIN department ON  employee.deptid = department.id
                            LEFT JOIN designation ON  employee.designation_id = designation.id
                            LEFT JOIN team_members ON  employee.id = team_members.emp_id
                            WHERE  team_members.team_id = @key
                            AND department.is_deleted = 0
                            AND employee.is_deleted = 0
                        ORDER BY employee.full_name ASC",
                        param: new { key }
                    );

            var empList = _EmployeeWorkforce.ToList();
            for (int i = 0; i < empList.Count; i++)
            {
                List<EmployeeProductivity> employeeProductivities = new List<EmployeeProductivity>();
                employeeProductivities.AddRange(await EmployeeProductivity(empList[i].emp_id, date, date).ConfigureAwait(false));

                if (employeeProductivities.Count > 0)
                {
                    working_emp_count += employeeProductivities.Count();
                    if (employeeProductivities.Count == 1 && employeeProductivities.Where(d => d.check_out.Contains("-")).Any() == true)
                        avgCheckout = "-";
                    else
                        avgCheckout = Convert.ToDateTime(employeeProductivities[0].check_out).ToString(@"hh:mm tt");


                    TimeSpan _break_hours = TimeSpan.FromMinutes(Convert.ToInt64(employeeProductivities.Where(d => d.break_hours != null)
                                                                                            .Sum(x => TimeSpan.Parse(x.break_hours).TotalMinutes)));

                    TimeSpan _total_hrs = TimeSpan.FromMinutes(Convert.ToInt64(employeeProductivities.Where(d => d.total_hrs != null)
                                                                                            .Sum(x => TimeSpan.Parse(x.total_hrs).TotalMinutes)));

                    TimeSpan _final_total_hours = TimeSpan.FromMinutes(Convert.ToInt64(employeeProductivities.Where(d => d.final_total_hours != null)
                                                                                            .Sum(x => TimeSpan.Parse(x.final_total_hours).TotalMinutes)));

                    TimeSpan _time_spend_activity = TimeSpan.FromMinutes(Convert.ToInt64(employeeProductivities.Where(d => d.time_spend_activity != null)
                                                                                           .Sum(x => TimeSpan.Parse(x.time_spend_activity).TotalMinutes)));

                    totaltime += (_time_spend_activity);

                    empList[i].emp_id = employeeProductivities[0].emp_id;
                    empList[i].arrival_time = employeeProductivities[0].check_in;
                    empList[i].left_time = avgCheckout;
                    //empList[i].break_hours = string.Format("{0:00}:{1:00}", (int)_break_hours.TotalHours, _break_hours.Minutes);
                    empList[i].time_at_work = string.Format("{0:00}:{1:00}", (int)_total_hrs.TotalHours, _total_hrs.Minutes);
                    empList[i].desktop_time = string.Format("{0:00}:{1:00}", (int)_final_total_hours.TotalHours, _final_total_hours.Minutes);
                    empList[i].productive_time = string.Format("{0:00}:{1:00}", (int)_time_spend_activity.TotalHours, _time_spend_activity.Minutes);
                    //empList[i].late = employeeProductivities.Sum(x => Convert.ToDouble(x.activity_count)).ToString();

                    int curr = 0;
                    if ((int)_final_total_hours.TotalMinutes == 0)
                    {
                        curr = (int)TimeSpan.FromMinutes((DateTime.Now.TimeOfDay - Convert.ToDateTime(employeeProductivities[0].check_in).TimeOfDay).TotalMinutes).TotalMinutes;
                    }
                    else
                        curr = (int)_final_total_hours.TotalMinutes;

                    float value = (int)_time_spend_activity.TotalMinutes * 100 / (int)curr;
                    empList[i].productive_percent = value.ToString();
                }
            }


            workforce.team_members = _EmployeeWorkforce.Count().ToString();
            workforce.working = working_emp_count.ToString();
            workforce.total_productive_percent = empList.Sum(x => Convert.ToDouble(x.productive_percent)).ToString();
            workforce.total_productive_time = string.Format("{0:00}:{1:00}", (int)totaltime.TotalHours, totaltime.Minutes);

            workforce.EmployeeWorkforce = empList;

            return workforce;
        }

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
	                    ISNULL(activityCount, 0) as activity_count,
 	                    ISNULL(CONVERT(VARCHAR(5),  ISNULL(DATEADD(s, ((DATEPART(hh, activity_hours) * 3600 ) + (DATEPART(mi, activity_hours) * 60 ) + DATEPART(ss, activity_hours)), 0), 0), 114), '00:00')  AS time_spend_activity,
                        (DATEDIFF(minute, 0,  ISNULL(DATEADD(s, ((DATEPART(hh, activity_hours) * 3600 ) + (DATEPART(mi, activity_hours) * 60 ) + DATEPART(ss, activity_hours)), 0), 0))) 
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
                            timesheetactivity.activity_hours
                    
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
                    
                        WHERE timesheet.empid = @EmpID
                        AND FORMAT(CAST(timesheet.ondate AS date), 'MM/dd/yyyy', 'EN-US')
                        BETWEEN FORMAT(CAST(@StartDate AS date), 'MM/dd/yyyy', 'EN-US')
                         AND FORMAT(CAST(@EndDate AS date), 'MM/dd/yyyy', 'EN-US')
                        AND timesheet.is_deleted = 0) X
 
                        ORDER BY  FORMAT(CAST(ondate AS datetime2), N'dd-MMM-yyyy HH:mm:ss', 'EN-US') ASC",
                            param: new { EmpID, StartDate, EndDate }
                        );
        }

    }
}