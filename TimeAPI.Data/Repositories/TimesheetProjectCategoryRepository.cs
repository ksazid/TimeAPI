﻿using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class TimesheetProjectCategoryRepository : RepositoryBase, ITimesheetProjectCategoryRepository
    {
        public TimesheetProjectCategoryRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(TimesheetProjectCategory entity)
        {

            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.timesheet_project_category
                                  (id, timesheet_id, groupid, project_category_type_id, system_id, created_date, createdby)
                           VALUES (@id, @timesheet_id, @groupid, @project_category_type_id, @system_id, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public TimesheetProjectCategory Find(string key)
        {
            return QuerySingleOrDefault<TimesheetProjectCategory>(
                sql: "SELECT * FROM dbo.timesheet_project_category WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_project_category
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void RemoveByGroupID(string GroupID)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_project_category
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE groupid = @GroupID",
                param: new { GroupID }
            );
        }

        public void Update(TimesheetProjectCategory entity)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_project_category
                   SET 
                    timesheet_id = @timesheet_id,
                    groupid = @groupid,
                    project_category_type_id = @project_category_type_id,
                    system_id = @system_id,
                    modified_date = @modified_date, 
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<TimesheetProjectCategory> All()
        {
            return Query<TimesheetProjectCategory>(
                sql: "SELECT * FROM [dbo].[timesheet_project_category] where is_deleted = 0"
            );
        }
     
    }
}
