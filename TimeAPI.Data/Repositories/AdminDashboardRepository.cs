﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class AdminDashboardRepository : RepositoryBase, IAdminDashboardRepository
    {
        public AdminDashboardRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        #region

        public void Add(AdminDashboard entity)
        {
            Execute(
                sql: @"",
                param: entity
            );
        }

        public void Update(AdminDashboard entity)
        {
            Execute(
                sql: @"",
                param: entity
            );
        }

        public async Task<IEnumerable<AdminDashboard>> All()
        {
            return await QueryAsync<AdminDashboard>(
                sql: ""
            );
        }

        public async Task<AdminDashboard> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<AdminDashboard>(
                sql: "",
                param: new { key }
            );
        }

        public AdminDashboard FindByNormalizedEmail(string normalizedEmail)
        {
            return QuerySingleOrDefault<AdminDashboard>(
                sql: "",
                param: new { normalizedEmail }
            );
        }

        public AdminDashboard FindByNormalizedUserName(string normalizedUserName)
        {
            return QuerySingleOrDefault<AdminDashboard>(
                sql: "",
                param: new { normalizedUserName }
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

        public async Task<dynamic> TotalDefaultEmpCountByOrgID(string org_id)
        {
            var resultsAspNetUsers = await QueryAsync<dynamic>(
                sql: @"SELECT employee_type_name, SUM(employee_count) as attandance  FROM
                        (
                     
                        SELECT
                            ISNULL(UPPER(employee_type.employee_type_name), 'PERMANENT') AS employee_type_name,
                            COUNT (DISTINCT [dbo].[employee].id) AS employee_count
                            FROM [dbo].[employee] WITH (NOLOCK)
                            LEFT JOIN [dbo].[employee_type] ON  [dbo].[employee].emp_type_id = [dbo].[employee_type].id
                            WHERE [dbo].[employee].is_deleted = 0 AND [dbo].[employee].is_inactive = 0
                            AND [dbo].[employee].org_id =  @org_id
                            GROUP BY
                            employee_type.employee_type_name
                        ) x  group by employee_type_name",
                param: new { org_id }
            );
            return resultsAspNetUsers;
        }

        public async Task<dynamic> TotalEmpAttentedCountByOrgIDAndDate(string org_id, string fromDate, string toDate)
        {
            List<dynamic> TimesheetDashboardData = new List<dynamic>();

            //OffDaysDates(org_id, fromDate, toDate)
            var DGridData = await GettingTimesheetDashboardDataPerDate(org_id, fromDate, toDate);

            if (DGridData.Count > 0)
            {
                TimesheetDashboardData.AddRange(DGridData);
            }
            return TimesheetDashboardData;
        }

        public async Task<dynamic> GetTimesheetDashboardGridDataByOrgIDAndDate(string org_id, string fromDate, string toDate)
        {
            var resultsAspNetUsers = await QueryAsync<dynamic>(
                sql: @"SELECT
                        ROW_NUMBER() OVER (ORDER BY full_name) AS rowno,
                        project_category_type, project_or_comp_id, project_or_comp_name,
                        timesheet_id, groupid, is_checkout,
                        employee_id, full_name, check_in,
                        check_out, total_hrs, lat, lang, ondate

                FROM (

                    SELECT
                        timesheet_x_project_category.project_category_type,
                        timesheet_x_project_category.project_or_comp_id,
                        timesheet_x_project_category.project_or_comp_name,
                        timesheet.id as timesheet_id,
                        timesheet.groupid as groupid,
                        employee.id as employee_id,
                        eTime.lat as lat,
                        eTime.lang as lang,
                        eTime.is_checkout as is_checkout,
                        employee.full_name,
                        employee_type.employee_type_name as employee_type_name,
                        employee.workemail as workemail,
                        employee.emp_code as emp_code,
                        employee.phone as phone,
                        FORMAT(CAST(timesheet.check_in AS DATETIME2), N'hh:mm tt') as check_in,
                        ISNULL(FORMAT(CAST(timesheet.check_out AS DATETIME2), N'hh:mm tt'), 'NA') as check_out,
                        ISNULL(total_hrs, 'NA') as total_hrs,
                        FORMAT(CAST(timesheet.ondate  AS DATE), 'MM/dd/yyyy', 'EN-US')  as ondate
                        FROM timesheet WITH (NOLOCK)
                        INNER JOIN employee ON timesheet.empid = employee.id
                        LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
                        INNER JOIN timesheet_x_project_category on timesheet.groupid = timesheet_x_project_category.groupid
                        INNER JOIN (select distinct(groupid), location.lat, location.lang, location.is_checkout
                        from dbo.location  where groupid IN (SELECT groupid FROM timesheet) and is_checkout = 0) eTime
                        ON eTime.groupid = dbo.timesheet.groupid
                    WHERE employee.org_id = @org_id
						AND FORMAT(CAST(timesheet.ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
						BETWEEN FORMAT(CAST(@fromDate  AS DATE), 'MM/dd/yyyy', 'EN-US')
                        AND FORMAT(CAST(@toDate AS DATE), 'MM/dd/yyyy', 'EN-US')
                        AND timesheet.is_deleted = 0) REALDATA",
                param: new { org_id, fromDate, toDate }
            );
            return resultsAspNetUsers;
        }

        public async Task<dynamic> GetTimesheetDashboardFirstCheckInGridDataByOrgIDAndDate(string org_id, string fromDate, string toDate)
        {
            List<DateTime> Dates = Enumerable.Range(0, (Convert.ToDateTime(toDate) - Convert.ToDateTime(fromDate)).Days + 1)
                              .Select(d => Convert.ToDateTime(fromDate).AddDays(d)).ToList();
            List<DateTime> ExceptOffDays = ExceptOffDaysDates(org_id, fromDate, toDate);
            List<string> OffDays = OffDaysDates(org_id, fromDate, toDate);
            List<AttendedEmployee> AttendedEmployeeList = new List<AttendedEmployee>();


            for (int i = 0; i < Dates.Count; i++)
            {
                var xdate = OffDays.Any(b => b.Contains(Dates[i].ToString()));
                List<ResultSingleCheckin> ResultSingleCheckinList = new List<ResultSingleCheckin>();

                var ResultSingleList = await SingleFirstCheckINResult(org_id, Dates[i], xdate).ConfigureAwait(false);
                var ResultMultipleList = await MultipleFirstCheckINResult(org_id, Dates[i], xdate).ConfigureAwait(false);

                ResultSingleCheckinList.AddRange(ResultSingleList);
                ResultSingleCheckinList.AddRange(ResultMultipleList);

                var REST = ResultSingleCheckinList.Cast<ResultSingleCheckin>().ToList();

                for (int x = 0; x < REST.Count(); x++)
                {

                    var resultsAspNetUsers = await QueryAsync<AttendedEmployee>(
                        sql: @"SELECT
                        ROW_NUMBER() OVER (ORDER BY full_name) AS rowno,
                        project_category_type, project_or_comp_name,
                        timesheet_id, groupid, is_checkout,
                        employee_id, full_name, check_in,
                        check_out, total_hrs, lat, lang, ondate

                FROM (

                    SELECT
                        timesheet_x_project_category.project_category_type,
                        timesheet_x_project_category.project_or_comp_name,
                        timesheet.id as timesheet_id,
                        timesheet.groupid as groupid,
                        employee.id as employee_id,
                        eTime.lat as lat,
                        eTime.lang as lang,
                        eTime.is_checkout as is_checkout,
                        employee.full_name,
                        employee_type.employee_type_name as employee_type_name,
                        employee.workemail as workemail,
                        employee.emp_code as emp_code,
                        employee.phone as phone,
                        FORMAT(CAST(timesheet.check_in AS DATETIME2), N'hh:mm tt') as check_in,
                        ISNULL(FORMAT(CAST(timesheet.check_out AS DATETIME2), N'hh:mm tt'), 'NA') as check_out,
                        ISNULL(total_hrs, 'NA') as total_hrs,
                        FORMAT(CAST(timesheet.ondate  AS DATE), 'MM/dd/yyyy', 'EN-US')  as ondate
                        FROM timesheet WITH (NOLOCK)
                        INNER JOIN employee ON timesheet.empid = employee.id
                        LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
                        INNER JOIN timesheet_x_project_category on timesheet.groupid = timesheet_x_project_category.groupid
                        INNER JOIN (select distinct(groupid), location.lat, location.lang, location.is_checkout
                        from dbo.location  where groupid IN (SELECT groupid FROM timesheet) and is_checkout = 0) eTime
                        ON eTime.groupid = dbo.timesheet.groupid
              WHERE employee.id = @emp
						AND FORMAT(CAST(timesheet.ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
						BETWEEN FORMAT(CAST(@date  AS DATE), 'MM/dd/yyyy', 'EN-US')
                        AND FORMAT(CAST(@date AS DATE), 'MM/dd/yyyy', 'EN-US')
                        AND timesheet.is_deleted = 0) 
						
						REALDATA ORDER BY  CAST(check_in as datetime)",
                        param: new { emp = REST[x].empid, date = Dates[i] }
                    );

                    if (resultsAspNetUsers.Count() > 1)
                    {
                        TimeSpan TotalWorkedHrs = TimeSpan.Zero;

                        var RESTAttendedEmployee = resultsAspNetUsers.Cast<AttendedEmployee>().ToList();
                        AttendedEmployee attendedEmployee = new AttendedEmployee();
                        attendedEmployee.rowno = RESTAttendedEmployee[0].rowno;
                        attendedEmployee.project_category_type = RESTAttendedEmployee[0].project_category_type;
                        attendedEmployee.project_or_comp_name = RESTAttendedEmployee[0].project_or_comp_name;
                        attendedEmployee.timesheet_id = RESTAttendedEmployee[0].timesheet_id;
                        attendedEmployee.groupid = RESTAttendedEmployee[0].groupid;
                        attendedEmployee.is_checkout = RESTAttendedEmployee[0].is_checkout;
                        attendedEmployee.employee_id = RESTAttendedEmployee[0].employee_id;
                        attendedEmployee.workhour = RESTAttendedEmployee[0].workhour;
                        attendedEmployee.full_name = RESTAttendedEmployee[0].full_name;
                        attendedEmployee.check_in = RESTAttendedEmployee[0].check_in;
                        attendedEmployee.check_out = RESTAttendedEmployee[0].check_out;

                        TotalWorkedHrs = new TimeSpan(RESTAttendedEmployee.Select(item => item.total_hrs).Where(x => !x.Contains("NA")).Sum(x =>
                         TimeSpan.ParseExact(x, "h\\:mm", CultureInfo.InvariantCulture).Ticks));

                        attendedEmployee.total_hrs = TotalWorkedHrs.ToString(@"hh\:mm");
                        attendedEmployee.lat = RESTAttendedEmployee[0].lat;
                        attendedEmployee.lang = RESTAttendedEmployee[0].lang;
                        attendedEmployee.ondate = RESTAttendedEmployee[0].ondate;

                        AttendedEmployeeList.Add(attendedEmployee);
                    }
                    else
                    {
                        AttendedEmployeeList.AddRange(resultsAspNetUsers);
                    }
                }
            }

            return AttendedEmployeeList;
        }

        public async Task<dynamic> GetTimesheetDashboardLastCheckoutGridDataByOrgIDAndDate(string org_id, string fromDate, string toDate)
        {
            List<DateTime> Dates = Enumerable.Range(0, (Convert.ToDateTime(toDate) - Convert.ToDateTime(fromDate)).Days + 1)
                              .Select(d => Convert.ToDateTime(fromDate).AddDays(d)).ToList();
            List<DateTime> ExceptOffDays = ExceptOffDaysDates(org_id, fromDate, toDate);
            List<string> OffDays = OffDaysDates(org_id, fromDate, toDate);
            List<AttendedEmployee> AttendedEmployeeList = new List<AttendedEmployee>();

            for (int i = 0; i < Dates.Count; i++)
            {
                var xdate = OffDays.Any(b => b.Contains(Dates[i].ToString()));
                List<ResultSingleCheckin> ResultSingleCheckinList = new List<ResultSingleCheckin>();

                var ResultSingleList = await SingleFirstCheckINResult(org_id, Dates[i], xdate).ConfigureAwait(false);
                var ResultMultipleList = await MultipleFirstCheckINResult(org_id, Dates[i], xdate).ConfigureAwait(false);

                ResultSingleCheckinList.AddRange(ResultSingleList);
                ResultSingleCheckinList.AddRange(ResultMultipleList);

                var REST = ResultSingleCheckinList.Cast<ResultSingleCheckin>().ToList();

                for (int x = 0; x < REST.Count(); x++)
                {

                    var resultsAspNetUsers = await QueryAsync<AttendedEmployee>(
                        sql: @"SELECT
                        ROW_NUMBER() OVER (ORDER BY full_name) AS rowno,
                        project_category_type, project_or_comp_name,
                        timesheet_id, groupid, is_checkout,
                        employee_id, full_name, check_in,
                        check_out, total_hrs, lat, lang, ondate

                FROM (

                    SELECT
                        timesheet_x_project_category.project_category_type,
                        timesheet_x_project_category.project_or_comp_name,
                        timesheet.id as timesheet_id,
                        timesheet.groupid as groupid,
                        employee.id as employee_id,
                        eTime.lat as lat,
                        eTime.lang as lang,
                        eTime.is_checkout as is_checkout,
                        employee.full_name,
                        employee_type.employee_type_name as employee_type_name,
                        employee.workemail as workemail,
                        employee.emp_code as emp_code,
                        employee.phone as phone,
                        FORMAT(CAST(timesheet.check_in AS DATETIME2), N'hh:mm tt') as check_in,
                        ISNULL(FORMAT(CAST(timesheet.check_out AS DATETIME2), N'hh:mm tt'), 'NA') as check_out,
                        ISNULL(total_hrs, 'NA') as total_hrs,
                        FORMAT(CAST(timesheet.ondate  AS DATE), 'MM/dd/yyyy', 'EN-US')  as ondate
                        FROM timesheet WITH (NOLOCK)
                        INNER JOIN employee ON timesheet.empid = employee.id
                        LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
                        INNER JOIN timesheet_x_project_category on timesheet.groupid = timesheet_x_project_category.groupid
                        INNER JOIN (select distinct(groupid), location.lat, location.lang, location.is_checkout
                        from dbo.location  where groupid IN (SELECT groupid FROM timesheet) and is_checkout = 0) eTime
                        ON eTime.groupid = dbo.timesheet.groupid
              WHERE employee.id = @emp
						AND FORMAT(CAST(timesheet.ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
						BETWEEN FORMAT(CAST(@date  AS DATE), 'MM/dd/yyyy', 'EN-US')
                        AND FORMAT(CAST(@date AS DATE), 'MM/dd/yyyy', 'EN-US')
                        AND timesheet.is_deleted = 0) 
						
						REALDATA ORDER BY  CAST(check_in as datetime)",
                        param: new { emp = REST[x].empid, date = Dates[i] }
                    );

                    if (resultsAspNetUsers.Count() > 1)
                    {
                        TimeSpan TotalWorkedHrs = TimeSpan.Zero;

                        var RESTAttendedEmployee = resultsAspNetUsers.Cast<AttendedEmployee>().ToList();
                        AttendedEmployee attendedEmployee = new AttendedEmployee();
                        attendedEmployee.rowno = RESTAttendedEmployee[resultsAspNetUsers.Count() - 1].rowno;
                        attendedEmployee.project_category_type = RESTAttendedEmployee[resultsAspNetUsers.Count() - 1].project_category_type;
                        attendedEmployee.project_or_comp_name = RESTAttendedEmployee[resultsAspNetUsers.Count() - 1].project_or_comp_name;
                        attendedEmployee.timesheet_id = RESTAttendedEmployee[resultsAspNetUsers.Count() - 1].timesheet_id;
                        attendedEmployee.groupid = RESTAttendedEmployee[resultsAspNetUsers.Count() - 1].groupid;
                        attendedEmployee.is_checkout = RESTAttendedEmployee[resultsAspNetUsers.Count() - 1].is_checkout;
                        attendedEmployee.employee_id = RESTAttendedEmployee[resultsAspNetUsers.Count() - 1].employee_id;
                        attendedEmployee.workhour = RESTAttendedEmployee[resultsAspNetUsers.Count() - 1].workhour;
                        attendedEmployee.full_name = RESTAttendedEmployee[resultsAspNetUsers.Count() - 1].full_name;
                        attendedEmployee.check_in = RESTAttendedEmployee[resultsAspNetUsers.Count() - 1].check_in;
                        attendedEmployee.check_out = RESTAttendedEmployee[resultsAspNetUsers.Count() - 1].check_out;

                        TotalWorkedHrs = new TimeSpan(RESTAttendedEmployee.Select(item => item.total_hrs).Where(x => !x.Contains("NA")).Sum(x =>
                         TimeSpan.ParseExact(x, "h\\:mm", CultureInfo.InvariantCulture).Ticks));

                        attendedEmployee.total_hrs = TotalWorkedHrs.ToString(@"hh\:mm");
                        attendedEmployee.lat = RESTAttendedEmployee[resultsAspNetUsers.Count() - 1].lat;
                        attendedEmployee.lang = RESTAttendedEmployee[resultsAspNetUsers.Count() - 1].lang;
                        attendedEmployee.ondate = RESTAttendedEmployee[resultsAspNetUsers.Count() - 1].ondate;

                        AttendedEmployeeList.Add(attendedEmployee);
                    }
                    else
                    {
                        AttendedEmployeeList.AddRange(resultsAspNetUsers);
                    }
                }
            }

            return AttendedEmployeeList;
        }

        public async Task<dynamic> TotalEmpAbsentCountByOrgIDAndDate(string org_id, string fromDate, string toDate)
        {
            int AbsentCount = 0;
            var OffDaysList = ExceptOffDaysDates(org_id, fromDate, toDate);

            for (int i = 0; i < OffDaysList.Count(); i++)
            {
                var count = await GetEmpAbsentCountAttendedByOrgIDAndDate(org_id, OffDaysList[i].ToString(), OffDaysList[i].ToString());
                AbsentCount += count.Count();
            }
            return AbsentCount;
        }

        public async Task<dynamic> GetTimesheetDashboardGridAbsentDataByOrgIDAndDate(string org_id, string fromDate, string toDate)
        {
            List<TimesheetAbsent> AbsentData = new List<TimesheetAbsent>();

            var OffDaysList = ExceptOffDaysDates(org_id, fromDate, toDate);
            for (int i = 0; i < OffDaysList.Count(); i++)
            {
                var EmpList = await GetEmpAbsentCountAttendedByOrgIDAndDate(org_id, OffDaysList[i].ToString(), OffDaysList[i].ToString()).ConfigureAwait(false);
                var Result = await GettingAllAbsentEmployeeList(OffDaysList[i].ToString(), EmpList.ToList()).ConfigureAwait(false);
                AbsentData.AddRange(Result);
            }

            return AbsentData;
        }

        public async Task<dynamic> GetCheckOutLocationByGroupID(string GroupID)
        {
            var resultsAspNetUsers = await QueryAsync<dynamic>(
                sql: @"SELECT lat, lang
                    FROM dbo.location
                    WHERE groupid =@GroupID
                        AND is_checkout = 1
                        AND is_deleted = 0",
                param: new { GroupID }
            );
            return resultsAspNetUsers;
        }

        public async Task<dynamic> TotalEmpOverTimeCountByOrgIDAndDate(string org_id, string fromDate, string toDate)
        {
            List<DateTime> Dates = Enumerable.Range(0, (Convert.ToDateTime(toDate) - Convert.ToDateTime(fromDate)).Days + 1)
                              .Select(d => Convert.ToDateTime(fromDate).AddDays(d)).ToList();
            List<DateTime> ExceptOffDays = ExceptOffDaysDates(org_id, fromDate, toDate);
            List<string> OffDays = OffDaysDates(org_id, fromDate, toDate);
            List<AttendedEmployee> AttendedEmployeeList = new List<AttendedEmployee>();


            for (int i = 0; i < Dates.Count; i++)
            {
                var xdate = OffDays.Any(b => b.Contains(Dates[i].ToString()));
                List<ResultSingleCheckin> ResultSingleCheckinList = new List<ResultSingleCheckin>();

                var ResultSingleList = await SingleCheckINResult(org_id, Dates[i], xdate).ConfigureAwait(false);
                var ResultMultipleList = await MultipleCheckINResult(org_id, Dates[i], xdate).ConfigureAwait(false);

                ResultSingleCheckinList.AddRange(ResultSingleList);
                ResultSingleCheckinList.AddRange(ResultMultipleList);

                var REST = ResultSingleCheckinList.Cast<ResultSingleCheckin>().ToList();

                for (int x = 0; x < REST.Count(); x++)
                {

                    var resultsAspNetUsers = await QueryAsync<AttendedEmployee>(
                        sql: @"SELECT
                        ROW_NUMBER() OVER (ORDER BY full_name) AS rowno,
                        project_category_type, project_or_comp_name,
                        timesheet_id, groupid, is_checkout,
                        employee_id, full_name, check_in,
                        check_out, total_hrs, lat, lang, ondate

                FROM (

                    SELECT
                        timesheet_x_project_category.project_category_type,
                        timesheet_x_project_category.project_or_comp_name,
                        timesheet.id as timesheet_id,
                        timesheet.groupid as groupid,
                        employee.id as employee_id,
                        eTime.lat as lat,
                        eTime.lang as lang,
                        eTime.is_checkout as is_checkout,
                        employee.full_name,
                        employee_type.employee_type_name as employee_type_name,
                        employee.workemail as workemail,
                        employee.emp_code as emp_code,
                        employee.phone as phone,
                        FORMAT(CAST(timesheet.check_in AS DATETIME2), N'hh:mm tt') as check_in,
                        ISNULL(FORMAT(CAST(timesheet.check_out AS DATETIME2), N'hh:mm tt'), 'NA') as check_out,
                        ISNULL(total_hrs, 'NA') as total_hrs,
                        FORMAT(CAST(timesheet.ondate  AS DATE), 'MM/dd/yyyy', 'EN-US')  as ondate
                        FROM timesheet WITH (NOLOCK)
                        INNER JOIN employee ON timesheet.empid = employee.id
                        LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
                        INNER JOIN timesheet_x_project_category on timesheet.groupid = timesheet_x_project_category.groupid
                        INNER JOIN (select distinct(groupid), location.lat, location.lang, location.is_checkout
                        from dbo.location  where groupid IN (SELECT groupid FROM timesheet) and is_checkout = 0) eTime
                        ON eTime.groupid = dbo.timesheet.groupid
              WHERE employee.id = @emp
						AND FORMAT(CAST(timesheet.ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
						BETWEEN FORMAT(CAST(@date  AS DATE), 'MM/dd/yyyy', 'EN-US')
                        AND FORMAT(CAST(@date AS DATE), 'MM/dd/yyyy', 'EN-US')
                        AND timesheet.is_deleted = 0) 
						
						REALDATA ORDER BY  CAST(check_in as datetime)",
                        param: new { emp = REST[x].empid, date = Dates[i] }
                    );

                    if (resultsAspNetUsers.Count() > 1)
                    {
                        TimeSpan TotalWorkedHrs = TimeSpan.Zero;

                        var RESTAttendedEmployee = resultsAspNetUsers.Cast<AttendedEmployee>().ToList();
                        AttendedEmployee attendedEmployee = new AttendedEmployee();
                        attendedEmployee.rowno = RESTAttendedEmployee[0].rowno;
                        attendedEmployee.project_category_type = RESTAttendedEmployee[0].project_category_type;
                        attendedEmployee.project_or_comp_name = RESTAttendedEmployee[0].project_or_comp_name;
                        attendedEmployee.timesheet_id = RESTAttendedEmployee[0].timesheet_id;
                        attendedEmployee.groupid = RESTAttendedEmployee[0].groupid;
                        attendedEmployee.is_checkout = RESTAttendedEmployee[0].is_checkout;
                        attendedEmployee.employee_id = RESTAttendedEmployee[0].employee_id;
                        attendedEmployee.workhour = RESTAttendedEmployee[0].workhour;
                        attendedEmployee.full_name = RESTAttendedEmployee[0].full_name;
                        attendedEmployee.check_in = RESTAttendedEmployee[0].check_in;
                        attendedEmployee.check_out = RESTAttendedEmployee[resultsAspNetUsers.Count() - 1].check_out;

                        TotalWorkedHrs = new TimeSpan(RESTAttendedEmployee.Select(item => item.total_hrs).Where(x => !x.Contains("NA")).Sum(x =>
                         TimeSpan.ParseExact(x, "h\\:mm", CultureInfo.InvariantCulture).Ticks));

                        attendedEmployee.total_hrs = TotalWorkedHrs.ToString(@"hh\:mm");
                        attendedEmployee.lat = RESTAttendedEmployee[0].lat;
                        attendedEmployee.lang = RESTAttendedEmployee[0].lang;
                        attendedEmployee.ondate = RESTAttendedEmployee[0].ondate;

                        AttendedEmployeeList.Add(attendedEmployee);
                    }
                    else
                    {
                        AttendedEmployeeList.AddRange(resultsAspNetUsers);
                    }
                }
            }

            return AttendedEmployeeList;
        }

        public async Task<dynamic> TotalEmpLessHoursByOrgIDAndDate(string org_id, string fromDate, string toDate)
        {
            List<DateTime> Dates = Enumerable.Range(0, (Convert.ToDateTime(toDate) - Convert.ToDateTime(fromDate)).Days + 1)
                              .Select(d => Convert.ToDateTime(fromDate).AddDays(d)).ToList();
            List<DateTime> ExceptOffDays = ExceptOffDaysDates(org_id, fromDate, toDate);
            List<string> OffDays = OffDaysDates(org_id, fromDate, toDate);
            List<AttendedEmployee> AttendedEmployeeList = new List<AttendedEmployee>();

            try
            {
                for (int i = 0; i < Dates.Count; i++)
                {
                    var xdate = OffDays.Any(b => b.Contains(Dates[i].ToString()));
                    List<ResultSingleCheckin> ResultSingleCheckinList = new List<ResultSingleCheckin>();

                    var ResultSingleList = await SingleCheckINExceptionResult(org_id, Dates[i], xdate).ConfigureAwait(false);
                    var ResultMultipleList = await MultipleCheckINExceptionResult(org_id, Dates[i], xdate).ConfigureAwait(false);

                    ResultSingleCheckinList.AddRange(ResultSingleList);
                    ResultSingleCheckinList.AddRange(ResultMultipleList);

                    var REST = ResultSingleCheckinList.Cast<ResultSingleCheckin>().ToList();

                    for (int x = 0; x < REST.Count(); x++)
                    {

                        var resultsAspNetUsers = await QueryAsync<AttendedEmployee>(
                                  sql: @"SELECT
                        ROW_NUMBER() OVER (ORDER BY full_name) AS rowno,
                        project_category_type, project_or_comp_name,
                        timesheet_id, groupid, is_checkout,
                        employee_id, full_name, check_in,
                        check_out, total_hrs, lat, lang, ondate

                FROM (

                    SELECT
                        timesheet_x_project_category.project_category_type,
                        timesheet_x_project_category.project_or_comp_name,
                        timesheet.id as timesheet_id,
                        timesheet.groupid as groupid,
                        employee.id as employee_id,
                        eTime.lat as lat,
                        eTime.lang as lang,
                        eTime.is_checkout as is_checkout,
                        employee.full_name,
                        employee_type.employee_type_name as employee_type_name,
                        employee.workemail as workemail,
                        employee.emp_code as emp_code,
                        employee.phone as phone,
                        FORMAT(CAST(timesheet.check_in AS DATETIME2), N'hh:mm tt') as check_in,
                        ISNULL(FORMAT(CAST(timesheet.check_out AS DATETIME2), N'hh:mm tt'), 'NA') as check_out,
                        ISNULL(total_hrs, 'NA') as total_hrs,
                        FORMAT(CAST(timesheet.ondate  AS DATE), 'MM/dd/yyyy', 'EN-US')  as ondate
                        FROM timesheet WITH (NOLOCK)
                        INNER JOIN employee ON timesheet.empid = employee.id
                        LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
                        INNER JOIN timesheet_x_project_category on timesheet.groupid = timesheet_x_project_category.groupid
                        INNER JOIN (select distinct(groupid), location.lat, location.lang, location.is_checkout
                        from dbo.location  where groupid IN (SELECT groupid FROM timesheet) and is_checkout = 0) eTime
                        ON eTime.groupid = dbo.timesheet.groupid
              WHERE employee.id = @emp
						AND FORMAT(CAST(timesheet.ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
						BETWEEN FORMAT(CAST(@date  AS DATE), 'MM/dd/yyyy', 'EN-US')
                        AND FORMAT(CAST(@date AS DATE), 'MM/dd/yyyy', 'EN-US')
                        AND timesheet.is_deleted = 0) 
						
						REALDATA ORDER BY  CAST(check_in as datetime)",
                                  param: new { emp = REST[x].empid, date = Dates[i] }
                        );

                        if (resultsAspNetUsers.Count() > 1)
                        {
                            TimeSpan TotalWorkedHrs = TimeSpan.Zero;


                            var RESTAttendedEmployee = resultsAspNetUsers.Cast<AttendedEmployee>().ToList();
                            AttendedEmployee attendedEmployee = new AttendedEmployee();
                            attendedEmployee.rowno = RESTAttendedEmployee[0].rowno;
                            attendedEmployee.project_category_type = RESTAttendedEmployee[0].project_category_type;
                            attendedEmployee.project_or_comp_name = RESTAttendedEmployee[0].project_or_comp_name;
                            attendedEmployee.timesheet_id = RESTAttendedEmployee[0].timesheet_id;
                            attendedEmployee.groupid = RESTAttendedEmployee[0].groupid;
                            attendedEmployee.is_checkout = RESTAttendedEmployee[0].is_checkout;
                            attendedEmployee.employee_id = RESTAttendedEmployee[0].employee_id;
                            attendedEmployee.workhour = RESTAttendedEmployee[0].workhour;
                            attendedEmployee.full_name = RESTAttendedEmployee[0].full_name;
                            attendedEmployee.check_in = RESTAttendedEmployee[0].check_in;
                            attendedEmployee.check_out = RESTAttendedEmployee[resultsAspNetUsers.Count() - 1].check_out;

                            TotalWorkedHrs = new TimeSpan(RESTAttendedEmployee.Select(item => item.total_hrs).Where(x => !x.Contains("NA")).Sum(x =>
                             TimeSpan.ParseExact(x, "h\\:mm", CultureInfo.InvariantCulture).Ticks));

                            attendedEmployee.total_hrs = TotalWorkedHrs.ToString(@"hh\:mm");
                            attendedEmployee.lat = RESTAttendedEmployee[0].lat;
                            attendedEmployee.lang = RESTAttendedEmployee[0].lang;
                            attendedEmployee.ondate = RESTAttendedEmployee[0].ondate;

                            AttendedEmployeeList.Add(attendedEmployee);
                        }
                        else
                        {
                            AttendedEmployeeList.AddRange(resultsAspNetUsers);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return AttendedEmployeeList;
        }

        public async Task<dynamic> TotalLocationExceptionByOrgIDAndDate(string org_id, string fromDate, string toDate)
        {
            List<DateTime> Dates = Enumerable.Range(0, (Convert.ToDateTime(toDate) - Convert.ToDateTime(fromDate)).Days + 1)
                              .Select(d => Convert.ToDateTime(fromDate).AddDays(d)).ToList();
            //List<DateTime> ExceptOffDays = ExceptOffDaysDates(org_id, fromDate, toDate);
            //List<string> OffDays = OffDaysDates(org_id, fromDate, toDate);
            List<AttendedEmployee> AttendedEmployeeList = new List<AttendedEmployee>();


            for (int i = 0; i < Dates.Count; i++)
            {
                var resultsAspNetUsers = await QueryAsync<AttendedEmployee>(
                    sql: @"SELECT
                        ROW_NUMBER() OVER (ORDER BY full_name) AS rowno,
                        project_category_type, project_or_comp_id, project_or_comp_name,
                        timesheet_id, groupid, is_checkout,
                        employee_id, full_name, check_in,
                        check_out, total_hrs, lat, lang, ondate

                FROM (

                    SELECT
                        timesheet_x_project_category.project_category_type,
                        timesheet_x_project_category.project_or_comp_id,
                        timesheet_x_project_category.project_or_comp_name,
                        timesheet.id as timesheet_id,
                        timesheet.groupid as groupid,
                        employee.id as employee_id,
                        location_exception.checkin_lat as lat,
                        location_exception.checkin_lang as lang,
                        eTime.is_checkout as is_checkout,
                        employee.full_name,
                        employee_type.employee_type_name as employee_type_name,
                        employee.workemail as workemail,
                        employee.emp_code as emp_code,
                        employee.phone as phone,
                        FORMAT(CAST(timesheet.check_in AS DATETIME2), N'hh:mm tt') as check_in,
                        ISNULL(FORMAT(CAST(timesheet.check_out AS DATETIME2), N'hh:mm tt'), 'NA') as check_out,
                        ISNULL(total_hrs, 'NA') as total_hrs,
                        FORMAT(CAST(timesheet.ondate  AS DATE), 'MM/dd/yyyy', 'EN-US')  as ondate
                        FROM timesheet WITH (NOLOCK)
                        INNER JOIN location_exception ON timesheet.groupid = location_exception.group_id
                        INNER JOIN employee ON timesheet.empid = employee.id
                        LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
                        INNER JOIN timesheet_x_project_category on timesheet.groupid = timesheet_x_project_category.groupid
                        INNER JOIN (select distinct(groupid), location.lat, location.lang, location.is_checkout
                        from dbo.location  where groupid IN (SELECT groupid FROM timesheet) and is_checkout = 0) eTime
                        ON eTime.groupid = dbo.timesheet.groupid
                    WHERE employee.org_id =@org_id
						AND FORMAT(CAST(timesheet.ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
						BETWEEN FORMAT(CAST(@date  AS DATE), 'MM/dd/yyyy', 'EN-US')
                        AND FORMAT(CAST(@date AS DATE), 'MM/dd/yyyy', 'EN-US')
						AND location_exception.is_chkin_inrange = 0
                        AND timesheet.is_deleted = 0) REALDATA
                        ORDER BY  CAST(check_in as datetime)",
                    param: new { org_id, date = Dates[i] }
                );

                AttendedEmployeeList.AddRange(resultsAspNetUsers);
            }

            return AttendedEmployeeList;
        }

        public async Task<dynamic> TotalLocationCheckOutExceptionByOrgIDAndDate(string org_id, string fromDate, string toDate)
        {
            List<DateTime> Dates = Enumerable.Range(0, (Convert.ToDateTime(toDate) - Convert.ToDateTime(fromDate)).Days + 1)
                              .Select(d => Convert.ToDateTime(fromDate).AddDays(d)).ToList();
            //List<DateTime> ExceptOffDays = ExceptOffDaysDates(org_id, fromDate, toDate);
            //List<string> OffDays = OffDaysDates(org_id, fromDate, toDate);
            List<AttendedEmployee> AttendedEmployeeList = new List<AttendedEmployee>();


            for (int i = 0; i < Dates.Count; i++)
            {
                var resultsAspNetUsers = await QueryAsync<AttendedEmployee>(
                    sql: @"SELECT
                        ROW_NUMBER() OVER (ORDER BY full_name) AS rowno,
                        project_category_type, project_or_comp_id, project_or_comp_name,
                        timesheet_id, groupid, is_checkout,
                        employee_id, full_name, check_in,
                        check_out, total_hrs, lat, lang, ondate

                FROM (

                    SELECT
                        timesheet_x_project_category.project_category_type,
                        timesheet_x_project_category.project_or_comp_id,
                        timesheet_x_project_category.project_or_comp_name,
                        timesheet.id as timesheet_id,
                        timesheet.groupid as groupid,
                        employee.id as employee_id,
                        location_exception.checkout_lat as lat,
                        location_exception.checkout_lang as lang,
                        eTime.is_checkout as is_checkout,
                        employee.full_name,
                        employee_type.employee_type_name as employee_type_name,
                        employee.workemail as workemail,
                        employee.emp_code as emp_code,
                        employee.phone as phone,
                        FORMAT(CAST(timesheet.check_in AS DATETIME2), N'hh:mm tt') as check_in,
                        ISNULL(FORMAT(CAST(timesheet.check_out AS DATETIME2), N'hh:mm tt'), 'NA') as check_out,
                        ISNULL(total_hrs, 'NA') as total_hrs,
                        FORMAT(CAST(timesheet.ondate  AS DATE), 'MM/dd/yyyy', 'EN-US')  as ondate
                        FROM timesheet WITH (NOLOCK)
                        INNER JOIN location_exception ON timesheet.groupid = location_exception.group_id
                        INNER JOIN employee ON timesheet.empid = employee.id
                        LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
                        INNER JOIN timesheet_x_project_category on timesheet.groupid = timesheet_x_project_category.groupid
                        INNER JOIN (select distinct(groupid), location.lat, location.lang, location.is_checkout
                        from dbo.location  where groupid IN (SELECT groupid FROM timesheet) and is_checkout = 0) eTime
                        ON eTime.groupid = dbo.timesheet.groupid
                    WHERE employee.org_id =@org_id
						AND FORMAT(CAST(timesheet.ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
						BETWEEN FORMAT(CAST(@date  AS DATE), 'MM/dd/yyyy', 'EN-US')
                        AND FORMAT(CAST(@date AS DATE), 'MM/dd/yyyy', 'EN-US')
						AND location_exception.is_chkout_inrange = 0
                        AND timesheet.is_deleted = 0) REALDATA
                        ORDER BY  CAST(check_in as datetime)",
                    param: new { org_id, date = Dates[i] }
                );

                AttendedEmployeeList.AddRange(resultsAspNetUsers);
            }

            return AttendedEmployeeList;
        }

        public async Task<dynamic> GetTimesheetActivityByGroupAndDate(string GroupID, string Date)
        {
            return await QueryAsync<dynamic>(
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
                            AND FORMAT(CAST(timesheet_administrative_activity.ondate AS DATE), 'dd-MM-yyyy', 'EN-US') = FORMAT(CAST(@Date AS DATE), 'dd-MM-yyyy', 'EN-US')

                        UNION

                          SELECT
		                        UPPER(dbo.timesheet_x_project_category.project_category_type) + ': '
		                        + ISNULL(dbo.project.project_name, 'Activity') + ' ('
		                        + ISNULL(NULLIF(FORMAT(CAST(eTime.check_in AS DATETIME2), N'hh:mm tt'), ' '), 'NA') + ' - '
		                        + ISNULL(NULLIF(FORMAT(CAST(eTime.check_out AS DATETIME2), N'hh:mm tt'), ' '), 'NA')  + ' | '
		                        + ISNULL(NULLIF(eTime.total_hrs, ' '), 'NA') +  ' )' as timesheet,
		                        activity_name =ISNULL( dbo.project_activity.activity_name, dbo.timesheet_activity.milestone_name),
		                        task_name = ISNULL( dbo.timesheet_activity.task_name, 'NA'),
		                        remarks= dbo.timesheet_activity.remarks,
		                        FORMAT(CAST(dbo.timesheet_activity.start_time AS DATETIME2), N'hh:mm tt') as start_time ,
		                        FORMAT(CAST(dbo.timesheet_activity.end_time AS DATETIME2), N'hh:mm tt') as end_time,
		                        timesheet_activity.total_hrs,
		                        timesheet_activity.is_billable,
		                        FORMAT(dbo.timesheet_activity.ondate, 'dd-MM-yyyy', 'en-US') AS ondate
                                    FROM
                                        [dbo].[timesheet_activity] WITH (NOLOCK)
										LEFT JOIN project on project.id = [timesheet_activity].project_id
										LEFT JOIN project_activity on project_activity.id =[timesheet_activity].milestone_id
                                        LEFT JOIN task on dbo.timesheet_activity.task_id = task.id
				                        INNER JOIN (select top 1 * from dbo.timesheet   where groupid IN (SELECT groupid FROM timesheet_activity
                                        WHERE dbo.timesheet_activity.groupid = @GroupID)) eTime
				                        on eTime.groupid = dbo.timesheet_activity.groupid
				                        INNER JOIN timesheet_x_project_category on timesheet_x_project_category.groupid = dbo.timesheet_activity.groupid
                                        WHERE dbo.timesheet_activity.groupid =@GroupID
                                        AND FORMAT(CAST(timesheet_activity.ondate AS DATE), 'dd-MM-yyyy', 'EN-US') = FORMAT(CAST(@Date AS DATE), 'dd-MM-yyyy', 'EN-US')",
                param: new { GroupID, Date }
            );
        }

        public async Task<dynamic> AllProjectRatioByOrgID(string OrgID)
        {
            List<UtilsProjectAndRatio> utilsProjectAndRatios = new List<UtilsProjectAndRatio>();

            var projectList = await QueryAsync<string>(
               sql: @"SELECT id from project where org_id = @OrgID and is_deleted = 0",
               param: new { OrgID }
           );

            foreach (var item in projectList)
            {
                UtilsProjectAndRatio utilsProjectAndRatio = await QuerySingleOrDefaultAsync<UtilsProjectAndRatio>(
                        sql: @"SELECT 
                            dbo.project.id as project_id,
                            dbo.project.project_name as project_name,
                            ISNULL(SUM((SELECT count(*)
                            FROM dbo.project_activity WITH(NOLOCK)
                            LEFT JOIN dbo.project_status on dbo.project_activity.status_id = dbo.project_status.id
                            LEFT JOIN dbo.project on dbo.project_activity.project_id = dbo.project.id
                            WHERE dbo.project.org_id = @OrgID
                            AND
                            dbo.project_activity.is_deleted = 0 and
                            dbo.project.id = @item
                            and
                            dbo.project_status.project_status_name = 'Completed'
                            group by 
                            dbo.project.project_name) * 100 / COUNT(*)

                            ) over() , '0') AS ratio
                            FROM dbo.project_activity WITH(NOLOCK)
                            inner JOIN dbo.project on dbo.project_activity.project_id = dbo.project.id
                            WHERE dbo.project.org_id = @OrgID
                            and
                            dbo.project.id = @item
                            AND
                            dbo.project_activity.is_deleted = 0
                            group by 
                            dbo.project.project_name,  dbo.project.id",
                        param: new { OrgID, item }
                      );

                var utilsProjectDetails = await QuerySingleOrDefaultAsync<UtilsProjectAndRatio>(
                       sql: @"SELECT [dbo].[project_type].type_name as project_type, 
                                dbo.status.status_name as project_status,
                            FORMAT(dbo.project.start_date, 'dd-MM-yyyy', 'en-US') as start_date,
                            FORMAT(dbo.project.end_date, 'dd-MM-yyyy', 'en-US') as end_date,
                                [dbo].[customer].cst_name  from dbo.project
                            LEFT JOIN [dbo].[project_type] on dbo.project.project_type_id =  [dbo].[project_type].id
                            LEFT JOIN dbo.status on dbo.project.project_status_id = dbo.status.id
                            LEFT JOIN [dbo].[customer_x_project] on dbo.project.id = [dbo].[customer_x_project].project_id
                            LEFT JOIN [dbo].[customer] on [dbo].[customer_x_project].cst_id = [dbo].[customer].id
                            WHERE dbo.project.id =  @item
                            AND   dbo.project.is_deleted = 0",
                       param: new { item }
                    );

                if (utilsProjectDetails != null && utilsProjectAndRatio != null)
                {
                    utilsProjectAndRatio.cst_name = ((utilsProjectDetails.cst_name) != null) ? utilsProjectDetails.cst_name : string.Empty;
                    utilsProjectAndRatio.project_type = ((utilsProjectDetails.project_type) != null) ? utilsProjectDetails.project_type : string.Empty;
                    utilsProjectAndRatio.project_status = ((utilsProjectDetails.project_status) != null) ? utilsProjectDetails.project_status : string.Empty; // (utilsProjectDetails.project_status ?? string.Empty);
                    utilsProjectAndRatio.start_date = ((utilsProjectDetails.start_date) != null) ? utilsProjectDetails.start_date : string.Empty;  //(utilsProjectDetails.start_date ?? string.Empty);
                    utilsProjectAndRatio.end_date = ((utilsProjectDetails.end_date) != null) ? utilsProjectDetails.end_date : string.Empty;  //(utilsProjectDetails.end_date ?? string.Empty);
                }
                utilsProjectAndRatios.Add(utilsProjectAndRatio);
            }

            return utilsProjectAndRatios;
        }

        public async Task<dynamic> GetAllTimesheetRecentActivityList(string org_id, string fromDate, string toDate)
        {
            var resultsAspNetUsers = await QueryAsync<dynamic>(
                sql: @"SELECT DISTINCT( dbo.timesheet.groupid), 
                            FORMAT(CAST( dbo.timesheet.ondate AS DATETIME2), N'hh:mm tt')  
                        FROM dbo.timesheet WITH (NOLOCK)
                            INNER JOIN dbo.employee on dbo.timesheet.empid = dbo.employee.id
                        WHERE dbo.employee.org_id =@org_id
                            AND FORMAT(CAST(timesheet.ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
                            BETWEEN FORMAT(CAST(@fromDate AS DATE), 'MM/dd/yyyy', 'EN-US')
                            AND FORMAT(CAST(@toDate AS DATE), 'MM/dd/yyyy', 'EN-US')
                            AND  dbo.timesheet.is_deleted = 0
                        ORDER BY FORMAT(CAST( dbo.timesheet.ondate AS DATETIME2), N'hh:mm tt') DESC;",
                      param: new { org_id, fromDate, toDate }
                  );
            return resultsAspNetUsers;
        }

        public async Task<dynamic> GetAllSingleCheckInEmployeesForHangFireJobs(string org_id, string fromDate, string toDate)
        {
            List<DateTime> Dates = Enumerable.Range(0, (Convert.ToDateTime(toDate) - Convert.ToDateTime(fromDate)).Days + 1)
                              .Select(d => Convert.ToDateTime(fromDate).AddDays(d)).ToList();
            List<DateTime> ExceptOffDays = ExceptOffDaysDates(org_id, fromDate, toDate);
            List<string> OffDays = OffDaysDates(org_id, fromDate, toDate);
            List<AttendedEmployee> AttendedEmployeeList = new List<AttendedEmployee>();

            for (int i = 0; i < Dates.Count; i++)
            {
                var xdate = OffDays.Any(b => b.Contains(Dates[i].ToString()));
                List<ResultSingleCheckin> ResultSingleCheckinList = new List<ResultSingleCheckin>();

                var ResultSingleList = await SingleFirstCheckINResult(org_id, Dates[i], xdate).ConfigureAwait(false);
                ResultSingleCheckinList.AddRange(ResultSingleList);

                var REST = ResultSingleCheckinList.Cast<ResultSingleCheckin>().ToList();

                for (int x = 0; x < REST.Count(); x++)
                {

                    var resultsAspNetUsers = await QueryAsync<AttendedEmployee>(
                        sql: @"SELECT
                        ROW_NUMBER() OVER (ORDER BY full_name) AS rowno,
                        project_category_type, project_or_comp_name,
                        timesheet_id, groupid, is_checkout,
                        employee_id, full_name, check_in,
                        check_out, total_hrs, lat, lang, ondate

                FROM (

                    SELECT
                        timesheet_x_project_category.project_category_type,
                        timesheet_x_project_category.project_or_comp_name,
                        timesheet.id as timesheet_id,
                        timesheet.groupid as groupid,
                        employee.id as employee_id,
                        eTime.lat as lat,
                        eTime.lang as lang,
                        eTime.is_checkout as is_checkout,
                        employee.full_name,
                        employee_type.employee_type_name as employee_type_name,
                        employee.workemail as workemail,
                        employee.emp_code as emp_code,
                        employee.phone as phone,
                        FORMAT(CAST(timesheet.check_in AS DATETIME2), N'hh:mm tt') as check_in,
                        ISNULL(FORMAT(CAST(timesheet.check_out AS DATETIME2), N'hh:mm tt'), 'NA') as check_out,
                        ISNULL(total_hrs, 'NA') as total_hrs,
                        FORMAT(CAST(timesheet.ondate  AS DATE), 'MM/dd/yyyy', 'EN-US')  as ondate
                        FROM timesheet WITH (NOLOCK)
                        INNER JOIN employee ON timesheet.empid = employee.id
                        LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
                        INNER JOIN timesheet_x_project_category on timesheet.groupid = timesheet_x_project_category.groupid
                        INNER JOIN (select distinct(groupid), location.lat, location.lang, location.is_checkout
                        from dbo.location  where groupid IN (SELECT groupid FROM timesheet) and is_checkout = 0) eTime
                        ON eTime.groupid = dbo.timesheet.groupid
              WHERE employee.id = @emp
						AND FORMAT(CAST(timesheet.ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
						BETWEEN FORMAT(CAST(@date  AS DATE), 'MM/dd/yyyy', 'EN-US')
                        AND FORMAT(CAST(@date AS DATE), 'MM/dd/yyyy', 'EN-US')
                        AND timesheet.is_deleted = 0) 
						
						REALDATA ORDER BY  CAST(check_in as datetime)",
                        param: new { emp = REST[x].empid, date = Dates[i] }
                    );

                    AttendedEmployeeList.AddRange(resultsAspNetUsers);
                }
            }

            return AttendedEmployeeList;
        }

        public async Task<IEnumerable<string>> GetAllOrgSetupForHangFireJobs()
        {
            return await QueryAsync<string>(
               sql: @"SELECT org_id FROM organization_setup 
                                    WHERE is_location_validation_req = 1 
                                    OR is_autocheckout_allowed = 1 
                                    AND is_deleted = 0"
                );
        }



        #region PrivateMethods

        //currently not in use
        private IEnumerable<TimesheetAdministrativeDataModel> GetTimesheetAdministrativeDataModel(string GroupID)
        {
            var TimesheetAdministrativeDataModel = Query<TimesheetAdministrativeDataModel>(
                sql: @"SELECT
                            timesheet_administrative_activity.id, timesheet_administrative_activity.administrative_id, administrative.administrative_name
                        FROM timesheet_administrative_activity WITH (NOLOCK)
                        LEFT JOIN administrative on timesheet_administrative_activity.administrative_id = administrative.id
                        WHERE timesheet_administrative_activity.groupid = @GroupID;",
                param: new { GroupID }
            );

            return TimesheetAdministrativeDataModel;
        }

        private IEnumerable<TimesheetCurrentLocationViewModel> GetTimesheetCurrentLocationViewModel(string GroupID)
        {
            var TimesheetCurrentLocationViewModel = Query<TimesheetCurrentLocationViewModel>(
               sql: @"SELECT id, groupid, formatted_address, lat, lang, street_number, route, locality,
                       administrative_area_level_2, administrative_area_level_1, postal_code, country, is_checkout
                FROM location  WITH (NOLOCK) where groupid  = @GroupID;",
                param: new { GroupID }
            );

            return TimesheetCurrentLocationViewModel;
        }

        private async Task<IEnumerable<string>> GetEmpAbsentCountAttendedByOrgIDAndDate(string OrgID, string fromDate, string toDate)
        {
            return await QueryAsync<string>(
                  sql: @"WITH TotalEmployeeAttended AS
                            (SELECT
	                                   employee.id as employee

                                        FROM dbo.employee WITH(NOLOCK)
				                            INNER JOIN timesheet ON  employee.id = timesheet.empid
                                        WHERE employee.org_id = @OrgID
			                            AND FORMAT(CAST(timesheet.ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
			                         BETWEEN FORMAT(CAST(@fromDate AS DATE), 'MM/dd/yyyy', 'EN-US')
			                            AND FORMAT(CAST(@toDate AS DATE), 'MM/dd/yyyy', 'EN-US')
			                            AND employee.is_deleted = 0
                                        AND employee.is_superadmin = 0
                                        AND timesheet.is_deleted = 0
                            ),
                            TotalEmployee as
                            (SELECT
                                        employee.id as employee

                                        FROM dbo.employee WITH(NOLOCK)
                                    WHERE employee.org_id =  @OrgID
                                        AND employee.is_deleted = 0 AND
                                            FORMAT(CAST(created_date AS DATE), 'MM/dd/yyyy', 'EN-US') 
                                        <=
											FORMAT(CAST(@fromDate AS DATE), 'MM/dd/yyyy', 'EN-US')
                            )
                            SELECT employee
                            FROM TotalEmployee
                            EXCEPT
                            SELECT employee
                            FROM TotalEmployeeAttended",
                   param: new { OrgID, fromDate, toDate }
               );
        }

        public static IEnumerable<DateTime> GetDateRange(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                throw new ArgumentException("endDate must be greater than or equal to startDate");

            while (startDate <= endDate)
            {
                yield return startDate;
                startDate = startDate.AddDays(1);
            }
        }

        private IEnumerable<string> GetWeekOffsFromOrg(string OrgID)
        {
            return Query<string>(
                  sql: @"SELECT day_name FROM weekdays 
                            WHERE org_id = @OrgID
                            AND is_deleted = 0
                            AND is_off = 1",
                   param: new { OrgID }
               );
        }

        private async Task<dynamic> GettingTimesheetDashboardDataPerDate(string org_id, string fromDate, string toDate)
        {
            return await QueryAsync<dynamic>(
                sql: @"SELECT employee_type_name, SUM(attandance) as attandance, ondate
                        FROM (
                            SELECT COUNT(DISTINCT(timesheet.empid)) as attandance,
                                ISNULL(UPPER(employee_type.employee_type_name), 'PERMANENT') AS employee_type_name,
			                    FORMAT(CAST(timesheet.ondate  AS DATE), 'dd/MM/yyyy', 'EN-US') as ondate
                                FROM timesheet
                                INNER JOIN employee on timesheet.empid = employee.id
                                LEFT JOIN employee_type on employee.emp_type_id = employee_type.id
                                WHERE
                               employee.org_id = @org_id
                                AND FORMAT(CAST(timesheet.ondate  AS DATE),'MM/dd/yyyy', 'EN-US')
			                    BETWEEN FORMAT(CAST(@fromDate AS DATE), 'MM/dd/yyyy', 'EN-US')
			                    AND FORMAT(CAST(@toDate AS DATE), 'MM/dd/yyyy', 'EN-US')
                                AND timesheet.is_deleted = 0
                                GROUP BY FORMAT(CAST(timesheet.ondate  AS DATE), 'dd/MM/yyyy', 'EN-US'),
                                employee_type.employee_type_name) X GROUP BY employee_type_name, ondate",
                param: new { org_id, fromDate, toDate }
            );
        }

        private async Task<IEnumerable<TimesheetAbsent>> GettingAllAbsentEmployeeList(string Date, List<string> List)
        {
            string EmpList = String.Join("','", List);

            return await QueryAsync<TimesheetAbsent>(
                sql: @"SELECT
	                        employee.id,
	                        employee.full_name,
	                        employee.workemail,
	                        employee.emp_code,
	                        employee.phone,
	                        employee_status.employee_status_name,
	                        employee_type.employee_type_name,
	                        AspNetRoles.Name as role_name,
	                        department.dep_name,
                            department.id as department_id,
	                        designation.designation_name,
							ondate = FORMAT(CAST(@Date AS DATE), 'MM/dd/yyyy', 'EN-US')
                          FROM dbo.employee WITH(NOLOCK)
	                          LEFT JOIN employee_status ON employee.emp_status_id = employee_status.id
	                          LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
	                          LEFT JOIN AspNetRoles ON employee.role_id = AspNetRoles.id
	                          LEFT JOIN department ON employee.deptid = department.id
	                          LEFT JOIN designation ON employee.designation_id = designation.id
                          WHERE employee.id IN ('" + EmpList + "') AND employee.is_deleted = 0",
                param: new { Date }
            );
        }

        private List<string> OffDaysDates(string org_id, string fromDate, string toDate)
        {
            List<string> OffDaysDate = new List<string>();

            string offdays = String.Join("','", GetWeekOffsFromOrg(org_id).ToList());
            if (offdays != null)
            {
                offdays = "Friday";
            }
            List<DateTime> Dates = Enumerable.Range(0, (Convert.ToDateTime(toDate) - Convert.ToDateTime(fromDate)).Days + 1)
                                    .Select(d => Convert.ToDateTime(fromDate).AddDays(d)).ToList();

            foreach (var item in offdays.Split(','))
            {
                var RangeDate = Dates.Where(x => x.DayOfWeek.ToString().Contains(item.ToString(), StringComparison.OrdinalIgnoreCase))
                                     .Select(x => DateTime.Parse(x.Date.ToString()).ToString("MM/dd/yyyy")).ToList();
                OffDaysDate.AddRange(RangeDate);
            }

            return OffDaysDate;
        }

        private List<DateTime> ExceptOffDaysDates(string org_id, string fromDate, string toDate)
        {
            List<DateTime> OffDaysList = new List<DateTime>();
            string offdays = String.Join("','", GetWeekOffsFromOrg(org_id).ToList());
            if (offdays != null)
            {
                offdays = "Friday";
            }
            List<DateTime> Dates = Enumerable.Range(0, (Convert.ToDateTime(toDate) - Convert.ToDateTime(fromDate)).Days + 1)
                                   .Select(d => Convert.ToDateTime(fromDate).AddDays(d)).ToList();

            foreach (var item in offdays.Split(','))
            {
                var RangeDate = Dates.Where(x => x.DayOfWeek.ToString().Contains(item.ToString(), StringComparison.OrdinalIgnoreCase))
                                     .Select(x => Convert.ToDateTime(DateTime.Parse(x.Date.ToString()).ToString("MM/dd/yyyy"))).ToList();

                OffDaysList.AddRange(RangeDate);
            }
            OffDaysList = Dates.Except(OffDaysList).ToList();

            return OffDaysList;
        }

        private async Task<IEnumerable<ResultSingleCheckin>> SingleCheckINResult(string org_id, DateTime Dates, bool xdate)
        {
            IEnumerable<ResultSingleCheckin> ResultSingleCheckinList = await QueryAsync<ResultSingleCheckin>(
                        sql: @"select  distinct(employee.id) as empid, COUNT(employee.id),
						         RIGHT('0' + CAST(SUM((DATEPART(HOUR,total_hrs)*3600)+(DATEPART(MINUTE,total_hrs)*60)+(DATEPART(Second,total_hrs))) / 3600 AS VARCHAR),2) + ':' +
							        RIGHT('0' + CAST((SUM((DATEPART(HOUR,total_hrs)*3600)+(DATEPART(MINUTE,total_hrs)*60)+(DATEPART(Second,total_hrs))) / 60) % 60 AS VARCHAR),2)  as time
                                from timesheet
                                  INNER JOIN employee ON timesheet.empid = employee.id
                                WHERE employee.org_id = @org_id
						        AND FORMAT(CAST(timesheet.ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
						        BETWEEN FORMAT(CAST(@date  AS DATE), 'MM/dd/yyyy', 'EN-US')
                                AND FORMAT(CAST(@date AS DATE), 'MM/dd/yyyy', 'EN-US') AND timesheet.is_deleted = 0
						        group by employee.id
                                having(SUM(( DATEPART(hh, total_hrs ) * 3600 ) + ( DATEPART(mi, total_hrs) * 60 )) > 32400)  
						        and COUNT(timesheet.empid) = 1",
                        param: new { org_id, date = Dates }
                    );
            if (xdate)
                ResultSingleCheckinList = await QueryAsync<ResultSingleCheckin>(
                       sql: @"select  distinct(employee.id) as empid, COUNT(employee.id),
						         RIGHT('0' + CAST(SUM((DATEPART(HOUR,total_hrs)*3600)+(DATEPART(MINUTE,total_hrs)*60)+(DATEPART(Second,total_hrs))) / 3600 AS VARCHAR),2) + ':' +
							        RIGHT('0' + CAST((SUM((DATEPART(HOUR,total_hrs)*3600)+(DATEPART(MINUTE,total_hrs)*60)+(DATEPART(Second,total_hrs))) / 60) % 60 AS VARCHAR),2)  as time
                                FROM timesheet
                                  INNER JOIN employee ON timesheet.empid = employee.id
                                WHERE employee.org_id = @org_id
						        AND FORMAT(CAST(timesheet.ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
						        BETWEEN FORMAT(CAST(@date  AS DATE), 'MM/dd/yyyy', 'EN-US')
                                AND FORMAT(CAST(@date AS DATE), 'MM/dd/yyyy', 'EN-US') AND timesheet.is_deleted = 0
						        GROUP BY employee.id
                                HAVING COUNT(timesheet.empid) = 1",
                       param: new { org_id, date = Dates }
                   );

            return ResultSingleCheckinList;
        }

        private async Task<IEnumerable<ResultSingleCheckin>> MultipleCheckINResult(string org_id, DateTime Dates, bool xdate)
        {
            IEnumerable<ResultSingleCheckin> ResultSingleCheckinList = await QueryAsync<ResultSingleCheckin>(
                        sql: @"select  distinct(employee.id) as empid, COUNT(employee.id),
						         RIGHT('0' + CAST(SUM((DATEPART(HOUR,total_hrs)*3600)+(DATEPART(MINUTE,total_hrs)*60)+(DATEPART(Second,total_hrs))) / 3600 AS VARCHAR),2) + ':' +
							        RIGHT('0' + CAST((SUM((DATEPART(HOUR,total_hrs)*3600)+(DATEPART(MINUTE,total_hrs)*60)+(DATEPART(Second,total_hrs))) / 60) % 60 AS VARCHAR),2)  as time
                                from timesheet
                                  INNER JOIN employee ON timesheet.empid = employee.id
                                WHERE employee.org_id = @org_id
						        AND FORMAT(CAST(timesheet.ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
						        BETWEEN FORMAT(CAST(@date  AS DATE), 'MM/dd/yyyy', 'EN-US')
                                AND FORMAT(CAST(@date AS DATE), 'MM/dd/yyyy', 'EN-US') AND timesheet.is_deleted = 0
						        group by employee.id
                                having(SUM(( DATEPART(hh, total_hrs ) * 3600 ) + ( DATEPART(mi, total_hrs) * 60 )) > 32400)  
						        and COUNT(timesheet.empid) > 1",
                        param: new { org_id, date = Dates }
                    );

            if (xdate)
                ResultSingleCheckinList = await QueryAsync<ResultSingleCheckin>(
                       sql: @"select  distinct(employee.id) as empid, COUNT(employee.id),
						         RIGHT('0' + CAST(SUM((DATEPART(HOUR,total_hrs)*3600)+(DATEPART(MINUTE,total_hrs)*60)+(DATEPART(Second,total_hrs))) / 3600 AS VARCHAR),2) + ':' +
							        RIGHT('0' + CAST((SUM((DATEPART(HOUR,total_hrs)*3600)+(DATEPART(MINUTE,total_hrs)*60)+(DATEPART(Second,total_hrs))) / 60) % 60 AS VARCHAR),2)  as time
                                FROM timesheet
                                  INNER JOIN employee ON timesheet.empid = employee.id
                                WHERE employee.org_id = @org_id
						        AND FORMAT(CAST(timesheet.ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
						        BETWEEN FORMAT(CAST(@date  AS DATE), 'MM/dd/yyyy', 'EN-US')
                                AND FORMAT(CAST(@date AS DATE), 'MM/dd/yyyy', 'EN-US') AND timesheet.is_deleted = 0
						        GROUP BY employee.id
                                HAVING COUNT(timesheet.empid) > 1",
                       param: new { org_id, date = Dates }
                   );

            return ResultSingleCheckinList;
        }

        private async Task<IEnumerable<ResultSingleCheckin>> SingleCheckINExceptionResult(string org_id, DateTime Dates, bool xdate)
        {
            IEnumerable<ResultSingleCheckin> ResultSingleCheckinList = await QueryAsync<ResultSingleCheckin>(
                        sql: @"select  distinct(employee.id) as empid, COUNT(employee.id),
						         RIGHT('0' + CAST(SUM((DATEPART(HOUR,isnull(total_hrs, '00:00'))*3600)+(DATEPART(MINUTE,isnull(total_hrs, '00:00'))*60)+(DATEPART(Second,isnull(total_hrs, '00:00')))) / 3600 AS VARCHAR),2) + ':' +
							        RIGHT('0' + CAST((SUM((DATEPART(HOUR,isnull(total_hrs, '00:00'))*3600)+(DATEPART(MINUTE,isnull(total_hrs, '00:00'))*60)+(DATEPART(Second,isnull(total_hrs, '00:00')))) / 60) % 60 AS VARCHAR),2)  as time
                                from timesheet
                                  INNER JOIN employee ON timesheet.empid = employee.id
                                WHERE employee.org_id = @org_id
						        AND FORMAT(CAST(timesheet.ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
						        BETWEEN FORMAT(CAST(@date  AS DATE), 'MM/dd/yyyy', 'EN-US')
                                AND FORMAT(CAST(@date AS DATE), 'MM/dd/yyyy', 'EN-US') AND timesheet.is_deleted = 0
						        group by employee.id
                                having(SUM(( DATEPART(hh, isnull(total_hrs, '00:00')) * 3600 ) + ( DATEPART(mi, isnull(total_hrs, '00:00')) * 60 )) < 32400)  
						        and COUNT(timesheet.empid) = 1",
                        param: new { org_id, date = Dates }
                    );

            return ResultSingleCheckinList;
        }

        private async Task<IEnumerable<ResultSingleCheckin>> MultipleCheckINExceptionResult(string org_id, DateTime Dates, bool xdate)
        {
            IEnumerable<ResultSingleCheckin> ResultSingleCheckinList = await QueryAsync<ResultSingleCheckin>(
                        sql: @"select   distinct(employee.id) as empid, COUNT(employee.id),
						         RIGHT('0' + CAST(SUM((DATEPART(HOUR,total_hrs)*3600)+(DATEPART(MINUTE,total_hrs)*60)+(DATEPART(Second,total_hrs))) / 3600 AS VARCHAR),2) + ':' +
							        RIGHT('0' + CAST((SUM((DATEPART(HOUR,total_hrs)*3600)+(DATEPART(MINUTE,total_hrs)*60)+(DATEPART(Second,total_hrs))) / 60) % 60 AS VARCHAR),2)  as time
                                from timesheet
                                  INNER JOIN employee ON timesheet.empid = employee.id
                                WHERE employee.org_id = @org_id
						        AND FORMAT(CAST(timesheet.ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
						        BETWEEN FORMAT(CAST(@date  AS DATE), 'MM/dd/yyyy', 'EN-US')
                                AND FORMAT(CAST(@date AS DATE), 'MM/dd/yyyy', 'EN-US') AND timesheet.is_deleted = 0
						        group by employee.id
                                having(SUM(( DATEPART(hh, total_hrs ) * 3600 ) + ( DATEPART(mi, total_hrs) * 60 )) < 32400)  
						        and COUNT(timesheet.empid) > 1",
                        param: new { org_id, date = Dates }
                    );

            return ResultSingleCheckinList;
        }

        private async Task<IEnumerable<ResultSingleCheckin>> SingleFirstCheckINResult(string org_id, DateTime Dates, bool xdate)
        {
            IEnumerable<ResultSingleCheckin> ResultSingleCheckinList = await QueryAsync<ResultSingleCheckin>(
                        sql: @"select   distinct(employee.id) as empid, COUNT(employee.id),
						         RIGHT('0' + CAST(SUM((DATEPART(HOUR,total_hrs)*3600)+(DATEPART(MINUTE,total_hrs)*60)+(DATEPART(Second,total_hrs))) / 3600 AS VARCHAR),2) + ':' +
							        RIGHT('0' + CAST((SUM((DATEPART(HOUR,total_hrs)*3600)+(DATEPART(MINUTE,total_hrs)*60)+(DATEPART(Second,total_hrs))) / 60) % 60 AS VARCHAR),2)  as time
                                from timesheet
                                  INNER JOIN employee ON timesheet.empid = employee.id
                                WHERE employee.org_id = @org_id
						        AND FORMAT(CAST(timesheet.ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
						        BETWEEN FORMAT(CAST(@date  AS DATE), 'MM/dd/yyyy', 'EN-US')
                                AND FORMAT(CAST(@date AS DATE), 'MM/dd/yyyy', 'EN-US') AND timesheet.is_deleted = 0
						        group by employee.id
                                having COUNT(timesheet.empid) = 1",
                        param: new { org_id, date = Dates }
                    );

            return ResultSingleCheckinList;
        }

        private async Task<IEnumerable<ResultSingleCheckin>> MultipleFirstCheckINResult(string org_id, DateTime Dates, bool xdate)
        {
            IEnumerable<ResultSingleCheckin> ResultSingleCheckinList = await QueryAsync<ResultSingleCheckin>(
                        sql: @"select  distinct(employee.id) as empid, COUNT(employee.id),
						         RIGHT('0' + CAST(SUM((DATEPART(HOUR,total_hrs)*3600)+(DATEPART(MINUTE,total_hrs)*60)+(DATEPART(Second,total_hrs))) / 3600 AS VARCHAR),2) + ':' +
							        RIGHT('0' + CAST((SUM((DATEPART(HOUR,total_hrs)*3600)+(DATEPART(MINUTE,total_hrs)*60)+(DATEPART(Second,total_hrs))) / 60) % 60 AS VARCHAR),2)  as time
                                from timesheet
                                  INNER JOIN employee ON timesheet.empid = employee.id
                                WHERE employee.org_id = @org_id
						        AND FORMAT(CAST(timesheet.ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
						        BETWEEN FORMAT(CAST(@date  AS DATE), 'MM/dd/yyyy', 'EN-US')
                                AND FORMAT(CAST(@date AS DATE), 'MM/dd/yyyy', 'EN-US') AND timesheet.is_deleted = 0
						        group by employee.id
                                having COUNT(timesheet.empid) > 1",
                        param: new { org_id, date = Dates }
                    );

            return ResultSingleCheckinList;
        }

        #endregion PrivateMethods
    }


    public class UtilsProjectAndRatio
    {
        public string project_id { get; set; }
        public string project_name { get; set; }
        public string ratio { get; set; }
        public string project_status { get; set; }
        public string project_type { get; set; }
        public string cst_name { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
    }

    public class AttendedEmployee
    {
        public string rowno { get; set; }
        public string project_category_type { get; set; }
        public string project_or_comp_name { get; set; }
        public string timesheet_id { get; set; }
        public string groupid { get; set; }
        public bool is_checkout { get; set; }
        public string employee_id { get; set; }
        public string workhour { get; set; }
        public string full_name { get; set; }
        public string check_in { get; set; }
        public string check_out { get; set; }
        public string total_hrs { get; set; }
        public string lat { get; set; }
        public string lang { get; set; }
        public string ondate { get; set; }
    }
    public class ResultSingleCheckin
    {
        public string empid { get; set; }
        public string time { get; set; }
    }

}
