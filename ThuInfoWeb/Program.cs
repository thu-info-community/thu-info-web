using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Identity;
using NLog;
using NLog.Web;
using ThuInfoWeb;
using ThuInfoWeb.Bots;
using ThuInfoWeb.DBModels;
using ThuInfoWeb.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Host.UseNLog();

// Add services to the container.
builder.Services.AddControllersWithViews().AddJsonOptions(x => x.JsonSerializerOptions.AllowTrailingCommas = true);
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddSingleton<LoginAttemptService>();
builder.Services.AddHttpClient("version-manager", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "THUInfoWeb/1.0");
});
builder.Services.AddHttpClient("feedback-notice", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "THUInfoWeb/1.0");
});
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = new PathString("/Home/Login");
        options.AccessDeniedPath = new PathString("/deny");
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
    });
builder.Services.AddSingleton<Data>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
    return new Data(
        configuration.GetConnectionString("Test") ?? "",
        environment.IsDevelopment(),
        serviceProvider.GetRequiredService<TimeProvider>());
});
// builder.Services.AddSingleton<SecretManager>();
builder.Services.AddSingleton<VersionManager>();
builder.Services.AddSingleton<IHostedService>(serviceProvider =>
    serviceProvider.GetRequiredService<VersionManager>());
builder.Services.AddScoped<UserManager>();
builder.Services.AddSingleton<FeedbackNoticeBot>();
builder.Services.AddSingleton<FeedbackNoticeDispatcher>();
builder.Services.AddSingleton<IHostedService>(serviceProvider =>
    serviceProvider.GetRequiredService<FeedbackNoticeDispatcher>());
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// if (!app.Environment.IsDevelopment())
//     app.UseHttpsRedirection();
var staticFileContentTypeProvider = new FileExtensionContentTypeProvider();
staticFileContentTypeProvider.Mappings[".apk"] = "application/vnd.android.package-archive";
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = staticFileContentTypeProvider
});
app.UseRouting();
app.UseHttpLoggingMiddleware(); // log http requests to database
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("default",
    "{controller}/{action}/{id?}");

app.MapFallbackToFile("/", "index.html");
app.MapFallbackToFile("/index", "index.html");
app.MapFallbackToFile("/download", "download.html");
app.MapFallbackToFile("/help", "help.html");
app.MapFallbackToFile("/privacy", "privacy.html");
app.MapFallbackToFile("/privacy-en", "privacy-en.html");
app.MapFallback("/deny", async r =>
{
    r.Response.StatusCode = 403;
    await r.Response.WriteAsync("access denied");
});
app.MapHub<ScheduleSyncHub>("/schedulesynchub");

app.Run();
LogManager.Shutdown();
