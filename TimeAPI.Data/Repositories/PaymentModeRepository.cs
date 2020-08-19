using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class PaymentModeRepository : RepositoryBase, IPaymentModeRepository
    {
        public PaymentModeRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(PaymentMode entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.project_payment_mode
                            (id, org_id, payment_mode_name, payment_mode_desc, created_date, createdby)
                    VALUES (@id, @org_id, @payment_mode_name, @payment_mode_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public PaymentMode Find(string key)
        {
            return QuerySingleOrDefault<PaymentMode>(
                sql: "SELECT * FROM dbo.project_payment_mode WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<PaymentMode> All()
        {
            return Query<PaymentMode>(
                sql: "SELECT * FROM dbo.project_payment_mode where is_deleted = 0"
            );
        }

        public IEnumerable<PaymentMode> PaymentModeByOrgID(string key)
        {
            return Query<PaymentMode>(
                sql: "SELECT * FROM dbo.project_payment_mode where is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.project_payment_mode
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(PaymentMode entity)
        {
            Execute(
                sql: @"UPDATE dbo.project_payment_mode
                           SET 
                            payment_mode_name = @payment_mode_name, 
                            payment_mode_desc = @payment_mode_desc,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}