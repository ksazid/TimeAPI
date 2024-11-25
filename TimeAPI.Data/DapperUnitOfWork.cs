﻿using System;
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
        private IOrganizationSetupRepository _organizationSetupRepository;
        private IDepartmentRepository _departmentRepository;
        private IProfileImageRepository _profileImageRepository;
        private ISocialRepository _socialRepository;
        private IReportingRepository _reportingRepository;
        private IDesignationRepositiory _designationRepositiory;
        private ITimesheetRepository _timesheetRepository;
        private ITimesheetBreakRepository _timesheetBreakRepository;
        private ITaskRepository _taskRepository;
        private ISubTaskRepository _subTaskRepository;
        private ITaskTeamMembersRepository _taskTeamMembersRepository;
        private ISetupRepository _setupRepository;
        private IAdministrativeRepository _administrativeRepository;
        private IPriorityRepository _priorityRepository;
        private IStatusRepository _statusRepository;
        private IEmployeeTypeRepository _employeeTypeRepository;
        private IEmployeeStatusRepository _employeeStatusRepository;
        private IIndustryTypeRepository _industryTypeRepository;
        private IEmployeeRoleRepository _employeeRoleRepository;
        private ITeamRepository _teamRepository;
        private ITeamMemberRepository _teamMemberRepository;
        private ITimesheetProjectCategoryRepository _timesheetProjectCategoryRepository;
        private ITimesheetAdministrativeRepository _timesheetAdministrativeRepository;
        private ITimesheetTeamRepository _timesheetTeamRepository;
        private ITimesheetActivityRepository _timesheetActivityRepository;
        private ITimesheetActivityCommentRepository _timesheetActivityCommentRepository;
        private ITimesheetActivityFileRepository _timesheetActivityFileRepository;
        private ITimesheetLocationRepository _timesheetLocationRepository;
        private ILocationRepository _locationRepository;
        private IEntityLocationRepository _entityLocationRepository;
        private IOrganizationBranchRepository _organizationBranchRepository;
        private IProjectRepository _projectRepository;
        private IProjectStatusRepository _projectStatusRepository;
        private IEntityContactRepository _entityContactRepository;
        private IProjectActivityRepository _projectActivityRepository;
        private IProjectActivityTaskRepository _projectActivityTaskRepository;
        private ICustomerRepository _customerRepository;
        private ICustomerProjectRepository _customerProjectRepository;
        private IDelegationsRepository _delegationsRepository;
        private IDelegationsDelegateeRepository _delegationsDelegateeRepository;
        private ISuperadminOrganizationRepository _superadminOrganizationRepository;
        private IAdminDashboardRepository _adminDashboardRepository;
        private IProductivityDashboardRepository _productivityDashboardRepository;
        private IAdminProductivityDashboardRepository _adminProductivityDashboardRepository;
        private ILocationExceptionRepository _locationExceptionRepository;
        private IEntityInvitationRepository _entityInvitationRepository;
        private IWeekdaysRepository _weekdaysRepository;
        private IDualApprovalRepository _orgWeekdaysRepository;
        private IProjectTypeRepository _projectTypeRepository;
        private IMilestoneTemplateRepository _milestoneTemplateRepository;
        private ITaskTemplateRepository _taskTemplateRepository;
        private ICostProjectRepository _costProjectRepository;
        private ICostProjectMilestoneRepository _costProjectMilestoneRepository;
        private ICostProjectTaskRepository _costProjectTaskRepository;

        private ITypeOfDesignRepository _typeOfDesignRepository;
        private ISpecifiationRepository _specifiationRepository;
        private IUnitDescriptionRepository _unitDescriptionRepository;
        private ITypeOfUnitRepository _typeOfUnitRepository;

        private IProjectTagsRepository _projectTagsRepository;
        private IProjectUnitRepository _projectUnitRepository;
        private IProjectDesignTypeRepository _projectDesignTypeRepository;
        private IPackagesRepository _packagesRepository;
        private ICostPerHourRepository _costPerHourRepository;
        private IProfitMarginRepository _profitMarginRepository;

        private ILeaveSetupRepository _leaveSetupRepository;
        private ILeaveTypeRepository _leaveTypeRepository;
        private ITimeoffTypeRepository _timeoffTypeRepository;
        private IEmployeeLeaveRepository _employeeLeaveRepository;
        private ILeaveStatusRepository _leaveStatusRepository;
        private IEmployeeScreenshotRepository _employeeScreenshotRepository;
        private IEmployeeAppUsageRepository _employeeAppUsageRepository;
        private IEmployeeAppTrackedRepository _employeeAppTrackedRepository;
        private IEmployeeLeaveLogRepository _employeeLeaveLogRepository;
        private ILeadCompanyRepository _leadCompanyRepository;
        private ILeadRepository _leadRepository;
        private ILeadDealRepository _LeadDealRepository;
        private ILeadSourceRepository _leadSourceRepository;
        private ILeadStatusRepository _leadStatusRepository;
        private ILeadRatingRepository _leadRatingRepository;
        private ITimesheetDeskRepository _timesheetDeskRepository;

        private ILeadDealTypeRepository _leadDealTypeRepository;
        private ILeadStageRepository _leadStageRepository;
        private ILeadContractRoleRepository _leadContractRoleRepository;
        private IPrefixRepository _prefixRepository;

        private IQuotationRepository _quotationRepository;
        private IPaymentModeRepository _paymentModeRepository;
        private IPaymentRepository _paymentRepository;
        private IWarrantyRepository _warrantyRepository;
        private IExclusionRepository _exclusionRepository;
        private IEntityMeetingRepository _entityMeetingRepository;
        private IEntityNotesRepository _entityNotesRepository;
        private IEntityMeetingParticipantsRepository _entityMeetingParticipantsRepository;
        private ILocalActivityRepository _localActivityRepository;
        private IEntityCallRepository _entityCallRepository;
        private IEntityHistoryLogRepository _entityHistoryLogRepository;
        private IWorkforceRepository _workforceRepository;


        #region systemadmin
        private IPlanRepository _planRepository;
        private IPlanFeatureRepository _planFeatureRepository;
        private IPlanPriceRepository _planPriceRepository;
        private ISubscriptionRepository _subscriptionRepository;
        private IBillingRepository _billingRepository;
        #endregion systemadmin

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

        public IOrganizationSetupRepository OrganizationSetupRepository
        {
            get
            {
                return _organizationSetupRepository
                    ?? (_organizationSetupRepository = new OrganizationSetupRepository(_transaction));
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

        public IAdministrativeRepository AdministrativeRepository
        {
            get
            {
                return _administrativeRepository
                    ?? (_administrativeRepository = new AdministrativeRepository(_transaction));
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

        public ISubTaskRepository SubTaskRepository
        {
            get
            {
                return _subTaskRepository
                    ?? (_subTaskRepository = new SubTaskRepository(_transaction));
            }
        }

        public ISetupRepository SetupRepository
        {
            get
            {
                return _setupRepository
                    ?? (_setupRepository = new SetupRepository(_transaction));
            }
        }

        public IPriorityRepository PriorityRepository
        {
            get
            {
                return _priorityRepository
                    ?? (_priorityRepository = new PriorityRepository(_transaction));
            }
        }

        public IStatusRepository StatusRepository
        {
            get
            {
                return _statusRepository
                    ?? (_statusRepository = new StatusRepository(_transaction));
            }
        }

        public IEmployeeTypeRepository EmployeeTypeRepository
        {
            get
            {
                return _employeeTypeRepository
                    ?? (_employeeTypeRepository = new EmployeeTypeRepository(_transaction));
            }
        }

        public IEmployeeStatusRepository EmployeeStatusRepository
        {
            get
            {
                return _employeeStatusRepository
                    ?? (_employeeStatusRepository = new EmployeeStatusRepository(_transaction));
            }
        }

        public IIndustryTypeRepository IndustryTypeRepository
        {
            get
            {
                return _industryTypeRepository
                    ?? (_industryTypeRepository = new IndustryTypeRepository(_transaction));
            }
        }

        public IEmployeeRoleRepository EmployeeRoleRepository
        {
            get
            {
                return _employeeRoleRepository
                    ?? (_employeeRoleRepository = new EmployeeRoleRepository(_transaction));
            }
        }

        public ITeamRepository TeamRepository
        {
            get
            {
                return _teamRepository
                    ?? (_teamRepository = new TeamRepository(_transaction));
            }
        }

        public ITeamMemberRepository TeamMemberRepository
        {
            get
            {
                return _teamMemberRepository
                    ?? (_teamMemberRepository = new TeamMemberRepository(_transaction));
            }
        }

        public ITimesheetProjectCategoryRepository TimesheetProjectCategoryRepository
        {
            get
            {
                return _timesheetProjectCategoryRepository
                    ?? (_timesheetProjectCategoryRepository = new TimesheetProjectCategoryRepository(_transaction));
            }
        }

        public ITimesheetAdministrativeRepository TimesheetAdministrativeRepository
        {
            get
            {
                return _timesheetAdministrativeRepository
                    ?? (_timesheetAdministrativeRepository = new TimesheetAdministrativeActivityRepository(_transaction));
            }
        }

        public ITimesheetTeamRepository TimesheetTeamRepository
        {
            get
            {
                return _timesheetTeamRepository
                    ?? (_timesheetTeamRepository = new TimesheetTeamRepository(_transaction));
            }
        }

        public ITimesheetActivityRepository TimesheetActivityRepository
        {
            get
            {
                return _timesheetActivityRepository
                    ?? (_timesheetActivityRepository = new TimesheetActivityRepository(_transaction));
            }
        }

        public ITimesheetActivityCommentRepository TimesheetActivityCommentRepository
        {
            get
            {
                return _timesheetActivityCommentRepository
                    ?? (_timesheetActivityCommentRepository = new TimesheetActivityCommentRepository(_transaction));
            }
        }

        public ITimesheetActivityFileRepository TimesheetActivityFileRepository
        {
            get
            {
                return _timesheetActivityFileRepository
                    ?? (_timesheetActivityFileRepository = new TimesheetActivityFileRepository(_transaction));
            }
        }

        public ITimesheetLocationRepository TimesheetLocationRepository
        {
            get
            {
                return _timesheetLocationRepository
                    ?? (_timesheetLocationRepository = new TimesheetLocationRepository(_transaction));
            }
        }

        public ILocationRepository LocationRepository
        {
            get
            {
                return _locationRepository
                    ?? (_locationRepository = new LocationRepository(_transaction));
            }
        }

        public IProductivityDashboardRepository ProductivityDashboardRepository
        {
            get
            {
                return _productivityDashboardRepository
                    ?? (_productivityDashboardRepository = new ProductivityDashboardRepository(_transaction));
            }
        }

        public IAdminProductivityDashboardRepository AdminProductivityDashboardRepository
        {
            get
            {
                return _adminProductivityDashboardRepository
                    ?? (_adminProductivityDashboardRepository = new AdminProductivityDashboardRepository(_transaction));
            }
        }

        public IEntityLocationRepository EntityLocationRepository
        {
            get
            {
                return _entityLocationRepository
                    ?? (_entityLocationRepository = new EntityLocationRepository(_transaction));
            }
        }

        public IOrganizationBranchRepository OrganizationBranchRepository
        {
            get
            {
                return _organizationBranchRepository
                    ?? (_organizationBranchRepository = new OrganizationBranchRepository(_transaction));
            }
        }

        public ITaskTeamMembersRepository TaskTeamMembersRepository
        {
            get
            {
                return _taskTeamMembersRepository
                    ?? (_taskTeamMembersRepository = new TaskTeamMembersRepository(_transaction));
            }
        }

        public IPlanRepository PlanRepository
        {
            get
            {
                return _planRepository
                    ?? (_planRepository = new PlanRepository(_transaction));
            }
        }

        public IPlanFeatureRepository PlanFeatureRepository
        {
            get
            {
                return _planFeatureRepository
                    ?? (_planFeatureRepository = new PlanFeatureRepository(_transaction));
            }
        }

        public IPlanPriceRepository PlanPriceRepository
        {
            get
            {
                return _planPriceRepository
                    ?? (_planPriceRepository = new PlanPriceRepository(_transaction));
            }
        }

        public IBillingRepository BillingRepository
        {
            get
            {
                return _billingRepository
                    ?? (_billingRepository = new BillingRepository(_transaction));
            }
        }

        public IProjectRepository ProjectRepository
        {
            get
            {
                return _projectRepository
                    ?? (_projectRepository = new ProjectRepository(_transaction));
            }
        }

        public IProjectStatusRepository ProjectStatusRepository
        {
            get
            {
                return _projectStatusRepository
                    ?? (_projectStatusRepository = new ProjectStatusRepository(_transaction));
            }
        }

        public IEntityContactRepository EntityContactRepository
        {
            get
            {
                return _entityContactRepository
                    ?? (_entityContactRepository = new EntityContactRepository(_transaction));
            }
        }

        public IProjectActivityRepository ProjectActivityRepository
        {
            get
            {
                return _projectActivityRepository
                    ?? (_projectActivityRepository = new ProjectActivityRepository(_transaction));
            }
        }

        public IProjectActivityTaskRepository ProjectActivityTaskRepository
        {
            get
            {
                return _projectActivityTaskRepository
                    ?? (_projectActivityTaskRepository = new ProjectActivityTaskRepository(_transaction));
            }
        }

        public ICustomerRepository CustomerRepository
        {
            get
            {
                return _customerRepository
                    ?? (_customerRepository = new CustomerRepository(_transaction));
            }
        }

        public ICustomerProjectRepository CustomerProjectRepository
        {
            get
            {
                return _customerProjectRepository
                    ?? (_customerProjectRepository = new CustomerProjectRepository(_transaction));
            }
        }

        public IDelegationsRepository DelegationsRepository
        {
            get
            {
                return _delegationsRepository
                    ?? (_delegationsRepository = new DelegationsRepository(_transaction));
            }
        }

        public IDelegationsDelegateeRepository DelegationsDelegateeRepository
        {
            get
            {
                return _delegationsDelegateeRepository
                    ?? (_delegationsDelegateeRepository = new DelegationsDelegateeRepository(_transaction));
            }
        }

        public ISuperadminOrganizationRepository SuperadminOrganizationRepository
        {
            get
            {
                return _superadminOrganizationRepository
                    ?? (_superadminOrganizationRepository = new SuperadminOrganizationRepository(_transaction));
            }
        }

        public IAdminDashboardRepository AdminDashboardRepository
        {
            get
            {
                return _adminDashboardRepository
                    ?? (_adminDashboardRepository = new AdminDashboardRepository(_transaction));
            }
        }

        public ILocationExceptionRepository LocationExceptionRepository
        {
            get
            {
                return _locationExceptionRepository
                    ?? (_locationExceptionRepository = new LocationExceptionRepository(_transaction));
            }
        }

        public IEntityInvitationRepository EntityInvitationRepository
        {
            get
            {
                return _entityInvitationRepository
                    ?? (_entityInvitationRepository = new EntityInvitationRepository(_transaction));
            }
        }

        public IWeekdaysRepository WeekdaysRepository
        {
            get
            {
                return _weekdaysRepository
                    ?? (_weekdaysRepository = new WeekdaysRepository(_transaction));
            }
        }

        public IDualApprovalRepository OrgWeekdaysRepository
        {
            get
            {
                return _orgWeekdaysRepository
                    ?? (_orgWeekdaysRepository = new DualApprovalRepository(_transaction));
            }
        }

        public IProjectTypeRepository ProjectTypeRepository
        {
            get
            {
                return _projectTypeRepository
                    ?? (_projectTypeRepository = new ProjectTypeRepository(_transaction));
            }
        }

        public ITimesheetBreakRepository TimesheetBreakRepository
        {
            get
            {
                return _timesheetBreakRepository
                    ?? (_timesheetBreakRepository = new TimesheetBreakRepository(_transaction));
            }
        }

        public IMilestoneTemplateRepository MilestoneTemplateRepository
        {
            get
            {
                return _milestoneTemplateRepository
                    ?? (_milestoneTemplateRepository = new MilestoneTemplateRepository(_transaction));
            }
        }

        public ITaskTemplateRepository TaskTemplateRepository
        {
            get
            {
                return _taskTemplateRepository
                    ?? (_taskTemplateRepository = new TaskTemplateRepository(_transaction));
            }
        }

        public ICostProjectRepository CostProjectRepository
        {
            get
            {
                return _costProjectRepository
                    ?? (_costProjectRepository = new CostProjectRepository(_transaction));
            }
        }

        public ICostProjectMilestoneRepository CostProjectMilestoneRepository
        {
            get
            {
                return _costProjectMilestoneRepository
                    ?? (_costProjectMilestoneRepository = new CostProjectMilestoneRepository(_transaction));
            }
        }

        public ICostProjectTaskRepository CostProjectTaskRepository
        {
            get
            {
                return _costProjectTaskRepository
                    ?? (_costProjectTaskRepository = new CostProjectTaskRepository(_transaction));
            }
        }

        public ITypeOfDesignRepository TypeOfDesignRepository
        {
            get
            {
                return _typeOfDesignRepository
                    ?? (_typeOfDesignRepository = new TypeOfDesignRepository(_transaction));
            }
        }

        public ISpecifiationRepository SpecifiationRepository
        {
            get
            {
                return _specifiationRepository
                    ?? (_specifiationRepository = new SpecifiationRepository(_transaction));
            }
        }

        public IUnitDescriptionRepository UnitDescriptionRepository
        {
            get
            {
                return _unitDescriptionRepository
                    ?? (_unitDescriptionRepository = new UnitDescriptionRepository(_transaction));
            }
        }

        public ITypeOfUnitRepository TypeOfUnitRepository
        {
            get
            {
                return _typeOfUnitRepository
                    ?? (_typeOfUnitRepository = new TypeOfUnitRepository(_transaction));
            }
        }

        public IProjectTagsRepository ProjectTagsRepository
        {
            get
            {
                return _projectTagsRepository
                    ?? (_projectTagsRepository = new ProjectTagsRepository(_transaction));
            }
        }

        public IProjectUnitRepository ProjectUnitRepository
        {
            get
            {
                return _projectUnitRepository
                    ?? (_projectUnitRepository = new ProjectUnitRepository(_transaction));
            }
        }

        public IProjectDesignTypeRepository ProjectDesignTypeRepository
        {
            get
            {
                return _projectDesignTypeRepository
                    ?? (_projectDesignTypeRepository = new ProjectDesignTypeRepository(_transaction));
            }
        }

        public IPackagesRepository PackagesRepository
        {
            get
            {
                return _packagesRepository
                    ?? (_packagesRepository = new PackagesRepository(_transaction));
            }
        }

        public ICostPerHourRepository CostPerHourRepository
        {
            get
            {
                return _costPerHourRepository
                    ?? (_costPerHourRepository = new CostPerHourRepository(_transaction));
            }
        }

        public IProfitMarginRepository ProfitMarginRepository
        {
            get
            {
                return _profitMarginRepository
                    ?? (_profitMarginRepository = new ProfitMarginRepository(_transaction));
            }
        }

        public ILeaveSetupRepository LeaveSetupRepository
        {
            get
            {
                return _leaveSetupRepository
                    ?? (_leaveSetupRepository = new LeaveSetupRepository(_transaction));
            }
        }

        public ILeaveTypeRepository LeaveTypeRepository
        {
            get
            {
                return _leaveTypeRepository
                    ?? (_leaveTypeRepository = new LeaveTypeRepository(_transaction));
            }
        }

        public ITimeoffTypeRepository TimeoffTypeRepository
        {
            get
            {
                return _timeoffTypeRepository
                    ?? (_timeoffTypeRepository = new TimeoffTypeRepository(_transaction));
            }
        }

        public IEmployeeLeaveRepository EmployeeLeaveRepository
        {
            get
            {
                return _employeeLeaveRepository
                    ?? (_employeeLeaveRepository = new EmployeeLeaveRepository(_transaction));
            }
        }

        public ILeaveStatusRepository LeaveStatusRepository
        {
            get
            {
                return _leaveStatusRepository
                    ?? (_leaveStatusRepository = new LeaveStatusRepository(_transaction));
            }
        }

        public IEmployeeScreenshotRepository EmployeeScreenshotRepository
        {
            get
            {
                return _employeeScreenshotRepository
                    ?? (_employeeScreenshotRepository = new EmployeeScreenshotRepository(_transaction));
            }
        }

        public IEmployeeAppUsageRepository EmployeeAppUsageRepository
        {
            get
            {
                return _employeeAppUsageRepository
                    ?? (_employeeAppUsageRepository = new EmployeeAppUsageRepository(_transaction));
            }
        }

        public IEmployeeAppTrackedRepository EmployeeAppTrackedRepository
        {
            get
            {
                return _employeeAppTrackedRepository
                    ?? (_employeeAppTrackedRepository = new EmployeeAppTrackedRepository(_transaction));
            }
        }

        public IEmployeeLeaveLogRepository EmployeeLeaveLogRepository
        {
            get
            {
                return _employeeLeaveLogRepository
                    ?? (_employeeLeaveLogRepository = new EmployeeLeaveLogRepository(_transaction));
            }
        }

        public ILeadCompanyRepository LeadCompanyRepository
        {
            get
            {
                return _leadCompanyRepository
                    ?? (_leadCompanyRepository = new LeadCompanyRepository(_transaction));
            }
        }

        public ILeadRepository LeadRepository
        {
            get
            {
                return _leadRepository
                    ?? (_leadRepository = new LeadRepository(_transaction));
            }
        }

        public ILeadDealRepository LeadDealRepository
        {
            get
            {
                return _LeadDealRepository
                    ?? (_LeadDealRepository = new LeadDealRepository(_transaction));
            }
        }

        public ILeadDealTypeRepository LeadDealTypeRepository
        {
            get
            {
                return _leadDealTypeRepository
                    ?? (_leadDealTypeRepository = new LeadDealTypeRepository(_transaction));
            }
        }

        public ILeadStageRepository LeadStageRepository
        {
            get
            {
                return _leadStageRepository
                    ?? (_leadStageRepository = new LeadStageRepository(_transaction));
            }
        }

        public ILeadContractRoleRepository LeadContractRoleRepository
        {
            get
            {
                return _leadContractRoleRepository
                    ?? (_leadContractRoleRepository = new LeadContractRoleRepository(_transaction));
            }
        }

        public ILeadSourceRepository LeadSourceRepository
        {
            get
            {
                return _leadSourceRepository
                    ?? (_leadSourceRepository = new LeadSourceRepository(_transaction));
            }
        }

        public ILeadStatusRepository LeadStatusRepository
        {
            get
            {
                return _leadStatusRepository
                    ?? (_leadStatusRepository = new LeadStatusRepository(_transaction));
            }
        }

        public ILeadRatingRepository LeadRatingRepository
        {
            get
            {
                return _leadRatingRepository
                    ?? (_leadRatingRepository = new LeadRatingRepository(_transaction));
            }
        }

        public ITimesheetDeskRepository TimesheetDeskRepository
        {
            get
            {
                return _timesheetDeskRepository
                    ?? (_timesheetDeskRepository = new TimesheetDeskRepository(_transaction));
            }
        }

        public IPrefixRepository PrefixRepository
        {
            get
            {
                return _prefixRepository
                    ?? (_prefixRepository = new PrefixRepository(_transaction));
            }
        }

        public IQuotationRepository QuotationRepository
        {
            get
            {
                return _quotationRepository
                    ?? (_quotationRepository = new QuotationRepository(_transaction));
            }
        }

        public IPaymentModeRepository PaymentModeRepository
        {
            get
            {
                return _paymentModeRepository
                    ?? (_paymentModeRepository = new PaymentModeRepository(_transaction));
            }
        }

        public IPaymentRepository PaymentRepository
        {
            get
            {
                return _paymentRepository
                    ?? (_paymentRepository = new PaymentRepository(_transaction));
            }
        }

        public IWarrantyRepository WarrantyRepository
        {
            get
            {
                return _warrantyRepository
                    ?? (_warrantyRepository = new WarrantyRepository(_transaction));
            }
        }

        public IExclusionRepository ExclusionRepository
        {
            get
            {
                return _exclusionRepository
                    ?? (_exclusionRepository = new ExclusionRepository(_transaction));
            }
        }

        public IEntityMeetingRepository EntityMeetingRepository
        {
            get
            {
                return _entityMeetingRepository
                    ?? (_entityMeetingRepository = new EntityMeetingRepository(_transaction));
            }
        }

        public IEntityNotesRepository EntityNotesRepository
        {
            get
            {
                return _entityNotesRepository
                    ?? (_entityNotesRepository = new EntityNotesRepository(_transaction));
            }
        }

        public IEntityMeetingParticipantsRepository EntityMeetingParticipantsRepository
        {
            get
            {
                return _entityMeetingParticipantsRepository
                    ?? (_entityMeetingParticipantsRepository = new EntityMeetingParticipantsRepository(_transaction));
            }
        }

        public ILocalActivityRepository LocalActivityRepository
        {
            get
            {
                return _localActivityRepository
                    ?? (_localActivityRepository = new LocalActivityRepository(_transaction));
            }
        }

        public IEntityCallRepository EntityCallRepository
        {
            get
            {
                return _entityCallRepository
                    ?? (_entityCallRepository = new EntityCallRepository(_transaction));
            }
        }

        public IEntityHistoryLogRepository EntityHistoryLogRepository
        {
            get
            {
                return _entityHistoryLogRepository
                    ?? (_entityHistoryLogRepository = new EntityHistoryLogRepository(_transaction));
            }
        }

        public IWorkforceRepository WorkforceRepository
        {
            get
            {
                return _workforceRepository
                    ?? (_workforceRepository = new WorkforceRepository(_transaction));
            }
        }

        

        public bool Commit()
        {
            bool isSuccess = false;
            try
            {
                _transaction.Commit();
                isSuccess = true;
            }
            catch
            {
                _transaction.Rollback();
                isSuccess = false;
            }
            finally
            {
                _transaction.Dispose();
                resetRepositories();
                _transaction = _connection.BeginTransaction();
            }
            return isSuccess;
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
            _planRepository = null;
            _planFeatureRepository = null;
            _planPriceRepository = null;
            _subscriptionRepository = null;
            _billingRepository = null;


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
            _administrativeRepository = null;
            _subscriptionRepository = null;
            _reportingRepository = null;
            _designationRepositiory = null;
            _timesheetRepository = null;
            _taskRepository = null;
            _setupRepository = null;
            _priorityRepository = null;
            _statusRepository = null;
            _employeeTypeRepository = null;
            _employeeStatusRepository = null;
            _industryTypeRepository = null;
            _employeeRoleRepository = null;
            _teamRepository = null;
            _timesheetProjectCategoryRepository = null;
            _timesheetAdministrativeRepository = null;
            _timesheetTeamRepository = null;
            _timesheetActivityRepository = null;
            _timesheetActivityCommentRepository = null;
            _timesheetActivityFileRepository = null;
            _timesheetLocationRepository = null;
            _locationRepository = null;
            _entityLocationRepository = null;
            _organizationBranchRepository = null;
            _taskTeamMembersRepository = null;
            _projectRepository = null;
            _projectStatusRepository = null;
            _entityContactRepository = null;
            _projectActivityRepository = null;
            _projectActivityTaskRepository = null;
            _customerRepository = null;
            _customerProjectRepository = null;
            _delegationsRepository = null;
            _delegationsDelegateeRepository = null;
            _superadminOrganizationRepository = null;
            _adminDashboardRepository = null;
            _productivityDashboardRepository = null;
            _adminProductivityDashboardRepository = null;
            _locationExceptionRepository = null;
            _entityInvitationRepository = null;
            _weekdaysRepository = null;
            _orgWeekdaysRepository = null;
            _projectTypeRepository = null;
            _milestoneTemplateRepository = null;
            _taskTemplateRepository = null;
            _costProjectRepository = null;
            _costProjectMilestoneRepository = null;
            _costProjectTaskRepository = null;
            _typeOfDesignRepository = null;
            _specifiationRepository = null;
            _unitDescriptionRepository = null;
            _typeOfUnitRepository = null;
            _projectTagsRepository = null;
            _projectUnitRepository = null;
            _projectDesignTypeRepository = null;
            _packagesRepository = null;
            _costPerHourRepository = null;
            _profitMarginRepository = null;
            _leaveSetupRepository = null;
            _leaveTypeRepository = null;
            _timeoffTypeRepository = null;
            _employeeLeaveRepository = null;
            _leaveStatusRepository = null;
            _employeeScreenshotRepository = null;
            _employeeAppUsageRepository = null;
            _employeeAppTrackedRepository = null;
            _employeeLeaveLogRepository = null;
            _leadCompanyRepository = null;
            _leadRepository = null;
            _LeadDealRepository = null;
            _leadSourceRepository = null;
            _leadStatusRepository = null;
            _leadRatingRepository = null;
            _timesheetDeskRepository = null;
            _leadDealTypeRepository = null;
            _leadStageRepository = null;
            _leadContractRoleRepository = null;
            _prefixRepository = null;
            _quotationRepository = null;
            _paymentModeRepository = null;
            _paymentRepository = null;
            _warrantyRepository = null;
            _exclusionRepository = null;
            _entityMeetingRepository = null;
            _entityNotesRepository = null;
            _entityMeetingParticipantsRepository = null;
            _localActivityRepository = null;
            _entityCallRepository = null;
            _entityHistoryLogRepository = null;

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

