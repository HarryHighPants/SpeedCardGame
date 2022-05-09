using Engine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Server;
using Server.Auth;
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
builder.Services.AddSingleton<StatService, StatService>();
builder.Services.AddSingleton<EloService, EloService>();
builder.Services.AddSingleton(new GameEngine(new EngineChecks(), new EngineActions()));

builder.Services
	.AddAuthentication("Basic")
	.AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>("Basic", null);


builder.Services.AddDbContextPool<GameResultContext>(options =>
{
	options.UseNpgsql(builder.Configuration.GetConnectionString("GameResult"));
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

// add services to DI container
{
	var services = builder.Services;
	services.AddControllers();
}


//configure
var app = builder.Build();

using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
	await using var context = serviceScope.ServiceProvider.GetRequiredService<GameResultContext>();
	await context.Database.MigrateAsync();
}

app.UseRouting();
app.UseCors();

// configure HTTP request pipeline
{
	app.MapControllers();
}

app.MapGet("/", () => "Speed Card Game Server root");
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<GameHub>("/server");
});

app.Run();
