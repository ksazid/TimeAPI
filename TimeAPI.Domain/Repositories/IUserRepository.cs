using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.Domain.Repositories
{
    public interface IUserRepository : IRepository<User, string>
    {
        User FindByNormalizedUserName(string normalizedUserName);
        User FindByNormalizedEmail(string normalizedEmail);
        //void CustomEmailConfirmedFlagUpdate(string UserID);
        UserDataGroupDataSet GetUserDataGroupByUserID(string EmpID, string Date);
    }
}
