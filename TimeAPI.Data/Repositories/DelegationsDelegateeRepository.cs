using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class DelegationsDelegateeRepository : RepositoryBase, IDelegationsDelegateeRepository
    {
        public DelegationsDelegateeRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(DelegationsDelegatee entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO delegations_x_delegatee
                                  (id, delegator_id, delegatee_id, is_type_temporary, expires_on, is_type_permanent, is_notify_delegator_and_delegatee, 
                                    is_notify_delegatee, created_date, createdby)
                           VALUES (@id, @delegator_id, @delegatee_id, @is_type_temporary, @expires_on, @is_type_permanent, @is_notify_delegator_and_delegatee, 
                                    @is_notify_delegatee, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public DelegationsDelegatee Find(string key)
        {
            return QuerySingleOrDefault<DelegationsDelegatee>(
                sql: "SELECT * FROM delegations_x_delegatee WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE delegations_x_delegatee
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void RemoveByDelegator(string key)
        {
            Execute(
                sql: @"UPDATE delegations_x_delegatee
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE delegator_id = @key",
                param: new { key }
            );
        }

        public void RemoveByDelegateeID(string key)
        {
            Execute(
                sql: @"UPDATE delegations_x_delegatee
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE delegatee_id = @key",
                param: new { key }
            );
        }

        public void Update(DelegationsDelegatee entity)
        {
            Execute(
                sql: @"UPDATE delegations_x_delegatee
                   SET
                    is_type_temporary = @is_type_temporary, 
                    expires_on = @expires_on, 
                    is_type_permanent = @is_type_permanent, 
                    is_notify_delegator_and_delegatee = @is_notify_delegator_and_delegatee, 
                    is_notify_delegatee = @is_notify_delegatee,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE delegatee_id = @delegatee_id AND delegator_id = @delegator_id",
                param: entity
            );
        }

        public IEnumerable<DelegationsDelegatee> All()
        {
            return Query<DelegationsDelegatee>(
                sql: "SELECT * FROM delegations_x_delegatee where is_deleted = 0"
            );
        }
 
    }
}