using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface IEmployeeLeaveRepository : IRepository<EmployeeLeave, string>
    {
        dynamic FetchEmployeeLeaveOrgID(string OrgID);
        dynamic FetchEmployeeLeaveEmpID(string EmpID);
        dynamic FetchEmployeeLeaveID(string EmpID);
        dynamic FetchEmployeeLeaveHistoryEmpID(string EmpID);
        dynamic GetDaysOfMonth(string startdate, string enddate);
        dynamic FetchEmployeeLeaveHistoryOrgID(string OrgID);
        dynamic FetchEmployeeLeaveHistoryApproverID(string ApproverID);
        void UpdateApprovedByID(EmployeeLeave employeeLeave);
    }
}
