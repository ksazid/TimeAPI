using System.Collections.Generic;
using System.Data;
using System.Linq;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    internal class UserRepository : RepositoryBase, IUserRepository
    {
        public UserRepository(IDbTransaction transaction)
            : base(transaction)
        { }

        public void Add(User entity)
        {
            Execute(
                sql: @"
                    INSERT INTO AspNetUsers(Id, AccessFailedCount, ConcurrencyStamp, Email,
	                    EmailConfirmed, LockoutEnabled, LockoutEnd, NormalizedEmail,
	                    NormalizedUserName, PasswordHash, PhoneNumber, PhoneNumberConfirmed,
	                    SecurityStamp, TwoFactorEnabled, UserName)
                    VALUES(@Id, @AccessFailedCount, @ConcurrencyStamp, @Email, @EmailConfirmed,
	                    @LockoutEnabled, @LockoutEnd, @NormalizedEmail, @NormalizedUserName,
	                    @PasswordHash, @PhoneNumber, @PhoneNumberConfirmed, @SecurityStamp,
	                    @TwoFactorEnabled, @UserName)",
                param: entity
            );
        }

        public IEnumerable<User> All()
        {
            return Query<User>(
                sql: "SELECT * FROM AspNetUsers"
            );
        }

        public User Find(string key)
        {
            return QuerySingleOrDefault<User>(
                sql: "SELECT * FROM AspNetUsers WHERE Id = @key",
                param: new { key }
            );
        }

        public User FindByNormalizedEmail(string normalizedEmail)
        {
            return QuerySingleOrDefault<User>(
                sql: "SELECT * FROM AspNetUsers WHERE NormalizedEmail = @normalizedEmail",
                param: new { normalizedEmail }
            );
        }

        public User FindByNormalizedUserName(string normalizedUserName)
        {
            return QuerySingleOrDefault<User>(
                sql: "SELECT * FROM AspNetUsers WHERE NormalizedUserName = @normalizedUserName",
                param: new { normalizedUserName }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: "DELETE FROM AspNetUsers WHERE Id = @key",
                param: new { key }
            );
        }

        public void Update(User entity)
        {
            Execute(
                sql: @"
                    UPDATE AspNetUsers SET AccessFailedCount = @AccessFailedCount,
	                    ConcurrencyStamp = @ConcurrencyStamp, Email = @Email,
	                    EmailConfirmed = @EmailConfirmed, LockoutEnabled = @LockoutEnabled,
	                    LockoutEnd = @LockoutEnd, NormalizedEmail = @NormalizedEmail,
	                    NormalizedUserName = @NormalizedUserName, PasswordHash = @PasswordHash,
	                    PhoneNumber = @PhoneNumber, PhoneNumberConfirmed = @PhoneNumberConfirmed,
	                    SecurityStamp = @SecurityStamp, TwoFactorEnabled = @TwoFactorEnabled,
	                    UserName = @UserName
                    WHERE Id = @Id",
                param: entity);
        }

        //public void CustomEmailConfirmedFlagUpdate(string UserID)
        //{
        //    Execute(
        //        sql: @"
        //            UPDATE AspNetUsers SET
        //                EmailConfirmed = 1
        //            WHERE Id = @UserID",
        //      param: new { UserID }

        //        );
        //}

        public UserDataGroupDataSet GetUserDataGroupByUserID(string UserID, string Date)
        {
            var resultsAspNetUsers = QuerySingleOrDefault<User>(
                sql: @"SELECT * from AspNetUsers WITH (NOLOCK) WHERE id = @UserID;",
                param: new { UserID }
            );

            var resultsOrganization = Query<Organization>(
                sql: @"SELECT * from organization WITH (NOLOCK) WHERE user_id = @UserID and is_deleted = 0;",
                param: new { UserID }
            );

            var resultsEmployee = QuerySingleOrDefault<Employee>(
                sql: @"SELECT * from employee WITH (NOLOCK) WHERE user_id = @UserID and is_deleted = 0;",
                param: new { UserID }
            );

            var resultsSubscription = QuerySingleOrDefault<Subscription>(
                   sql: @"SELECT * from subscription WITH (NOLOCK) WHERE user_id = @UserID and is_deleted = 0;",
                   param: new { UserID }
            );

            var resultsTimesheetGrpID = Query<string>(
                sql: @"SELECT distinct(groupid), FORMAT(CAST(timesheet.ondate AS DATETIME2), N'hh:mm tt')  from timesheet WITH (NOLOCK)
                        WHERE empid = @empid
                        AND FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US') = FORMAT(CAST(@Date AS DATE), 'd', 'EN-US')
                        AND timesheet.is_deleted = 0
                        ORDER BY FORMAT(CAST(timesheet.ondate AS DATETIME2), N'hh:mm tt') DESC;",
                param: new { empid = resultsEmployee.id, Date }
            );

            List<Organization> orgList = GetOrgAddress(resultsOrganization);

            var RootTimesheetDataList = GetTimesheetProperty(resultsTimesheetGrpID);

            UserDataGroupDataSet _UserDataGroupDataSet = new UserDataGroupDataSet();

            _UserDataGroupDataSet.User = resultsAspNetUsers;
            _UserDataGroupDataSet.Organization = orgList;
            _UserDataGroupDataSet.Employee = resultsEmployee;
            _UserDataGroupDataSet.Subscription = resultsSubscription;
            _UserDataGroupDataSet.Timesheet = RootTimesheetDataList;

            return _UserDataGroupDataSet;
        }

        private List<Organization> GetOrgAddress(IEnumerable<Organization> resultsOrganization)
        {
            List<Organization> orgList = (resultsOrganization as List<Organization>);
            for (int i = 0; i < orgList.Count; i++)
            {
                var entityLocation = QuerySingleOrDefault<EntityLocation>(
                   sql: @"SELECT * from entity_location WITH (NOLOCK) WHERE entity_id = @item and is_deleted = 0;",
                   param: new { item = orgList[i].org_id }
                  );

                orgList[i].EntityLocation = entityLocation;
            }

            return orgList;
        }

        public dynamic TotalEmployeeDashboardDataByOrgID(string org_id)
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

        public dynamic TotalEmployeeAbsentDashboardDataByOrgID(string org_id, string fromDate, string toDate)
        {
            List<TimesheetAbsent> TotalEmpCount = new List<TimesheetAbsent>();

            var GetDates =  GetDateFromTimesheet(org_id, fromDate, toDate).ToList();
            var TotalEmp =  GetTotalEmpCountByOrgID(org_id);

            for (int i = 0; i < GetDates.Count(); i++)
            {
                var EmpCountAttended = GetEmpCountAttendedByOrgIDAndDate(org_id, GetDates[i], GetDates[i]);
                var result = TotalEmp.Except(EmpCountAttended);
                TotalEmpCount.AddRange(result);
            }

            return TotalEmpCount.Count();
        }

        public dynamic GetTimesheetDashboardDataByOrgIDAndDate(string org_id, string fromDate, string toDate)
        {
            var resultsAspNetUsers = Query<dynamic>(
                sql: @"SELECT
                    employee_type_name, SUM(attandance) as attandance
                FROM (
                    SELECT COUNT (DISTINCT(timesheet.empid)) as attandance,
                        ISNULL(UPPER(employee_type.employee_type_name), 'PERMANENT') AS employee_type_name
                        FROM timesheet
                        INNER JOIN employee ON timesheet.empid = employee.id
                        INNER JOIN superadmin_x_org ON superadmin_x_org.superadmin_empid = employee.id
                        LEFT JOIN employee_type on employee.emp_type_id = employee_type.id
                        WHERE
                        superadmin_x_org.org_id = @org_id
                        AND FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US')
                        BETWEEN FORMAT(CAST(@fromDate   AS DATE), 'd', 'EN-US')
                        AND FORMAT(CAST(@toDate AS DATE), 'd', 'EN-US')
                        AND timesheet.is_deleted = 0
                        GROUP BY FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US'),   employee_type.employee_type_name

                    UNION ALL

                    SELECT COUNT(DISTINCT(timesheet.empid)) as attandance,
                        ISNULL(UPPER(employee_type.employee_type_name), 'PERMANENT') AS employee_type_name
                        FROM timesheet
                        INNER JOIN employee on timesheet.empid = employee.id
                        LEFT JOIN employee_type on employee.emp_type_id = employee_type.id
                        WHERE
                        employee.org_id = @org_id
                        AND FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US') between FORMAT(CAST(@fromDate   AS DATE), 'd', 'EN-US')
                        AND FORMAT(CAST(@toDate AS DATE), 'd', 'EN-US')
                        AND timesheet.is_deleted = 0
                        GROUP BY FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US'),  employee_type.employee_type_name) X  GROUP BY employee_type_name",
                param: new { org_id, fromDate, toDate }
            );
            return resultsAspNetUsers;
        }

        public dynamic GetTimesheetDashboardGridDataByOrgIDAndDate(string org_id, string fromDate, string toDate)
        {
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
                        ISNULL(FORMAT(CAST(timesheet.total_hrs AS DATETIME2), N'hh:mm tt'), 'NA') as total_hrs,
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
                        AND FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US')
                        BETWEEN FORMAT(CAST(@fromDate AS DATE), 'd', 'EN-US')
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
                        ISNULL(FORMAT(CAST(timesheet.total_hrs AS DATETIME2), N'hh:mm tt'), 'NA') as total_hrs,
                        FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US')  as ondate
                        FROM timesheet WITH (NOLOCK)
                        INNER JOIN employee ON timesheet.empid = employee.id
                        LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
                        INNER JOIN timesheet_x_project_category on timesheet.groupid = timesheet_x_project_category.groupid
                        INNER JOIN (select distinct(groupid), location.lat, location.lang, location.is_checkout
                        from dbo.location  where groupid IN (SELECT groupid FROM timesheet) and is_checkout = 0) eTime
                        ON eTime.groupid = dbo.timesheet.groupid
                    WHERE
                        employee.org_id = @org_id
                        AND FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US')
                        BETWEEN FORMAT(CAST(@fromDate AS DATE), 'd', 'EN-US')
                        AND FORMAT(CAST(@toDate AS DATE), 'd', 'EN-US')
                        AND timesheet.is_deleted = 0) DATA",
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
            //    var resultsAspNetUsers = Query<dynamic>(
            //        sql: @"SELECT
            //                 employee.id,
            //                 employee.full_name,
            //                 employee.workemail,
            //                 employee.emp_code,
            //                 employee.phone,
            //                 employee_status.employee_status_name,
            //                 employee_type.employee_type_name,
            //                 AspNetRoles.Name as role_name,
            //                 department.dep_name,
            //                    department.id as department_id,
            //                 designation.designation_name
            //                  FROM dbo.employee WITH(NOLOCK)
            //                   LEFT JOIN employee_status ON employee.emp_status_id = employee_status.id
            //                   LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
            //                   LEFT JOIN AspNetRoles ON employee.role_id = AspNetRoles.id
            //                   LEFT JOIN department ON employee.deptid = department.id
            //                   LEFT JOIN designation ON employee.designation_id = designation.id
            //                  WHERE employee.org_id = @org_id
            //AND employee.is_deleted = 0

            //                EXCEPT

            //                SELECT
            //                 employee.id,
            //                 employee.full_name,
            //                 employee.workemail,
            //                 employee.emp_code,
            //                 employee.phone,
            //                 employee_status.employee_status_name,
            //                 employee_type.employee_type_name,
            //                 AspNetRoles.Name as role_name,
            //                 department.dep_name,
            //                    department.id as department_id,
            //                 designation.designation_name
            //                  FROM dbo.employee WITH(NOLOCK)
            // INNER JOIN timesheet ON  employee.id = timesheet.empid
            //                   LEFT JOIN employee_status ON employee.emp_status_id = employee_status.id
            //                   LEFT JOIN employee_type ON employee.emp_type_id = employee_type.id
            //                   LEFT JOIN AspNetRoles ON employee.role_id = AspNetRoles.id
            //                   LEFT JOIN department ON employee.deptid = department.id
            //                   LEFT JOIN designation ON employee.designation_id = designation.id
            //                  WHERE employee.org_id = @org_id
            //AND FORMAT(CAST(timesheet.ondate  AS DATE), 'd', 'EN-US') BETWEEN FORMAT(CAST(@fromDate AS DATE), 'd', 'EN-US')
            //AND FORMAT(CAST(@toDate AS DATE), 'd', 'EN-US')
            //AND employee.is_deleted = 0
            //                  AND employee.is_superadmin = 0
            //                  AND timesheet.is_deleted = 0
            //                  ORDER BY employee.full_name ASC",
            //        param: new { org_id, fromDate, toDate }
            //    );
            //    return resultsAspNetUsers;


            List<TimesheetAbsent> TotalEmpCount = new List<TimesheetAbsent>();

            var GetDates = GetDateFromTimesheet(org_id, fromDate, toDate).ToList();
            var TotalEmp = GetTotalEmpCountByOrgID(org_id);

            for (int i = 0; i < GetDates.Count(); i++)
            {
                var EmpCountAttended = GetEmpCountAttendedByOrgIDAndDate(org_id, GetDates[i], GetDates[i]);
                var result = TotalEmp.Except(EmpCountAttended);
                TotalEmpCount.AddRange(result);
            }

            return TotalEmpCount;
        }

        #region PrivateMethods

        private List<RootTimesheetData> GetTimesheetProperty(IEnumerable<string> resultsTimesheetGrpID)
        {
            List<RootTimesheetData> RootTimesheetDataList = new List<RootTimesheetData>();

            foreach (var item in resultsTimesheetGrpID)
            {
                RootTimesheetData rootTimesheetData = new RootTimesheetData();

                //lists
                List<TimesheetDataModel> TimesheetDataModelList = new List<TimesheetDataModel>();
                List<TimesheetTeamDataModel> TimesheetTeamDataModelList = new List<TimesheetTeamDataModel>();
                List<TimesheetCurrentLocationViewModel> TimesheetCurrentLocationViewModelList = new List<TimesheetCurrentLocationViewModel>();

                TimesheetDataModelList.AddRange(GetTimesheetDataModel(item));
                TimesheetTeamDataModelList.AddRange(GetTimesheetTeamDataModel(item));
                TimesheetCurrentLocationViewModelList.AddRange(GetTimesheetCurrentLocationViewModel(item));

                #region
                rootTimesheetData.TimesheetDataModels = TimesheetDataModelList;
                rootTimesheetData.TimesheetProjectCategoryDataModel = GetTimesheetProjectCategoryDataModel(item);
                rootTimesheetData.TimesheetTeamDataModels = TimesheetTeamDataModelList;
                rootTimesheetData.TimesheetSearchLocationViewModel = GetTimesheetSearchLocationViewModel(item);
                rootTimesheetData.TimesheetCurrentLocationViewModels = TimesheetCurrentLocationViewModelList;
                #endregion PrivateMethods

                RootTimesheetDataList.Add(rootTimesheetData);
            }

            return RootTimesheetDataList;
        }

        private IEnumerable<TimesheetDataModel> GetTimesheetDataModel(string GroupID)
        {
            var TimesheetDataModel = Query<TimesheetDataModel>(
                sql: @"SELECT
                            timesheet.id, employee.id as emp_id, employee.full_name as emp_name, timesheet.groupid,
			                timesheet.ondate, timesheet.check_in, timesheet.check_out,
                            timesheet.is_checkout, timesheet.total_hrs, timesheet.created_date,
                            timesheet.createdby, timesheet.modified_date, timesheet.modifiedby, timesheet.is_deleted
                FROM timesheet WITH (NOLOCK)
                    LEFT JOIN employee on timesheet.empid = Employee.id
                    WHERE timesheet.groupid = @GroupID
                    ORDER BY timesheet.ondate asc;",
                param: new { GroupID }
            );

            return TimesheetDataModel;
        }

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

        private TimesheetProjectCategoryDataModel GetTimesheetProjectCategoryDataModel(string GroupID)
        {
            var TimesheetAdministrativeDataModel = QuerySingleOrDefault<TimesheetProjectCategoryDataModel>(
                sql: @"SELECT
                        timesheet_x_project_category.id as category_id,
                        timesheet_x_project_category.groupid as groupid,
                        timesheet_x_project_category.project_category_type as project_type,
                        timesheet_x_project_category.project_or_comp_id as project_or_comp_id,
                        timesheet_x_project_category.project_or_comp_name as project_or_comp_name,
                        timesheet_x_project_category.project_or_comp_type as project_or_comp_type
                        FROM timesheet_x_project_category WITH (NOLOCK)
                    WHERE timesheet_x_project_category.groupid = @GroupID;",
                param: new { GroupID }
            );

            return TimesheetAdministrativeDataModel;
        }

        private IEnumerable<TimesheetTeamDataModel> GetTimesheetTeamDataModel(string GroupID)
        {
            var TimesheetTeamDataModel = Query<TimesheetTeamDataModel>(
                sql: @"SELECT
                        timesheet_x_team.id, timesheet_x_team.teamid, team.team_name
                    FROM timesheet_x_team   WITH (NOLOCK)
                    LEFT JOIN team on timesheet_x_team.teamid = team.id
                    WHERE timesheet_x_team.groupid = @GroupID;",
                param: new { GroupID }
            );

            return TimesheetTeamDataModel;
        }

        private TimesheetSearchLocationViewModel GetTimesheetSearchLocationViewModel(string GroupID)
        {
            var TimesheetSearchLocationViewModel = QuerySingleOrDefault<TimesheetSearchLocationViewModel>(
                sql: @"SELECT id, groupid, manual_address, geo_address, formatted_address, lat, lang, street_number, route, locality,
                       administrative_area_level_2, administrative_area_level_1, postal_code, country, is_office, is_manual
                    FROM timesheet_location  WITH (NOLOCK) where groupid  = @GroupID;",
                param: new { GroupID }
            );

            return TimesheetSearchLocationViewModel;
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
	                        designation.designation_name,
                            FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US')  as ondate
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

        private IEnumerable<string> GetDateFromTimesheet(string OrgID, string fromDate, string toDate)
        {
            return Query<string>(
                  sql: @"SELECT
	                         FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US') 
                          FROM dbo.employee WITH(NOLOCK)
							  INNER JOIN timesheet ON  employee.id = timesheet.empid
	                      WHERE employee.org_id = @OrgID
						  AND FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US') 
						  BETWEEN FORMAT(CAST(@fromDate AS DATE), 'd', 'EN-US')
						  AND FORMAT(CAST(@toDate AS DATE), 'd', 'EN-US')
						  AND employee.is_deleted = 0
                          AND timesheet.is_deleted = 0
						  group by FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US') ",
                   param: new { OrgID, fromDate, toDate }
               );
        }

        #endregion PrivateMethods
    }
}