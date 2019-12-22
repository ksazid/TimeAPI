using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class SocialRepository : RepositoryBase, ISocialRepository
    {

        public SocialRepository(IDbTransaction transaction) : base(transaction)
        {

        }

        public void Add(Social entity)
        {

            entity.id = ExecuteScalar<string>(
                    sql: @"
                    INSERT INTO [dbo].[social]  
                           (id, empid, social_media_name, url, created_date, createdby)
                    VALUES (@id, @empid, @social_media_name, @url, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Social Find(string key)
        {
            return QuerySingleOrDefault<Social>(
                sql: "SELECT * FROM [dbo].[social] WHERE id = @key and  is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<Social> All()
        {
            return Query<Social>(
                sql: "SELECT * FROM [dbo].[employee] WHERE  is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.social
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Social entity)
        {
            Execute(
                sql: @"UPDATE dbo.employee
                   SET 
                       empid = @empid, social_media_name = @social_media_name, 
                        url = @url, modified_date = @modified_date, modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<Social> FindSocialIdsByEmpID(string EmpID)
        {
            return Query<Social>(
                sql: "SELECT * FROM [dbo].[employee] WHERE empid = @EmpID and  is_deleted = 0",
                param: new { EmpID }
            );
        }
    }
}
