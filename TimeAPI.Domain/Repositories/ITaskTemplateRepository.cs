using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ITaskTemplateRepository : IRepository<TaskTemplate, string>
    {
        //void RemoveByOrgID(string OrgID);
       IEnumerable<TaskTemplate> FindByOrgID(string OrgID);
    }
}
