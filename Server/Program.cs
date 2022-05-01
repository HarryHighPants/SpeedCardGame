using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Server;
using Server.Hubs;
using Server.Models.Database;
using Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSignalR().AddHubOptions<GameHub>(options =>
{
	options.ClientTimeoutInterval = TimeSpan.FromSeconds(15);
	options.KeepAliveInterval = TimeSpan.FromSeconds(7.5);
});
builder.Services.AddSingleton<IGameService, InMemoryGameService>();
builder.Services.AddSingleton<IBotService, BotService>();

builder.Services.AddDbContextPool<GameResultContext>(options =>
{
	options.UseSqlite(builder.Configuration.GetConnectionString("GameResult"));
});

//services cors
builder.Services.AddCors(options => options.AddDefaultPolicy(
    builder1 =>
    {
        builder1
	        .SetIsOriginAllowed(s=>true)
            .AllowAnyHeader()
            .AllowCredentials()
            .AllowAnyMethod();
    })
);


//configure
var app = builder.Build();

using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
	await using var context = serviceScope.ServiceProvider.GetRequiredService<GameResultContext>();
	await context.Database.MigrateAsync();
}

app.UseRouting();
app.UseCors();

app.MapGet("/", () => "Speed Card Game Server root");
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<GameHub>("/server");
});

app.Run();
