using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface IProfileImageRepository : IRepository<Image, string>
    {
        public Image FindByProfileUserID(string key);
    }
}
