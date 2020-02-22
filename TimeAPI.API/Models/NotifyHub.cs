using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TimeAPI.Domain.Repositories;

namespace TimeAPI.API.Models
{
    public class NotifyHub : Hub<ITypedHubClient>
    {

    }


}
