using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class LocationRepository : RepositoryBase, ILocationRepository
    {
        public LocationRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(Location entity)
        {

            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.location
                                  (id, groupid, formatted_address, lat, lang, street_number, route, locality, administrative_area_level_2, 
                                   administrative_area_level_1, postal_code, country, created_date, createdby, is_checkout)
                           VALUES (@id, @groupid, @formatted_address, @lat, @lang, @street_number, @route, @locality, @administrative_area_level_2, 
                                   @administrative_area_level_1, @postal_code, @country, @created_date, @createdby, @is_checkout);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Location Find(string key)
        {
            return QuerySingleOrDefault<Location>(
                sql: "SELECT * FROM dbo.location WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.location
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        } 

        //public void RemoveByGroupID(string GroupID)
        //{
        //    Execute(
        //        sql: @"UPDATE dbo.location
        //           SET
        //               modified_date = GETDATE(), is_deleted = 1
        //            WHERE groupid = @GroupID",
        //        param: new { GroupID }
        //    );
        //}

        public void Update(Location entity)
        {
            Execute(
                sql: @"UPDATE dbo.location
                   SET 
                    formatted_address = @formatted_address, 
                    lat = @lat, 
                    lang = @lang, 
                    street_number = @street_number, 
                    route = @route, 
                    locality = @locality, 
                    administrative_area_level_2 = @administrative_area_level_2, 
                    administrative_area_level_1 = @administrative_area_level_1,
                    postal_code = @postal_code, 
                    country = @country,
                    modified_date = @modified_date, 
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<Location> All()
        {
            return Query<Location>(
                sql: "SELECT * FROM [dbo].[location] where is_deleted = 0"
            );
        }

    }
}
