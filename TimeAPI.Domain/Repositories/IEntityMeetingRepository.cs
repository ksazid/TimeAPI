using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IEntityMeetingRepository : IRepositoryAsync<EntityMeeting, string>
    {
        Task<IEnumerable<EntityMeeting>> EntityMeetingByOrgID(string OrgID);
        Task<dynamic> EntityMeetingByEntityID(string EntityID);
        Task<dynamic> GetAllOpenActivitiesEntityID(string EntityID);
        Task<dynamic> GetAllCloseActivitiesEntityID(string EntityID);
        Task<dynamic> GetLocalActivitieEntityID(string EntityID);
        Task<dynamic> GetRecentTimesheetByEmpID(string EmpID, string Date);
    }
}
