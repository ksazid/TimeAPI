using System.Collections.Generic;
using System.Data;
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

        public Customer Find(string key)
        {
            return QuerySingleOrDefault<Customer>(
                sql: "SELECT * FROM dbo.customer WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public Customer FindByEmpUserID(string key)
        {
            return QuerySingleOrDefault<Customer>(
                sql: "SELECT * FROM dbo.customer WHERE user_id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<Customer> All()
        {
            return Query<Customer>(
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

        public IEnumerable<dynamic> FindCustomerByOrgID(string key)
        {
            return Query<dynamic>(
                sql: "SELECT  ROW_NUMBER() OVER (ORDER BY dbo.customer.cst_name) AS rowno, * FROM dbo.customer where is_deleted = 0 AND org_id = @key",
                 param: new { key }
            );
        }

        public Customer FindCustomerByProjectID(string key)
        {
            return QuerySingleOrDefault<Customer>(
                sql: @"SELECT dbo.customer.* FROM dbo.customer
                        INNER JOIN customer_x_project ON customer_x_project.cst_id = customer.id
                        WHERE customer_x_project.project_id = @key",
                 param: new { key }
            );
        }

        public Customer FindByCustomerByNameAndEmail(string Name, string Email)
        {
            return QuerySingleOrDefault<Customer>(
                sql: @"SELECT * FROM dbo.customer WHERE cst_name = @Name and email = @Email and is_deleted = 0",
                 param: new { Name, Email }
            );
        }
    }
}