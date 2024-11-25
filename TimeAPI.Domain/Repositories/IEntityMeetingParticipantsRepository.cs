﻿using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IEntityMeetingParticipantsRepository : IRepository<EntityMeetingParticipants, string>
    {
        void RemoveEntityMeetingParticipantsByMeetingID(string MeetingID);
        IEnumerable<EntityMeetingParticipants> EntityMeetingParticipantsByMeetingID(string MeetingID);
    }
}
