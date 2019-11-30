﻿using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;
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

        public Timesheet Find(string key)
        {
            return QuerySingleOrDefault<Timesheet>(
                sql: "SELECT * FROM dbo.timesheet WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.timesheet
                   SET
                       modified_date = @modified_date, modifiedby = @modifiedby, is_deleted = 1
                    WHERE id = @key",
                param: new { key }
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
                    modifiedby = @modifiedby, 
                    is_deleted = @is_deleted",
                param: entity
            );
        }

        public IEnumerable<Timesheet> All()
        {
            return Query<Timesheet>(
                sql: "SELECT * FROM [dbo].[timesheet] where is_deleted = 0"
            );
        }

        public Timesheet CheckOutByEmpID(string key)
        {
            return QuerySingleOrDefault<Timesheet>(
                 sql: "SELECT * FROM dbo.timesheet WHERE is_deleted = 0 and empid = @key",
                 param: new { key }
             );
        }
    }
}