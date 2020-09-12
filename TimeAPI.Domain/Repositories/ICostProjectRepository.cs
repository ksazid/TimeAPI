using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ICostProjectRepository : IRepositoryAsync<CostProject, string>
    {
        //IEnumerable<Team> FindTeamsByOrgID(string OrgID);
        //dynamic FindByTeamID(string TeamID);
        Task<IEnumerable<dynamic>> FetchAllCostProjectByOrgID(string OrgID);

        Task UpdateCostProjectStatusByID(CostProject entity);
 
        Task UpdateCostProjectDiscountAndProfitMarginByID(CostProject entity);

        Task UpdateIsQuotationByCostProjectID(CostProject entity);

        Task<dynamic> FindByCostProjectID(string CostProjectID);

        Task UpdateCostProjectFinalValueByCostProjectID(CostProject entity);

        Task<string> GetLastAddedCostPrefixByOrgID(string key);


    }
}
