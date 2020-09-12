using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface ITypeOfDesignRepository : IRepositoryAsync<TypeOfDesign, string>
    {
        Task<IEnumerable<TypeOfDesign>> FetchAllTypeOfDesignByProjectID(string ProjectID);
        Task<IEnumerable<TypeOfDesign>> FetchAllTypeOfDesignByOrgID(string ProjectID);
    }
}
