﻿using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class TimesheetRepository : RepositoryBase, ITimesheetRepository
    {
        public TimesheetRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(Timesheet entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.timesheet
                                  (id, empid, ondate, check_in, check_out, is_checkout, groupid, created_date, createdby)
                           VALUES (@id, @empid, @ondate, @check_in, @check_out, @is_checkout, @groupid, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public async Task<Timesheet> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<Timesheet>(
                sql: "SELECT * FROM dbo.timesheet WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public async Task<Timesheet> FindTimeSheetByEmpID(string empid, string groupid)
        {
            return await QuerySingleOrDefaultAsync<Timesheet>(
                sql: "SELECT * FROM dbo.timesheet WHERE is_deleted = 0 and empid = @empid and groupid = @groupid",
                param: new { empid, groupid }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void RemoveByGroupID(string GroupID)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE groupid = @GroupID",
                param: new { GroupID }
            );
        }

        public void Update(Timesheet entity)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet
                   SET
                    empid = @empid,
                    ondate = @ondate,
                    check_in = @check_in,
                    check_out = @check_out,
                    is_checkout = @is_checkout,
                    groupid = @groupid,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public async Task<IEnumerable<Timesheet>> All()
        {
            return await QueryAsync<Timesheet>(
                sql: "SELECT * FROM [dbo].[timesheet] where is_deleted = 0"
            );
        }

        public async Task CheckOutByEmpID(Timesheet entity)
        {
            await ExecuteAsync(
                  sql: @"UPDATE dbo.timesheet
                   SET
                    check_out = @check_out,
                    is_checkout = @is_checkout,
                    total_hrs = @total_hrs,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE empid = @empid and groupid = @groupid",
                  param: entity
              );
        }

        public async Task<dynamic> GetAllTimesheetByOrgID(string OrgID)
        {
            return await QueryAsync<dynamic>(
           sql: "SELECT * FROM [dbo].[timesheet] where is_deleted = 0 AND timesheet.groupid = @OrgID",
                param: new { OrgID }
            );
        }

        public async Task<IEnumerable<string>> GetAllEmpByGroupID(string OrgID)
        {
            return await QueryAsync<string>(
           sql: @"SELECT full_name FROM employee 
                INNER JOIN timesheet on employee.id = timesheet.empid
                WHERE timesheet.groupid = @OrgID",
                param: new { OrgID }
            );
        }


    }
}