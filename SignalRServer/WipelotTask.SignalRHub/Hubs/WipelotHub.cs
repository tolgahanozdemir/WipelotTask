using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace WipelotTask.SignalRHub.Hubs
{
    public class WipelotHub : Hub
    {
        public async Task SendMessage(string message)
        {
        }
    }
}
