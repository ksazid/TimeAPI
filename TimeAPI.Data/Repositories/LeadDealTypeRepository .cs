using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class LeadDealTypeRepository  : RepositoryBase, ILeadDealTypeRepository 
    {
        public LeadDealTypeRepository (IDbTransaction transaction) : base(transaction)
        { }

        public void Add(LeadDealType entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.lead_deal_type
                                  (id, org_id, deal_type_name, deal_type_desc, created_date, createdby)
                           VALUES (@id, @org_id, @deal_type_name, @deal_type_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public LeadDealType Find(string key)
        {
            return QuerySingleOrDefault<LeadDealType>(
                sql: "SELECT * FROM dbo.lead_deal_type WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.lead_deal_type
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(LeadDealType entity)
        {
            Execute(
                sql: @"UPDATE dbo.lead_deal_type
                   SET
                    org_id = @org_id,
                    deal_type_name = @deal_type_name,
                    deal_type_desc = @deal_type_desc,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<LeadDealType> All()
        {
            return Query<LeadDealType>(
                sql: "SELECT * FROM [dbo].[lead_deal_type] where is_deleted = 0"
            );
        }

        public IEnumerable<LeadDealType> GetLeadDealTypeByOrgID(string key)
        {
            return Query<LeadDealType>(
                sql: @"SELECT * FROM [dbo].[lead_deal_type]
                        WHERE is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }
    }
}