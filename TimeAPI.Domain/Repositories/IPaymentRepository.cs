using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IPaymentRepository : IRepository<Payment, string>
    {
        public IEnumerable<Payment> PaymentByOrgID(string OrgID);
    }
}
