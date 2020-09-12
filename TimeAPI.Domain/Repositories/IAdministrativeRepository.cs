using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IAdministrativeRepository : IRepositoryAsync<Administrative, string>
    {
        Task<RootObject> GetByOrgID(string OrgID);

        Task<dynamic> GetAdministrativeTaskByOrgID(string OrgID);
    }
}
