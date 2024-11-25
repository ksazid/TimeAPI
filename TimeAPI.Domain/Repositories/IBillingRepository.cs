﻿using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IBillingRepository : IRepository<Billing, string>
    {
        IEnumerable<Billing> FindBillingsByOrgID(string OrgID);
        //dynamic FindByTeamID(string TeamID);
        //IEnumerable<dynamic> FetchAllTeamsByOrgID(string OrgID);
        //IEnumerable<dynamic> FetchAllTeamMembersByTeamID(string key);
        //dynamic GetAllTeamMembersByTeamID(string key);

    }
}
