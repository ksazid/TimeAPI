using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ISubscriptionRepository : IRepository<Subscription, string>
    {
        IEnumerable<Subscription> FindByApiKeyByUserID(string user_id);
        Subscription FindByApiKeyOrgID(string org_id);
    }
}
