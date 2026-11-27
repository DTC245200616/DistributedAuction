using Auction.Api.Data;
using BidValidator.Grpc;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuctionDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Tự động tạo bảng trong PostgreSQL khi khởi chạy
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
    db.Database.EnsureCreated();
}

// 1. Tạo phiên đấu giá mới
app.MapPost("/auctions", async (CreateAuctionRequest req, AuctionDbContext db, ILogger<Program> logger) =>
{
    if (string.IsNullOrWhiteSpace(req.ProductName) || req.StartingPrice <= 0 || req.DurationMinutes <= 0)
        return Results.BadRequest("Thông tin phiên đấu giá không hợp lệ.");

    var auction = new AuctionItem
    {
        ProductName = req.ProductName,
        Description = req.Description,
        StartingPrice = req.StartingPrice,
        CurrentPrice = req.StartingPrice,
        MinStep = req.MinStep > 0 ? req.MinStep : 10,
        StartTime = DateTime.UtcNow,
        EndTime = DateTime.UtcNow.AddMinutes(req.DurationMinutes),
        Status = AuctionStatus.Open
    };

    db.Auctions.Add(auction);
    await db.SaveChangesAsync();

    logger.LogInformation("Đã tạo phiên đấu giá {AuctionId} - {ProductName}", auction.Id, auction.ProductName);
    return Results.Created($"/auctions/{auction.Id}", auction);
});

// 2. Tra cứu danh sách phiên đấu giá
app.MapGet("/auctions", async (AuctionDbContext db) =>
{
    var list = await db.Auctions.OrderByDescending(a => a.StartTime).ToListAsync();
    return Results.Ok(list);
});

// 3. Tra cứu chi tiết 1 phiên đấu giá
app.MapGet("/auctions/{id:guid}", async (Guid id, AuctionDbContext db) =>
{
    var auction = await db.Auctions.FindAsync(id);
    return auction is not null ? Results.Ok(auction) : Results.NotFound("Không tìm thấy phiên đấu giá.");
});

// 4. Gửi mức giá mới (Đặt giá - gọi sang BidValidator.Grpc kiểm tra)
app.MapPost("/auctions/{id:guid}/bids", async (Guid id, PlaceBidRequest req, AuctionDbContext db, ILogger<Program> logger) =>
{
    var auction = await db.Auctions.FindAsync(id);
    if (auction is null)
        return Results.NotFound("Không tìm thấy phiên đấu giá.");

    // Gọi sang BidValidator.Grpc (cổng 5088) để kiểm tra lượt trả giá
    using var channel = GrpcChannel.ForAddress("http://localhost:5088");
    var grpcClient = new BidValidator.Grpc.BidValidator.BidValidatorClient(channel);

    var validationReply = await grpcClient.ValidateBidAsync(new ValidateBidRequest
    {
        BidderName = req.BidderName ?? "",
        BidAmount = (double)req.Amount,
        CurrentPrice = (double)auction.CurrentPrice,
        MinStep = (double)auction.MinStep,
        EndTimeUnix = new DateTimeOffset(auction.EndTime).ToUnixTimeSeconds(),
        Status = (int)auction.Status
    });

    if (!validationReply.IsValid)
        return Results.BadRequest(validationReply.Message);

    auction.CurrentPrice = req.Amount;
    auction.HighestBidder = req.BidderName;
    await db.SaveChangesAsync();

    logger.LogInformation("{Bidder} đã đặt giá {Amount} cho phiên {AuctionId}", req.BidderName, req.Amount, id);
    return Results.Ok(auction);
});

app.Run();