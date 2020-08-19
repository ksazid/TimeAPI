using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class PaymentRepository : RepositoryBase, IPaymentRepository
    {
        public PaymentRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(Payment entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.project_payment
                            (id, org_id, payment_name, payment_desc, created_date, createdby)
                    VALUES (@id, @org_id, @payment_name, @payment_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Payment Find(string key)
        {
            return QuerySingleOrDefault<Payment>(
                sql: "SELECT * FROM dbo.project_payment WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<Payment> All()
        {
            return Query<Payment>(
                sql: "SELECT * FROM dbo.project_payment where is_deleted = 0"
            );
        }

        public IEnumerable<Payment> PaymentByOrgID(string key)
        {
            return Query<Payment>(
                sql: "SELECT * FROM dbo.project_payment where is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.project_payment
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Payment entity)
        {
            Execute(
                sql: @"UPDATE dbo.project_payment
                           SET 
                            payment_name = @payment_name, 
                            payment_desc = @payment_desc,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}