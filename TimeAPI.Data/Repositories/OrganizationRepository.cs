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
                            (org_id, user_id, org_name, type, summary, img_url, img_name, country_id, adr1, adr2, city, primary_cont_name,
                                primary_cont_type, time_zone_id, created_date, createdby)
                    VALUES (@org_id, @user_id, @org_name, @type,@summary, @img_url, @img_name, @country_id, @adr1, @adr2, @city, @primary_cont_name,
                               @primary_cont_type, @time_zone_id, @created_date, @createdby);
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
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE org_id = @key",
                param: new { key }
            );
        }

        public void Update(Organization entity)
        {
            Execute(
                sql: @"UPDATE dbo.organization
                           SET  user_id = @user_id,
                                org_name = @org_name,
                                type = @type,
                                summary= @summary,
                                img_url= @img_url,
                                img_name =@img_name,
                                country_id = @country_id,
                                adr1 = @adr1,
                                adr2 = @adr2,
                                city = @city,
                                primary_cont_name = @primary_cont_name,
                                primary_cont_type = @primary_cont_type,
                                time_zone_id = @time_zone_id,
                                modified_date = @modified_date,
                                modifiedby = @modifiedby
                         WHERE org_id = @org_id",
                param: entity
            );
        }

        public Organization FindByOrgName(string org_name)
        {
            return QuerySingleOrDefault<Organization>(
                sql: "SELECT * FROM [dbo].[organization] WHERE org_name = @org_name and  is_deleted = 0 ORDER BY org_name ASC",
                param: new { org_name }
            );
        }

        public dynamic FindByUsersID(string user_id)
        {
            var Rest = Query<Organization>(
                  sql: @"SELECT org_id, user_id, org_name, type, summary, img_url,
                       img_name, country_id, adr1, adr2, city, primary_cont_name,
                       primary_cont_type, time_zone_id, created_date, createdby,
                       modified_date, modifiedby, is_deleted FROM [dbo].[organization]
                WHERE user_id = @user_id and  is_deleted = 0",
                  param: new { user_id }
              );

            var result = GetOrgAddress(Rest);
            return result;
        }

        private List<Organization> GetOrgAddress(IEnumerable<Organization> resultsOrganization)
        {
            List<Organization> orgList = (resultsOrganization as List<Organization>);
            for (int i = 0; i < orgList.Count; i++)
            {
                var entityLocation = QuerySingleOrDefault<EntityLocation>(
                   sql: @"SELECT * from entity_location WITH (NOLOCK) WHERE entity_id = @item and is_deleted = 0;",
                   param: new { item = orgList[i].org_id }
                  );

                orgList[i].EntityLocation = entityLocation;
            }

            return orgList;
        }
    }
}