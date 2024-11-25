﻿using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
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

        public async Task<Quotation> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<Quotation>(
                sql: "SELECT * FROM dbo.project_quotation WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public async Task<Quotation> FindByQuotationID(string key)
        {
            return await QuerySingleOrDefaultAsync<Quotation>(
                sql: @"SELECT
                        CASE
                            WHEN dbo.customer.is_company = 1 THEN dbo.customer.company_name
                            ELSE dbo.customer.first_name + ' ' + dbo.customer.last_name
                            END AS lead_name,
                            dbo.customer.company_name,
                            dbo.customer.first_name,
                            dbo.customer.last_name,
                            dbo.customer.is_company,
                            dbo.cost_project.total_hours,
                            dbo.cost_project.gross_total_amount,
                            dbo.cost_project.profit_margin_amount,
                            dbo.cost_project.discount_amount,
                            dbo.cost_project.total_amount,
                            dbo.cost_project.vat_amount,
                            dbo.cost_project.net_total_amount,
                            dbo.project_quotation.*
                            FROM dbo.project_quotation
                            INNER JOIN cost_project on project_quotation.lead_id = cost_project.id
                            INNER JOIN customer_x_project on cost_project.id = customer_x_project.project_id
                            INNER JOIN customer on customer_x_project.cst_id = customer.id
                            WHERE cost_project.is_deleted = 0 and customer.is_deleted = 0  and project_quotation.is_deleted = 0 and
                            project_quotation.id  = @key",
                param: new { key }
            );
        }

        public async Task<IEnumerable<Quotation>> All()
        {
            return await QueryAsync<Quotation>(
                sql: "SELECT * FROM dbo.project_quotation where is_deleted = 0"
            );
        }

        public async Task<dynamic> QuotationByOrgID(string key)
        {
            return await QueryAsync<dynamic>(
                sql: @"SELECT
                        customer.cst_name as lead_name,
                        customer_x_project.cst_id,
                        dbo.customer.company_name,
                        dbo.customer.first_name,
                        dbo.customer.last_name,
                        dbo.customer.is_company,
						entity_contact.phone,
						entity_contact.email,
                        FORMAT(CAST(dbo.project_quotation.quotation_date AS DATETIME2), N'dd/MM/yyyy') AS ondate,
	                    dbo.cost_project.total_hours,
	                    dbo.cost_project.gross_total_amount,
	                    dbo.cost_project.profit_margin_amount,
	                    dbo.cost_project.discount_amount,
	                    dbo.cost_project.total_amount,
	                    dbo.cost_project.vat_amount,
	                    dbo.cost_project.net_total_amount,
	                    dbo.lead_stage.stage_name,
                        dbo.project_quotation.*
                        FROM dbo.project_quotation
                        LEFT JOIN cost_project on project_quotation.lead_id = cost_project.id
						LEFT JOIN entity_contact on cost_project.id = entity_contact.entity_id
                        LEFT JOIN customer_x_project on cost_project.id = customer_x_project.project_id
                        LEFT JOIN customer on customer_x_project.cst_id = customer.id
                        LEFT JOIN lead_stage on dbo.project_quotation.stage_id = lead_stage.id
                        WHERE cost_project.is_deleted = 0 
						and customer.is_deleted = 0 
						and project_quotation.is_deleted = 0
						and entity_contact.is_primary =1 
						and entity_contact.is_deleted =0 
						and cost_project.org_id = @key",
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

        public async Task UpdateQuotationStageByQuotationID(Quotation entity)
        {
           await ExecuteAsync(
                sql: @"UPDATE dbo.project_quotation
                           SET 
                            stage_id = @stage_id,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }

        public async Task<string> GetLastAddedQuotationPrefixByOrgID(string key)
        {
            return await QuerySingleOrDefaultAsync<string>(
                sql: @"SELECT TOP 1 quotation_prefix from dbo.project_quotation 
                        WHERE project_quotation.org_id = @key
                        AND project_quotation.is_deleted = 0
                        ORDER BY FORMAT(CAST( dbo.project_quotation.created_date AS DATETIME2), N'dd/MM/yyyy hh:mm tt') DESC",
                param: new { key }
            );
        }
    }
}