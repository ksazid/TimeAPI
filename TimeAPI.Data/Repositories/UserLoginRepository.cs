using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    internal class UserLoginRepository : RepositoryBase, IUserLoginRepository
    {
        public UserLoginRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(UserLogin entity)
        {
            Execute(
                sql: @"
                    INSERT INTO AspNetUserLogins(LoginProvider, ProviderKey, ProviderDisplayName, UserId)
                    VALUES(@LoginProvider, @ProviderKey, @ProviderDisplayName, @UserId)",
                param: entity
            );
        }

        public IEnumerable<UserLogin> All()
        {
            return Query<UserLogin>(
                sql: "SELECT * FROM AspNetUserLogins"
            );
        }

        public UserLogin Find(UserLoginKey id)
        {
            return QuerySingleOrDefault<UserLogin>(
                sql: @"
                    SELECT * FROM AspNetUserLogins
                    WHERE LoginProvider = @LoginProvider AND ProviderKey = @ProviderKey",
                param: id
            );
        }

        public IEnumerable<UserLogin> FindByUserId(string userId)
        {
            return Query<UserLogin>(
                sql: "SELECT * FROM AspNetUserLogins WHERE UserId = @userId",
                param: new { userId }
            );
        }

        public void Remove(UserLoginKey key)
        {
            Execute(
                sql: @"
                    DELETE FROM AspNetUserLogins
                    WHERE LoginProvider = @LoginProvider AND ProviderKey = @ProviderKey",
                param: key
            );
        }

        public void Update(UserLogin entity)
        {
            Execute(
                sql: @"
                    UPDATE AspNetUserLogins SET ProviderDisplayName = @ProviderDisplayName,
                        UserId = @UserId
                    WHERE LoginProvider = @LoginProvider AND ProviderKey = @ProviderKey",
                param: entity
            );
        }
    }
}