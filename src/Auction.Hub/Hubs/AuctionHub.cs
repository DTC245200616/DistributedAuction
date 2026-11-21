using Microsoft.AspNetCore.SignalR;

namespace Auction.Hub.Hubs;

public class AuctionHub(ILogger<AuctionHub> logger) : Microsoft.AspNetCore.SignalR.Hub
{
    public async Task JoinAuction(string auctionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, auctionId);
        logger.LogInformation("Client {ConnectionId} đã tham gia theo dõi phiên {AuctionId}", Context.ConnectionId, auctionId);
        await Clients.Caller.SendAsync("JoinedAuction", $"Đã tham gia theo dõi phiên đấu giá {auctionId}");
    }

    public async Task LeaveAuction(string auctionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, auctionId);
        logger.LogInformation("Client {ConnectionId} đã rời phiên {AuctionId}", Context.ConnectionId, auctionId);
    }
}