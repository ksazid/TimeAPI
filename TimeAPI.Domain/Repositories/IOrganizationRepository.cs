using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IOrganizationRepository : IRepository<Organization, string>
    {
        Organization FindByOrgName(string full_name);
        dynamic FindByUsersID(string user_id);
        IEnumerable<OrganizationBranchViewModel> FindByAllBranchByParengOrgID(string OrgID);
    }
}
