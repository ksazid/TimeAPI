﻿using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class ProjectUnitRepository : RepositoryBase, IProjectUnitRepository
    {
        public ProjectUnitRepository(IDbTransaction transaction)
           : base(transaction)
        { }
        public void Add(ProjectUnit entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.project_unit
                            (id, org_id, project_id, unit_id, unit_name, no_of_unit, unit_qty, note, created_date, createdby)
                    VALUES (@id, @org_id, @project_id, @unit_id, @unit_name, @no_of_unit, @unit_qty, @note, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public ProjectUnit Find(string key)
        {
            return QuerySingleOrDefault<ProjectUnit>(
                sql: "SELECT * FROM dbo.project_unit WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }
        public IEnumerable<ProjectUnit> FindByProjectID(string key)
        {
            return Query<ProjectUnit>(
                sql: "SELECT * FROM dbo.project_unit where is_deleted = 0 and project_id = @key",
                param: new { key }
            );
        }

        
        public void RemoveByProjectID(string key)
        {
            Execute(
                sql: @"UPDATE dbo.project_unit
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE project_id = @key",
                param: new { key }
            );
        }




        public IEnumerable<ProjectUnit> All()
        {
            return Query<ProjectUnit>(
                sql: "SELECT * FROM dbo.project_unit where is_deleted = 0"
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.project_unit
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(ProjectUnit entity)
        {
            Execute(
                sql: @"UPDATE dbo.project_unit
                           SET 
                            org_id = @org_id, 
                            project_id = @project_id, 
                            unit_id = @unit_id, 
                            unit_name = @unit_name, 
                            no_of_unit = @no_of_unit, 
                            unit_qty = @unit_qty, 
                            note = @note,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}