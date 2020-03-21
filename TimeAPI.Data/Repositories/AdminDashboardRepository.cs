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

        public dynamic TotalDefaultEmpCountByOrgID(string org_id)
        {
            //SELECT
            //                ISNULL(UPPER(employee_type.employee_type_name), 'PERMANENT') AS employee_type_name,
            //                COUNT (DISTINCT[dbo].[employee].id) AS employee_count
            //                FROM[dbo].[employee] WITH(NOLOCK)

            //                INNER JOIN superadmin_x_org ON superadmin_x_org.superadmin_empid = employee.id

            //                LEFT JOIN employee_type on employee.emp_type_id = employee_type.id
            //                WHERE[dbo].[employee].is_deleted = 0 AND[dbo].[employee].is_inactive = 0
            //                AND superadmin_x_org.org_id = @org_id
            //                GROUP BY
            //                employee_type.employee_type_name

            //        UNION ALL

            var resultsAspNetUsers = Query<dynamic>(
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

        public dynamic TotalEmpAttentedCountByOrgIDAndDate(string org_id, string fromDate, string toDate)
        {
            List<dynamic> TimesheetDashboardData = new List<dynamic>();

            //OffDaysDates(org_id, fromDate, toDate)
            var DGridData = GettingTimesheetDashboardDataPerDate(org_id, fromDate, toDate);

            if (DGridData.Count > 0)
            {
                TimesheetDashboardData.AddRange(DGridData);
            }
            return TimesheetDashboardData;
        }

        public dynamic GetTimesheetDashboardGridDataByOrgIDAndDate(string org_id, string fromDate, string toDate)
        {
            //string weekoffs = String.Join("','", OffDaysDates(org_id, fromDate, toDate));


            //      SELECT
            //                  timesheet_x_project_category.project_category_type,
            //                  timesheet_x_project_category.project_or_comp_name,
            //                  timesheet.id as timesheet_id,
            //                  timesheet.groupid as groupid,
            //                  employee.id as employee_id,
            //                  eTime.lat as lat,
            //                  eTime.lang as lang,
            //                  eTime.is_checkout as is_checkout,
            //                  employee.full_name,
            //                  employee_type.employee_type_name as employee_type_name,
            //                  employee.workemail as workemail,
            //                  employee.emp_code as emp_code,
            //                  employee.phone as phone,
            //                  FORMAT(CAST(timesheet.check_in AS DATETIME2), N'hh:mm tt') as check_in,
            //                  ISNULL(FORMAT(CAST(timesheet.check_out AS DATETIME2), N'hh:mm tt'), 'NA') as check_out,
            //                  ISNULL(total_hrs, 'NA') as total_hrs,
            //                  FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US') as ondate
            //                  FROM timesheet WITH(NOLOCK)
            //                  INNER JOIN employee ON timesheet.empid = employee.id
            //                  LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
            //                  INNER JOIN timesheet_x_project_category on timesheet.groupid = timesheet_x_project_category.groupid
            //                  INNER JOIN superadmin_x_org ON superadmin_x_org.superadmin_empid = employee.id
            //                  INNER JOIN(select distinct(groupid), location.lat, location.lang, location.is_checkout
            //                  FROM dbo.location WHERE groupid IN (SELECT groupid FROM timesheet) and is_checkout = 0) eTime
            //                  ON eTime.groupid = dbo.timesheet.groupid
            //              WHERE
            //                  superadmin_x_org.org_id = @org_id
            //            AND FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US')
            //BETWEEN FORMAT(CAST(@fromDate AS DATE), 'd', 'EN-US')
            //                  AND FORMAT(CAST(@toDate AS DATE), 'd', 'EN-US')
            //                  AND timesheet.is_deleted = 0

            //              UNION

            //WHERE FORMAT(CAST(ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
            //            NOT IN('" + weekoffs + "')

            var resultsAspNetUsers = Query<dynamic>(
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
                        AND timesheet.is_deleted = 0) REALDATA",
                param: new { org_id, fromDate, toDate }
            );
            return resultsAspNetUsers;
        }

        public dynamic TotalEmpAbsentCountByOrgIDAndDate(string org_id, string fromDate, string toDate)
        {
            int AbsentCount = 0;
            var OffDaysList = ExceptOffDaysDates(org_id, fromDate, toDate);

            for (int i = 0; i < OffDaysList.Count(); i++)
                AbsentCount += GetEmpAbsentCountAttendedByOrgIDAndDate(org_id, OffDaysList[i].ToString(), OffDaysList[i].ToString()).Count();

            return AbsentCount;
        }

        public dynamic GetTimesheetDashboardGridAbsentDataByOrgIDAndDate(string org_id, string fromDate, string toDate)
        {
            List<TimesheetAbsent> AbsentData = new List<TimesheetAbsent>();

            var OffDaysList = ExceptOffDaysDates(org_id, fromDate, toDate);
            for (int i = 0; i < OffDaysList.Count(); i++)
            {
                List<string> EmpList = GetEmpAbsentCountAttendedByOrgIDAndDate(org_id, OffDaysList[i].ToString(), OffDaysList[i].ToString()).ToList();
                var Result = GettingAllAbsentEmployeeList(OffDaysList[i].ToString(), EmpList);
                AbsentData.AddRange(Result);
            }

            return AbsentData;
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

        public dynamic TotalEmpOverTimeCountByOrgIDAndDate(string org_id, string fromDate, string toDate)
        {
            string weekoffs = String.Join("','", ExceptOffDaysDates(org_id, fromDate, toDate));


            //      SELECT
            //                  timesheet_x_project_category.project_category_type,
            //                  timesheet_x_project_category.project_or_comp_name,
            //                  timesheet.id as timesheet_id,
            //                  timesheet.groupid as groupid,
            //                  employee.id as employee_id,
            //                  eTime.lat as lat,
            //                  eTime.lang as lang,
            //                  eTime.is_checkout as is_checkout,
            //                  employee.full_name,
            //                  employee_type.employee_type_name as employee_type_name,
            //                  employee.workemail as workemail,
            //                  employee.emp_code as emp_code,
            //                  employee.phone as phone,
            //                  FORMAT(CAST(timesheet.check_in AS DATETIME2), N'hh:mm tt') as check_in,
            //                  ISNULL(FORMAT(CAST(timesheet.check_out AS DATETIME2), N'hh:mm tt'), 'NA') as check_out,
            //                  ISNULL(total_hrs, 'NA') as total_hrs,
            //                  FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US') as ondate
            //                  FROM timesheet WITH(NOLOCK)
            //                  INNER JOIN employee ON timesheet.empid = employee.id
            //                  LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
            //                  INNER JOIN timesheet_x_project_category on timesheet.groupid = timesheet_x_project_category.groupid
            //                  INNER JOIN superadmin_x_org ON superadmin_x_org.superadmin_empid = employee.id
            //                  INNER JOIN(select distinct(groupid), location.lat, location.lang, location.is_checkout
            //                  FROM dbo.location WHERE groupid IN (SELECT groupid FROM timesheet) and is_checkout = 0) eTime
            //                  ON eTime.groupid = dbo.timesheet.groupid
            //              WHERE
            //                  superadmin_x_org.org_id = @org_id
            //            AND FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US')
            //BETWEEN FORMAT(CAST(@fromDate AS DATE), 'd', 'EN-US')
            //                  AND FORMAT(CAST(@toDate AS DATE), 'd', 'EN-US')
            //                  AND timesheet.is_deleted = 0

            //              UNION

            var resultsAspNetUsers = Query<dynamic>(
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
						WHERE FORMAT(CAST(ondate  AS DATE), 'MM/dd/yyyy', 'EN-US')
                        NOT IN ('" + weekoffs + "')",
                      param: new { org_id, fromDate, toDate }
                  );
            return resultsAspNetUsers;
        }

        public dynamic GetTimesheetActivityByGroupAndDate(string GroupID, string Date)
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

        public dynamic AllProjectRatioByOrgID(string OrgID)
        {
            List<UtilsProjectAndRatio> utilsProjectAndRatios = new List<UtilsProjectAndRatio>();

            var projectList = Query<string>(
               sql: @"SELECT id from project where org_id = @OrgID and is_deleted = 0",
               param: new { OrgID }
           );

            foreach (var item in projectList)
            {
                UtilsProjectAndRatio utilsProjectAndRatio = QuerySingleOrDefault<UtilsProjectAndRatio>(
                        sql: @"SELECT 
                            dbo.project.project_name as project_name,
                             ISNULL(SUM((SELECT count(*)
                            FROM dbo.project_activity WITH(NOLOCK)
                            INNER JOIN dbo.project_status on dbo.project_activity.status_id = dbo.project_status.id
                            INNER JOIN dbo.project on dbo.project_activity.project_id = dbo.project.id
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
                            dbo.project.project_name",
                        param: new { OrgID, item }
            );
                utilsProjectAndRatios.Add(utilsProjectAndRatio);
            }

            return utilsProjectAndRatios;
        }

        public dynamic GetAllTimesheetRecentActivityList(string org_id, string fromDate, string toDate)
        {
            var resultsAspNetUsers = Query<dynamic>(
                sql: @"SELECT DISTINCT( dbo.timesheet.groupid), 
                            FORMAT(CAST( dbo.timesheet.ondate AS DATETIME2), N'hh:mm tt')  
                        FROM dbo.timesheet WITH (NOLOCK)
                            INNER JOIN dbo.employee on dbo.timesheet.empid = dbo.employee.id
                        WHERE dbo.employee.org_id =@org_id
                            AND FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US')
                            BETWEEN FORMAT(CAST(@fromDate AS DATE), 'd', 'EN-US')
                            AND FORMAT(CAST(@toDate AS DATE), 'd', 'EN-US')
                            AND  dbo.timesheet.is_deleted = 0
                        ORDER BY FORMAT(CAST( dbo.timesheet.ondate AS DATETIME2), N'hh:mm tt') DESC;",
                      param: new { org_id, fromDate, toDate }
                  );
            return resultsAspNetUsers;
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

        private IEnumerable<string> GetEmpAbsentCountAttendedByOrgIDAndDate(string OrgID, string fromDate, string toDate)
        {
            return Query<string>(
                  sql: @"WITH TotalEmployeeAttended AS
                            (SELECT
	                                   employee.id as employee

                                        FROM dbo.employee WITH(NOLOCK)
				                            INNER JOIN timesheet ON  employee.id = timesheet.empid
                                        WHERE employee.org_id = @OrgID
			                            AND FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US')
			                         BETWEEN FORMAT(CAST(@fromDate AS DATE), 'd', 'EN-US')
			                            AND FORMAT(CAST(@toDate AS DATE), 'd', 'EN-US')
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
                                        FORMAT(CAST(created_date AS DATE), 'd', 'EN-US') 
                                        <= FORMAT(CAST(@fromDate AS DATE), 'd', 'EN-US')
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

        private string GetWeekOffsFromOrg(string OrgID)
        {
            return QuerySingleOrDefault<string>(
                  sql: @"SELECT start_of_week FROM [dbo].[organization_setup]
                        WHERE org_id= @OrgID AND is_deleted = 0",
                   param: new { OrgID }
               );
        }

        private dynamic GettingTimesheetDashboardDataPerDate(string org_id, string fromDate, string toDate)
        {
            //, List<string> OffDaysList
            //string weekoffs = String.Join("','", OffDaysList);

            //SELECT COUNT(DISTINCT(timesheet.empid)) as attandance,
            //            ISNULL(UPPER(employee_type.employee_type_name), 'PERMANENT') AS employee_type_name,
            //            FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US') as ondate
            //            FROM timesheet
            //            INNER JOIN employee ON timesheet.empid = employee.id
            //            INNER JOIN superadmin_x_org ON superadmin_x_org.superadmin_empid = employee.id
            //            LEFT JOIN employee_type on employee.emp_type_id = employee_type.id
            //            WHERE
            //            superadmin_x_org.org_id = @org_id
            //            AND FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US')
            //   BETWEEN FORMAT(CAST(@fromDate AS DATE), 'd', 'EN-US')
            //            AND FORMAT(CAST(@toDate AS DATE), 'd', 'EN-US')
            //            AND timesheet.is_deleted = 0
            //            GROUP BY FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US'),
            //            employee_type.employee_type_name

            //        UNION ALL
            //WHERE FORMAT(CAST(ondate  AS DATE), 'MM/dd/yyyy', 'EN-US') NOT IN ('" + weekoffs + "')" +

            return Query<dynamic>(
                sql: @"SELECT employee_type_name, SUM(attandance) as attandance, ondate
                FROM (
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
                        employee_type.employee_type_name) X GROUP BY employee_type_name, ondate",
                param: new { org_id, fromDate, toDate }
            );
        }

        private IEnumerable<TimesheetAbsent> GettingAllAbsentEmployeeList(string Date, List<string> List)
        {
            string EmpList = String.Join("','", List);

            return Query<TimesheetAbsent>(
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
							ondate = FORMAT(CAST(@Date AS DATE), 'd', 'EN-US')
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

            string offdays = GetWeekOffsFromOrg(org_id);
            if (offdays == null)
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
            string offdays = GetWeekOffsFromOrg(org_id);
            if (offdays == null)
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

        #endregion PrivateMethods

    }


    public class UtilsProjectAndRatio
    {
        public string project_name { get; set; }
        public string ratio { get; set; }
    }

}