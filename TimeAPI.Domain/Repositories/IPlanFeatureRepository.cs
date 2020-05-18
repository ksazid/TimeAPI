using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IPlanFeatureRepository : IRepository<PlanFeature, string>
    {
        dynamic GetAllPlanFeatures();

        IEnumerable<PlanFeature> GetPlanFeatureByPlanID(string plan_id);

    }
}
