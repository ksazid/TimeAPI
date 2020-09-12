using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Models;
using TimeAPI.API.Models.DepartmentViewModels;
using TimeAPI.API.Pre_Defined;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Controllers
{
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize]
    public class DepartmentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly DateTime _dateTime;

        public DepartmentController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, ILogger<DepartmentController> logger, IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;
        }

        [HttpPost]
        [Route("AddDepartment")]
        public async Task<object> AddDepartment([FromBody] DepartmentViewModel departmentViewModels, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (departmentViewModels == null)
                    throw new ArgumentNullException(nameof(departmentViewModels));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<DepartmentViewModel, Department>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Department>(departmentViewModels);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.DepartmentRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Department registered successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateDepartment")]
        public async Task<object> UpdateDepartment([FromBody] DepartmentViewModel departmentViewModels, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (departmentViewModels == null)
                    throw new ArgumentNullException(nameof(departmentViewModels));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<DepartmentViewModel, Department>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Department>(departmentViewModels);

                modal.modified_date = _dateTime.ToString();

                _unitOfWork.DepartmentRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Department updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveDepartment")]
        public async Task<object> RemoveDepartment([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.DepartmentRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Department removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllDepartments")]
        public async Task<object> GetAllDepartments(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.DepartmentRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByDepartmentName")]
        public async Task<object> FindByDepartmentName([FromBody] UtilsName UtilsName, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (UtilsName == null)
                    throw new ArgumentNullException(nameof(UtilsName.FullName));

                var result = _unitOfWork.DepartmentRepository.FindByDepartmentName(UtilsName.FullName);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByDepartmentAlias")]
        public async Task<object> FindByDepartmentAlias([FromBody] UtilsAlias UtilsAlias, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (UtilsAlias == null)
                    throw new ArgumentNullException(nameof(UtilsAlias.Alias));

                var result = _unitOfWork.DepartmentRepository.FindByDepartmentAlias(UtilsAlias.Alias);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindDepartmentByOrgID")]
        public async Task<object> FindDepartmentByOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.DepartmentRepository.FindDepartmentByOrgID(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByDepartmentID")]
        public async Task<object> FindByDepartmentID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.DepartmentRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindDepLeadByDepID")]
        public async Task<object> FindDepLeadByDepID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.DepartmentRepository.FindDepLeadByDepID(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchGridDataByDepartmentOrgID")]
        public async Task<object> FetchGridDataByDepartmentOrgID([FromBody] UtilsOrgID UtilsOrgID, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (UtilsOrgID == null)
                    throw new ArgumentNullException(nameof(UtilsOrgID.OrgID));

                oDataTable _oDataTable = new oDataTable();
                var results = _unitOfWork.DepartmentRepository.FetchGridDataByDepOrgID(UtilsOrgID.OrgID);
                var xResult = _oDataTable.ToDataTable(results);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllPreDefinedDepartment")]
        public async Task<object> GetAllPreDefinedDepartment(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = PreDefined.GetDepartment();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #region SetUp

        [HttpPost]
        [Route("SetupPreDefinedDepartment")]
        public async Task<object> SetupPreDefinedDepartment([FromBody] UtilDepartmentChecked utilDepartmentChecked, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (utilDepartmentChecked == null)
                    throw new ArgumentNullException(nameof(utilDepartmentChecked.org_id));

                SetupDepartmentDynamically(utilDepartmentChecked);

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Successful", Desc = "Department Added Successfully" }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        private void SetupDepartmentDynamically(UtilDepartmentChecked utilDepartmentChecked)
        {
            if (utilDepartmentChecked.is_accounts)
            {
                var Result = PreDefined.GetDepartment();
                var Department = new Department()
                {
                    id = Guid.NewGuid().ToString(),
                    org_id = utilDepartmentChecked.org_id,
                    dep_name = Result["1"].ToString(),
                    alias = Result["1"].ToString().ToLower(),
                    createdby = utilDepartmentChecked.createdby,
                    created_date = _dateTime.ToString(),
                    is_deleted = false
                };
                string DeptID = Department.id;
                _unitOfWork.DepartmentRepository.Add(Department);

                var Accounts = PreDefined.GetAccountsDesignation();

                for (int i = 0; i < Accounts.Count; i++)
                {
                    var Designation = new Designation()
                    {
                        id = Guid.NewGuid().ToString(),
                        dep_id = DeptID,
                        designation_name = Accounts[i].ToString(),
                        alias = Accounts[i].ToString().ToLower(),
                        createdby = utilDepartmentChecked.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.DesignationRepositiory.Add(Designation);
                }
            }
            if (utilDepartmentChecked.is_administrative)
            {
                var Result = PreDefined.GetDepartment();
                var Department = new Department()
                {
                    id = Guid.NewGuid().ToString(),
                    org_id = utilDepartmentChecked.org_id,
                    dep_name = Result["2"].ToString(),
                    alias = Result["2"].ToString().ToLower(),
                    createdby = utilDepartmentChecked.createdby,
                    created_date = _dateTime.ToString(),
                    is_deleted = false
                };
                string DeptID = Department.id;
                _unitOfWork.DepartmentRepository.Add(Department);

                var Accounts = PreDefined.GetAdministrativeDesignation();

                for (int i = 0; i < Accounts.Count; i++)
                {
                    var Designation = new Designation()
                    {
                        id = Guid.NewGuid().ToString(),
                        dep_id = DeptID,
                        designation_name = Accounts[i].ToString(),
                        alias = Accounts[i].ToString().ToLower(),
                        createdby = utilDepartmentChecked.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.DesignationRepositiory.Add(Designation);
                }
            }
            if (utilDepartmentChecked.is_advertisement_marketing)
            {
                var Result = PreDefined.GetDepartment();
                var Department = new Department()
                {
                    id = Guid.NewGuid().ToString(),
                    org_id = utilDepartmentChecked.org_id,
                    dep_name = Result["3"].ToString(),
                    alias = Result["3"].ToString().ToLower(),
                    createdby = utilDepartmentChecked.createdby,
                    created_date = _dateTime.ToString(),
                    is_deleted = false
                };
                string DeptID = Department.id;
                _unitOfWork.DepartmentRepository.Add(Department);

                var Accounts = PreDefined.GetAdvertisementDesignation();

                for (int i = 0; i < Accounts.Count; i++)
                {
                    var Designation = new Designation()
                    {
                        id = Guid.NewGuid().ToString(),
                        dep_id = DeptID,
                        designation_name = Accounts[i].ToString(),
                        alias = Accounts[i].ToString().ToLower(),
                        createdby = utilDepartmentChecked.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.DesignationRepositiory.Add(Designation);
                }
            }
            if (utilDepartmentChecked.is_construction)
            {
                var Result = PreDefined.GetDepartment();
                var Department = new Department()
                {
                    id = Guid.NewGuid().ToString(),
                    org_id = utilDepartmentChecked.org_id,
                    dep_name = Result["4"].ToString(),
                    alias = Result["4"].ToString().ToLower(),
                    createdby = utilDepartmentChecked.createdby,
                    created_date = _dateTime.ToString(),
                    is_deleted = false
                };
                string DeptID = Department.id;
                _unitOfWork.DepartmentRepository.Add(Department);

                var Accounts = PreDefined.GetConstructionDesignation();

                for (int i = 0; i < Accounts.Count; i++)
                {
                    var Designation = new Designation()
                    {
                        id = Guid.NewGuid().ToString(),
                        dep_id = DeptID,
                        designation_name = Accounts[i].ToString(),
                        alias = Accounts[i].ToString().ToLower(),
                        createdby = utilDepartmentChecked.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.DesignationRepositiory.Add(Designation);
                }
            }
            if (utilDepartmentChecked.is_customer_service)
            {
                var Result = PreDefined.GetDepartment();
                var Department = new Department()
                {
                    id = Guid.NewGuid().ToString(),
                    org_id = utilDepartmentChecked.org_id,
                    dep_name = Result["5"].ToString(),
                    alias = Result["5"].ToString().ToLower(),
                    createdby = utilDepartmentChecked.createdby,
                    created_date = _dateTime.ToString(),
                    is_deleted = false
                };
                string DeptID = Department.id;
                _unitOfWork.DepartmentRepository.Add(Department);

                var Accounts = PreDefined.GetCustomerServiceDesignation();

                for (int i = 0; i < Accounts.Count; i++)
                {
                    var Designation = new Designation()
                    {
                        id = Guid.NewGuid().ToString(),
                        dep_id = DeptID,
                        designation_name = Accounts[i].ToString(),
                        alias = Accounts[i].ToString().ToLower(),
                        createdby = utilDepartmentChecked.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.DesignationRepositiory.Add(Designation);
                }
            }
            if (utilDepartmentChecked.is_design)
            {
                var Result = PreDefined.GetDepartment();
                var Department = new Department()
                {
                    id = Guid.NewGuid().ToString(),
                    org_id = utilDepartmentChecked.org_id,
                    dep_name = Result["6"].ToString(),
                    alias = Result["6"].ToString().ToLower(),
                    createdby = utilDepartmentChecked.createdby,
                    created_date = _dateTime.ToString(),
                    is_deleted = false
                };
                string DeptID = Department.id;
                _unitOfWork.DepartmentRepository.Add(Department);

                var Accounts = PreDefined.GetDesignDesignation();

                for (int i = 0; i < Accounts.Count; i++)
                {
                    var Designation = new Designation()
                    {
                        id = Guid.NewGuid().ToString(),
                        dep_id = DeptID,
                        designation_name = Accounts[i].ToString(),
                        alias = Accounts[i].ToString().ToLower(),
                        createdby = utilDepartmentChecked.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.DesignationRepositiory.Add(Designation);
                }
            }
            if (utilDepartmentChecked.is_engineering)
            {
                var Result = PreDefined.GetDepartment();
                var Department = new Department()
                {
                    id = Guid.NewGuid().ToString(),
                    org_id = utilDepartmentChecked.org_id,
                    dep_name = Result["7"].ToString(),
                    alias = Result["7"].ToString().ToLower(),
                    createdby = utilDepartmentChecked.createdby,
                    created_date = _dateTime.ToString(),
                    is_deleted = false
                };
                string DeptID = Department.id;
                _unitOfWork.DepartmentRepository.Add(Department);

                var Accounts = PreDefined.GetEngineerDesignation();

                for (int i = 0; i < Accounts.Count; i++)
                {
                    var Designation = new Designation()
                    {
                        id = Guid.NewGuid().ToString(),
                        dep_id = DeptID,
                        designation_name = Accounts[i].ToString(),
                        alias = Accounts[i].ToString().ToLower(),
                        createdby = utilDepartmentChecked.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.DesignationRepositiory.Add(Designation);
                }
            }
            if (utilDepartmentChecked.is_facilities)
            {
                var Result = PreDefined.GetDepartment();
                var Department = new Department()
                {
                    id = Guid.NewGuid().ToString(),
                    org_id = utilDepartmentChecked.org_id,
                    dep_name = Result["8"].ToString(),
                    alias = Result["8"].ToString().ToLower(),
                    createdby = utilDepartmentChecked.createdby,
                    created_date = _dateTime.ToString(),
                    is_deleted = false
                };
                string DeptID = Department.id;
                _unitOfWork.DepartmentRepository.Add(Department);

                var Accounts = PreDefined.GetFacilitiesDesignation();

                for (int i = 0; i < Accounts.Count; i++)
                {
                    var Designation = new Designation()
                    {
                        id = Guid.NewGuid().ToString(),
                        dep_id = DeptID,
                        designation_name = Accounts[i].ToString(),
                        alias = Accounts[i].ToString().ToLower(),
                        createdby = utilDepartmentChecked.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.DesignationRepositiory.Add(Designation);
                }
            }
            if (utilDepartmentChecked.is_finance)
            {
                var Result = PreDefined.GetDepartment();
                var Department = new Department()
                {
                    id = Guid.NewGuid().ToString(),
                    org_id = utilDepartmentChecked.org_id,
                    dep_name = Result["9"].ToString(),
                    alias = Result["9"].ToString().ToLower(),
                    createdby = utilDepartmentChecked.createdby,
                    created_date = _dateTime.ToString(),
                    is_deleted = false
                };
                string DeptID = Department.id;
                _unitOfWork.DepartmentRepository.Add(Department);

                var Accounts = PreDefined.GetFinanceDesignation();

                for (int i = 0; i < Accounts.Count; i++)
                {
                    var Designation = new Designation()
                    {
                        id = Guid.NewGuid().ToString(),
                        dep_id = DeptID,
                        designation_name = Accounts[i].ToString(),
                        alias = Accounts[i].ToString().ToLower(),
                        createdby = utilDepartmentChecked.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.DesignationRepositiory.Add(Designation);
                }
            }
            if (utilDepartmentChecked.is_human_resources)
            {
                var Result = PreDefined.GetDepartment();
                var Department = new Department()
                {
                    id = Guid.NewGuid().ToString(),
                    org_id = utilDepartmentChecked.org_id,
                    dep_name = Result["10"].ToString(),
                    alias = Result["10"].ToString().ToLower(),
                    createdby = utilDepartmentChecked.createdby,
                    created_date = _dateTime.ToString(),
                    is_deleted = false
                };
                string DeptID = Department.id;
                _unitOfWork.DepartmentRepository.Add(Department);

                var Accounts = PreDefined.GetHumanResourcesDesignation();

                for (int i = 0; i < Accounts.Count; i++)
                {
                    var Designation = new Designation()
                    {
                        id = Guid.NewGuid().ToString(),
                        dep_id = DeptID,
                        designation_name = Accounts[i].ToString(),
                        alias = Accounts[i].ToString().ToLower(),
                        createdby = utilDepartmentChecked.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.DesignationRepositiory.Add(Designation);
                }
            }
            if (utilDepartmentChecked.is_it_and_development)
            {
                var Result = PreDefined.GetDepartment();
                var Department = new Department()
                {
                    id = Guid.NewGuid().ToString(),
                    org_id = utilDepartmentChecked.org_id,
                    dep_name = Result["11"].ToString(),
                    alias = Result["11"].ToString().ToLower(),
                    createdby = utilDepartmentChecked.createdby,
                    created_date = _dateTime.ToString(),
                    is_deleted = false
                };
                string DeptID = Department.id;
                _unitOfWork.DepartmentRepository.Add(Department);

                var Accounts = PreDefined.GetITDevelopmentDesignation();

                for (int i = 0; i < Accounts.Count; i++)
                {
                    var Designation = new Designation()
                    {
                        id = Guid.NewGuid().ToString(),
                        dep_id = DeptID,
                        designation_name = Accounts[i].ToString(),
                        alias = Accounts[i].ToString().ToLower(),
                        createdby = utilDepartmentChecked.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.DesignationRepositiory.Add(Designation);
                }
            }
            if (utilDepartmentChecked.is_legal)
            {
                var Result = PreDefined.GetDepartment();
                var Department = new Department()
                {
                    id = Guid.NewGuid().ToString(),
                    org_id = utilDepartmentChecked.org_id,
                    dep_name = Result["12"].ToString(),
                    alias = Result["12"].ToString().ToLower(),
                    createdby = utilDepartmentChecked.createdby,
                    created_date = _dateTime.ToString(),
                    is_deleted = false
                };
                string DeptID = Department.id;
                _unitOfWork.DepartmentRepository.Add(Department);

                var Accounts = PreDefined.GetLegalDesignation();

                for (int i = 0; i < Accounts.Count; i++)
                {
                    var Designation = new Designation()
                    {
                        id = Guid.NewGuid().ToString(),
                        dep_id = DeptID,
                        designation_name = Accounts[i].ToString(),
                        alias = Accounts[i].ToString().ToLower(),
                        createdby = utilDepartmentChecked.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.DesignationRepositiory.Add(Designation);
                }
            }
            if (utilDepartmentChecked.is_logistics)
            {
                var Result = PreDefined.GetDepartment();
                var Department = new Department()
                {
                    id = Guid.NewGuid().ToString(),
                    org_id = utilDepartmentChecked.org_id,
                    dep_name = Result["13"].ToString(),
                    alias = Result["13"].ToString().ToLower(),
                    createdby = utilDepartmentChecked.createdby,
                    created_date = _dateTime.ToString(),
                    is_deleted = false
                };
                string DeptID = Department.id;
                _unitOfWork.DepartmentRepository.Add(Department);

                var Accounts = PreDefined.GetLogisticsDesignation();

                for (int i = 0; i < Accounts.Count; i++)
                {
                    var Designation = new Designation()
                    {
                        id = Guid.NewGuid().ToString(),
                        dep_id = DeptID,
                        designation_name = Accounts[i].ToString(),
                        alias = Accounts[i].ToString().ToLower(),
                        createdby = utilDepartmentChecked.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.DesignationRepositiory.Add(Designation);
                }
            }
            if (utilDepartmentChecked.is_operation_and_production)
            {
                var Result = PreDefined.GetDepartment();
                var Department = new Department()
                {
                    id = Guid.NewGuid().ToString(),
                    org_id = utilDepartmentChecked.org_id,
                    dep_name = Result["14"].ToString(),
                    alias = Result["14"].ToString().ToLower(),
                    createdby = utilDepartmentChecked.createdby,
                    created_date = _dateTime.ToString(),
                    is_deleted = false
                };
                string DeptID = Department.id;
                _unitOfWork.DepartmentRepository.Add(Department);

                var Accounts = PreDefined.GetOperationProductionDesignation();

                for (int i = 0; i < Accounts.Count; i++)
                {
                    var Designation = new Designation()
                    {
                        id = Guid.NewGuid().ToString(),
                        dep_id = DeptID,
                        designation_name = Accounts[i].ToString(),
                        alias = Accounts[i].ToString().ToLower(),
                        createdby = utilDepartmentChecked.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.DesignationRepositiory.Add(Designation);
                }
            }
            if (utilDepartmentChecked.is_real_estate)
            {
                var Result = PreDefined.GetDepartment();
                var Department = new Department()
                {
                    id = Guid.NewGuid().ToString(),
                    org_id = utilDepartmentChecked.org_id,
                    dep_name = Result["15"].ToString(),
                    alias = Result["15"].ToString().ToLower(),
                    createdby = utilDepartmentChecked.createdby,
                    created_date = _dateTime.ToString(),
                    is_deleted = false
                };
                string DeptID = Department.id;
                _unitOfWork.DepartmentRepository.Add(Department);

                var Accounts = PreDefined.GetRealEstateDesignation();

                for (int i = 0; i < Accounts.Count; i++)
                {
                    var Designation = new Designation()
                    {
                        id = Guid.NewGuid().ToString(),
                        dep_id = DeptID,
                        designation_name = Accounts[i].ToString(),
                        alias = Accounts[i].ToString().ToLower(),
                        createdby = utilDepartmentChecked.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.DesignationRepositiory.Add(Designation);
                }
            }
            if (utilDepartmentChecked.is_sales)
            {
                var Result = PreDefined.GetDepartment();
                var Department = new Department()
                {
                    id = Guid.NewGuid().ToString(),
                    org_id = utilDepartmentChecked.org_id,
                    dep_name = Result["16"].ToString(),
                    alias = Result["16"].ToString().ToLower(),
                    createdby = utilDepartmentChecked.createdby,
                    created_date = _dateTime.ToString(),
                    is_deleted = false
                };
                string DeptID = Department.id;
                _unitOfWork.DepartmentRepository.Add(Department);

                var Accounts = PreDefined.GetSalesDesignation();

                for (int i = 0; i < Accounts.Count; i++)
                {
                    var Designation = new Designation()
                    {
                        id = Guid.NewGuid().ToString(),
                        dep_id = DeptID,
                        designation_name = Accounts[i].ToString(),
                        alias = Accounts[i].ToString().ToLower(),
                        createdby = utilDepartmentChecked.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.DesignationRepositiory.Add(Designation);
                }
            }
        }

        #endregion SetUp
    }
}