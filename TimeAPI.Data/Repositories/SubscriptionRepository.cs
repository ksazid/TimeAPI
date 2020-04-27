using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    internal class SubscriptionRepository : RepositoryBase, ISubscriptionRepository
    {
        public SubscriptionRepository(IDbTransaction transaction) : base(transaction)
        {
        }

        public void Add(Subscription entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO [dbo].[subscription]
                                   (id, user_id, api_key, current_plan_id, subscription_start_date, subscription_end_date,
                                    on_date_subscribed, offer_id, offer_start_date, offer_end_date, is_trial,
                                    is_subscibe_after_trial, is_active, created_date, createdby)
                           VALUES (@id, @user_id, @api_key, @current_plan_id, @subscription_start_date, @subscription_end_date,
                                    @on_date_subscribed, @offer_id, @offer_start_date, @offer_end_date, @is_trial,
                                    @is_subscibe_after_trial, @is_active, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Subscription Find(string key)
        {
            return QuerySingleOrDefault<Subscription>(
                sql: "SELECT * FROM dbo.subscription WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.subscription
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Subscription entity)
        {
            Execute(
                sql: @"UPDATE dbo.subscription
                   SET
                        current_plan_id = @current_plan_id,
                        subscription_start_date = @subscription_start_date,
                        subscription_end_date = @subscription_end_date,
                        on_date_subscribed = @on_date_subscribed,
                        offer_id = @offer_id,
                        offer_start_date = @offer_start_date,
                        offer_end_date = @offer_end_date,
                        is_trial  = @is_trial,
                        is_subscibe_after_trial = @is_subscibe_after_trial,
                        is_active = @is_active,
                        modified_date = @modified_date,
                        modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<Subscription> All()
        {
            return Query<Subscription>(
                sql: "SELECT * FROM [dbo].[subscription] WHERE is_deleted = 0"
            );
        }

        public IEnumerable<Subscription> FindByApiKeyByUserID(string user_id)
        {
            return Query<Subscription>(
                sql: "SELECT * FROM dbo.subscription WHERE user_id = @user_id and is_deleted = 0",
                param: new { user_id }
            );
        }

        public Subscription GetByApiKeyByUserID(string user_id)
        {
            Subscription _Subscription = new Subscription();
            var result = QuerySingleOrDefault<Subscription>(
                sql: "SELECT * FROM dbo.subscription WHERE user_id = @user_id and is_deleted = 0",
                param: new { user_id }
                );
            _Subscription = result;

            if (result == null)
            {
                var xresult =  QuerySingleOrDefault<Subscription>(
                    sql: @"SELECT subscription.* FROM subscription
                            INNER JOIN organization on  subscription.api_key = organization.subscription_key
                            INNER JOIN employee on  organization.org_id = employee.org_id
                            WHERE employee.user_id = @user_id
                            AND subscription.is_deleted = 0
                            AND organization.is_deleted = 0
                            AND employee.is_deleted = 0",
                    param: new { user_id }
                    );
                _Subscription = xresult;
            }

            return _Subscription;
        }

        //public Subscription FindByApiKeyOrgID(string org_id)
        //{
        //    return QuerySingleOrDefault<Subscription>(
        //        sql: "SELECT * FROM dbo.subscription WHERE org_id = @org_id and is_deleted = 0",
        //        param: new { org_id }
        //    );
        //}
    }
}