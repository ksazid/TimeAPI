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
                                  (id, org_id, delegation_name, delegator, delegations_desc,  created_date, createdby)
                           VALUES (@id, @org_id, @delegation_name, @delegator, @delegations_desc, @created_date, @createdby);
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

        public dynamic FindByDelegateesID(string key)
        {
            return QuerySingleOrDefault<dynamic>(
                sql: @"SELECT top 1 delegations_x_delegatee.delegatee_id as empid,  
                        dbo.delegations.*, 
                        delegations_x_delegatee.is_type_permanent,
                        delegations_x_delegatee.is_notify_delegator_and_delegatee,
                        delegations_x_delegatee.is_notify_delegatee,
                        delegations_x_delegatee.expires_on
                        FROM dbo.delegations 
                        INNER JOIN  dbo.delegations_x_delegatee on dbo.delegations.id = delegations_x_delegatee.delegator_id
                        WHERE delegations_x_delegatee.is_deleted = 0 and delegations_x_delegatee.delegatee_id =  @key",
                param: new { key }
            );
        }

        public void RemoveByDelegateesID(string key)
        {
            Execute(
                sql: @"UPDATE dbo.delegations_x_delegatee
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE delegatee_id = @key",
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
                    delegation_name = @delegation_name, 
                    delegator = @delegator,
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

        public dynamic GetAllDelegateeByOrgIDAndEmpID(string OrgID, string EmpID)
        {
            return Query<dynamic>(
                sql: @"SELECT 
                            [dbo].[delegations].id as delegationsid,
                            [dbo].[employee].id as empid,
                            [dbo].[employee].full_name, 
	                        [dbo].[delegations_x_delegatee].is_type_temporary, 
	                        [dbo].[delegations_x_delegatee].is_type_permanent ,
	                        [dbo].[delegations_x_delegatee].expires_on,
	                        [dbo].[delegations].delegations_desc
	                    FROM [dbo].[delegations]
	                    INNER JOIN [dbo].[delegations_x_delegatee] ON [dbo].[delegations].id = [dbo].[delegations_x_delegatee].delegator_id
	                    INNER JOIN [dbo].[employee] on [dbo].[delegations_x_delegatee].delegatee_id = [dbo].[employee].id
	                    WHERE [dbo].[delegations].org_id  = @OrgID
	                AND  [dbo].[delegations].delegator = @EmpID
	                AND  [dbo].[delegations_x_delegatee].is_deleted = 0
	                AND  [dbo].[delegations].is_deleted = 0",
                param: new { OrgID, EmpID }
            );
        }
    }
}