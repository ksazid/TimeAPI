using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class QuotationRepository : RepositoryBase, IQuotationRepository
    {
        public QuotationRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(Quotation entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.project_quotation
                            (id, org_id, lead_id, quotation_prefix, quotation_date, customer_id, project_name, quotation_subject, quotation_body, warranty_id, validity, 
                                                        payment_id, mode_of_payment_id, no_of_days, exclusion_id, remarks, tax_id, stage_id, created_date, createdby)
                    VALUES (@id, @org_id, @lead_id, @quotation_prefix, @quotation_date, @customer_id, @project_name, @quotation_subject, @quotation_body, @warranty_id, @validity, 
                                                        @payment_id, @mode_of_payment_id, @no_of_days, @exclusion_id, @remarks, @tax_id, @stage_id, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Quotation Find(string key)
        {
            return QuerySingleOrDefault<Quotation>(
                sql: "SELECT * FROM dbo.project_quotation WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<Quotation> All()
        {
            return Query<Quotation>(
                sql: "SELECT * FROM dbo.project_quotation where is_deleted = 0"
            );
        }

        public dynamic QuotationByOrgID(string key)
        {
            return Query<dynamic>(
                sql: @"SELECT
                        CASE
                            WHEN dbo.customer.is_company = 1 THEN dbo.customer.company_name
                            ELSE dbo.customer.first_name + ' ' + dbo.customer.last_name
                            END AS lead_name,
                            dbo.customer.company_name,
                            dbo.customer.first_name,
                            dbo.customer.last_name,
                            dbo.customer.is_company,
                            dbo.project_quotation.*
                            FROM dbo.project_quotation
                            INNER JOIN cost_project on project_quotation.lead_id = cost_project.id
                            INNER JOIN customer_x_project on cost_project.id = customer_x_project.project_id
                            INNER JOIN customer on customer_x_project.cst_id = customer.id
                            WHERE cost_project.is_deleted = 0 and customer.is_deleted = 0  and project_quotation.is_deleted = 0 and 
                                  cost_project.org_id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.project_quotation
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Quotation entity)
        {
            Execute(
                sql: @"UPDATE dbo.project_quotation
                           SET 
                            org_id = @org_id, 
                            lead_id = @lead_id, 
                            quotation_prefix = @quotation_prefix, 
                            quotation_date = @quotation_date, 
                            customer_id = @customer_id, 
                            project_name= @project_name,
                            quotation_subject = @quotation_subject, 
                            quotation_body = @quotation_body, 
                            warranty_id = @warranty_id, 
                            validity = @validity, 
                            payment_id = @payment_id, 
                            mode_of_payment_id = @mode_of_payment_id, 
                            no_of_days = @no_of_days, 
                            exclusion_id = @exclusion_id, 
                            remarks = @remarks, 
                            tax_id = @tax_id, 
                            stage_id = @stage_id,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }


    }
}