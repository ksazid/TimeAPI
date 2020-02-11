using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class TimesheetLocationRepository : RepositoryBase, ITimesheetLocationRepository
    {
        public TimesheetLocationRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(TimesheetLocation entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.timesheet_location
                                  (id,  groupid, manual_address, geo_address, formatted_address, lat, lang, street_number, route, locality, administrative_area_level_2,
                                   administrative_area_level_1, postal_code, country, is_office, is_manual, created_date, createdby)
                           VALUES (@id, @groupid, @manual_address, @geo_address, @formatted_address, @lat, @lang, @street_number, @route, @locality, @administrative_area_level_2,
                                   @administrative_area_level_1, @postal_code, @country, @is_office, @is_manual, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public TimesheetLocation Find(string key)
        {
            return QuerySingleOrDefault<TimesheetLocation>(
                sql: "SELECT * FROM dbo.timesheet_location WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_location
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void RemoveByGroupID(string GroupID)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_location
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE groupid = @GroupID",
                param: new { GroupID }
            );
        }

        public void Update(TimesheetLocation entity)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_location
                   SET
                    groupid = @groupid,
                    manual_address = @manual_address,
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
                    is_office = @is_office,
                    is_manual = @is_manual,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<TimesheetLocation> All()
        {
            return Query<TimesheetLocation>(
                sql: "SELECT * FROM [dbo].[timesheet_location] where is_deleted = 0"
            );
        }
    }
}