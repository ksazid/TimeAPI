using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class TaskTeamMembersRepository : RepositoryBase, ITaskTeamMembersRepository
    {

        public TaskTeamMembersRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(TaskTeamMember entity)
        {

            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.task_team_members
                                  (id, task_id, empid, created_date, createdby)
                           VALUES (@id, @task_id, @empid, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public TaskTeamMember Find(string key)
        {
            return QuerySingleOrDefault<TaskTeamMember>(
                sql: "SELECT * FROM dbo.task_team_members WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.task_team_members
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(TaskTeamMember entity)
        {
            Execute(
                sql: @"UPDATE dbo.task_team_members
                   SET 
                    task_id = @task_id,
                    empid = @empid,
                    modified_date = @modified_date, 
                    modifiedby = @modifiedby
                    WHERE id =  @id",
                param: entity
            );
        }

        public IEnumerable<TaskTeamMember> All()
        {
            return Query<TaskTeamMember>(
                sql: "SELECT * FROM [dbo].[task_team_members] where is_deleted = 0"
            );
        }

        public void RemoveByTaskID(string key)
        {
            Execute(
                sql: @"UPDATE dbo.task_team_members
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE task_id = @key",
                param: new { key }
            );
        }
    }
}
