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
                                primary_cont_type, time_zone, created_date, createdby, modified_date, modifiedby, is_deleted)
                    VALUES (@org_id, @user_id, @org_name, @type,@summary, @img_url, @country, @adr1, @adr2, @city, @primary_cont_name,
                               @primary_cont_type, @time_zone, @created_date, @createdby, @modified_date, @modifiedby, @is_deleted);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public Organization Find(string key)
        {
            return QuerySingleOrDefault<Organization>(
                sql: "SELECT * FROM [dbo].[organization] WHERE org_id = @key",
                param: new { key }
            );
        }

        public Organization FindByOrgName(string org_name)
        {
            return QuerySingleOrDefault<Organization>(
                sql: "SELECT * FROM [dbo].[organization] WHERE org_name = @org_name",
                param: new { org_name }
            );
        }
        public IEnumerable<Organization> FindByUsersID(string user_id)
        {
            return Query<Organization>(
                sql: "SELECT * FROM [dbo].[organization] WHERE user_id = @user_id",
                param: new { user_id }
            );
        }


        public IEnumerable<Organization> All()
        {
            return Query<Organization>(
                sql: "SELECT * FROM [dbo].[organization]"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.organization
                   SET
                       modified_date = @modified_date, modifiedby = @modifiedby, is_deleted = 1, is_admin = @is_admin
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
                              modified_date = @modified_date, modifiedby = @modifiedby,  is_deleted = @is_deleted,  
                         WHERE org_id = org_id",
                param: entity
            );
        }

    }
}
