using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using WebBanHang.Models;
using WebBanHang.Repositories;
using WebBanHang.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false; 
})
.AddDefaultTokenProviders()
.AddDefaultUI()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

// Thêm Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(SD.Role_Admin));
    options.AddPolicy("CompanyOnly", policy => policy.RequireRole(SD.Role_Company));
    options.AddPolicy("AdminOrCompany", policy => policy.RequireRole(SD.Role_Admin, SD.Role_Company));
    options.AddPolicy("AdminOrEmployee", policy => policy.RequireRole(SD.Role_Admin, SD.Role_Employee));
});

// Đăng ký Repository
builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
builder.Services.AddScoped<IProductImageRepository, EFProductImageRepository>();
builder.Services.AddScoped<ICartRepository, EFCartRepository>();
builder.Services.AddScoped<IOrderRepository, EFOrderRepository>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
 options.IdleTimeout = TimeSpan.FromMinutes(30);
 options.Cookie.HttpOnly = true;
 options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

// Admin area route
app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Initialize database and create admin user
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.Initialize(scope.ServiceProvider);
}

app.Run();
