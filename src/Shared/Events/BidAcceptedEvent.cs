namespace Shared.Events;

public class BidAcceptedEvent
{
    public Guid AuctionId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string BidderName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}