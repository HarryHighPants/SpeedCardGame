using Server;
using Server.Hubs;
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


//services cors
builder.Services.AddCors(options => options.AddDefaultPolicy(
    builder1 =>
    {
	    // builder1.WithOrigins("http://192.168.20.35:3000")
        builder1
	        .SetIsOriginAllowed(s=>true)
            .AllowAnyHeader()
            .AllowCredentials()
            .AllowAnyMethod();
    })
);


//configure
var app = builder.Build();
app.UseRouting();

app.UseCors();

app.MapGet("/", () => "Hello World!");
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<GameHub>("/server");
});

app.Run();
