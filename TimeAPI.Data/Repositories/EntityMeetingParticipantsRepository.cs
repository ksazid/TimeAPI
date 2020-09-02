using System.Collections.Generic;
using System.Data;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.Data.Repositories
{
    public class EntityMeetingParticipantsRepository : RepositoryBase, IEntityMeetingParticipantsRepository
    {
        public EntityMeetingParticipantsRepository(IDbTransaction transaction)
           : base(transaction)
        { }

        public void Add(EntityMeetingParticipants entity)
        {
            entity.id = ExecuteScalar<string>(
                    sql: @"INSERT INTO dbo.entity_meeting_x_participants
                            (id, meeting_id, entity_or_emp_id, created_date, createdby)
                    VALUES (@id, @meeting_id, @entity_or_emp_id, @created_date, @createdby);
                    SELECT SCOPE_IDENTITY()",
                    param: entity
                );
        }

        public EntityMeetingParticipants Find(string key)
        {
            return QuerySingleOrDefault<EntityMeetingParticipants>(
                sql: "SELECT * FROM dbo.entity_meeting_x_participants WHERE id = @key and is_deleted = 0",
                param: new { key }
            );
        }
         
        public IEnumerable<EntityMeetingParticipants> All()
        {
            return Query<EntityMeetingParticipants>(
                sql: "SELECT * FROM dbo.entity_meeting_x_participants where is_deleted = 0"
            );
        }

        public IEnumerable<EntityMeetingParticipants> EntityMeetingParticipantsByMeetingID(string key)
        {
            return Query<EntityMeetingParticipants>(
                sql: "SELECT * FROM dbo.entity_meeting_x_participants where is_deleted = 0 and meeting_id = @key",
                param: new { key }
            );
        }

        public void Remove(string key)
        {
            Execute(
                sql: @"UPDATE dbo.entity_meeting_x_participants
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void RemoveEntityMeetingParticipantsByMeetingID(string key)
        {
            Execute(
                sql: @"UPDATE dbo.entity_meeting_x_participants
                   SET
                       modified_date = GETDATE(), is_deleted = 1
                    WHERE id = @key",
                param: new { key }
            );
        }

        public void Update(EntityMeetingParticipants entity)
        {
            Execute(
                sql: @"UPDATE dbo.entity_meeting_x_participants
                           SET 
                            meeting_id = @meeting_id, 
                            entity_or_emp_id = @entity_or_emp_id,
                            modified_date = @modified_date,
                            modifiedby = @modifiedby
                         WHERE id = @id",
                param: entity
            );
        }
    }
}