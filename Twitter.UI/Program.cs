using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Twitter.UI.Helper;
using Twitter.Core.Services;
using Twitter.Data.Contexts;
using Twitter.Data.UnitOfWork;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<TwitterContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Twitter")));    
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddTransient(typeof(UserService));
builder.Services.AddSession(options => {
	options.Cookie.Name = "TwitterSession";
	options.Cookie.IsEssential = true;
	options.IdleTimeout = TimeSpan.FromMinutes(60); 
	options.Cookie.HttpOnly = true; 
});
builder.Services.AddMemoryCache();



builder.Services.AddMvc();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.LoginPath = "/Account/Login";
		options.AccessDeniedPath = "/Account/AccessDenied";
	});
builder.Services.AddHttpContextAccessor();

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

app.UseAuthorization();
app.UseAuthentication();
app.UseSession();

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
