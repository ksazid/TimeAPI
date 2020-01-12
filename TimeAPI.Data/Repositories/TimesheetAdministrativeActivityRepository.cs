﻿using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class TimesheetAdministrativeActivityRepository : RepositoryBase, ITimesheetAdministrativeRepository
    {
        public TimesheetAdministrativeActivityRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(TimesheetAdministrative entity)
        {

            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.timesheet_administrative_activity
                                  (id, administrative_id, groupid, purpose, remarks, ondate, start_time, end_time, created_date, createdby)
                           VALUES (@id, @administrative_id, @groupid, @purpose, @remarks, @ondate, @start_time, @end_time, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public TimesheetAdministrative Find(string key)
        {
            return QuerySingleOrDefault<TimesheetAdministrative>(
                sql: "SELECT * FROM dbo.timesheet_administrative_activity WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_administrative_activity
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void RemoveByGroupID(string GroupID)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_administrative_activity
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE groupid = @GroupID",
                param: new { GroupID }
            );
        }

        public void Update(TimesheetAdministrative entity)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_administrative_activity
                   SET 
                    administrative_id = @administrative_id,
                    groupid = @groupid,
                    purpose = @purpose, 
                    remarks = @remarks, 
                    ondate = @ondate, 
                    start_time = @start_time, 
                    end_time = @end_time,
                    modified_date = @modified_date, 
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<TimesheetAdministrative> All()
        {
            return Query<TimesheetAdministrative>(
                sql: "SELECT * FROM [dbo].[timesheet_administrative_activity] where is_deleted = 0"
            );
        }
     
    }
}