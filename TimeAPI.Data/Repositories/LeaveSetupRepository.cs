using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class LeaveSetupRepository : RepositoryBase, ILeaveSetupRepository
    {
        public LeaveSetupRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(LeaveSetup entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.leave_setup
                            (id, org_id, leave_name, leave_type_id, max_leave_days, created_date, createdby)
                    VALUES (@id, @org_id, @leave_name, @leave_type_id, @max_leave_days, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public LeaveSetup Find(string key)
        {
            return QuerySingleOrDefault<LeaveSetup>(
                sql: "SELECT * FROM dbo.leave_setup WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<LeaveSetup> FetchLeaveSetupOrgID(string key)
        {
            return Query<LeaveSetup>(
                sql: "SELECT * FROM dbo.leave_setup WHERE org_id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<LeaveSetup> All()
        {
            return Query<LeaveSetup>(
                sql: "SELECT * FROM dbo.leave_setup where is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.leave_setup
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }
        public void Update(LeaveSetup entity)
        {
            Execute(
                sql: @"UPDATE dbo.leave_setup
                           SET 
                            org_id = @org_id, 
                            leave_name = @leave_name, 
                            leave_type_id = @leave_type_id, 
                            max_leave_days = @max_leave_days,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}