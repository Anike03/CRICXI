using CRICXI.Models;
using CRICXI.Services;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authentication.Cookies;

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
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ContestService>();
builder.Services.AddScoped<ScoringService>();
builder.Services.AddScoped<LeaderboardService>();
builder.Services.AddScoped<FantasyTeamService>();
builder.Services.AddScoped<CricbuzzApiService>();
builder.Services.AddScoped<MatchService>();
builder.Services.AddScoped<ContestEntryService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<CricketNewsService>();


// 🔧 Add HttpClientFactory services
builder.Services.AddHttpClient();  // 👈 Add this line to register IHttpClientFactory

// 🔧 Add controllers + session
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

// 🔧 Authentication basic (Cookie based only for Admin login right now)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie();

// 🔧 Setup CORS to allow frontend React (Vite) connection
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // 🔧 Your frontend React dev server URL
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
app.UseRouting();
app.UseCors("AllowReact");
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// 🔧 Map both API routes & optional Razor MVC
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();