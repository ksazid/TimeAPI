using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class EntityInvitationRepository : RepositoryBase, IEntityInvitationRepository
    {
        public EntityInvitationRepository(IDbTransaction transaction) : base(transaction)
        { }
        public void Add(EntityInvitation entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.entity_invitation
                                  (id, entity_id, org_id, emp_id, role_id, email, created_date, createdby)
                           VALUES (@id, @entity_id, @org_id, @emp_id, @role_id, @email, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public EntityInvitation Find(string key)
        {
            return QuerySingleOrDefault<EntityInvitation>(
                sql: "SELECT * FROM dbo.entity_invitation WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.entity_invitation
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void RemoveByEntityID(string key)
        {
            Execute(
                sql: @"UPDATE dbo.entity_invitation
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE entity_id = @entity_id",
                param: new { key }
            );
        }

        public void Update(EntityInvitation entity)
        {
            Execute(
                sql: @"UPDATE dbo.entity_invitation
                   SET
                    org_id = @org_id, 
                    emp_id = @emp_id, 
                    role_id = @role_id, 
                    email = @email,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE entity_id = @entity_id",
                param: entity
            );
        }

        public IEnumerable<EntityInvitation> All()
        {
            return Query<EntityInvitation>(
                sql: "SELECT * FROM dbo.entity_invitation where is_deleted = 0"
            );
        }
    }
}