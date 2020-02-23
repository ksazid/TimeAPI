using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class DelegationsRepository : RepositoryBase, IDelegationsRepository
    {
        public DelegationsRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(Delegations entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.delegations
                                  (id, org_id, delegator, is_type_temporary, is_type_permanent, is_notify_delegator_and_delegatee, 
                                    is_notify_delegatee, delegations_desc, created_date, createdby)
                           VALUES (@id, @org_id, @delegator, @is_type_temporary, @is_type_permanent, @is_notify_delegator_and_delegatee, 
                                    @is_notify_delegatee, @delegations_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Delegations Find(string key)
        {
            return QuerySingleOrDefault<Delegations>(
                sql: "SELECT * FROM dbo.delegations WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.delegations
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Delegations entity)
        {
            Execute(
                sql: @"UPDATE dbo.delegations
                   SET
                    org_id = @org_id,
                    delegator = @delegator, 
                    is_type_temporary = @is_type_temporary, 
                    is_type_permanent = @is_type_permanent, 
                    is_notify_delegator_and_delegatee = @is_notify_delegator_and_delegatee, 
                    is_notify_delegatee = @is_notify_delegatee, 
                    delegations_desc = @delegations_desc,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<Delegations> All()
        {
            return Query<Delegations>(
                sql: "SELECT * FROM dbo.delegations where is_deleted = 0"
            );
        }

        //public IEnumerable<Delegations> FindBillingsByOrgID(string key)
        //{
        //    return Query<Delegations>(
        //        sql: "SELECT * FROM dbo.delegations WHERE is_deleted = 0 and org_id = @key",
        //        param: new { key }
        //    );
        //}
    }
}