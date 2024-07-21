using NLog;
using NLog.Web;
using ThuInfoWeb;
using ThuInfoWeb.Bots;
using ThuInfoWeb.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Host.UseNLog();

// Add services to the container.
builder.Services.AddControllersWithViews().AddJsonOptions(x => x.JsonSerializerOptions.AllowTrailingCommas = true);
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = new PathString("/Home/Login");
        options.AccessDeniedPath = new PathString("/deny");
    });
builder.Services.AddSingleton(new Data(builder.Configuration.GetConnectionString("Test") ?? "",
    builder.Environment.IsDevelopment()));
// builder.Services.AddSingleton<SecretManager>();
builder.Services.AddSingleton<VersionManager>();
builder.Services.AddScoped<UserManager>();
builder.Services.AddSingleton<FeedbackNoticeBot>();
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
app.UseStaticFiles();
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
