using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkLog.Data;
using WorkLog.Middleware;
using WorkLog.Models;
using WorkLog.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<WorkLogContext>(options => options.UseSqlServer(DbConstant.WorkLogDBConnectionString ?? throw new InvalidOperationException("Connection string 'WorkLogContext' not found.")));

builder.Services.AddIdentity<AppUser, IdentityRole>().AddEntityFrameworkStores<WorkLogContext>().AddDefaultTokenProviders()
    .AddRoles<IdentityRole>();

builder.Services.Configure<IdentityOptions>(opt =>
{
    opt.Password.RequiredLength = 6;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireDigit = true;
    opt.Password.RequireLowercase = true;
    opt.Password.RequireUppercase = false;
    opt.User.RequireUniqueEmail = true;
    opt.Lockout.MaxFailedAccessAttempts = 3;
    opt.Lockout.DefaultLockoutTimeSpan = System.TimeSpan.FromMinutes(60);
});

builder.Services.AddHttpContextAccessor();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/SignIn";
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout= TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("Session:TimeOut"));
});

//builder.Services.AddHostedService<IISApplicationLifetime>();

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
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.UseMiddleware<TenantMiddleware>();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=WorkLog}/{action=Welcome}");

app.Run();
