using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface IEmployeeLeaveRepository : IRepositoryAsync<EmployeeLeave, string>
    {
        Task<dynamic> FetchEmployeeLeaveOrgID(string OrgID);
        Task<dynamic> FetchEmployeeLeaveEmpID(string EmpID);
        Task<dynamic> FetchEmployeeLeaveID(string EmpID);
        Task<dynamic> FetchEmployeeLeaveHistoryEmpID(string EmpID);
        Task<dynamic> GetDaysOfMonth(string startdate, string enddate);
        Task<dynamic> FetchEmployeeLeaveHistoryOrgID(string OrgID);
        Task<dynamic> FetchEmployeeLeaveHistoryApproverID(string ApproverID);
        Task UpdateApprovedByID(EmployeeLeave employeeLeave);
    }
}
