using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class LeadContractRoleRepository : RepositoryBase, ILeadContractRoleRepository
    {
        public LeadContractRoleRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(LeadContractRole entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.lead_contact_role
                                  (id, org_id, contact_role_name, contact_role_desc, created_date, createdby)
                           VALUES (@id, @org_id, @contact_role_name, @contact_role_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public LeadContractRole Find(string key)
        {
            return QuerySingleOrDefault<LeadContractRole>(
                sql: "SELECT * FROM dbo.lead_contact_role WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.lead_contact_role
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(LeadContractRole entity)
        {
            Execute(
                sql: @"UPDATE dbo.lead_contact_role
                   SET
                    org_id = @org_id,
                    contact_role_name = @contact_role_name,
                    contact_role_desc = @contact_role_desc,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<LeadContractRole> All()
        {
            return Query<LeadContractRole>(
                sql: "SELECT * FROM [dbo].[lead_contact_role] where is_deleted = 0"
            );
        }

        public IEnumerable<LeadContractRole> GetLeadContractRoleByOrgID(string key)
        {
            return Query<LeadContractRole>(
                sql: @"SELECT * FROM [dbo].[lead_contact_role]
                        WHERE is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }
    }
}