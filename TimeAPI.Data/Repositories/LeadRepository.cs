using System.Collections.Generic;
using System.Data;
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
                            (id, org_id, lead_company_id, lead_owner_emp_id, first_name, last_name, lead_source_id, lead_status_id, annual_revenue, rating_id, industry_id, no_of_employee, email, website, adr_1, adr_2, city, country_id, created_date, createdby)
                    VALUES (@id, @org_id, @lead_company_id, @lead_owner_emp_id, @first_name, @last_name, @lead_source_id, @lead_status_id, @annual_revenue, @rating_id, @industry_id, @no_of_employee, @email, @website, @adr_1, @adr_2, @city, @country_id,  @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Lead Find(string key)
        {
            return QuerySingleOrDefault<Lead>(
                sql: "SELECT * FROM dbo.lead WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<Lead> All()
        {
            return Query<Lead>(
                sql: "SELECT * FROM dbo.lead where is_deleted = 0"
            );
        }

        public dynamic LeadByOrgID(string key)
        {
            return Query<dynamic>(
                sql: @"SELECT 
                            dbo.lead_company.company_name, dbo.employee.full_name,
                            dbo.lead_source.lead_source, dbo.lead_status.lead_status,
                            dbo.lead_rating.rating_name, dbo.industry_type.industry_type_name,  
                    dbo.lead.* FROM  dbo.lead
                        LEFT JOIN dbo.lead_company on dbo.lead.lead_company_id = dbo.lead_company.id
                        LEFT JOIN dbo.employee on dbo.lead.lead_owner_emp_id = dbo.employee.id
                        LEFT JOIN  dbo.lead_source on dbo.lead.lead_source_id =  dbo.lead_source.id
                        LEFT JOIN dbo.lead_status on dbo.lead.lead_status_id =  dbo.lead_status.id
                        LEFT JOIN dbo.lead_rating on dbo.lead.rating_id =  dbo.lead_rating.id
                        LEFT JOIN dbo.industry_type  on dbo.lead.industry_id =  dbo.industry_type.id
                    where dbo.lead.is_deleted = 0
                        AND dbo.lead.org_id = @key",
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
                            lead_company_id = @lead_company_id, 
                            lead_owner_emp_id = @lead_owner_emp_id,
                            first_name = @first_name, 
                            last_name = @last_name,
                            lead_source_id = @lead_source_id, 
                            lead_status_id = @lead_status_id, 
                            annual_revenue = @annual_revenue, 
                            rating_id = @rating_id,
                            industry_id = @industry_id, 
                            no_of_employee = @no_of_employee,
                            email = @email, 
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

    }
}