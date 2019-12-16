using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class TeamRepository : RepositoryBase, ITeamRepository
    {
        public TeamRepository(IDbTransaction transaction) : base(transaction)
        { }
        public void Add(Team entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.team
                                  (id, org_id, team_name, team_desc, team_by, team_department_id, team_lead_empid, created_date, createdby)
                           VALUES (@id, @org_id, @team_name,  @team_desc, @team_by, @team_department_id, @team_lead_empid, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }   
        public Team Find(string key)
        {
            return QuerySingleOrDefault<Team>(
                sql: "SELECT * FROM dbo.team WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }
        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.team
                   SET
                       modified_date = @modified_date, modifiedby = @modifiedby, is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }
        public void Update(Team entity)
        {
            Execute(
                sql: @"UPDATE dbo.team
                   SET 
                    org_id = @org_id,
                    team_name = @team_name, 
                    team_desc = @team_desc, 
                    team_by = @team_by, 
                    team_department_id = @team_department_id, 
                    team_lead_empid = @team_lead_empid,
                    modified_date = @modified_date, 
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }
        public IEnumerable<Team> All()
        {
            return Query<Team>(
                sql: "SELECT * FROM [dbo].[team] where is_deleted = 0"
            );
        }
        public IEnumerable<Team> FindTeamsByOrgID(string key)
        {
            return Query<Team>(
                sql: "SELECT * FROM dbo.team WHERE is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }
        public dynamic FindByTeamID(string key)
        {
            return QuerySingleOrDefault<dynamic>(
                   sql: @"SELECT 
		                        team.id as team_id,
		                        team.team_name,
		                        team.team_by,
		                        e_tl.full_name as team_lead,
		                        e_tl.workemail,
		                        e_tl.emp_code
	                        FROM dbo.team WITH(NOLOCK)
	                        LEFT JOIN employee e_tl ON team.team_lead_empid = e_tl.id
	                        WHERE team.id =  @key  
							AND team.is_deleted = 0
                            ORDER BY team.team_name ASC",
                      param: new { key }
               );
        }
        //public IEnumerable<dynamic> FetchByAllTeamMembersTeamID(string key)
        //{
        //    return Query<dynamic>(
        //           sql: @"SELECT 
		      //                  team.id as team_id,
		      //                  team.team_name,
		      //                  team.team_by,
        //                        team_members.id as team_members_id,
		      //                  department.dep_name,
		      //                  e.full_name,
		      //                  e.workemail,
		      //                  e.emp_code,
								//e_tl.full_name as teamlead
	       //                 FROM dbo.team WITH(NOLOCK)
	       //                 LEFT JOIN team_members ON team.id = team_members.team_id
	       //                 LEFT JOIN employee e ON team_members.emp_id = e.id
	       //                 LEFT JOIN employee e_tl ON team.team_lead_empid = e.id
	       //                 LEFT JOIN department ON team.team_department_id = department.id
	       //                 WHERE team.id =  @key 
        //                    AND team.is_deleted = 0
        //                    ORDER BY team.team_name ASC",
        //              param: new { key }
        //       );
        //}
        public IEnumerable<dynamic> FetchAllTeamsByOrgID(string key)
        {
            return Query<dynamic>(
                   sql: @"SELECT 
                            team.id as team_id,
                            team.team_name,
                            team.team_by,
                            team_members.id as team_members_id,
                            e.full_name,
                            e.workemail,
                            e.emp_code,
                            e_tl.full_name as team_lead
                            FROM dbo.team WITH(NOLOCK)
                            LEFT JOIN team_members ON team.id = team_members.team_id
                            LEFT JOIN employee e ON team_members.emp_id = e.id
                            LEFT JOIN employee e_tl ON team.team_lead_empid = e.id
                            WHERE team.org_id =  @key
                            AND team.is_deleted = 0
                            ORDER BY team.team_name ASC",
                      param: new { key }
               );
        }
        public IEnumerable<dynamic> FetchAllTeamMembersByTeamID(string key)
        {
            return Query<dynamic>(
                   sql: @"SELECT 
		                        team_members.id as team_members_id,
		                        team_members.emp_id,
			                    department.id as team_department_id,
		                        e_tl.full_name as name,
		                        department.dep_name,
			                    designation.designation_name,
		                        e_tl.workemail,
		                        e_tl.emp_code
	                        FROM dbo.team_members WITH(NOLOCK)
	                        LEFT JOIN employee e_tl ON team_members.emp_id = e_tl.id
		                    LEFT JOIN department on e_tl.deptid = department.id
		                    LEFT JOIN designation on e_tl.designation_id = designation.id
	                        WHERE team_members.team_id =  @key  
		                    AND team_members.is_deleted = 0
                            ORDER BY e_tl.full_name ASC",
                      param: new { key }
               );
        }

    }
}
