using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ICostProjectRepository : IRepository<CostProject, string>
    {
        //IEnumerable<Team> FindTeamsByOrgID(string OrgID);
        //dynamic FindByTeamID(string TeamID);
        IEnumerable<dynamic> FetchAllCostProjectByOrgID(string OrgID);

        void UpdateCostProjectStatusByID(CostProject entity);

        //CostProject FindAutoCostProjectPrefixByOrgID(string key, string key1);
        //CostProject FindCustomCostProjectPrefixByOrgIDAndPrefix(string key, string key1);
        //IEnumerable<dynamic> FindAllCostProjectActivityByCostProjectID(string ProjectID);
        //string ProjectActivityCount(string key);
        //string CostProjectTaskCount(string key);
        void UpdateCostProjectDiscountAndProfitMarginByID(CostProject entity);
        void UpdateIsQuotationByCostProjectID(CostProject entity);
        dynamic FindByCostProjectID(string CostProjectID);

    }
}
