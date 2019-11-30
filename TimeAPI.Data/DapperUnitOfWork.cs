using System;
using System.Data;
using System.Reflection;
using TimeAPI.Data.Repositories;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;
using System.Data.SqlClient;


namespace TimeAPI.Data
{
    public class DapperUnitOfWork : IUnitOfWork
    {
        #region Fields
        private IDbConnection _connection;
        private IDbTransaction _transaction;
        private IRoleRepository _roleRepository;
        private IRoleClaimRepository _roleClaimRepository;
        private IUserRepository _userRepository;
        private IUserClaimRepository _userClaimRepository;
        private IUserLoginRepository _userLoginRepository;
        private IRepository<UserToken, UserTokenKey> _userTokenRepository;
        private IUserRoleRepository _userRoleRepository;
        private IEmployeeRepository _employeeRepository;
        private IOrganizationRepository _organizationRepository;
        private IDepartmentRepository _departmentRepository;
        private IProfileImageRepository _profileImageRepository;
        private ISocialRepository _socialRepository;
        private ISubscriptionRepository _subscriptionRepository;
        private IReportingRepository _reportingRepository;
        private IDesignationRepositiory _designationRepositiory;
        private ITimesheetRepository _timesheetRepository;
        private ITaskRepository _taskRepository;

        private bool _disposed;
        #endregion

        public DapperUnitOfWork(string connectionString)
        {
            _connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }

        #region IUnitOfWork Members
        public IRoleRepository RoleRepository
        {
            get
            {
                return _roleRepository
                    ?? (_roleRepository = new RoleRepository(_transaction));
            }
        }

        public IRoleClaimRepository RoleClaimRepository
        {
            get
            {
                return _roleClaimRepository
                    ?? (_roleClaimRepository = new RoleClaimRepository(_transaction));
            }
        }

        public IUserRepository UserRepository
        {
            get
            {
                return _userRepository
                    ?? (_userRepository = new UserRepository(_transaction));
            }
        }

        public IUserClaimRepository UserClaimRepository
        {
            get
            {
                return _userClaimRepository
                    ?? (_userClaimRepository = new UserClaimRepository(_transaction));
            }
        }

        public IUserLoginRepository UserLoginRepository
        {
            get
            {
                return _userLoginRepository
                    ?? (_userLoginRepository = new UserLoginRepository(_transaction));
            }
        }

        public IRepository<UserToken, UserTokenKey> UserTokenRepository
        {
            get
            {
                return _userTokenRepository
                    ?? (_userTokenRepository = new UserTokenRepository(_transaction));
            }
        }

        public IUserRoleRepository UserRoleRepository
        {
            get
            {
                return _userRoleRepository
                    ?? (_userRoleRepository = new UserRoleRepository(_transaction));
            }
        }

        public IEmployeeRepository EmployeeRepository
        {
            get
            {
                return _employeeRepository
                    ?? (_employeeRepository = new EmployeeRepository(_transaction));
            }
        }

        public IOrganizationRepository OrganizationRepository
        {
            get
            {
                return _organizationRepository
                    ?? (_organizationRepository = new OrganizationRepository(_transaction));
            }
        }

        public IDepartmentRepository DepartmentRepository
        {
            get
            {
                return _departmentRepository
                    ?? (_departmentRepository = new DepartmentRepository(_transaction));
            }
        }

        public IProfileImageRepository ProfileImageRepository
        {
            get
            {
                return _profileImageRepository
                    ?? (_profileImageRepository = new ProfileImageRepository(_transaction));
            }
        }

        public ISocialRepository SocialRepository
        {
            get
            {
                return _socialRepository
                    ?? (_socialRepository = new SocialRepository(_transaction));
            }
        }

        public ISubscriptionRepository SubscriptionRepository
        {
            get
            {
                return _subscriptionRepository
                    ?? (_subscriptionRepository = new SubscriptionRepository(_transaction));
            }
        }

        public IReportingRepository ReportingRepository
        {
            get
            {
                return _reportingRepository
                    ?? (_reportingRepository = new ReportingRepository(_transaction));
            }
        }

        public IDesignationRepositiory DesignationRepositiory
        {
            get
            {
                return _designationRepositiory
                    ?? (_designationRepositiory = new DesignationRepository(_transaction));
            }
        }

        public ITimesheetRepository TimesheetRepository
        {
            get
            {
                return _timesheetRepository
                    ?? (_timesheetRepository = new TimesheetRepository(_transaction));
            }
        }

        public ITaskRepository TaskRepository
        {
            get
            {
                return _taskRepository
                    ?? (_taskRepository = new TaskRepository(_transaction));
            }
        }

        public void Commit()
        {
            try
            {
                _transaction.Commit();
            }
            catch
            {
                _transaction.Rollback();
            }
            finally
            {
                _transaction.Dispose();
                resetRepositories();
                _transaction = _connection.BeginTransaction();
            }
        }

        public void Dispose()
        {
            dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Private Methods
        private void resetRepositories()
        {
            _roleRepository = null;
            _roleClaimRepository = null;
            _userRepository = null;
            _userClaimRepository = null;
            _userLoginRepository = null;
            _userTokenRepository = null;
            _userRoleRepository = null;
            _employeeRepository = null;
            _organizationRepository = null;
            _departmentRepository = null;
            _profileImageRepository = null;
            _socialRepository = null;
            _subscriptionRepository = null;
            _reportingRepository = null;
            _designationRepositiory = null;
            _timesheetRepository = null;
        }

        private void dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_transaction != null)
                    {
                        _transaction.Dispose();
                        _transaction = null;
                    }
                    if (_connection != null)
                    {
                        _connection.Dispose();
                        _connection = null;
                    }
                }
                _disposed = true;
            }
        }

        ~DapperUnitOfWork()
        {
            dispose(false);
        }
        #endregion

    }

}

