using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class LocationExceptionRepository : RepositoryBase, ILocationExceptionRepository
    {
        public LocationExceptionRepository(IDbTransaction transaction) : base(transaction)
        { }
        public void Add(LocationException entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.location_exception
                                  (id, group_id, checkin_lat, checkin_lang, is_chkin_inrange, created_date, createdby)
                           VALUES (@id, @group_id, @checkin_lat, @checkin_lang, @is_chkin_inrange, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public LocationException Find(string key)
        {
            return QuerySingleOrDefault<LocationException>(
                sql: "SELECT * FROM dbo.location_exception WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public LocationException FindByGroupID(string key)
        {
            return QuerySingleOrDefault<LocationException>(
                sql: "SELECT * FROM dbo.location_exception WHERE is_deleted = 0 and group_id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.location_exception
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void RemoveByGroupID(string key)
        {
            Execute(
                sql: @"UPDATE dbo.location_exception
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE group_id = @key",
                param: new { key }
            );
        }

        public void Update(LocationException entity)
        {
            Execute(
                sql: @"UPDATE dbo.location_exception
                   SET
                    group_id = @group_id, 
                    checkout_lat = @checkout_lat,
                    checkout_lang = @checkout_lang,
                    is_chkout_inrange = @is_chkout_inrange,
                    is_checkout = @is_checkout,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE group_id = @group_id",
                param: entity
            );
        }

        public IEnumerable<LocationException> All()
        {
            return Query<LocationException>(
                sql: "SELECT * FROM dbo.location_exception where is_deleted = 0"
            );
        }
    }
}