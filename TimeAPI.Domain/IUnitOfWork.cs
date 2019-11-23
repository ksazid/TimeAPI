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

        //commit all after all completes
        void Commit();
    }
}
