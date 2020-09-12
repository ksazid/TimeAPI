using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
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

        public async Task<TaskTeamMember> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<TaskTeamMember>(
                sql: "SELECT * FROM dbo.task_team_members WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }

        public async Task<IEnumerable<TaskTeamMember>> FindByTaskID(string key)
        {
            return await QueryAsync<TaskTeamMember>(
                sql: "SELECT * FROM dbo.task_team_members WHERE is_deleted = 0 and task_id = @key",
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

        public async Task<IEnumerable<TaskTeamMember>> All()
        {
            return await QueryAsync<TaskTeamMember>(
                sql: "SELECT * FROM [dbo].[task_team_members] where is_deleted = 0"
            );
        }

        public async Task RemoveByTaskID(string key)
        {
            await ExecuteAsync(
                 sql: @"UPDATE dbo.task_team_members
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE task_id = @key",
                 param: new { key }
             );
        }
    }
}