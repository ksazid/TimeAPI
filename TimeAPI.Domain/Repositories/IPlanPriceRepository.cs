using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IPlanPriceRepository : IRepository<PlanPrice, string>
    {
        dynamic GetAllPlanPrice();
        IEnumerable<PlanPrice> GetPlanPriceByPlanID(string PlanID);
    }
}
