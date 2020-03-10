using System.Collections.Generic;
using System.Data;
using System.Linq;
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
                sql: @"SELECT 
                            dbo.organization.org_id
                          , organization_branch.parent_org_id
                          , dbo.organization.user_id
                          , dbo.organization.org_name
                          , dbo.organization.type
                          , dbo.organization.summary
                          , dbo.organization.img_url
                          , dbo.organization.img_name
                          , dbo.organization.country_id
                          , dbo.organization.adr1
                          , dbo.organization.adr2
                          , dbo.organization.city
                          , dbo.organization.primary_cont_name
                          , dbo.organization.primary_cont_type
                          , dbo.organization.time_zone_id
                          , dbo.organization.created_date
                          , dbo.organization.createdby
                          , dbo.organization.modified_date
                          , dbo.organization.modifiedby
                          , dbo.organization.is_deleted
                      FROM dbo.organization WITH(NOLOCK)
                      LEFT JOIN dbo.organization_branch
                      ON dbo.organization.org_id = dbo.organization_branch.org_id
                    WHERE dbo.organization.org_id = @key 
                      AND organization.is_deleted = 0",
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
                  sql: @"SELECT 
	                        dbo.organization.org_id,  
	                        dbo.organization.user_id, 
	                        dbo.organization.org_name, 
	                        organization_branch.parent_org_id as parent_org_id,
	                        (select org_name from dbo.organization 
	                        where org_id = dbo.organization_branch.parent_org_id) as branchname,
	                        dbo.organization.type,  dbo.organization.summary,  dbo.organization.img_url,
	                        dbo.organization.img_name,  dbo.organization.country_id, 
	                        dbo.organization.adr1,  dbo.organization.adr2,  dbo.organization.city,  
	                        dbo.organization.primary_cont_name,
	                        dbo.organization.primary_cont_type,  dbo.organization.time_zone_id,  
	                        dbo.organization.created_date,  dbo.organization.createdby
 
                        FROM 
                        dbo.organization
                        LEFT JOIN organization_branch on dbo.organization.org_id = organization_branch.org_id
                        WHERE dbo.organization.user_id = 'da59e4af-6e4d-4818-a169-d74a90b1c2d9'
                        AND  dbo.organization.is_deleted = 0",
                  param: new { user_id }
              );

            var result = GetOrgAddress(Rest);
            return result;
        }

        public IEnumerable<OrganizationBranchViewModel> FindByAllBranchByParengOrgID(string ParengOrgID)
        {
            var Rest = Query<OrganizationBranchViewModel>(
                  sql: @"SELECT 
                            dbo.organization_branch.parent_org_id,  
                            dbo.organization_branch.org_id,  
                            dbo.organization.user_id,  
                            dbo.organization.org_name, 
                            dbo.organization.type,
                            dbo.organization.summary, dbo.organization.img_url,
                            dbo.organization.img_name, dbo.organization.country_id, dbo.organization.adr1, 
                            dbo.organization.adr2, dbo.organization.city, dbo.organization.primary_cont_name,
                            dbo.organization.primary_cont_type, dbo.organization.time_zone_id, dbo.organization.created_date, 
                            dbo.organization.createdby, dbo.organization.modified_date, dbo.organization.modifiedby, dbo.organization.is_deleted 
                            FROM dbo.organization_branch
                            INNER JOIN dbo.organization on dbo.organization_branch.org_id = dbo.organization.org_id
                            WHERE dbo.organization_branch.parent_org_id = @ParengOrgID
                            and  dbo.organization_branch.is_deleted = 0
                            and  dbo.organization.is_deleted = 0",
                  param: new { ParengOrgID }
              );

            var result = GetOrgBranchAddress(Rest);
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

                var entitySetup = QuerySingleOrDefault<OrganizationSetup>(
                       sql: @"SELECT * from organization_setup WITH (NOLOCK) WHERE org_id = @item and is_deleted = 0;",
                       param: new { item = orgList[i].org_id }
                    );


                orgList[i].EntityLocation = entityLocation;
                orgList[i].OrganizationSetup = entitySetup;
            }

            return orgList;
        }

        private List<OrganizationBranchViewModel> GetOrgBranchAddress(IEnumerable<OrganizationBranchViewModel> resultsOrganization)
        {
            List<OrganizationBranchViewModel> orgList = (resultsOrganization as List<OrganizationBranchViewModel>);
            for (int i = 0; i < orgList.Count; i++)
            {
                var entityLocation = QuerySingleOrDefault<EntityLocation>(
                   sql: @"SELECT * from entity_location WITH (NOLOCK) WHERE entity_id = @item and is_deleted = 0;",
                   param: new { item = orgList[i].org_id }
                  );

                var entitySetup = QuerySingleOrDefault<OrganizationSetup>(
                       sql: @"SELECT * from organization_setup WITH (NOLOCK) WHERE org_id = @item and is_deleted = 0;",
                       param: new { item = orgList[i].org_id }
                    );

    
                orgList[i].EntityLocationViewModel = entityLocation;
                orgList[i].OrganizationSetup = entitySetup;
            }

            return orgList;
        }
    }
}