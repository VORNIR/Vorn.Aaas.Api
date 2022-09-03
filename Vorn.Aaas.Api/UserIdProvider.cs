using Microsoft.AspNetCore.SignalR;

internal class UserIdProvider: IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        return connection.User?.Claims.SingleOrDefault(C=>C.Type== "client_id")?.Value;
    }
}