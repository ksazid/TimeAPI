using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ILeadRatingRepository : IRepository<LeadRating, string>
    {
        public IEnumerable<LeadRating> LeadRatingByOrgID(string OrgID);
    }
}
