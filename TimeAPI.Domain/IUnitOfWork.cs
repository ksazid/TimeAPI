using System;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Domain
{
    public interface IUnitOfWork : IDisposable
    {
        IRoleRepository RoleRepository { get; }
        IRoleClaimRepository RoleClaimRepository { get; }
        IUserRepository UserRepository { get; }
        IUserClaimRepository UserClaimRepository { get; }
        IUserLoginRepository UserLoginRepository { get; }
        IRepository<UserToken, UserTokenKey> UserTokenRepository { get; }
        IUserRoleRepository UserRoleRepository { get; }
        IEmployeeRepository EmployeeRepository { get; }
        IOrganizationRepository OrganizationRepository { get; }
        IDepartmentRepository DepartmentRepository { get; }
        IProfileImageRepository ProfileImageRepository { get; }
        IReportingRepository ReportingRepository { get; }
        ISocialRepository SocialRepository { get; }
        IAdministrativeRepository AdministrativeRepository { get; }
        IDesignationRepositiory DesignationRepositiory { get; }
        ITimesheetRepository TimesheetRepository { get; }
        ITaskRepository TaskRepository { get; }
        ITaskTeamMembersRepository TaskTeamMembersRepository { get; }
        ISetupRepository SetupRepository { get; }
        IPriorityRepository PriorityRepository { get; }
        IStatusRepository StatusRepository { get; }
        IEmployeeTypeRepository EmployeeTypeRepository { get; }
        IEmployeeStatusRepository EmployeeStatusRepository { get; }
        IIndustryTypeRepository IndustryTypeRepository { get; }
        IEmployeeRoleRepository EmployeeRoleRepository { get; }
        ITeamRepository TeamRepository { get; }
        ITeamMemberRepository TeamMemberRepository { get; }
        ITimesheetProjectCategoryRepository TimesheetProjectCategoryRepository { get; }
        ITimesheetAdministrativeRepository TimesheetAdministrativeRepository { get; }
        ITimesheetTeamRepository TimesheetTeamRepository { get; }
        ITimesheetActivityRepository TimesheetActivityRepository { get; }
        ITimesheetActivityCommentRepository TimesheetActivityCommentRepository { get; }
        ITimesheetActivityFileRepository TimesheetActivityFileRepository { get; }
        ITimesheetLocationRepository TimesheetLocationRepository { get; }
        ILocationRepository LocationRepository { get; }
        IEntityLocationRepository EntityLocationRepository { get; }
        IOrganizationBranchRepository OrganizationBranchRepository { get; }
        IProjectRepository ProjectRepository { get; }
        IProjectStatusRepository ProjectStatusRepository { get; }
        IEntityContactRepository EntityContactRepository { get; }


        #region systemadmin
        IPlanRepository PlanRepository { get; }
        IPlanFeatureRepository PlanFeatureRepository { get; }
        IPlanPriceRepository PlanPriceRepository { get; }
        ISubscriptionRepository SubscriptionRepository { get; }
        IBillingRepository BillingRepository { get; }

        #endregion systemadmin

        //commit all after all completes
        bool Commit();
    }
}
