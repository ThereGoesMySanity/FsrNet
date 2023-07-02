using FsrNet.Controllers;
using FsrNet.Options;
using FsrNet.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<LocalProfileOptions>(builder.Configuration.GetSection("LocalProfile"));
builder.Services.Configure<SerialConnectionOptions>(builder.Configuration.GetSection("SerialConnection"));
builder.Services.AddOptions<ValuesPollOptions>().Bind(builder.Configuration.GetSection("ValuesPoll"))
            .ValidateDataAnnotations();

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddSingleton<SerialConnection>();
builder.Services.AddSingleton<ImageStore>();
builder.Services.AddSingleton<ProfileStore>();
builder.Services.AddSingleton<HubDataStore>();
builder.Services.AddHostedService<ValuesPoll>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();


app.MapControllers();

app.MapFallbackToFile("index.html");

app.MapHub<ProfileHub>("/profilehub");


app.Run();
