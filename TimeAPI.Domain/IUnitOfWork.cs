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
        IOrganizationSetupRepository OrganizationSetupRepository { get; }
        IDepartmentRepository DepartmentRepository { get; }
        IProfileImageRepository ProfileImageRepository { get; }
        IReportingRepository ReportingRepository { get; }
        ISocialRepository SocialRepository { get; }
        IAdministrativeRepository AdministrativeRepository { get; }
        IDesignationRepositiory DesignationRepositiory { get; }
        ITimesheetRepository TimesheetRepository { get; }
        ITimesheetBreakRepository TimesheetBreakRepository { get; }
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
        IProjectActivityRepository ProjectActivityRepository { get; }
        IProjectActivityTaskRepository ProjectActivityTaskRepository { get; }
        ICustomerRepository CustomerRepository { get; }
        ICustomerProjectRepository CustomerProjectRepository { get; }
        IDelegationsRepository DelegationsRepository { get; }
        IDelegationsDelegateeRepository DelegationsDelegateeRepository { get; }
        ISuperadminOrganizationRepository SuperadminOrganizationRepository { get; }
        IAdminDashboardRepository AdminDashboardRepository { get; }
        IProductivityDashboardRepository ProductivityDashboardRepository { get; }
        IAdminProductivityDashboardRepository AdminProductivityDashboardRepository { get; }
        ILocationExceptionRepository LocationExceptionRepository { get; }
        IEntityInvitationRepository EntityInvitationRepository { get; }
        IWeekdaysRepository WeekdaysRepository { get; }
        IDualApprovalRepository OrgWeekdaysRepository { get; }
        IProjectTypeRepository ProjectTypeRepository { get; }
        IMilestoneTemplateRepository MilestoneTemplateRepository { get; }
        ITaskTemplateRepository TaskTemplateRepository { get; }

        //cost binsalem design
        ICostProjectRepository CostProjectRepository { get; }
        ICostProjectMilestoneRepository CostProjectMilestoneRepository { get; }
        ICostProjectTaskRepository CostProjectTaskRepository { get; }
        ITypeOfDesignRepository TypeOfDesignRepository { get; }
        ISpecifiationRepository SpecifiationRepository { get; }
        IUnitDescriptionRepository UnitDescriptionRepository{ get; }
        ITypeOfUnitRepository TypeOfUnitRepository { get; }
        IProjectUnitRepository ProjectUnitRepository { get; }
        IProjectTagsRepository ProjectTagsRepository { get; }
        IProjectDesignTypeRepository ProjectDesignTypeRepository { get; }
        IPackagesRepository PackagesRepository { get; }
        ICostPerHourRepository CostPerHourRepository { get; }
        IProfitMarginRepository ProfitMarginRepository { get; }
        ILeaveSetupRepository LeaveSetupRepository { get; }
        ILeaveTypeRepository LeaveTypeRepository { get; }
        ITimeoffTypeRepository TimeoffTypeRepository { get; }
        IEmployeeLeaveRepository EmployeeLeaveRepository { get; }
        ILeaveStatusRepository LeaveStatusRepository { get; }
        IEmployeeScreenshotRepository EmployeeScreenshotRepository { get; }
        IEmployeeAppUsageRepository EmployeeAppUsageRepository { get; }
        IEmployeeAppTrackedRepository EmployeeAppTrackedRepository { get; }
        IEmployeeLeaveLogRepository EmployeeLeaveLogRepository { get; }
        ILeadCompanyRepository LeadCompanyRepository { get; }
        ILeadRepository LeadRepository { get; }
        ILeadDealRepository LeadDealRepository { get; }
        ILeadSourceRepository LeadSourceRepository { get; }
        ILeadStatusRepository LeadStatusRepository { get; }
        ILeadRatingRepository LeadRatingRepository { get; }
        ITimesheetDeskRepository TimesheetDeskRepository { get; }
        ILeadDealTypeRepository LeadDealTypeRepository { get; }
        ILeadStageRepository LeadStageRepository { get; }
        ILeadContractRoleRepository LeadContractRoleRepository { get; }
        IPrefixRepository PrefixRepository { get; }
        IQuotationRepository QuotationRepository { get; }
        IPaymentModeRepository PaymentModeRepository { get; }
        IPaymentRepository PaymentRepository { get; }
        IWarrantyRepository WarrantyRepository { get; }
        IExclusionRepository ExclusionRepository { get; }
        IEntityMeetingRepository EntityMeetingRepository { get; }
        IEntityNotesRepository EntityNotesRepository { get; }
        IEntityMeetingParticipantsRepository EntityMeetingParticipantsRepository { get; }
        ILocalActivityRepository LocalActivityRepository { get; }
        IEntityCallRepository EntityCallRepository { get; }
        IEntityHistoryLogRepository EntityHistoryLogRepository { get; }
      


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
