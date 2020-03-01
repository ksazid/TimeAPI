using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TimeAPI.Domain.Repositories
{
    public interface ITypedHubClient
    {
        Task BroadcastMessage(string type, string payload);
        void BroadcastMessage(string v1, string v2, string v3);
    }
}
