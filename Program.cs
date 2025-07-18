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

// 🔧 Enhanced CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins(
            "https://cricxi.vercel.app",  // Vercel production URL
            "http://localhost:5173"       // Local development
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });

    // Fallback policy for all other requests
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
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

// Handle OPTIONS requests first
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin", context.Request.Headers["Origin"]);
        context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
        return;
    }
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

// Apply CORS middleware once before routing
app.UseCors("AllowReact");

// Add security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
    await next();
});

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
