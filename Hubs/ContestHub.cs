using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CRICXI.Hubs
{
    public class ContestHub : Hub
    {
        // Notify all clients when contest data changes
        public async Task NotifyContestUpdate(string contestId)
        {
            await Clients.All.SendAsync("ReceiveContestUpdate", contestId);
        }
    }
}
