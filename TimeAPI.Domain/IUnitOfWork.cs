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
        ISubscriptionRepository SubscriptionRepository { get; }
        IDesignationRepositiory DesignationRepositiory { get; }
        ITimesheetRepository TimesheetRepository { get; }
        ITaskRepository TaskRepository { get; }

        //commit all after all completes
        void Commit();
    }
}
