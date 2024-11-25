﻿using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IEntityInvitationRepository : IRepository<EntityInvitation, string>
    {
        void RemoveByEntityID(string OrgID);
    }
}
