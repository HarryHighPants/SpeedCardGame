using System.Collections;
using Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSignalR();

//services cors
builder.Services.AddCors(options => options.AddDefaultPolicy(
    builder1 =>
    {
        builder1.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
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