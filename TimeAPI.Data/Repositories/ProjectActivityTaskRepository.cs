using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class ProjectActivityTaskRepository : RepositoryBase, IProjectActivityTaskRepository
    {
        public ProjectActivityTaskRepository(IDbTransaction transaction) : base(transaction)
        { }
        public void Add(ProjectActivityTask entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.project_activity_x_task
                                  (id, project_id, activity_id, task_id, created_date, createdby)
                           VALUES (@id, @project_id, @activity_id, @task_id, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }
        public ProjectActivityTask Find(string key)
        {
            return QuerySingleOrDefault<ProjectActivityTask>(
                sql: "SELECT * FROM dbo.project_activity_x_task WHERE is_deleted = 0 and id = @key",
                param: new { key }
            );
        }
        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.project_activity_x_task
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE task_id = @key",
                param: new { key }
            );
        }
        public void Update(ProjectActivityTask entity)
        {
            Execute(
                sql: @"UPDATE dbo.project_activity_x_task
                   SET 
                    project_id = @project_id, 
                    activity_id = @activity_id, 
                    task_id = @task_id,
                    modified_date = @modified_date, 
                    modifiedby = @modifiedby
                    WHERE id = @id",
                param: entity
            );
        }

        public IEnumerable<ProjectActivityTask> All()
        {
            return Query<ProjectActivityTask>(
                sql: "SELECT * FROM [dbo].[project_activity_x_task] where is_deleted = 0"
            );
        }

        public void RemoveByProjectActivityID(string key)
        {
            Execute(
                sql: @"UPDATE dbo.project_activity_x_task
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE activity_id = @key",
                param: new { key }
            );
        }

        public void RemoveByProjectID(string key)
        {
            Execute(
                sql: @"UPDATE dbo.project_activity_x_task
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE project_id = @key",
                param: new { key }
            );
        }


        public IEnumerable<dynamic> GetAllTaskByActivityID(string key)
        {
            return Query<dynamic>(
                   sql: @"SELECT 
                            dbo.task.id, 
                            dbo.task.task_desc  
                        FROM dbo.task WITH(NOLOCK)
                        INNER JOIN  dbo.project_activity_x_task on task.id = dbo.project_activity_x_task.task_id
                        WHERE dbo.project_activity_x_task.activity_id = @key
                        AND dbo.task.is_deleted = 0
                        ORDER BY dbo.task.task_name DESC",
                      param: new { key }
               );
        }
    }
}
