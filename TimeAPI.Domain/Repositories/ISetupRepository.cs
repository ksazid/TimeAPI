using System;
using System.Collections.Generic;
using System.Text;
using TimeAPI.Domain.Entities;

namespace TimeAPI.Domain.Repositories
{
    public interface ISetupRepository 
    {
        public IEnumerable<Country> Country();
        public IEnumerable<Timezones> Timezones();
        public IEnumerable<Country> PhoneCodes();

    }
}
