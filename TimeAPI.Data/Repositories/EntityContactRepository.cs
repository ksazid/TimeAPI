using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class EntityContactRepository : RepositoryBase, IEntityContactRepository
    {
        public EntityContactRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(EntityContact entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO entity_contact
                                  (id, entity_id, name, first_name, last_name, relationship, position, phone, mobile, email, adr_1, adr_2, city, country, is_primary, 
                                   department, designation, note, created_date, createdby)
                           VALUES (@id, @entity_id, @name, @first_name, @last_name, @relationship, @position, @phone, @mobile, @email, @adr_1, @adr_2, @city, @country, @is_primary, 
                                   @department, @designation, @note, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public EntityContact Find(string key)
        {
            return QuerySingleOrDefault<EntityContact>(
                sql: "SELECT * FROM entity_contact WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public EntityContact FindByEntityID(string key)
        {
            return QuerySingleOrDefault<EntityContact>(
                sql: "SELECT * FROM entity_contact WHERE is_deleted = 0 and entity_id = @key",
                param: new { key }
            );
        }

        public IEnumerable<EntityContact> FindByEntityListID(string key)
        {
            return Query<EntityContact>(
                sql: "SELECT * FROM [dbo].[entity_contact] where is_deleted = 0 AND entity_id = @key",
                param: new { key }
            );
        }
        
        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE entity_contact
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(EntityContact entity)
        {
            Execute(
                sql: @"UPDATE entity_contact
                   SET
                    name = @name,
                    first_name = @first_name, 
                    last_name = @last_name,
                    relationship =@relationship,
                    position = @position,
                    phone = @phone,
                    mobile = @mobile,
                    email = @email,
                    adr_1 = @adr_1, 
                    adr_2 = @adr_2,
                    city = @city, 
                    country = @country,
                    is_primary = @is_primary,
                    department = @department, 
                    designation = @designation, 
                    note = @note,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public void UpdateByEntityID(EntityContact entity)
        {
            Execute(
                sql: @"UPDATE entity_contact
                   SET
                    name = @name,
                    first_name = @first_name, 
                    last_name = @last_name,
                    relationship =@relationship,
                    position = @position,
                    phone = @phone,
                    mobile = @mobile,
                    email = @email,
                    adr_1 = @adr_1, 
                    adr_2 = @adr_2,
                    city = @city, 
                    country = @country,
                    department = @department, 
                    designation = @designation, 
                    note = @note,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE entity_id = @entity_id",
                param: entity
            );
        }

        public IEnumerable<EntityContact> All()
        {
            return Query<EntityContact>(
                sql: "SELECT * FROM [dbo].[entity_contact] where is_deleted = 0"
            );
        }

        public void RemoveByEntityID(string key)
        {
            Execute(
                sql: @"UPDATE entity_contact
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE entity_id = @key",
                param: new { key }
            );
        }
    }
}