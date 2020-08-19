using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IPaymentModeRepository : IRepository<PaymentMode, string>
    {
        public IEnumerable<PaymentMode> PaymentModeByOrgID(string OrgID);
    }
}
