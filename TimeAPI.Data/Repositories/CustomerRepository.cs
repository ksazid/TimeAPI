using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class CustomerRepository : RepositoryBase, ICustomerRepository
    {
        public CustomerRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public void Add(Customer entity)
        {
            entity.id = ExecuteScalar<string>(
                        sql: @"
                    INSERT INTO dbo.customer (id, org_id, cst_name, cst_type, first_name, last_name, is_company, company_name, annual_revenue, 
                                                  no_of_emp, industry_id, website, email, phone, adr, street, country, city, created_date, createdby)
                    VALUES (@id, @org_id, @cst_name, @cst_type, @first_name, @last_name, @is_company, @company_name, @annual_revenue, 
                                                  @no_of_emp, @industry_id, @website, @email, @phone, @adr, @street, @country, @city, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                        param: entity
                    );
        }

        public async Task<Customer> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<Customer>(
                sql: "SELECT * FROM dbo.customer WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public async Task<Customer> FindByEmpUserID(string key)
        {
            return await QuerySingleOrDefaultAsync<Customer>(
                sql: "SELECT * FROM dbo.customer WHERE user_id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public async Task<IEnumerable<Customer>> All()
        {
            return await QueryAsync<Customer>(
                sql: "SELECT * FROM dbo.customer where is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.customer
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Customer entity)
        {
            Execute(
                sql: @"UPDATE dbo.customer
                   SET
                    cst_name = @cst_name,   
                    cst_type = @cst_type, 
                    first_name = @first_name, 
                    last_name = @last_name, 
                    is_company = @is_company,
                    company_name = @company_name, 
                    annual_revenue = @annual_revenue, 
                    no_of_emp = @no_of_emp, 
                    industry_id = @industry_id, 
                    website = @website,
                    email = @email, 
                    phone = @phone, 
                    adr = @adr, 
                    street = @street, 
                    country = @country,
                    city = @city, 
                    modified_date = @modified_date, 
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public async Task<IEnumerable<dynamic>> FindCustomerByOrgID(string key)
        {
            return await QueryAsync<dynamic>(
                sql: @"SELECT dbo.entity_contact.is_primary,
                        dbo.entity_contact.name as primary_name,
                        dbo.entity_contact.email as primary_email,
                        dbo.entity_contact.phone as primary_phone,
                        dbo.industry_type.industry_type_name,
                        dbo.customer.*
                        FROM dbo.customer
                    LEFT JOIN  dbo.entity_contact on  dbo.customer.id = dbo.entity_contact.entity_id
                    LEFT JOIN  dbo.industry_type on  dbo.customer.industry_id = dbo.industry_type.id
                    WHERE
                        dbo.customer.is_deleted = 0
                        AND dbo.entity_contact.is_deleted = 0
                        AND dbo.customer.org_id = @key",
                 param: new { key }
            );
        }

        public async Task<Customer> FindCustomerByProjectID(string key)
        {
            return await QuerySingleOrDefaultAsync<Customer>(
                sql: @"SELECT dbo.customer.* FROM dbo.customer
                        INNER JOIN customer_x_project ON customer_x_project.cst_id = customer.id
                        WHERE customer_x_project.project_id = @key",
                 param: new { key }
            );
        }

        public async Task<Customer> FindByCustomerByNameAndEmail(string Name, string Email)
        {
            return await QuerySingleOrDefaultAsync<Customer>(
                sql: @"SELECT * FROM dbo.customer WHERE cst_name = @Name and email = @Email and is_deleted = 0",
                 param: new { Name, Email }
            );
        }
    }
}