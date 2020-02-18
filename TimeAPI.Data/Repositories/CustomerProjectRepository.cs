using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class CustomerProjectRepository : RepositoryBase, ICustomerProjectRepository
    {
        public CustomerProjectRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public void Add(CustomerProject entity)
        {
            entity.id = ExecuteScalar<string>(
                        sql: @"INSERT INTO dbo.customer_x_project 
                                    (id, cst_id, project_id, created_date, createdby)
                                VALUES (@id, @cst_id, @project_id, @created_date, @createdby);
                                SELECT SCOPE_IDENTITY()",
                        param: entity
                    );
        }

        public CustomerProject Find(string key)
        {
            return QuerySingleOrDefault<CustomerProject>(
                sql: "SELECT * FROM dbo.customer_x_project WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<CustomerProject> All()
        {
            return Query<CustomerProject>(
                sql: "SELECT * FROM dbo.customer_x_project where is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.customer_x_project
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(CustomerProject entity)
        {
            Execute(
                sql: @"UPDATE dbo.customer_x_project
                   SET
                    cst_id = @cst_id, 
                    modified_date = @modified_date, 
                    modifiedby = @modifiedby
                    WHERE project_id = @project_id",
                param: entity
            );
        }

        //public IEnumerable<Customer> FindCustomerByOrgID(string key)
        //{
        //    return Query<Customer>(
        //        sql: "SELECT * FROM dbo.customer_x_project where is_deleted = 0 AND org_id = @key",
        //         param: new { key }
        //    );
        //}

    }
}