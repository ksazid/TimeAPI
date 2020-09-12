using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IQuotationRepository : IRepositoryAsync<Quotation, string>
    {
        Task<dynamic> QuotationByOrgID(string OrgID);
        Task<dynamic> FindByQuotationID(string QuotationID);
        Task UpdateQuotationStageByQuotationID(Quotation entity);
        Task<string> GetLastAddedQuotationPrefixByOrgID(string key);
    }
}
