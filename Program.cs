using CRICXI.Models;
using CRICXI.Services;

using MongoDB.Driver;

using Microsoft.AspNetCore.Authentication.Cookies;


var builder = WebApplication.CreateBuilder(args);

// 🔧 Load MongoDB settings
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

// 🔧 Register all services
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

// 🔧 MVC and session
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

// 🔧 Authentication & OAuth
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie()

.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["OAuth:Google:ClientId"];
    options.ClientSecret = builder.Configuration["OAuth:Google:ClientSecret"];
})

.AddFacebook(options =>
{
    options.AppId = builder.Configuration["OAuth:Facebook:AppId"];
    options.AppSecret = builder.Configuration["OAuth:Facebook:AppSecret"];
})

.AddOAuth("Twitter", options =>
{
    options.ClientId = builder.Configuration["OAuth:Twitter:ClientId"];
    options.ClientSecret = builder.Configuration["OAuth:Twitter:ClientSecret"];
    options.CallbackPath = "/signin-twitter";
    options.AuthorizationEndpoint = "https://twitter.com/i/oauth2/authorize";
    options.TokenEndpoint = "https://api.twitter.com/2/oauth2/token";
    options.UserInformationEndpoint = "https://api.twitter.com/2/users/me";
    options.SaveTokens = true;
})

.AddOAuth("Instagram", options =>
{
    options.ClientId = builder.Configuration["OAuth:Instagram:ClientId"];
    options.ClientSecret = builder.Configuration["OAuth:Instagram:ClientSecret"];
    options.CallbackPath = "/signin-instagram";
    options.AuthorizationEndpoint = "https://api.instagram.com/oauth/authorize";
    options.TokenEndpoint = "https://api.instagram.com/oauth/access_token";
    options.UserInformationEndpoint = "https://graph.instagram.com/me";
    options.SaveTokens = true;
});

// 🔧 Build pipeline
var app = builder.Build();

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
app.UseAuthentication();
app.UseAuthorization();

// 🔧 Routing
app.MapControllerRoute(
    name: "verify-email",
    pattern: "verify/{token?}",
    defaults: new { controller = "Auth", action = "VerifyEmail" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
