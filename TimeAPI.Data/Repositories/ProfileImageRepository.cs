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
                                   (id, user_id, img_name, img_url, created_date, createdby, modified_date, modifiedby, is_deleted)
                           VALUES (@id, @user_id, @img_name, @img_url, @created_date, @createdby, @modified_date, @modifiedby, @is_deleted);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Image Find(string key)
        {
            return QuerySingleOrDefault<Image>(
                sql: "SELECT * FROM dbo.image WHERE id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.image
                   SET
                       modified_date = @modified_date, modifiedby = @modifiedby, is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(Image entity)
        {
            Execute(
                sql: @"UPDATE dbo.image
                   SET 
                        id = @id, 
                        user_id = @user_id, 
                        img_name = @img_name, 
                        img_url = @img_url, 
                        created_date = @created_date, 
                        createdby = @createdby, 
                        modified_date = @modified_date, 
                        modifiedby = @modifiedby, 
                        is_deleted = @is_deleted
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<Image> All()
        {
            return Query<Image>(
                sql: "SELECT * FROM [dbo].[image]"
            );
        }

    }
}
