using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace Auction.Api.Data;

public class AuctionDbContext(DbContextOptions<AuctionDbContext> options) : DbContext(options)
{
    public DbSet<AuctionItem> Auctions => Set<AuctionItem>();
}