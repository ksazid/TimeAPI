using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
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

        public async Task< EntityContact> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<EntityContact>(
                sql: "SELECT * FROM entity_contact WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public async Task<EntityContact> FindByEntityID(string key)
        {
            return await QuerySingleOrDefaultAsync<EntityContact>(
                sql: "SELECT * FROM entity_contact WHERE is_deleted = 0 and entity_id = @key",
                param: new { key }
            );
        }

        public async Task<IEnumerable<EntityContact>> FindByEntityListID(string key)
        {
            return await QueryAsync<EntityContact>(
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

        public async Task UpdateByEntityID(EntityContact entity)
        {
           await ExecuteAsync(
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

        public async Task<dynamic> FindByEntityContactOrgID(string key)
        {
            return await QueryAsync<dynamic>(
             sql: @"SELECT 
                        DISTINCT(dbo.entity_contact.id), dbo.entity_contact.name 
	                    FROM dbo.entity_contact
	                    LEFT JOIN dbo.customer on dbo.entity_contact.entity_id = dbo.customer.id
	                    WHERE dbo.customer.org_id = @key
	                    AND dbo.entity_contact.is_deleted = 0
	                    AND dbo.customer.is_deleted = 0",
                param: new { key }
            );
        }

        public async Task<IEnumerable<EntityContact>> All()
        {
            return await QueryAsync<EntityContact>(
                sql: "SELECT * FROM [dbo].[entity_contact] where is_deleted = 0"
            );
        }

        public async Task RemoveByEntityID(string key)
        {
            await ExecuteAsync(
                sql: @"UPDATE entity_contact
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE entity_id = @key",
                param: new { key }
            );
        }
    }
}