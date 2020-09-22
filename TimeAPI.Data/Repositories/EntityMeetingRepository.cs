using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class EntityMeetingRepository : RepositoryBase, IEntityMeetingRepository
    {
        public EntityMeetingRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public void Add(EntityMeeting entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.entity_meeting
                            (id, org_id, entity_id, meeting_name, location, [desc], start_time, end_time, host,  created_date, createdby)
                    VALUES (@id, @org_id, @entity_id, @meeting_name, @location, @desc, @start_time, @end_time, @host,  @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public async Task<EntityMeeting> Find(string key)
        {
            return await QuerySingleOrDefaultAsync<EntityMeeting>(
                sql: "SELECT * FROM dbo.entity_meeting WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }

        public async Task<IEnumerable<EntityMeeting>> All()
        {
            return await QueryAsync<EntityMeeting>(
                sql: "SELECT * FROM dbo.entity_meeting where is_deleted = 0"
            );
        }

        public async Task<IEnumerable<EntityMeeting>> EntityMeetingByOrgID(string key)
        {
            return await QueryAsync<EntityMeeting>(
                sql: "SELECT * FROM dbo.entity_meeting where is_deleted = 0 and org_id = @key",
                param: new { key }
            );
        }

        public async Task<dynamic> EntityMeetingByEntityID(string key)
        {
            return await QueryAsync<dynamic>(
                sql: @"SELECT employee.full_name as host_name, dbo.entity_meeting.* FROM dbo.entity_meeting
                            LEFT JOIN employee on dbo.entity_meeting.host = employee.id
                        WHERE entity_meeting.is_deleted = 0 
                        AND entity_id =  @key",
                param: new { key }
            );
        }

        public async Task<dynamic> GetAllOpenActivitiesEntityID(string key)
        {
            return await QueryAsync<dynamic>(
                sql: @"SELECT 
                            dbo.task.id,
	                        dbo.task.task_name as subject,
	                        'Task' as entity_type,
	                        dbo.status.status_name as status,
	                        FORMAT(CAST(dbo.task.due_date AS date), 'MM/dd/yyyy', 'EN-US') as due_date,
                            NULL as call_type,
	                        NULL as start_time,
	                        NULL as end_time,
                            dbo.task.task_desc as remarks,
	                        dbo.employee.full_name as activity_owner,
	                        FORMAT(CAST(dbo.task.created_date AS date), 'MM/dd/yyyy', 'EN-US') AS ondate
                        FROM dbo.task
                        LEFT JOIN dbo.project_activity_x_task on dbo.task.id= project_activity_x_task.task_id
                        LEFT JOIN dbo.status on dbo.task.status_id =  dbo.status.id
                        LEFT JOIN dbo.employee on dbo.task.assigned_empid =  dbo.employee.id
                        WHERE project_activity_x_task.project_id = @key 
                        AND task.is_deleted = 0
                        AND dbo.status.status_name != 'Completed' 
                        AND dbo.task.is_local_activity = 1",
                param: new { key }
            );
        }

        public async Task<dynamic> GetAllCloseActivitiesEntityID(string key)
        {
            return await QueryAsync<dynamic>(
                sql: @"SELECT   *  FROM (
                                SELECT 
								    null as task_id,
	                                dbo.entity_meeting.id,
	                                dbo.entity_meeting.meeting_name as subject,
	                                'Meeting' as entity_type,
	                                NULL as status,
	                                NULL as due_date,
	                                NULL as call_type,
	                                dbo.entity_meeting.start_time,
	                                dbo.entity_meeting.end_time,
                                    dbo.entity_meeting.[desc] as remarks,
	                                dbo.employee.full_name as activity_owner,
	                                FORMAT(CAST(dbo.entity_meeting.created_date AS date), 'MM/dd/yyyy', 'EN-US') AS ondate
                                FROM dbo.entity_meeting 
                                LEFT JOIN dbo.employee on dbo.entity_meeting.host = dbo.employee.id
                                WHERE entity_id = @key AND entity_meeting.is_deleted = 0
                                

                                UNION ALL

                                SELECT 
									null as task_id,
	                                dbo.entity_call.id,
	                                dbo.entity_call.subject as subject,
	                                'Call' as entity_type,
	                                NULL as status,
	                                NULL as due_date,
	                                CASE
                                    WHEN dbo.entity_call.is_completed_call is not null THEN 'Completed Call'
                                    WHEN  dbo.entity_call.is_current_call  is not null THEN 'Current Call'
                                    WHEN  dbo.entity_call.is_schedule_call is not null THEN 'Schedule Call'
                                    ELSE NULL END as call_type,
	                                dbo.entity_call.start_time,
	                                dbo.entity_call.end_time,
                                    dbo.entity_call.call_desc as remarks,
	                                dbo.employee.full_name as activity_owner,
	                                FORMAT(CAST(dbo.entity_call.created_date AS date), 'MM/dd/yyyy', 'EN-US') AS ondate
                                FROM dbo.entity_call
                                LEFT JOIN dbo.employee on dbo.entity_call.host = dbo.employee.id
                                WHERE entity_id = @key AND entity_call.is_deleted = 0

								UNION ALL

                                SELECT 
	                                dbo.task.id as task_id,
	                                dbo.task.id,
	                                dbo.task.task_name as subject,
	                                'Task' as entity_type,
	                                dbo.status.status_name as status,
	                                FORMAT(CAST(dbo.task.due_date AS date), 'MM/dd/yyyy', 'EN-US') as due_date,
	                                NULL as call_type,
	                                NULL as start_time,
	                                NULL as end_time,
                                    dbo.task.task_desc as remarks,
	                                dbo.employee.full_name as activity_owner,
	                                FORMAT(CAST(dbo.task.created_date AS date), 'MM/dd/yyyy', 'EN-US') AS ondate
                                FROM dbo.task
								LEFT JOIN dbo.timesheet_activity on dbo.task.id = timesheet_activity.task_id
                                LEFT JOIN dbo.project_activity_x_task on dbo.task.id= project_activity_x_task.task_id
                                LEFT JOIN dbo.status on dbo.task.status_id =  dbo.status.id
                                LEFT JOIN dbo.employee on dbo.task.assigned_empid =  dbo.employee.id
                                WHERE project_activity_x_task.project_id = @key
								AND task.is_deleted = 0
								AND dbo.task.is_local_activity = 1  
								AND (dbo.status.status_name != 'Completed' 
								AND dbo.status.status_name != 'Open' AND dbo.status.status_name != 'In Progress')

								UNION ALL

							    SELECT 				
									dbo.task.id as task_id,
	                                dbo.timesheet_activity.id,
	                                dbo.timesheet_activity.task_name as subject,
	                                'Task' as entity_type,
	                                dbo.status.status_name as status,
	                                FORMAT(CAST(dbo.task.due_date AS date), 'MM/dd/yyyy', 'EN-US') as due_date,
	                                NULL as call_type,
	                                timesheet_activity.start_time as start_time,
	                                timesheet_activity.end_time as end_time,
                                    timesheet_activity.remarks as remarks,
									dbo.employee.full_name as activity_owner,
	                                FORMAT(CAST(dbo.timesheet_activity.created_date AS date), 'MM/dd/yyyy', 'EN-US') AS ondate
                                FROM dbo.timesheet_activity
								LEFT JOIN dbo.project_activity_x_task on dbo.timesheet_activity.task_id= project_activity_x_task.task_id
								LEFT JOIN dbo.task on dbo.timesheet_activity.task_id= task.id
								LEFT JOIN dbo.status on dbo.task.status_id =  dbo.status.id
                                LEFT JOIN dbo.timesheet on dbo.timesheet_activity.groupid =  dbo.timesheet.groupid
                                LEFT JOIN dbo.employee on dbo.timesheet.empid =  dbo.employee.id
                                WHERE timesheet_activity.project_id = @key  
								AND timesheet_activity.is_deleted = 0
								AND (dbo.task.is_local_activity = 1 )
							   ) X
                               ORDER BY  FORMAT(CAST(ondate AS datetime2), N'dd-MMM-yyyy HH:mm:ss', 'EN-US') DESC",
                param: new { key }
            );
        }

        public async Task<dynamic> GetLocalActivitieEntityID(string key)
        {
            return await QueryAsync<dynamic>(
                sql: @"SELECT   *  FROM (
                                SELECT 
								    null as task_id,
	                                dbo.entity_meeting.id,
	                                dbo.entity_meeting.meeting_name as subject,
	                                'Meeting' as entity_type,
	                                NULL as status,
	                                NULL as due_date,
	                                NULL as call_type,
	                                dbo.entity_meeting.start_time,
	                                dbo.entity_meeting.end_time,
                                    dbo.entity_meeting.[desc] as remarks,
	                                dbo.employee.full_name as activity_owner,
	                                FORMAT(CAST(dbo.entity_meeting.created_date AS date), 'MM/dd/yyyy', 'EN-US') AS ondate
                                FROM dbo.entity_meeting 
                                LEFT JOIN dbo.employee on dbo.entity_meeting.host = dbo.employee.id
                                WHERE entity_meeting.id = @key AND entity_meeting.is_deleted = 0

                                UNION 

                                SELECT 
									null as task_id,
	                                dbo.entity_call.id,
	                                dbo.entity_call.subject as subject,
	                                'Call' as entity_type,
	                                NULL as status,
	                                NULL as due_date,
	                                CASE
                                    WHEN dbo.entity_call.is_completed_call is not null THEN 'Completed Call'
                                    WHEN  dbo.entity_call.is_current_call  is not null THEN 'Current Call'
                                    WHEN  dbo.entity_call.is_schedule_call is not null THEN 'Schedule Call'
                                    ELSE NULL END as call_type,
	                                dbo.entity_call.start_time,
	                                dbo.entity_call.end_time,
                                    dbo.entity_call.call_desc as remarks,
	                                dbo.employee.full_name as activity_owner,
	                                FORMAT(CAST(dbo.entity_call.created_date AS date), 'MM/dd/yyyy', 'EN-US') AS ondate
                                FROM dbo.entity_call
                                LEFT JOIN dbo.employee on dbo.entity_call.host = dbo.employee.id
                                WHERE entity_call.id = @key AND entity_call.is_deleted = 0

								UNION 

                                SELECT 
	                                dbo.task.id as task_id,
	                                dbo.task.id,
	                                dbo.task.task_name as subject,
	                                'Task' as entity_type,
	                                dbo.status.status_name as status,
	                                FORMAT(CAST(dbo.task.due_date AS date), 'MM/dd/yyyy', 'EN-US') as due_date,
	                                NULL as call_type,
	                                NULL as start_time,
	                                NULL as end_time,
                                    dbo.task.task_desc as remarks,
	                                dbo.employee.full_name as activity_owner,
	                                FORMAT(CAST(dbo.task.created_date AS date), 'MM/dd/yyyy', 'EN-US') AS ondate
                                FROM dbo.task
								LEFT JOIN dbo.timesheet_activity on dbo.task.id = timesheet_activity.task_id
                                LEFT JOIN dbo.project_activity_x_task on dbo.task.id= project_activity_x_task.task_id
                                LEFT JOIN dbo.status on dbo.task.status_id =  dbo.status.id
                                LEFT JOIN dbo.employee on dbo.task.assigned_empid =  dbo.employee.id
                               WHERE project_activity_x_task.task_id = @key 
								AND task.is_deleted = 0
								AND dbo.task.is_local_activity = 1  
								AND (dbo.status.status_name != 'Completed' 
								AND dbo.status.status_name != 'Open' AND dbo.status.status_name != 'In Progress')

								UNION 

							    SELECT 				
									dbo.task.id as task_id,
	                                dbo.timesheet_activity.id,
	                                dbo.timesheet_activity.task_name as subject,
	                                'Task' as entity_type,
	                                dbo.status.status_name as status,
	                                FORMAT(CAST(dbo.task.due_date AS date), 'MM/dd/yyyy', 'EN-US') as due_date,
	                                NULL as call_type,
	                                timesheet_activity.start_time as start_time,
	                                timesheet_activity.end_time as end_time,
                                    timesheet_activity.remarks as remarks,
									dbo.employee.full_name as activity_owner,
	                                FORMAT(CAST(dbo.timesheet_activity.created_date AS date), 'MM/dd/yyyy', 'EN-US') AS ondate
                                FROM dbo.timesheet_activity
								LEFT JOIN dbo.project_activity_x_task on dbo.timesheet_activity.task_id= project_activity_x_task.task_id
								LEFT JOIN dbo.task on dbo.timesheet_activity.task_id= task.id
								LEFT JOIN dbo.status on dbo.task.status_id =  dbo.status.id
                                LEFT JOIN dbo.timesheet on dbo.timesheet_activity.groupid =  dbo.timesheet.groupid
                                LEFT JOIN dbo.employee on dbo.timesheet.empid =  dbo.employee.id
                                where dbo.timesheet_activity.id = @key
								AND timesheet_activity.is_deleted = 0
								AND (dbo.task.is_local_activity = 1 )
							   ) X
                               ORDER BY  FORMAT(CAST(ondate AS datetime2), N'dd-MMM-yyyy HH:mm:ss', 'EN-US') DESC",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.entity_meeting
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(EntityMeeting entity)
        {
            Execute(
                sql: @"UPDATE dbo.entity_meeting
                           SET 
                            entity_id = @entity_id, 
                            meeting_name = @meeting_name, 
                            location = @location, 
                            desc = @desc,
                            start_time = @start_time, 
                            end_time = @end_time,
                            host = @host,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }

        public async Task<dynamic> GetRecentTimesheetByEmpID(string EmpID, string Date)
        {
            return await QuerySingleOrDefaultAsync<string>(
                    sql: @"SELECT  TOP 1 groupid   
                                    FROM timesheet WITH (NOLOCK)
                                    WHERE empid = @empid
                                    AND FORMAT(CAST(timesheet.ondate AS DATE), 'd', 'EN-US') = FORMAT(CAST(@Date AS DATE), 'd', 'EN-US')
                                    AND timesheet.is_deleted = 0
                            ORDER BY FORMAT(CAST(timesheet.ondate AS DATETIME2), N'HH:mm') DESC;",
                    param: new { empid = EmpID, Date }
             );
        }

    }
}