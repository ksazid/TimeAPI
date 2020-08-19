using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class LeadDealRepository : RepositoryBase, ILeadDealRepository
    {
        public LeadDealRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public void Add(LeadDeal entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.lead_deal
                            (id, lead_id, deal_prefix, deal_name, deal_type_id, est_amount, stage_id, contact_role_id, est_closing_date, created_date, createdby)
                    VALUES (@id, @lead_id, @deal_prefix, @deal_name, @deal_type_id, @est_amount, @stage_id, @contact_role_id, @est_closing_date, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public LeadDeal Find(string key)
        {
            return QuerySingleOrDefault<LeadDeal>(
                sql: "SELECT * FROM dbo.lead_deal WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<LeadDeal> All()
        {
            return Query<LeadDeal>(
                sql: "SELECT * FROM dbo.lead_deal where is_deleted = 0"
            );
        }

        public IEnumerable<LeadDeal> LeadDealByOrgID(string key)
        {
            return Query<LeadDeal>(
                sql: "SELECT * FROM dbo.lead_deal where is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }

        public LeadDeal LeadDealByLeadID(string key)
        {
            return QuerySingleOrDefault<LeadDeal>(
                sql: "SELECT * FROM dbo.lead_deal where is_deleted = 0 and lead_id = @key",
                param: new { key }
            );
        }

        
        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.lead_deal
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(LeadDeal entity)
        {
            Execute(
                sql: @"UPDATE dbo.lead_deal
                           SET 
                            lead_id = @lead_id, 
                            deal_prefix = @deal_prefix, 
                            deal_name = @deal_name, 
                            deal_type_id = @deal_type_id, 
                            est_amount = @est_amount,
                            stage_id = @stage_id,
                            contact_role_id = @contact_role_id,
                            est_closing_date = @est_closing_date,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }

        public void UpdateEstDealValueByLeadID(LeadDeal entity)
        {
            Execute(
                sql: @"UPDATE dbo.lead_deal
                           SET 
                            est_amount = @est_amount,
                            remarks = @remarks,
                            is_manual = @is_manual,
                            basic_cost = @basic_cost,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE lead_id = @lead_id",
                param: entity
            );
        }
    }
}