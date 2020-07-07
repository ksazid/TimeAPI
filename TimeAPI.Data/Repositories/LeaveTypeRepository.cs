using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class LeaveTypeRepository : RepositoryBase, ILeaveTypeRepository
    {
        public LeaveTypeRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(LeaveType entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.leave_type
                            (id, org_id, leave_type_name,  created_date, createdby)
                    VALUES (@id, @org_id, @leave_type_name, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public LeaveType Find(string key)
        {
            return QuerySingleOrDefault<LeaveType>(
                sql: "SELECT * FROM dbo.leave_type WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<LeaveType> FetchLeaveTypeOrgID(string key)
        {
            return Query<LeaveType>(
                sql: "SELECT * FROM dbo.leave_type WHERE org_id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<LeaveType> All()
        {
            return Query<LeaveType>(
                sql: "SELECT * FROM dbo.leave_type where is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.leave_type
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }
        public void Update(LeaveType entity)
        {
            Execute(
                sql: @"UPDATE dbo.leave_type
                           SET 
                            org_id = @org_id, 
                            leave_type_name = @leave_type_name,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}