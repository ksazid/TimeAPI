using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;


namespace TimeAPI.Data.Repositories
{
    class SubscriptionRepository : RepositoryBase, ISubscriptionRepository
    {
        public SubscriptionRepository(IDbTransaction transaction) : base(transaction)
        {

        }

        public void Add(Subscription entity)
        {

            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO [dbo].[subscription]
                                   (id, user_id, org_id, api_key, subscription_start_date, subscription_end_date, on_date_subscribed,
		                            current_plan_id, offer_id, offer_start_date, offer_end_date, is_trial, is_subscibe_after_trial,
			                        is_active, created_date, createdby, modified_date, modifiedby, is_deleted)
                           VALUES (@id, @user_id, @org_id, @api_key, @subscription_start_date, @subscription_end_date, @on_date_subscribed,
		                           @current_plan_id, @offer_id, @offer_start_date, @offer_end_date, @is_trial, @is_subscibe_after_trial,
			                       @is_active, @created_date, @createdby, @modified_date, @modifiedby, @is_deleted);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Subscription Find(string key)
        {
            return QuerySingleOrDefault<Subscription>(
                sql: "SELECT * FROM dbo.subscription WHERE id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.subscription
                   SET
                       modified_date = @modified_date, modifiedby = @modifiedby, is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Subscription entity)
        {
            Execute(
                sql: @"UPDATE dbo.subscription
                   SET 
                        id = @id, 
                        user_id = @user_id, 
                        img_name = @img_name, 
                        img_url = @img_url, 
                        created_date = @created_date, 
                        createdby = @createdby, 
                        modified_date = @modified_date, 
                        modifiedby = @modifiedby, 
                        is_deleted = @is_deleted
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<Subscription> All()
        {
            return Query<Subscription>(
                sql: "SELECT * FROM [dbo].[image]"
            );
        }

        public IEnumerable<Subscription> FindByApiKeyByUserID(string user_id)
        {
            return Query<Subscription>(
                sql: "SELECT * FROM dbo.reporting WHERE id = @key",
                param: new { user_id }
            );
        }

        public Subscription FindByApiKeyOrgID(string org_id)
        {
            return QuerySingleOrDefault<Subscription>(
                sql: "SELECT * FROM dbo.reporting WHERE id = @key",
                param: new { org_id }
            );
        }
    }
}
