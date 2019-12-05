using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class IndustryTypeRepository : RepositoryBase, IIndustryTypeRepository
    {

        public IndustryTypeRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(IndustryType entity)
        {

            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.industry_type
                                  (id, org_id, industry_type_name, industry_type_desc, created_date, createdby)
                           VALUES (@id, org_id, @industry_type_name, @industry_type_desc, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public IndustryType Find(string key)
        {
            return QuerySingleOrDefault<IndustryType>(
                sql: "SELECT * FROM dbo.industry_type WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.industry_type
                   SET
                       modified_date = @modified_date, modifiedby = @modifiedby, is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(IndustryType entity)
        {
            Execute(
                sql: @"UPDATE dbo.industry_type
                   SET 
                    org_id =@org_id,
                    industry_type_name = @industry_type_name, 
                    industry_type_desc = @industry_type_desc, 
                    modified_date = @modified_date, 
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<IndustryType> All()
        {
            return Query<IndustryType>(
                sql: "SELECT * FROM [dbo].[industry_type] where is_deleted = 0"
            );
        }

    }
}
