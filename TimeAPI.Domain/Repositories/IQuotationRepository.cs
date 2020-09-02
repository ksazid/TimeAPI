using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IQuotationRepository : IRepository<Quotation, string>
    {
        dynamic QuotationByOrgID(string OrgID);
        dynamic FindByQuotationID(string QuotationID);
        void UpdateQuotationStageByQuotationID(Quotation entity);

        string GetLastAddedQuotationPrefixByOrgID(string key);

    }
}
