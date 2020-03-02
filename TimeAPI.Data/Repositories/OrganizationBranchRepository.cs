using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class OrganizationBranchRepository : RepositoryBase, IOrganizationBranchRepository
    {
        public OrganizationBranchRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public void Add(OrganizationBranch entity)
        {
            entity.org_id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.organization_branch
                            (id, parent_org_id, org_id, created_date, createdby)
                    VALUES (@id, @parent_org_id, @org_id, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public OrganizationBranch Find(string key)
        {
            return QuerySingleOrDefault<OrganizationBranch>(
                sql: "SELECT * FROM [dbo].[organization_branch] WHERE parent_org_id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<OrganizationBranch> FindByParentOrgID(string key)
        {
            return Query<OrganizationBranch>(
                sql: "SELECT * FROM [dbo].[organization_branch] WHERE parent_org_id = @key and is_deleted = 0",
                param: new { key }
            );
        }


        public IEnumerable<OrganizationBranch> All()
        {
            return Query<OrganizationBranch>(
                sql: "SELECT * FROM [dbo].[organization_branch] where is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.organization_branch
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE org_id = @key",
                param: new { key }
            );
        }

        public void Update(OrganizationBranch entity)
        {
            Execute(
                sql: @"UPDATE dbo.organization_branch
                           SET  parent_org_id = @parent_org_id,
                                org_id = @org_id,
                                modified_date = @modified_date,
                                modifiedby = @modifiedby
                         WHERE org_id = @org_id",
                param: entity
            );
        }

        //public OrganizationBranch FindByOrgName(string org_name)
        //{
        //    return QuerySingleOrDefault<OrganizationBranch>(
        //        sql: "SELECT * FROM [dbo].[organization_branch] WHERE org_name = @org_name and  is_deleted = 0 ORDER BY org_name ASC",
        //        param: new { org_name }
        //    );
        //}

        //public IEnumerable<OrganizationBranch> FindByUsersID(string user_id)
        //{
        //    return Query<OrganizationBranch>(
        //        sql: "SELECT * FROM [dbo].[organization_branch] WHERE user_id = @user_id and  is_deleted = 0",
        //        param: new { user_id }
        //    );
        //}
    }
}