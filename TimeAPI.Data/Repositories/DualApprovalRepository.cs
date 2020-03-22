using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class DualApprovalRepository : RepositoryBase, IDualApprovalRepository
    {
        public DualApprovalRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(DualApproval entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.dual_approval
                                  (id, entity_id, approver1_empid,approver2_empid, created_date, createdby)
                           VALUES (@id, @entity_id, @approver1_empid, @approver2_empid, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public DualApproval Find(string key)
        {
            return QuerySingleOrDefault<DualApproval>(
                sql: "SELECT * FROM dbo.dual_approval WHERE is_deleted = 0 and entity_id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.dual_approval
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE Find = @key",
                param: new { key }
            );
        }

        public void RemoveByEntityID(string key)
        {
            Execute(
                sql: @"UPDATE dbo.dual_approval
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE entity_id = @entity_id",
                param: new { key }
            );
        }

        public void Update(DualApproval entity)
        {
            Execute(
                sql: @"UPDATE dbo.dual_approval
                   SET
                    entity_id = @entity_id, 
                    approver1_empid = @approver1_empid, 
                    approver2_empid = @approver2_empid,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE entity_id = @entity_id",
                param: entity
            );
        }

        public IEnumerable<DualApproval> All()
        {
            return Query<DualApproval>(
                sql: "SELECT * FROM dbo.dual_approval where is_deleted = 0"
            );
        }
    }
}