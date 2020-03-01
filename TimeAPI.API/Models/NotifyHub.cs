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

        //public override Task OnConnectedAsync()
        //{

        //    if (string.IsNullOrEmpty(Context.ConnectionId))
        //    {
        //        throw new System.Exception(nameof(Context.ConnectionId)); //This code is never hit
        //    }
        //    Clients.All.BroadcastMessage("broadcastMessage", "system", $"{Context.ConnectionId} joined the conversation");
        //    return base.OnConnectedAsync();
        //}
        //public void Send(string name, string message)
        //{
        //    Clients.All.BroadcastMessage("broadcastMessage", name, message);
        //}
        //public override Task OnDisconnectedAsync(System.Exception exception)
        //{
        //    Clients.All.BroadcastMessage("broadcastMessage", "system", $"{Context.ConnectionId} left the conversation");
        //    return base.OnDisconnectedAsync(exception);
        //}
    }
}
