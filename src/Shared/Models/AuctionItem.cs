namespace Shared.Models;

public enum AuctionStatus
{
    Open,
    Closed
}

public class AuctionItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal MinStep { get; set; }
    public string? HighestBidder { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime EndTime { get; set; }
    public AuctionStatus Status { get; set; } = AuctionStatus.Open;
}

public record CreateAuctionRequest(
    string ProductName,
    string Description,
    decimal StartingPrice,
    decimal MinStep,
    int DurationMinutes
);

public record PlaceBidRequest(
    string BidderName,
    decimal Amount
);