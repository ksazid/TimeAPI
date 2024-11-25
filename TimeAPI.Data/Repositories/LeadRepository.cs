﻿using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class LeadRepository : RepositoryBase, ILeadRepository
    {
        public LeadRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public void Add(Lead entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.lead
                            (id, org_id, cst_id, is_company, lead_owner_emp_id, lead_source_id, lead_status_id, reason_id, others_remarks, annual_revenue, rating_id, industry_id, no_of_employee, email, phone, website, adr_1, adr_2, city, country_id, created_date, createdby)
                    VALUES (@id, @org_id, @cst_id, @is_company, @lead_owner_emp_id, @lead_source_id, @lead_status_id,  @reason_id, @others_remarks, @annual_revenue, @rating_id, @industry_id, @no_of_employee, @email, @phone, @website, @adr_1, @adr_2, @city, @country_id,  @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public async Task<Lead> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<Lead>(
                sql: "SELECT * FROM dbo.lead WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public async Task<dynamic> FindByLeadID(string key)
        {
            return await QuerySingleOrDefaultAsync<dynamic>(
                sql: @"SELECT 
                         dbo.customer.cst_name, dbo.employee.full_name as lead_owner,
                        dbo.lead_source.lead_source,  dbo.lead_status.lead_status,
                        dbo.lead_deal.deal_name,
                        dbo.lead_deal_type.deal_type_name,
                        dbo.lead.* FROM dbo.lead 
                        LEFT JOIN dbo.customer on dbo.lead.cst_id =  dbo.customer.id
                        LEFT join dbo.employee on dbo.lead.lead_owner_emp_id =  dbo.employee.id
                        LEFT join dbo.lead_source on dbo.lead.lead_source_id =  dbo.lead_source.id
                        LEFT join dbo.lead_status on dbo.lead.lead_status_id =  dbo.lead_status.id
                        LEFT join dbo.lead_deal on dbo.lead.id =  dbo.lead_deal.lead_id
                        LEFT join dbo.lead_deal_type on dbo.lead_deal.deal_type_id =  dbo.lead_deal_type.id
                        WHERE  dbo.lead.id = @key AND  dbo.lead.is_deleted = 0",
                param: new { key }
            );
        }

        public async Task<IEnumerable<Lead>> All()
        {
            return await QueryAsync<Lead>(
                sql: "SELECT * FROM dbo.lead where is_deleted = 0"
            );
        }

        public async Task<dynamic> LeadByOrgID(string key)
        {
            return await QueryAsync<dynamic>(
                sql: @"SELECT 
			                CASE
			                WHEN dbo.lead.is_company = 1 THEN dbo.customer.company_name
			                ELSE dbo.customer.first_name + ' ' + dbo.customer.last_name
			                END AS lead_name,
			                dbo.customer.company_name,
			                dbo.customer.cst_name,
			                dbo.customer.first_name,
			                dbo.customer.last_name,
			                dbo.customer.is_company,
			                dbo.entity_contact.email,
			                dbo.entity_contact.phone,
                            dbo.employee.full_name,
                            dbo.lead_source.lead_source,
			                dbo.lead_status.lead_status,
                            dbo.lead_rating.rating_name, 
			                dbo.industry_type.industry_type_name, 
			                dbo.lead_deal.deal_prefix,
                            dbo.lead_deal.deal_name, 
			                dbo.lead_deal_type.deal_type_name,
							dbo.lead_contact_role.contact_role_name,
                            FORMAT(CAST(dbo.lead.created_date AS DATETIME2), N'dd/MM/yyyy') as ondate,
                            dbo.lead.id
                        FROM  dbo.lead
                        LEFT JOIN dbo.customer on dbo.lead.cst_id = dbo.customer.id
                        LEFT JOIN dbo.employee on dbo.lead.lead_owner_emp_id = dbo.employee.id
                        LEFT JOIN dbo.lead_source on dbo.lead.lead_source_id =  dbo.lead_source.id
                        LEFT JOIN dbo.lead_status on dbo.lead.lead_status_id =  dbo.lead_status.id
                        LEFT JOIN dbo.lead_rating on dbo.lead.rating_id =  dbo.lead_rating.id
                        LEFT JOIN dbo.industry_type  on dbo.lead.industry_id =  dbo.industry_type.id
                        LEFT JOIN dbo.lead_deal  on dbo.lead.id =  dbo.lead_deal.lead_id
                        LEFT JOIN dbo.lead_deal_type  on dbo.lead_deal.deal_type_id =  dbo.lead_deal_type.id
                        LEFT JOIN dbo.lead_contact_role  on dbo.lead_deal.contact_role_id =  dbo.lead_contact_role.id
                        LEFT JOIN dbo.entity_contact  on dbo.lead.id =  dbo.entity_contact.entity_id
                    WHERE dbo.lead.is_deleted = 0 
						 AND dbo.lead_status.lead_status != 'Cost Estimation'
					     AND dbo.lead_status.lead_status != 'Project'
					     AND dbo.entity_contact.is_primary = 1
					     AND dbo.entity_contact.is_deleted = 0
                         AND dbo.lead.org_id = @key
				ORDER BY FORMAT(CAST( dbo.lead.created_date AS DATETIME2), N'MM/dd/yyyy hh:mm tt') DESC",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.lead
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Lead entity)
        {
            Execute(
                sql: @"UPDATE dbo.lead
                           SET 
                            org_id = @org_id, 
                            cst_id = @cst_id, 
                            is_company = @is_company, 
                            lead_owner_emp_id = @lead_owner_emp_id,
                            lead_source_id = @lead_source_id, 
                            lead_status_id = @lead_status_id, 
                            reason_id = @reason_id, 
                            others_remarks = @others_remarks,
                            annual_revenue = @annual_revenue, 
                            rating_id = @rating_id,
                            industry_id = @industry_id, 
                            no_of_employee = @no_of_employee,
                            email = @email, 
                            phone = @phone,
                            website = @website,
                            adr_1 = @adr_1, 
                            adr_2 = @adr_2, 
                            city = @city, 
                            country_id = @country_id,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }

        public async Task UpdateLeadStatusByLeadID(Lead entity)
        {
            await ExecuteAsync(
                   sql: @"UPDATE dbo.lead
                           SET 
                            lead_status_id = @lead_status_id, 
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                  param: entity
              );
        }
    }
}