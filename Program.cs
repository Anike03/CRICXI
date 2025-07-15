using CRICXI.Hubs;
using CRICXI.Models;
using CRICXI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Load MongoDB settings from appsettings.json
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDBSettings"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = builder.Configuration.GetSection("MongoDBSettings").Get<MongoDBSettings>();
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddScoped(serviceProvider =>
{
    var settings = builder.Configuration.GetSection("MongoDBSettings").Get<MongoDBSettings>();
    var client = serviceProvider.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});

// 🔧 Register services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ContestService>();
builder.Services.AddScoped<ScoringService>();
builder.Services.AddScoped<LeaderboardService>();
builder.Services.AddScoped<CricbuzzApiService>();
builder.Services.AddScoped<MatchService>();
builder.Services.AddScoped<ContestEntryService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<CricketNewsService>();
builder.Services.AddScoped<LeaderboardService>();
builder.Services.AddSingleton<FantasyTeamService>();

builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings")
);

// 🔧 Add HttpClientFactory
builder.Services.AddHttpClient();

// 🔧 MVC + Session + SignalR
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddSignalR();

// 🔧 Cookie authentication (for Admin)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie();

// 🔧 CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://cricxi.vercel.app")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// 🔧 Error handling
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 🔧 HTTP pipeline
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("AllowReact"); // ✅ must be before UseRouting

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ContestHub>("/contestHub");

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
