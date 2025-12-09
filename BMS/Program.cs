using BMS.Data;
using BMS.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BMS.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Add DbContext FIRST
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Add Identity
builder.Services.AddDefaultIdentity<AppUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// 3. Add MVC + Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Optional timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddRazorPages();

// 4. Add TmdbService as SCOPED (recommended)
builder.Services.AddScoped<TmdbService>();

var app = builder.Build();

// 5. Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();


// 6. Authentication BEFORE Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// 7. Map Razor Pages for Identity
app.MapRazorPages();

// 8. MVC route (DEFAULT route)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Movies}/{action=NowPlayingFromApi}/{id?}");

// If you want Login FIRST instead, comment the above route and use this:
// app.MapControllerRoute(
//     name: "default",
//     pattern: "{area=Identity}/{page=/Account/Login}");

app.Run();
