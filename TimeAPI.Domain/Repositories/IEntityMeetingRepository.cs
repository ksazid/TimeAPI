using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IEntityMeetingRepository : IRepository<EntityMeeting, string>
    {
        IEnumerable<EntityMeeting> EntityMeetingByOrgID(string OrgID);
        dynamic EntityMeetingByEntityID(string EntityID);
        dynamic GetAllOpenActivitiesEntityID(string EntityID);
        dynamic GetAllCloseActivitiesEntityID(string EntityID);
    }
}
