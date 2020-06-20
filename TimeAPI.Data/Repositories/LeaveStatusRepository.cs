using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class LeaveStatusRepository : RepositoryBase, ILeaveStatusRepository
    {
        public LeaveStatusRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(LeaveStatus entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.leave_status
                                  (id, org_id, leave_status_name, leave_status_desc, created_date, createdby)
                           VALUES (@id, @org_id, @leave_status_name, @leave_status_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public LeaveStatus Find(string key)
        {
            return QuerySingleOrDefault<LeaveStatus>(
                sql: "SELECT * FROM dbo.leave_status WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.leave_status
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(LeaveStatus entity)
        {
            Execute(
                sql: @"UPDATE dbo.leave_status
                   SET
                    org_id = @org_id,
                    leave_status_name = @leave_status_name,
                    leave_status_desc = @leave_status_desc,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<LeaveStatus> All()
        {
            return Query<LeaveStatus>(
                sql: "SELECT * FROM [dbo].[leave_status] where is_deleted = 0"
            );
        }

        public IEnumerable<LeaveStatus> GetLeaveStatusByOrgID(string key)
        {
            return Query<LeaveStatus>(
                sql: @"SELECT * FROM [dbo].[leave_status]
                        WHERE is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }
    }
}