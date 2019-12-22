using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;


namespace TimeAPI.Data.Repositories
{
    public class ProfileImageRepository : RepositoryBase, IProfileImageRepository
    {
        public ProfileImageRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public void Add(Image entity)
        {

            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.image
                                   (id, user_id, img_name, img_url, created_date, createdby)
                           VALUES (@id, @user_id, @img_name, @img_url, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Image Find(string key)
        {
            return QuerySingleOrDefault<Image>(
                sql: "SELECT * FROM dbo.image WHERE id = @key and  is_deleted = 0",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.image
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Image entity)
        {
            Execute(
                sql: @"UPDATE dbo.image
                   SET 
                        user_id = @user_id, 
                        img_name = @img_name, 
                        img_url = @img_url, 
                        modified_date = @modified_date, 
                        modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<Image> All()
        {
            return Query<Image>(
                sql: "SELECT * FROM [dbo].[image] where is_deleted = 0"
            );
        }

        public Image FindByProfileUserID(string key)
        {
            return QuerySingleOrDefault<Image>(
                sql: "SELECT * FROM dbo.image WHERE user_id = @key and  is_deleted = 0",
                param: new { key }
            );
        }

    }
}
