using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class OrganizationRepository : RepositoryBase, IOrganizationRepository
    {
        public OrganizationRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public void Add(Organization entity)
        {

            entity.org_id = ExecuteScalar<string>(
                    sql: @"
                    INSERT INTO dbo.organization 
                            (org_id, user_id, org_name, type, summary, img_url, country, adr1, adr2, city, primary_cont_name,
                                primary_cont_type, time_zone, created_date, createdby)
                    VALUES (@org_id, @user_id, @org_name, @type,@summary, @img_url, @country, @adr1, @adr2, @city, @primary_cont_name,
                               @primary_cont_type, @time_zone, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Organization Find(string key)
        {
            return QuerySingleOrDefault<Organization>(
                sql: "SELECT * FROM [dbo].[organization] WHERE org_id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<Organization> All()
        {
            return Query<Organization>(
                sql: "SELECT * FROM [dbo].[organization] where is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.organization
                   SET
                       modified_date = @modified_date, modifiedby = @modifiedby, is_deleted = 1
                    WHERE org_id = @key",
                param: new { key }
            );
        }

        public void Update(Organization entity)
        {
            Execute(
                sql: @"UPDATE dbo.organization
                           SET  user_id = @user_id, org_name = @org_name, type = @type, summary= @summary, img_url= @img_url, country = @country, 
                              adr1 = @adr1, adr2 = @adr2, city = @city, primary_cont_name = @primary_cont_name,  
                              primary_cont_type = @primary_cont_type, time_zone = @time_zone,  
                              modified_date = @modified_date, modifiedby = @modifiedby
                         WHERE org_id = org_id",
                param: entity
            );
        }

        public Organization FindByOrgName(string org_name)
        {
            return QuerySingleOrDefault<Organization>(
                sql: "SELECT * FROM [dbo].[organization] WHERE org_name = @org_name and  is_deleted = 0",
                param: new { org_name }
            );
        }

        public IEnumerable<Organization> FindByUsersID(string user_id)
        {
            return Query<Organization>(
                sql: "SELECT * FROM [dbo].[organization] WHERE user_id = @user_id and  is_deleted = 0",
                param: new { user_id }
            );
        }
    }
}
