using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class EntityLocationRepository : RepositoryBase, IEntityLocationRepository
    {
        public EntityLocationRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(EntityLocation entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.entity_location
                                  (id, entity_id, geo_address, formatted_address, lat, lang, street_number, route, locality, administrative_area_level_2,
                                   administrative_area_level_1, postal_code, country, created_date, createdby)
                           VALUES (@id, @entity_id, @geo_address, @formatted_address, @lat, @lang, @street_number, @route, @locality, @administrative_area_level_2,
                                   @administrative_area_level_1, @postal_code, @country, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public EntityLocation Find(string key)
        {
            return QuerySingleOrDefault<EntityLocation>(
                sql: "SELECT * FROM dbo.entity_location WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }
        public EntityLocation FindByEnitiyID(string key)
        {
            return QuerySingleOrDefault<EntityLocation>(
                sql: "SELECT * FROM dbo.entity_location WHERE is_deleted = 0 and entity_id = @key",
                param: new { key }
            );
        }
        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.entity_location
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(EntityLocation entity)
        {
            Execute(
                sql: @"UPDATE dbo.entity_location
                   SET
                    geo_address = @geo_address,
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
                    WHERE entity_id = @entity_id",
                param: entity
            );
        }

        public IEnumerable<EntityLocation> All()
        {
            return Query<EntityLocation>(
                sql: "SELECT * FROM dbo.entity_location WHERE is_deleted = 0"
            );
        }

        public void RemoveByEntityID(string key)
        {
            Execute(
                sql: @"UPDATE dbo.entity_location
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE entity_id = @key",
                param: new { key }
            );
        }
    }
}