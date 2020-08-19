using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IQuotationRepository : IRepository<Quotation, string>
    {
       dynamic QuotationByOrgID(string OrgID);
    }
}
