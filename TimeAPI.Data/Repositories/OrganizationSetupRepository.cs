﻿using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class OrganizationSetupRepository : RepositoryBase, IOrganizationSetupRepository
    {
        public OrganizationSetupRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public void Add(OrganizationSetup entity)
        {
            entity.org_id = ExecuteScalar<string>(
                    sql: @"
                    INSERT INTO dbo.organization_setup
                            (id, org_id, country, fiscal_year, start_of_week, working_hrs, date_format, currency, time_zome, created_date, createdby)
                    VALUES (@id, @org_id, @country, @fiscal_year, @start_of_week, @working_hrs, @date_format, @currency, @time_zome, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public OrganizationSetup Find(string key)
        {
            return QuerySingleOrDefault<OrganizationSetup>(
                sql: "SELECT * FROM [dbo].[organization_setup] WHERE org_id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public IEnumerable<OrganizationSetup> All()
        {
            return Query<OrganizationSetup>(
                sql: "SELECT * FROM [dbo].[organization_setup] where is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.organization_setup
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE org_id = @key",
                param: new { key }
            );
        }

        public void Update(OrganizationSetup entity)
        {
            Execute(
                sql: @"UPDATE dbo.organization_setup
                           SET 
                                country = @country, 
                                fiscal_year = @fiscal_year, 
                                start_of_week = @start_of_week, 
                                working_hrs = @working_hrs, 
                                date_format = @date_format, 
                                currency = @currency, 
                                time_zome = @time_zome
                                modified_date = @modified_date,
                                modifiedby = @modifiedby
                         WHERE org_id = @org_id",
                param: entity
            );
        }
        public void RemoveByOrgID(string key)
        {
            Execute(
                sql: @"UPDATE dbo.organization_setup
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE org_id = @key",
                param: new { key }
            );
        }

        public OrganizationSetup FindByEnitiyID(string key)
        {
            return QuerySingleOrDefault<OrganizationSetup>(
                sql: "SELECT * FROM [dbo].[organization_setup] WHERE org_id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        
    }
}