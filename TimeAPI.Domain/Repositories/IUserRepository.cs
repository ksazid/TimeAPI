using System.Collections.Generic;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface IUserRepository : IRepositoryAsync<User, string>
    {
        User FindByNormalizedUserName(string normalizedUserName);

        User FindByNormalizedEmail(string normalizedEmail);

        Task<UserDataGroupDataSet> GetUserDataGroupByUserID(string EmpID, string Date);

        Task<IEnumerable<RootTimesheetData>> GetAllTimesheetByEmpID(string EmpID, string Date);

        //Task<IEnumerable<RootTimesheetData>> GetAllProjectTaskByEmpID(string EmpID, string Date);

        Task<IEnumerable<RootTimesheetData>> GetAllTimesheetByOrgID(string EmpID, string FromDate, string ToDate);

        Task<dynamic> LastCheckinByEmpID(string EmpID, string Date);

        Task<IEnumerable<RootTimesheetData>> GetEmployeeTasksTimesheetByEmpID(string EmpID, string FromDate, string ToDate);

        Task<IEnumerable<RootTimesheetData>> GetEmployeeTasksTimesheetByOrgID(string EmpID, string FromDate, string ToDate);

        IEnumerable<User> GetAll();
    }
}
