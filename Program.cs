using CRICXI.Models;
using CRICXI.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Load MongoDB settings
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));

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

// 🔧 Register custom services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ContestService>();
builder.Services.AddScoped<ScoringService>();
builder.Services.AddScoped<LeaderboardService>();
builder.Services.AddScoped<FantasyTeamService>();
builder.Services.AddScoped<CricbuzzApiService>();
builder.Services.AddScoped<MatchService>();



// 🔧 MVC and session
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

var app = builder.Build();

// 🔧 Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

// 🔧 Routes
app.MapControllerRoute(
    name: "verify-email",
    pattern: "verify/{token?}",
    defaults: new { controller = "Auth", action = "VerifyEmail" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
