using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class SetupRepository : RepositoryBase, ISetupRepository
    {
        public SetupRepository(IDbTransaction transaction) : base(transaction)
        { }

        public IEnumerable<Country> Country()
        {
            return Query<Country>(
                sql: "SELECT * FROM countries"
            );
        }

        public IEnumerable<Timezones> Timezones()
        {
            return Query<Timezones>(
                sql: "SELECT * FROM timezones"
            );
        }

        public IEnumerable<Country> PhoneCodes()
        {
            return Query<Country>(
                sql: "SELECT * FROM countries"
            );
        }
    }
}