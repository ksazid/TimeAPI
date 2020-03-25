using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IPlanRepository : IRepository<Plan, string>
    {
        IEnumerable<PlanPrice>  FindPlanPriceByPlanID(string PlanID);
        string  GetPlanIDByPlanName(string PlanName);
    }
}
