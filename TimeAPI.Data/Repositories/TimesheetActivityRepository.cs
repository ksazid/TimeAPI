﻿using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class TimesheetActivityRepository : RepositoryBase, ITimesheetActivityRepository
    {
        public TimesheetActivityRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(TimesheetActivity entity)
        {

            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.timesheet_activity
                                  (id, groupid, task_id, subtask_id, remarks, ondate, start_time, end_time, created_date, createdby)
                           VALUES (@id, @groupid, @task_id, @subtask_id, @remarks, @ondate, @start_time, @end_time, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public TimesheetActivity Find(string key)
        {
            return QuerySingleOrDefault<TimesheetActivity>(
                sql: "SELECT * FROM dbo.timesheet_activity WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        //public TimesheetActivity FindTimeSheetByEmpID(string empid, string groupid)
        //{
        //    return QuerySingleOrDefault<TimesheetActivity>(
        //        sql: "SELECT * FROM dbo.timesheet_activity WHERE is_deleted = 0 and empid = @empid and groupid = @groupid",
        //        param: new { empid, groupid }
        //    );
        //}

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_activity
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        //public void RemoveByGroupID(string GroupID)
        //{
        //    Execute(
        //        sql: @"UPDATE dbo.timesheet_activity
        //           SET
        //               modified_date = GETDATE(), is_deleted = 1
        //            WHERE groupid = @GroupID",
        //        param: new { GroupID }
        //    );
        //}

        public void Update(TimesheetActivity entity)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet_activity
                   SET 
                    groupid = @groupid, 
                    task_id = @task_id, 
                    subtask_id = @subtask_id, 
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

        public IEnumerable<TimesheetActivity> All()
        {
            return Query<TimesheetActivity>(
                sql: "SELECT * FROM [dbo].[timesheet_activity] where is_deleted = 0"
            );
        }
        //public void CheckOutByEmpID(TimesheetActivity entity)
        //{
        //    Execute(
        //         sql: @"UPDATE dbo.timesheet_activity
        //           SET 
        //            check_out = @check_out,
        //            is_checkout = @is_checkout,
        //            total_hrs = @total_hrs,
        //            modified_date = @modified_date, 
        //            modifiedby = @modifiedby
        //            WHERE empid = @empid and groupid = @groupid",
        //         param: entity 
        //     );
        //}

    }
}
