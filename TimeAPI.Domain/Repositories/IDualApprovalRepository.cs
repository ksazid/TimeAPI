using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IDualApprovalRepository : IRepository<DualApproval, string>
    {
        //DualApproval FindByEntityID(EntityID);
    }
}
