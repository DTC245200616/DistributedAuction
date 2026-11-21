using Auction.Hub.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(policy =>
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(_ => true)
              .AllowCredentials());
});

var app = builder.Build();

app.UseCors();
app.MapHub<AuctionHub>("/hubs/auction");
app.MapGet("/", () => "Auction SignalR Hub is running!");

app.Run();