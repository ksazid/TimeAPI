using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
        {
        }

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

            //var resultsTimesheetGrpID = Query<string>(
            //    sql: @"SELECT distinct(groupid), FORMAT(CAST(timesheet.ondate AS DATETIME2), N'hh:mm tt')  from timesheet WITH (NOLOCK)
            //            WHERE empid = @empid
            //            AND FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US') = FORMAT(CAST(@Date AS DATE), 'd', 'EN-US')
            //            AND timesheet.is_deleted = 0
            //            ORDER BY FORMAT(CAST(timesheet.ondate AS DATETIME2), N'hh:mm tt') DESC;",
            //    param: new { empid = resultsEmployee.id, Date }
            //);

            List<Organization> orgList = GetOrgAddress(resultsOrganization);

            //var RootTimesheetDataList = GetTimesheetProperty(resultsTimesheetGrpID);

            UserDataGroupDataSet _UserDataGroupDataSet = new UserDataGroupDataSet();

            _UserDataGroupDataSet.User = resultsAspNetUsers;
            _UserDataGroupDataSet.Organization = orgList;
            _UserDataGroupDataSet.Employee = resultsEmployee;
            _UserDataGroupDataSet.Subscription = resultsSubscription;

            //_UserDataGroupDataSet.Timesheet = RootTimesheetDataList;

            return _UserDataGroupDataSet;
        }

        public IEnumerable<RootTimesheetData> GetAllTimesheetByEmpID(string EmpID, string Date)
        {

            var resultsTimesheetGrpID = Query<string>(
                sql: @"SELECT distinct(groupid), FORMAT(CAST(timesheet.ondate AS DATETIME2), N'hh:mm tt')  from timesheet WITH (NOLOCK)
                        WHERE empid = @empid
                        AND FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US') = FORMAT(CAST(@Date AS DATE), 'd', 'EN-US')
                        AND timesheet.is_deleted = 0
                        ORDER BY FORMAT(CAST(timesheet.ondate AS DATETIME2), N'hh:mm tt') DESC;",
                param: new { empid = EmpID, Date }
            );

            List<RootTimesheetData> RootTimesheetDataList = GetTimesheetProperty(resultsTimesheetGrpID);
            return RootTimesheetDataList;
        }

        public IEnumerable<RootTimesheetData> GetEmployeeTasksTimesheetByEmpID(string EmpID, string fromDate, string toDate)
        {

            var resultsTimesheetGrpID = Query<string>(
                sql: @"SELECT distinct(groupid), FORMAT(CAST(timesheet.ondate AS datetime2), N'dd-MMM-yyyy HH:mm:ss', 'EN-US')  from timesheet WITH (NOLOCK)
                        WHERE empid = @empid
                        AND FORMAT(CAST(timesheet.ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
                            BETWEEN FORMAT(CAST(@fromDate AS DATE), 'MM/dd/yyyy', 'EN-US')
                            AND FORMAT(CAST(@toDate AS DATE), 'MM/dd/yyyy', 'EN-US')
                        AND timesheet.is_deleted = 0
						
                        ORDER BY FORMAT(CAST(timesheet.ondate AS datetime2), N'dd-MMM-yyyy HH:mm:ss', 'EN-US') ASC",
                param: new { empid = EmpID, fromDate, toDate }
            );

            List<RootTimesheetData> RootTimesheetDataList = GetTimesheetPropertyForEmployeeActivitesDashboard(resultsTimesheetGrpID);
          
            return RootTimesheetDataList;
        }

        public IEnumerable<RootTimesheetData> GetAllTimesheetByOrgID(string OrgID, string fromDate, string toDate)
        {

            var resultsTimesheetGrpID = Query<string>(
                sql: @"SELECT distinct(groupid), FORMAT(CAST(timesheet.ondate AS DATE), 'dd/MM/yyyy'), FORMAT(CAST(timesheet.ondate AS DATETIME2), N'hh:mm tt')  
                            from timesheet WITH (NOLOCK)
                            INNER JOIN employee on timesheet.empid = employee.id
                            WHERE employee.org_id = @OrgID
                            AND FORMAT(CAST(timesheet.ondate AS DATE), 'MM/dd/yyyy', 'EN-US')
                            BETWEEN FORMAT(CAST(@fromDate AS DATE), 'MM/dd/yyyy', 'EN-US')
                            AND FORMAT(CAST(@toDate AS DATE), 'MM/dd/yyyy', 'EN-US')
                            AND timesheet.is_deleted = 0
                            ORDER BY FORMAT(CAST(timesheet.ondate AS DATE), 'dd/MM/yyyy') DESC;",
                param: new { OrgID, fromDate, toDate }
            );

            List<RootTimesheetData> RootTimesheetDataList = GetTimesheetProperty(resultsTimesheetGrpID);
            return RootTimesheetDataList;
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

        private List<RootTimesheetData> GetTimesheetProperty(IEnumerable<string> resultsTimesheetGrpID)
        {
            List<RootTimesheetData> RootTimesheetDataList = new List<RootTimesheetData>();

            foreach (var item in resultsTimesheetGrpID)
            {
                RootTimesheetData rootTimesheetData = new RootTimesheetData();
                List<string> MembersList = new List<string>();

                //lists
                List<TimesheetDataModel> TimesheetDataModelList = new List<TimesheetDataModel>();
                List<TimesheetTeamDataModel> TimesheetTeamDataModelList = new List<TimesheetTeamDataModel>();
                List<TimesheetCurrentLocationViewModel> TimesheetCurrentLocationViewModelList = new List<TimesheetCurrentLocationViewModel>();
                List<FirstCheckInLastCheckout> FirstCheckInLastCheckoutList = new List<FirstCheckInLastCheckout>();

                TimesheetDataModelList.AddRange(GetTimesheetDataModel(item));

                TimesheetDataModelList.Select(d => d.viewLogDataModels = GetTimesheetActivityByGroupAndProjectID(d.groupid, "", d.ondate).ToList())
                                            .ToList();

                MembersList.AddRange(TimesheetDataModelList.Select(x => x.emp_name));
                TimesheetTeamDataModelList.AddRange(GetTimesheetTeamDataModel(item));
                TimesheetCurrentLocationViewModelList.AddRange(GetTimesheetCurrentLocationViewModel(item));

                #region
                rootTimesheetData.TimesheetDataModels = TimesheetDataModelList;
                rootTimesheetData.TimesheetProjectCategoryDataModel = GetTimesheetProjectCategoryDataModel(item);
                rootTimesheetData.TimesheetTeamDataModels = TimesheetTeamDataModelList;
                rootTimesheetData.Members = MembersList;
                rootTimesheetData.TimesheetSearchLocationViewModel = GetTimesheetSearchLocationViewModel(item);
                rootTimesheetData.TimesheetCurrentLocationViewModels = TimesheetCurrentLocationViewModelList;
                #endregion PrivateMethods

                RootTimesheetDataList.Add(rootTimesheetData);
            }

            return RootTimesheetDataList;
        }

        private List<RootTimesheetData> GetTimesheetPropertyForEmployeeActivitesDashboard(IEnumerable<string> resultsTimesheetGrpID)
        {
            List<RootTimesheetData> RootTimesheetDataList = new List<RootTimesheetData>();

            foreach (var item in resultsTimesheetGrpID)
            {
                RootTimesheetData rootTimesheetData = new RootTimesheetData();
                List<string> MembersList = new List<string>();

                //lists
                List<TimesheetDataModel> TimesheetDataModelList = new List<TimesheetDataModel>();
                List<TimesheetTeamDataModel> TimesheetTeamDataModelList = new List<TimesheetTeamDataModel>();
                List<TimesheetCurrentLocationViewModel> TimesheetCurrentLocationViewModelList = new List<TimesheetCurrentLocationViewModel>();
                
                TimesheetDataModelList.AddRange(GetTimesheetDataModel(item));

                TimesheetDataModelList.Select(d => d.viewLogDataModels = GetTimesheetActivityByGroupAndProjectID(d.groupid, "", d.ondate).ToList()).ToList();
                TimesheetDataModelList.Select(d => d.FirstCheckInLastCheckout = FirstCheckInLastCheckout(d.emp_id, d.ondate, d.ondate)).ToList();

                MembersList.AddRange(TimesheetDataModelList.Select(x => x.emp_name));
                TimesheetTeamDataModelList.AddRange(GetTimesheetTeamDataModel(item));
                TimesheetCurrentLocationViewModelList.AddRange(GetTimesheetCurrentLocationViewModel(item));

                #region PrivateMethods
                rootTimesheetData.TimesheetDataModels = TimesheetDataModelList;
                rootTimesheetData.TimesheetProjectCategoryDataModel = GetTimesheetProjectCategoryDataModel(item);
                rootTimesheetData.TimesheetTeamDataModels = TimesheetTeamDataModelList;
                rootTimesheetData.Members = MembersList;
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

        public dynamic LastCheckinByEmpID(string EmpID, string Date)
        {
            return Query<dynamic>(
                sql: @"SELECT groupid, empid, check_in, check_out, is_checkout, ondate
                            FROM timesheet
                            WHERE groupid in (SELECT TOP 1 groupid 
                            FROM timesheet
                            WHERE empid = @EmpID
                            AND is_deleted = 0 AND is_checkout = 0 
							AND FORMAT(CAST(timesheet.ondate AS DATE), N'MM/dd/yyyy') = FORMAT(CAST(@Date AS DATE), N'MM/dd/yyyy')
                            ORDER BY
                            FORMAT(CAST(timesheet.ondate AS DATE), N'MM/dd/yyyy') desc,
                            CONVERT(CHAR(30),timesheet.check_out,131) asc)",
                      param: new { EmpID, Date }
                  );
        }

        public IEnumerable<ViewLogDataModel> GetTimesheetActivityByGroupAndProjectID(string GroupID, string ProjectID, string Date)
        {
            return Query<ViewLogDataModel>(
                sql: @"SELECT  project_type, project_name, milestone_name, task_name, remarks, total_hrs, is_billable, ondate, start_time, end_time, groupid FROM
					    (
                        SELECT  

								ISNULL(NULLIF(FORMAT(CAST(dbo.timesheet_activity.start_time AS DATETIME2), N'MM/dd/yyyy hh:mm tt'), ' '), 'NA') as start_time,  
		                        ISNULL(NULLIF(FORMAT(CAST(dbo.timesheet_activity.end_time AS DATETIME2), N' MM/dd/yyyy hh:mm tt'), ' '), 'NA')   as end_time,
								UPPER(dbo.timesheet_x_project_category.project_category_type) as project_type,
								ISNULL(dbo.project.project_name, 'Activity') as project_name,
								ISNULL(dbo.project_activity.activity_name, ISNULL(dbo.timesheet_activity.milestone_name, dbo.timesheet_activity.task_name)) as milestone_name,
								ISNULL(dbo.timesheet_activity.task_name, dbo.timesheet_activity.milestone_name) as task_name,
								remarks= dbo.timesheet_activity.remarks,
								timesheet_activity.total_hrs,
		                        timesheet_activity.is_billable,
		                        FORMAT(dbo.timesheet_activity.ondate, 'dd-MM-yyyy', 'en-US') AS ondate ,

								--dbo.timesheet_activity.start_time as start_time,
								--dbo.timesheet_activity.end_time as end_time,
								dbo.timesheet_activity.groupid as groupid

								
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
				                        AND [dbo].[timesheet_activity].is_deleted  = 0
                                        AND FORMAT(CAST(timesheet_activity.ondate AS DATE), 'MM/dd/yyyy', 'EN-US') = FORMAT(CAST(@Date AS DATE),  'MM/dd/yyyy', 'EN-US')
							UNION ALL

						    SELECT   
		          
								ISNULL(NULLIF(FORMAT(CAST(dbo.timesheet_administrative_activity.start_time AS DATETIME2), N'MM/dd/yyyy hh:mm tt'), ' '), 'NA') as start_time,  
		                        ISNULL(NULLIF(FORMAT(CAST(dbo.timesheet_administrative_activity.end_time AS DATETIME2), N'MM/dd/yyyy hh:mm tt'), ' '), 'NA')   as end_time,
								UPPER(dbo.timesheet_x_project_category.project_category_type) as project_type,
								ISNULL([dbo].[administrative].administrative_name, 'NA') as project_name,
								ISNULL( [dbo].[administrative].administrative_name, 'NA') as milestone_name,
								ISNULL(dbo.timesheet_administrative_activity.purpose, 'NA') as task_name,
								remarks= dbo.timesheet_administrative_activity.remarks,
								timesheet_administrative_activity.total_hrs,
		                        timesheet_administrative_activity.is_billable,
		                        FORMAT(dbo.timesheet_administrative_activity.ondate, 'dd-MM-yyyy', 'en-US') AS ondate,
								 
								dbo.timesheet_administrative_activity.groupid as groupid

                        FROM
                            dbo.timesheet_administrative_activity WITH (NOLOCK)
							LEFT JOIN (select top 1 * from dbo.timesheet   where groupid IN (SELECT groupid FROM timesheet_activity 
                            WHERE dbo.timesheet_activity.groupid = @GroupID)) eTime
	                        on eTime.groupid = dbo.timesheet_administrative_activity.groupid
	                        inner JOIN timesheet_x_project_category on timesheet_x_project_category.groupid = dbo.timesheet_administrative_activity.groupid
	                        left JOIN (select top 1 * from dbo.timesheet_location  WHERE groupid IN (SELECT groupid FROM timesheet_activity 
                            WHERE dbo.timesheet_activity.groupid = @GroupID)) eTimeLocation
	                        ON eTimeLocation.groupid = dbo.timesheet_administrative_activity.groupid
	                        INNER JOIN  [dbo].[administrative] on   [dbo].[timesheet_administrative_activity].administrative_id = [dbo].[administrative].id 
                            WHERE dbo.timesheet_administrative_activity.groupid =@GroupID
	                        AND [dbo].timesheet_administrative_activity.is_deleted  = 0
						) xDATA ORDER BY start_time ASC",
                param: new { GroupID, ProjectID, Date }
            );
        }

        public FirstCheckInLastCheckout FirstCheckInLastCheckout(string EmpID, string StartDate, string EndDate)
        {
            return QuerySingleOrDefault<FirstCheckInLastCheckout>(
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
    }
}