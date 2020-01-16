using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IOrganizationBranchRepository : IRepository<OrganizationBranch, string>
    {
        //OrganizationBranch FindByOrgName(string full_name);
        //IEnumerable<OrganizationBranch> FindByUsersID(string user_id);
    }
}
