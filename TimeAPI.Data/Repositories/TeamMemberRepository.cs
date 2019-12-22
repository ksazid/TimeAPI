using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class TeamMemberRepository : RepositoryBase, ITeamMemberRepository
    {

        public TeamMemberRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(TeamMembers entity)
        {

            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.team_members
                                  (id, team_id, emp_id, is_teamlead, created_date, createdby, is_deleted)
                           VALUES (@id, @team_id, @emp_id, @is_teamlead, @created_date, @createdby, @is_deleted);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public TeamMembers Find(string key)
        {
            return QuerySingleOrDefault<TeamMembers>(
                sql: "SELECT * FROM dbo.team_members WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.team_members
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void RemoveByTeamID(string key)
        {
            Execute(
                sql: @"UPDATE dbo.team_members
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE team_id = @key",
                param: new { key }
            );
        }

        public void Update(TeamMembers entity)
        {
            Execute(
                sql: @"UPDATE dbo.team_members
                   SET 
                        team_id = @team_id,
                        emp_id = @emp_id,
                        is_teamlead = @is_teamlead,
                        modified_date = @modified_date, 
                        modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<TeamMembers> All()
        {
            return Query<TeamMembers>(
                sql: "SELECT * FROM [dbo].[team_members] where is_deleted = 0"
            );
        }



    }
}
