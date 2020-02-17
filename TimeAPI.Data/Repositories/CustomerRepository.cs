﻿using System.Collections.Generic;
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
                    INSERT INTO dbo.customer (id, cst_name, cst_type, email, phone, adr,street, city, created_date, createdby)
                    VALUES (@id, @cst_name, @cst_type, @email, @phone, @adr, @street, @city, @created_date, @createdby);
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
                    email = @email, 
                    phone = @phone, 
                    adr = @adr, 
                    street = @street, 
                    city = @city, 
                    modified_date = @modified_date, 
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }
    }
}