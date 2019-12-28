using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class AdministrativeRepository : RepositoryBase, IAdministrativeRepository
    {

        public AdministrativeRepository(IDbTransaction transaction) : base(transaction)
        {

        }

        public void Add(Administrative entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"
                    INSERT INTO [dbo].[administrative]  
                           (id, org_id, administrative_name, summary, created_date, createdby)
                    VALUES (@id, @org_id, @administrative_name,  @summary, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Administrative Find(string key)
        {
            return QuerySingleOrDefault<Administrative>(
                sql: "SELECT * FROM [dbo].[administrative] WHERE id = @key and  is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<Administrative> All()
        {
            return Query<Administrative>(
                sql: "SELECT * FROM [dbo].[administrative] WHERE  is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.administrative
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Administrative entity)
        {
            Execute(
                sql: @"UPDATE dbo.administrative
                   SET 
                       administrative_name = @administrative_name, 
                       org_id  = @org_id
                       summary = @summary,
                       modified_date = @modified_date, 
                       modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }
    }
}
