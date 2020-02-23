using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class BillingRepository : RepositoryBase, IBillingRepository
    {
        public BillingRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(Billing entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.saas_billing
                                  (id, user_id, user_email, current_plan_id, billing_cycle, total_user, total_cost, first_name, last_name, 
                                    card_no, expire_month, expire_year, cvv, adr1, zip, state, country, created_date createdby)
                           VALUES (@id, @user_id, @user_email, @current_plan_id, @billing_cycle, @total_user, @total_cost, @first_name, @last_name,
                                    @card_no, @expire_month, @expire_year, @cvv, @adr1, @zip, @state, @country, @created_date @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Billing Find(string key)
        {
            return QuerySingleOrDefault<Billing>(
                sql: "SELECT * FROM dbo.saas_billing WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.saas_billing
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Billing entity)
        {
            Execute(
                sql: @"UPDATE dbo.saas_billing
                   SET
                    user_id = @user_id, 
                    user_email = @user_email, 
                    current_plan_id = @current_plan_id, 
                    billing_cycle = @billing_cycle, 
                    total_user = @total_user, 
                    total_cost = @total_cost, 
                    first_name = @first_name, 
                    last_name = @last_name, 
                    card_no = @card_no, 
                    expire_month = @expire_month,
                    expire_year = @expire_year, 
                    cvv = @cvv, 
                    adr1 = @adr1, 
                    zip = @zip, 
                    state = @state, 
                    country = @country,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<Billing> All()
        {
            return Query<Billing>(
                sql: "SELECT * FROM [dbo].[team] where is_deleted = 0"
            );
        }

        public IEnumerable<Billing> FindBillingsByOrgID(string key)
        {
            return Query<Billing>(
                sql: "SELECT * FROM dbo.saas_billing WHERE is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }
    }
}