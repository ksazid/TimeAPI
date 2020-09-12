using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class CostProjectTaskRepository : RepositoryBase, ICostProjectTaskRepository
    {
        public CostProjectTaskRepository(IDbTransaction transaction) : base(transaction)
        { }

        public void Add(CostProjectTask entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.cost_task
                                  (id, project_id, is_selected, milestone_id, task_name, total_unit, default_unit_hours, unit_type, unit, qty, created_date, createdby)
                           VALUES (@id, @project_id, @is_selected, @milestone_id, @task_name, @total_unit, @default_unit_hours, @unit_type, @unit, @qty, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public async Task<CostProjectTask> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<CostProjectTask>(
                sql: @"SELECT dbo.project_activity_x_task.project_id as project_id, dbo.project_activity_x_task.activity_id as activtity_id, dbo.cost_task.* 
						FROM dbo.cost_task
						LEFT JOIN dbo.project_activity_x_task on dbo.cost_task.id = dbo.project_activity_x_task.task_id
						WHERE dbo.cost_task.is_deleted = 0 and dbo.cost_task.id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.cost_task
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public async Task RemoveByProjectID(string key)
        {
            await ExecuteAsync(
                sql: @"UPDATE dbo.cost_task
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE project_id = @key",
                param: new { key }
            );
        }

        public void Update(CostProjectTask entity)
        {
            Execute(
                sql: @"UPDATE dbo.cost_task
                   SET
                    project_id = @project_id,
					is_selected  = @is_selected, 
					milestone_id = @milestone_id, 
					task_name = @task_name, 
                    total_unit = @total_unit,
                    default_unit_hours = @default_unit_hours,
                    unit_type = @unit_type,
					unit = @unit, 
					qty = @qty,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby,
                    WHERE id =  @id",
                param: entity
            );
        }

        public async Task UpdateStaticCostProjectTask(CostProjectTask entity)
        {
            await ExecuteAsync(
                sql: @"UPDATE dbo.static_tasks
                   SET
					unit = @unit, 
					qty = @qty,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id =  @id",
                param: entity
            );
        }

        public async Task UpdateCostProjectTaskQtyTaskID(CostProjectTask entity)
        {
            await ExecuteAsync(
                sql: @"UPDATE dbo.cost_task
                   SET
					qty = @qty,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id =  @id",
                param: entity
            );
        }

        public async Task UpdateCostProjectNotesQtyTaskID(CostProjectTask entity)
        {
            await ExecuteAsync(
                sql: @"UPDATE dbo.cost_task
                   SET
					notes = @notes,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id =  @id",
                param: entity
            );
        }

        public async Task UpdateCostProjectDiscountAndTotalCostTaskID(CostProjectTask entity)
        {
            await ExecuteAsync(
                sql: @"UPDATE dbo.cost_task
                   SET
					discount_amount = @discount_amount,
					total_cost_amount = @total_cost_amount,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id =  @id",
                param: entity
            );
        }

        public async Task UpdateCostProjectBudgetedHoursTaskID(CostProjectTask entity)
        {
            await ExecuteAsync(
                sql: @"UPDATE dbo.cost_task
                   SET
					qty = @qty,
					total_cost_amount = @total_cost_amount,
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id =  @id",
                param: entity
            );
        }

        public async Task UpdateIsSelectedByTaskID(CostProjectTask entity)
        {
            await ExecuteAsync(
                 sql: @"UPDATE dbo.cost_task
                   SET
					is_selected  = @is_selected, 
                    modified_date = @modified_date,
                    modifiedby = @modifiedby
                    WHERE id =  @id",
                 param: entity
             );
        }

        public async Task<IEnumerable<CostProjectTask>> All()
        {
            return await QueryAsync<CostProjectTask>(
                sql: "SELECT * FROM [dbo].[cost_task] where is_deleted = 0"
            );
        }

        public async Task<IEnumerable<CostProjectTask>> GetAllStaticMilestoneTasksByMilestoneID(string MilestoneID, string OrgID)
        {
            return await QueryAsync<CostProjectTask>(
                sql: "SELECT * FROM [dbo].[static_tasks] where is_deleted = 0 and  milestone_id = @MilestoneID and org_id = @OrgID",
                 param: new { MilestoneID, OrgID }
            );
        }

        public async Task<IEnumerable<CostProjectTask>> GetAllMilestoneTasksByMilestoneID(string MilestoneID, string OrgID)
        {
            return await QueryAsync<CostProjectTask>(
                sql: "SELECT * FROM [dbo].[cost_task] where is_deleted = 0 and  milestone_id = @MilestoneID and is_selected = 1",
                 param: new { MilestoneID }
            );
        }
    }
}