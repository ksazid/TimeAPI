using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IWorkforceRepository : IRepositoryAsync<Workforce, string>
    {
        Task<Workforce> WorkforceProductiveByDeptIDAndDate(string DeptID, string date);
        Task<Workforce> WorkforceProductiveByTeamIDAndDate(string DeptID, string date);
        Task<Workforce> WorkforceProductiveByOrgIDAndDate(string DeptID, string date);
    }
}
