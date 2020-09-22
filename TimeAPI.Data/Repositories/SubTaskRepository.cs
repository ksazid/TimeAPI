using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class SubTaskRepository : RepositoryBase, ISubTaskRepository
    {
        public SubTaskRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(SubTasks entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.sub_task
                                  (id, task_id, sub_task_name, sub_task_desc, priority_id, status_id, lead_id, due_date, unit_type, qty, created_date, createdby, is_approver, is_approver_id)
                           VALUES (@id, @task_id, @sub_task_name, @sub_task_desc, @priority_id, @status_id, @lead_id, @due_date, @unit_type, @qty, @created_date, @createdby, @is_approver, @is_approver_id);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public async Task<SubTasks> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<SubTasks>(
                sql: @"SELECT  * from dbo.sub_task where id = @key",
                param: new { key }
            );
        }
        public async Task<IEnumerable<SubTasks>> FindSubTaskByTaskID(string key)
        {
            return await QueryAsync<SubTasks>(
                sql: @"SELECT  * from dbo.sub_task where task_id = @key",
                param: new { key }
            );
        }
       
        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.sub_task
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(SubTasks entity)
        {
            Execute(
                sql: @"UPDATE dbo.sub_task
                   SET
                    task_id = @task_id,
					sub_task_name = @sub_task_name, 
					sub_task_desc = @sub_task_desc,
                    priority_id = @priority_id,
                    status_id = @status_id,
                    lead_id = @lead_id,
                    due_date = @due_date,
					unit_type = @unit_type, 
					qty = @qty, 
                    modified_date = @modified_date,
                    modifiedby = @modifiedby,
                    is_approver = @is_approver,
                    is_approver_id = @is_approver_id
                    WHERE id =  @id",
                param: entity
            );
        }

        public async Task<IEnumerable<SubTasks>> All()
        {
            return await QueryAsync<SubTasks>(
                sql: "SELECT * FROM dbo.sub_task where is_deleted = 0"
            );
        }



        public async Task UpdateSubTaskStatus(SubTasks entity)
        {
            await ExecuteAsync(
                 sql: @"UPDATE dbo.sub_task
                   SET
                    status_id = @status_id,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id =  @id",
                 param: entity
             );
        }

        public async Task UpdateSubTaskLeadBySubTaskID(SubTasks entity)
        {
            await ExecuteAsync(
                 sql: @"UPDATE dbo.sub_task
                   SET
                    lead_id = @lead_id,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id =  @id",
                 param: entity
             );
        }

        
    }
}