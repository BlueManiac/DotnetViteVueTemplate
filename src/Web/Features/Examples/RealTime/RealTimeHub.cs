using Microsoft.AspNetCore.SignalR;

namespace Web.Features.Examples.RealTime;
public class RealTimeHub : Hub<IRealTimeClient>
{
    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.Message("Connected");
    }

    public Task Test(string message)
    {
        return Clients.Caller.Message("Server received " + message + " " + DateTime.Now);
    }
}

public interface IRealTimeClient
{
    public Task Message(string message);
}