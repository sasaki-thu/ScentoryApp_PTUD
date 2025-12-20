using AspNetCoreHero.ToastNotification;
using Microsoft.AspNetCore.Authentication.Cookies;
using ScentoryApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// kết nối db
var stringConnectdb = builder.Configuration.GetConnectionString("ScentoryPTUD");
builder.Services.AddDbContext<ScentoryPtudContext>(options => options.UseSqlServer(stringConnectdb));
// hiểu kí tự tiếng Việt
builder.Services.AddSingleton<HtmlEncoder>(HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.All }));

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
    })
    .AddCookie("External") // cookie tạm để nhận info từ Google/Facebook
    .AddGoogle("Google", options =>
    {
        options.SignInScheme = "External";
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        options.Scope.Add("email");
        options.Scope.Add("profile");
    })
    .AddFacebook("Facebook", options =>
    {
        options.SignInScheme = "External";
        options.AppId = builder.Configuration["Authentication:Facebook:AppId"]!;
        options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"]!;
        options.Scope.Add("email");
        options.Fields.Add("email");
        options.Fields.Add("name");
    });
builder.Services.AddAuthorization();


builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 10;
    config.IsDismissable = true;
    config.Position = NotyfPosition.BottomRight;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();


app.UseAuthentication();
// Enforce admin access rules: only signed-in admins can reach /Admin routes.
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    if (path.HasValue && path.Value.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
    {
        var isAuthenticated = context.User?.Identity?.IsAuthenticated == true;
        if (!isAuthenticated)
        {
            var returnUrl = Uri.EscapeDataString(path + context.Request.QueryString);
            context.Response.Redirect($"/Home/Login?returnUrl={returnUrl}");
            return;
        }

        var role = context.User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        var isAdmin = role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                      || role.IndexOf("admin", StringComparison.OrdinalIgnoreCase) >= 0
                      || role.IndexOf("quan", StringComparison.OrdinalIgnoreCase) >= 0
                      || role.IndexOf("qu?n", StringComparison.OrdinalIgnoreCase) >= 0;
        if (!isAdmin)
        {
            var returnUrl = Uri.EscapeDataString(path + context.Request.QueryString);
            context.Response.Redirect($"/Home/AccessDenied?returnUrl={returnUrl}");
            return;
        }
    }

    await next();
});
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
