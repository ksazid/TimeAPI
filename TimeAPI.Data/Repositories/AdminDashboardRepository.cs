using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

        public IEnumerable<AdminDashboard> All()
        {
            return Query<AdminDashboard>(
                sql: ""
            );
        }

        public AdminDashboard Find(string key)
        {
            return QuerySingleOrDefault<AdminDashboard>(
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

        //public dynamic TotalEmployeeDashboardDataByOrgID(string OrgID, string toDate, string fromDate)
        //{
        //    var resultsAspNetUsers = Query<dynamic>(
        //        sql: @"SELECT
        //                SUM(empcount) as attandance, employee_type_name
        //                       FROM
        //                            (

        //                        SELECT
        //                            DISTINCT(FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US')) as ondate,
        //                            count(DISTINCT empid) as empcount,
        //                            ISNULL(UPPER(employee_type.employee_type_name), 'PERMANENT') AS employee_type_name
        //                            FROM timesheet
        //                            INNER JOIN [dbo].[employee] ON  [dbo].[timesheet].empid = [dbo].[employee].id
        //                            LEFT JOIN [dbo].[employee_type] ON  [dbo].[employee].emp_type_id = [dbo].[employee_type].id
        //                            INNER JOIN [dbo].[superadmin_x_org] ON  [dbo].[timesheet].empid = [dbo].[superadmin_x_org].superadmin_empid
        //                        WHERE FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US')
        //                        BETWEEN
        //                            FORMAT(CAST(@fromDate   AS DATE), 'd', 'EN-US')
        //                            AND FORMAT(CAST(@toDate AS DATE), 'd', 'EN-US')
        //                            AND [dbo].[employee].org_id =  @OrgID
        //                            AND [dbo].[employee].is_deleted = 0
        //                            AND [dbo].[timesheet].is_deleted = 0
        //                            group by FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US') , employee_type.employee_type_name

        //                        UNION ALL

        //                        SELECT
        //                            DISTINCT(FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US')) as ondate,
        //                            count(DISTINCT empid) as empcount,
        //                            ISNULL(UPPER(employee_type.employee_type_name), 'PERMANENT') AS employee_type_name
        //                            FROM timesheet
        //                            INNER JOIN [dbo].[employee] ON  [dbo].[timesheet].empid = [dbo].[employee].id
        //                            LEFT JOIN [dbo].[employee_type] ON  [dbo].[employee].emp_type_id = [dbo].[employee_type].id
        //                        WHERE FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US')
        //                            BETWEEN
        //                            FORMAT(CAST(@fromDate   AS DATE), 'd', 'EN-US')
        //                            AND FORMAT(CAST(@toDate AS DATE), 'd', 'EN-US')
        //                            AND [dbo].[employee].org_id =  @OrgID
        //                            AND [dbo].[employee].is_deleted = 0
        //                            AND [dbo].[timesheet].is_deleted = 0
        //                        group by FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US'), employee_type.employee_type_name

        //                            ) x  group by  employee_type_name",
        //        param: new { OrgID, toDate, fromDate }
        //    );
        //    return resultsAspNetUsers;
        //}

        public dynamic TotalDefaultEmpCountByOrgID(string org_id)
        {

            var resultsAspNetUsers = Query<dynamic>(
                sql: @"SELECT employee_type_name, SUM(employee_count) as attandance  FROM
                        (
                        SELECT
                            ISNULL(UPPER(employee_type.employee_type_name), 'PERMANENT') AS employee_type_name,
                            COUNT (DISTINCT [dbo].[employee].id) AS employee_count
                            FROM [dbo].[employee] WITH (NOLOCK)
	                        INNER JOIN superadmin_x_org ON superadmin_x_org.superadmin_empid = employee.id
	                        LEFT JOIN employee_type on employee.emp_type_id = employee_type.id
                            WHERE [dbo].[employee].is_deleted = 0 AND [dbo].[employee].is_inactive = 0
                            AND superadmin_x_org.org_id =  @org_id
                            GROUP BY
                            employee_type.employee_type_name

	                UNION ALL

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

        public dynamic TotalEmpAbsentCountByOrgIDAndDate(string org_id, string fromDate, string toDate)
        {
            List<TimesheetAbsent> TotalEmpCount = new List<TimesheetAbsent>();

            string offdays = GetWeekOffsFromOrg(org_id);
            List<DateTime> OffDaysList = new List<DateTime>();

            //List<DateTime> Dates = GetDateRange(Convert.ToDateTime(fromDate), Convert.ToDateTime(toDate)).ToList();

            //for (int i = 0; i < Dates.Count(); i++)
            //{
            //    if (!offdays.Contains(Dates[i].DayOfWeek.ToString(), StringComparison.OrdinalIgnoreCase))
            //        OffDaysList.Add(Dates[i].Date.ToString());
            //}

            List<DateTime> Dates = Enumerable.Range(0, (Convert.ToDateTime(toDate) - Convert.ToDateTime(fromDate)).Days + 1)
                                   .Select(d => Convert.ToDateTime(fromDate).AddDays(d)).ToList();

            foreach (var item in offdays.Split(','))
            {
                var RangeDate = Dates.Where(x => x.DayOfWeek.ToString().Contains(item.ToString(), StringComparison.OrdinalIgnoreCase))
                                     .Select(x => Convert.ToDateTime(DateTime.Parse(x.Date.ToString()).ToString("MM/dd/yyyy"))).ToList();

                OffDaysList.AddRange(RangeDate);
            }

            OffDaysList = Dates.Except(OffDaysList).ToList();


            var TotalEmp = GetTotalEmpCountByOrgID(org_id);

            for (int i = 0; i < OffDaysList.Count(); i++)
            {
                var result = TotalEmp.Except(GetEmpCountAttendedByOrgIDAndDate(org_id, OffDaysList[i].ToString(), OffDaysList[i].ToString()),
                                new MyComparer()).ToList();
                TotalEmpCount.AddRange(result);
            }

            return TotalEmpCount.Count();
        }

        public dynamic TotalEmpAttentedCountByOrgIDAndDate(string org_id, string fromDate, string toDate)
        {
            List<dynamic> TimesheetDashboardData = new List<dynamic>();
            List<string> OffDaysDate = new List<string>();

            string offdays = GetWeekOffsFromOrg(org_id);
            List<DateTime> Dates = Enumerable.Range(0, (Convert.ToDateTime(toDate) - Convert.ToDateTime(fromDate)).Days + 1)
                                    .Select(d => Convert.ToDateTime(fromDate).AddDays(d)).ToList();

            foreach (var item in offdays.Split(','))
            {
                var RangeDate = Dates.Where(x => x.DayOfWeek.ToString().Contains(item.ToString(), StringComparison.OrdinalIgnoreCase))
                                     .Select(x => DateTime.Parse(x.Date.ToString()).ToString("MM/dd/yyyy")).ToList();
                OffDaysDate.AddRange(RangeDate);
            }

            var DGridData = GettingTimesheetDashboardDataPerDate(org_id, fromDate, toDate, OffDaysDate);

            if (DGridData.Count > 0)
            {
                TimesheetDashboardData.AddRange(DGridData);
            }
            return TimesheetDashboardData;
        }

        public dynamic GetTimesheetDashboardGridDataByOrgIDAndDate(string org_id, string fromDate, string toDate)
        {
            List<string> OffDaysList = new List<string>();

            //string offdays = GetWeekOffsFromOrg(org_id);
            //List<DateTime> Dates = GetDateRange(Convert.ToDateTime(fromDate), Convert.ToDateTime(toDate)).ToList();

            //for (int i = 0; i < Dates.Count(); i++)
            //    if (offdays.Contains(Dates[i].DayOfWeek.ToString(), StringComparison.OrdinalIgnoreCase))
            //        OffDaysList.Add(DateTime.Parse(Dates[i].Date.ToString().Split(' ')[0]).ToString("MM/dd/yyyy"));


            //var date = Dates.Select(x => x.DayOfWeek.ToString().Contains(offdays, StringComparison.OrdinalIgnoreCase));


            string offdays = GetWeekOffsFromOrg(org_id);
            List<DateTime> Dates = Enumerable.Range(0, (Convert.ToDateTime(toDate) - Convert.ToDateTime(fromDate)).Days + 1)
                                    .Select(d => Convert.ToDateTime(fromDate).AddDays(d)).ToList();

            foreach (var item in offdays.Split(','))
            {
                var RangeDate = Dates.Where(x => x.DayOfWeek.ToString().Contains(item.ToString(), StringComparison.OrdinalIgnoreCase))
                                     .Select(x => DateTime.Parse(x.Date.ToString()).ToString("MM/dd/yyyy")).ToList();
                OffDaysList.AddRange(RangeDate);
            }

            string weekoffs = String.Join("','", OffDaysList);

            var resultsAspNetUsers = Query<dynamic>(
                sql: @"SELECT
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
                        FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US')  as ondate
                        FROM timesheet WITH (NOLOCK)
                        INNER JOIN employee ON timesheet.empid = employee.id
                        LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
                        INNER JOIN timesheet_x_project_category on timesheet.groupid = timesheet_x_project_category.groupid
                        INNER JOIN superadmin_x_org ON superadmin_x_org.superadmin_empid = employee.id
                        INNER JOIN (select distinct(groupid), location.lat, location.lang, location.is_checkout
                        FROM dbo.location WHERE groupid IN (SELECT groupid FROM timesheet) and is_checkout = 0) eTime
                        ON eTime.groupid = dbo.timesheet.groupid
                    WHERE
                        superadmin_x_org.org_id = @org_id
                  AND FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US') 
						BETWEEN FORMAT(CAST(@fromDate  AS DATE), 'd', 'EN-US')
                        AND FORMAT(CAST(@toDate AS DATE), 'd', 'EN-US')
                        AND timesheet.is_deleted = 0

                    UNION

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
                        FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US')  as ondate
                        FROM timesheet WITH (NOLOCK)
                        INNER JOIN employee ON timesheet.empid = employee.id
                        LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
                        INNER JOIN timesheet_x_project_category on timesheet.groupid = timesheet_x_project_category.groupid
                        INNER JOIN (select distinct(groupid), location.lat, location.lang, location.is_checkout
                        from dbo.location  where groupid IN (SELECT groupid FROM timesheet) and is_checkout = 0) eTime
                        ON eTime.groupid = dbo.timesheet.groupid
            
                        WHERE employee.org_id =@org_id
						AND FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US') 
						BETWEEN FORMAT(CAST(@fromDate  AS DATE), 'd', 'EN-US')
                        AND FORMAT(CAST(@toDate AS DATE), 'd', 'EN-US')
                        AND timesheet.is_deleted = 0) REALDATA 
						WHERE FORMAT(CAST(ondate  AS DATE), 'MM/dd/yyyy', 'EN-US') NOT IN ('" + weekoffs + "')",
                param: new { org_id, fromDate, toDate }
            );
            return resultsAspNetUsers;
        }

        public dynamic GetCheckOutLocationByGroupID(string GroupID)
        {
            var resultsAspNetUsers = Query<dynamic>(
                sql: @"SELECT lat, lang
                    FROM dbo.location
                    WHERE groupid =@GroupID
                        AND is_checkout = 1
                        AND is_deleted = 0",
                param: new { GroupID }
            );
            return resultsAspNetUsers;
        }

        public dynamic GetTimesheetDashboardGridAbsentDataByOrgIDAndDate(string org_id, string fromDate, string toDate)
        {
            List<TimesheetAbsent> AbsentData = new List<TimesheetAbsent>();

            string offdays = GetWeekOffsFromOrg(org_id);
            List<string> OffDaysList = new List<string>();

            List<DateTime> Dates = GetDateRange(Convert.ToDateTime(fromDate), Convert.ToDateTime(toDate)).ToList();

            for (int i = 0; i < Dates.Count(); i++)
            {
                if (!offdays.Contains(Dates[i].DayOfWeek.ToString(), StringComparison.OrdinalIgnoreCase))
                    OffDaysList.Add(Dates[i].Date.ToString());
            }

            var TotalEmp = GetTotalEmpCountByOrgID(org_id).OrderByDescending(x => x.full_name);

            for (int i = 0; i < OffDaysList.Count(); i++)
            {
                List<TimesheetAbsent> AbsentDataTemp = new List<TimesheetAbsent>();
                var EmpCountAttended = GetEmpCountAttendedByOrgIDAndDate(org_id, OffDaysList[i], OffDaysList[i]);
                var result = TotalEmp.Except(EmpCountAttended, new MyComparer()).ToList();
                AbsentData.AddRange(result);
            }

            return AbsentData;
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

        private IEnumerable<TimesheetAbsent> GetTotalEmpCountByOrgID(string OrgID)
        {
            return Query<TimesheetAbsent>(
                  sql: @"	SELECT
							distinct(employee.id),
							employee.full_name,
							employee.workemail,
							employee.emp_code,
							employee.mobile,
							employee_status.employee_status_name,
							employee_type.employee_type_name,
							AspNetRoles.Name as role_name,
							department.dep_name,
							department.id as department_id,
							designation.designation_name
						FROM dbo.employee WITH(NOLOCK)
							LEFT JOIN employee_status ON employee.emp_status_id = employee_status.id
							LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
							LEFT JOIN AspNetRoles ON employee.role_id = AspNetRoles.id
							LEFT JOIN department ON employee.deptid = department.id
							LEFT JOIN designation ON employee.designation_id = designation.id
						WHERE employee.org_id = @OrgID
							AND employee.is_deleted = 0
					UNION ALL
						SELECT
							distinct(employee.id),
							employee.full_name,
							employee.workemail,
							employee.emp_code,
							employee.phone,
							employee_status.employee_status_name,
							employee_type.employee_type_name,
							AspNetRoles.Name as role_name,
							department.dep_name,
							department.id as department_id,
							designation.designation_name
						FROM dbo.employee WITH(NOLOCK)
							INNER JOIN superadmin_x_org on  dbo.employee.id = superadmin_x_org.superadmin_empid
							LEFT JOIN employee_status ON employee.emp_status_id = employee_status.id
							LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
							LEFT JOIN AspNetRoles ON employee.role_id = AspNetRoles.id
							LEFT JOIN department ON employee.deptid = department.id
							LEFT JOIN designation ON employee.designation_id = designation.id
						WHERE superadmin_x_org.org_id = @OrgID
							AND employee.is_deleted = 0",
                   param: new { OrgID }
               );
        }

        private IEnumerable<TimesheetAbsent> GetEmpCountAttendedByOrgIDAndDate(string OrgID, string fromDate, string toDate)
        {
            return Query<TimesheetAbsent>(
                  sql: @"SELECT
	                        distinct(employee.id),
	                        employee.full_name,
	                        employee.workemail,
	                        employee.emp_code,
	                        employee.phone,
	                        employee_status.employee_status_name,
	                        employee_type.employee_type_name,
	                        AspNetRoles.Name as role_name,
	                        department.dep_name,
                            department.id as department_id,
	                        designation.designation_name
                          FROM dbo.employee WITH(NOLOCK)
							  INNER JOIN timesheet ON  employee.id = timesheet.empid
	                          LEFT JOIN employee_status ON employee.emp_status_id = employee_status.id
	                          LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
	                          LEFT JOIN AspNetRoles ON employee.role_id = AspNetRoles.id
	                          LEFT JOIN department ON employee.deptid = department.id
	                          LEFT JOIN designation ON employee.designation_id = designation.id
                          WHERE employee.org_id = @OrgID
						  AND FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US') 
				          BETWEEN FORMAT(CAST(@fromDate AS DATE), 'd', 'EN-US')
						  AND FORMAT(CAST(@toDate AS DATE), 'd', 'EN-US')
						  AND employee.is_deleted = 0
                          AND employee.is_superadmin = 0
                          AND timesheet.is_deleted = 0
					UNION ALL
						SELECT
	                        distinct(employee.id),
	                        employee.full_name,
	                        employee.workemail,
	                        employee.emp_code,
	                        employee.phone,
	                        employee_status.employee_status_name,
	                        employee_type.employee_type_name,
	                        AspNetRoles.Name as role_name,
	                        department.dep_name,
                            department.id as department_id,
	                        designation.designation_name
                          FROM dbo.employee WITH(NOLOCK)
							  INNER JOIN superadmin_x_org on  dbo.employee.id = superadmin_x_org.superadmin_empid
							  INNER JOIN timesheet ON  employee.id = timesheet.empid
	                          LEFT JOIN employee_status ON employee.emp_status_id = employee_status.id
	                          LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
	                          LEFT JOIN AspNetRoles ON employee.role_id = AspNetRoles.id
	                          LEFT JOIN department ON employee.deptid = department.id
	                          LEFT JOIN designation ON employee.designation_id = designation.id
                          WHERE superadmin_x_org.org_id = @OrgID
						  AND FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US') 
						  BETWEEN FORMAT(CAST(@fromDate AS DATE), 'd', 'EN-US')
						  AND FORMAT(CAST(@toDate AS DATE), 'd', 'EN-US')
						  AND employee.is_deleted = 0
                          AND employee.is_superadmin = 1
                          AND timesheet.is_deleted = 0
                          ORDER BY employee.full_name ASC",
                   param: new { OrgID, fromDate, toDate }
               );
        }

        //private IEnumerable<string> GetDateFromTimesheet(string OrgID, string fromDate, string toDate)
        //{
        //    return Query<string>(
        //          sql: @"SELECT
        //                  FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US') 
        //                  FROM dbo.employee WITH(NOLOCK)
        // INNER JOIN timesheet ON  employee.id = timesheet.empid
        //               WHERE employee.org_id = @OrgID
        //AND FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US') 
        //BETWEEN FORMAT(CAST(@fromDate AS DATE), 'd', 'EN-US')
        //AND FORMAT(CAST(@toDate AS DATE), 'd', 'EN-US')
        //AND employee.is_deleted = 0
        //                  AND timesheet.is_deleted = 0
        //group by FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US') ",
        //           param: new { OrgID, fromDate, toDate }
        //       );
        //}

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
        private string GetWeekOffsFromOrg(string OrgID)
        {
            return QuerySingleOrDefault<string>(
                  sql: @"SELECT start_of_week FROM [dbo].[organization_setup] 
                        WHERE org_id= @OrgID AND is_deleted = 0",
                   param: new { OrgID }
               );
        }
        private dynamic GettingTimesheetDashboardDataPerDate(string org_id, string fromDate, string toDate, List<string> OffDaysList)
        {
            string weekoffs = String.Join("','", OffDaysList);

            return Query<dynamic>(
                sql: @"SELECT employee_type_name, SUM(attandance) as attandance, ondate
                FROM (
                    SELECT COUNT (DISTINCT(timesheet.empid)) as attandance,
                        ISNULL(UPPER(employee_type.employee_type_name), 'PERMANENT') AS employee_type_name,
						FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US') as ondate 
                        FROM timesheet
                        INNER JOIN employee ON timesheet.empid = employee.id
                        INNER JOIN superadmin_x_org ON superadmin_x_org.superadmin_empid = employee.id
                        LEFT JOIN employee_type on employee.emp_type_id = employee_type.id
                        WHERE
                        superadmin_x_org.org_id = @org_id
                        AND FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US') 
			            BETWEEN FORMAT(CAST(@fromDate  AS DATE), 'd', 'EN-US')
                        AND FORMAT(CAST(@toDate AS DATE), 'd', 'EN-US')
                        AND timesheet.is_deleted = 0
                        GROUP BY FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US'),   
                        employee_type.employee_type_name

                    UNION ALL

                    SELECT COUNT(DISTINCT(timesheet.empid)) as attandance,
                        ISNULL(UPPER(employee_type.employee_type_name), 'PERMANENT') AS employee_type_name,
						FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US') as ondate
                        FROM timesheet
                        INNER JOIN employee on timesheet.empid = employee.id
                        LEFT JOIN employee_type on employee.emp_type_id = employee_type.id
                        WHERE
                        employee.org_id = @org_id
                        AND FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US') 
			            BETWEEN FORMAT(CAST(@fromDate  AS DATE), 'd', 'EN-US')
                        AND FORMAT(CAST(@toDate AS DATE), 'd', 'EN-US')
                        AND timesheet.is_deleted = 0
                        GROUP BY FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US'), 
                        employee_type.employee_type_name) X  
                        WHERE FORMAT(CAST(ondate  AS DATE), 'MM/dd/yyyy', 'EN-US') NOT IN ('" + weekoffs + "')" +
                        "GROUP BY employee_type_name, ondate",
                param: new { org_id, fromDate, toDate }
            );
        }

        #endregion PrivateMethods
    }


    sealed class MyComparer : IEqualityComparer<TimesheetAbsent>
    {
        public bool Equals(TimesheetAbsent x, TimesheetAbsent y)
        {
            if (x == null)
                return y == null;
            else if (y == null)
                return false;
            else
                return x.id == y.id && x.full_name == y.full_name;
        }

        public int GetHashCode(TimesheetAbsent obj)
        {
            return obj.id.GetHashCode();
        }
    }
}