using System.Collections.Generic;
using System.Data;
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

        public UserDataGroupDataSet GetUserDataGroupByUserID(string UserID)
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

            var resultsTimesheetGrpID = Query<string>(
                sql: @"SELECT distinct(groupid) from timesheet WITH (NOLOCK)
                        WHERE empid = @empid
                        AND FORMAT(cast(timesheet.ondate as date), 'd', 'en-us') = FORMAT(getdate(), 'd', 'en-us') 
                        AND timesheet.is_deleted = 0;",
                param: new { empid = resultsEmployee.id }
            );

            var RootTimesheetDataList = GetTimesheetProperty(resultsTimesheetGrpID);

            UserDataGroupDataSet _UserDataGroupDataSet = new UserDataGroupDataSet();

            _UserDataGroupDataSet.User = resultsAspNetUsers;
            _UserDataGroupDataSet.Organization = resultsOrganization;
            _UserDataGroupDataSet.Employee = resultsEmployee;
            _UserDataGroupDataSet.Timesheet = RootTimesheetDataList;

            return _UserDataGroupDataSet;
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
                #endregion

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

        private IEnumerable<TimesheetAdministrativeDataModel> GetTimesheetAdministrativeDataModel(string GroupID)
        {
            var TimesheetAdministrativeDataModel = Query<TimesheetAdministrativeDataModel>(
                sql: @"SELECT 
                            timesheet_x_administrative.id, timesheet_x_administrative.administrative_id, administrative.administrative_name
                        FROM timesheet_x_administrative WITH (NOLOCK)
                        LEFT JOIN administrative on timesheet_x_administrative.administrative_id = administrative.id
                        WHERE timesheet_x_administrative.groupid = @GroupID;",
                param: new { GroupID }
            );

            return TimesheetAdministrativeDataModel;
        }

        private TimesheetProjectCategoryDataModel GetTimesheetProjectCategoryDataModel(string GroupID)
        {
            var TimesheetAdministrativeDataModel = QuerySingleOrDefault<TimesheetProjectCategoryDataModel>(
                sql: @"SELECT 
                        timesheet_x_project_category.id, timesheet_x_project_category.groupid,  
                        timesheet_x_project_category.project_category_id as project_name,
                        timesheet_x_project_category.project_or_comp_id as system_name 
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
                sql: @"SELECT id, groupid, manual_address, formatted_address, lat, lang, street_number, route, locality, 
                       administrative_area_level_2, administrative_area_level_1, postal_code, country, is_office, is_manual
                    FROM timesheet_location  WITH (NOLOCK) where groupid  = @GroupID;",
                param: new { GroupID }
            );

            return TimesheetSearchLocationViewModel;
        }

        private IEnumerable<TimesheetCurrentLocationViewModel>  GetTimesheetCurrentLocationViewModel(string GroupID)
        {
            var TimesheetCurrentLocationViewModel = Query<TimesheetCurrentLocationViewModel>(
               sql: @"SELECT id, groupid, formatted_address, lat, lang, street_number, route, locality, 
                       administrative_area_level_2, administrative_area_level_1, postal_code, country, is_checkout
                FROM location  WITH (NOLOCK) where groupid  = @GroupID;",
                param: new { GroupID }
            );

            return TimesheetCurrentLocationViewModel;
        }

        #endregion PrivateMethods
    }
}
