using MongoDB.Driver;
using Microsoft.Extensions.Options;
using AdminDashboard.Models;
using AdminDashboard.Repositories;
using AdminDashboard.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using AdminDashboard.Hubs; // Thêm namespace cho Hubs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình DatabaseSettings từ appsettings.json
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("DatabaseSettings"));

// Cấu hình MongoClient cho MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;

    if (string.IsNullOrEmpty(settings.ConnectionString))
    {
        throw new InvalidOperationException("MongoDB connection string is not configured in DatabaseSettings.");
    }

    return new MongoClient(settings.ConnectionString);
});

// Cấu hình Database MongoDB
builder.Services.AddSingleton(sp =>
{
    var mongoClient = sp.GetRequiredService<IMongoClient>();
    var settings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;

    if (string.IsNullOrEmpty(settings.DatabaseName))
    {
        throw new InvalidOperationException("MongoDB database name is not configured in DatabaseSettings.");
    }

    return mongoClient.GetDatabase(settings.DatabaseName);
});

builder.Services.AddDistributedMemoryCache(); // Dùng bộ nhớ để lưu trữ Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn của session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Đảm bảo session hoạt động ngay cả khi không có cookie
});

// Cấu hình dịch vụ xác thực cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login"; // Đường dẫn đăng nhập
        options.LogoutPath = "/Login/Logout"; // Đường dẫn đăng xuất
        options.AccessDeniedPath = "/Home/AccessDenied"; // Đường dẫn truy cập bị từ chối
    });

// Đăng ký các repository
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<EventRepository>();
builder.Services.AddScoped<CategoryEventRepository>();
builder.Services.AddScoped<AdminRepository>();
builder.Services.AddScoped<NotificationRepository>();

// Thêm dịch vụ SignalR
builder.Services.AddSignalR();

// Thêm dịch vụ Controllers với Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Cấu hình pipeline xử lý yêu cầu
if (!app.Environment.IsDevelopment())
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

// Map các endpoint SignalR Hub
app.MapHub<NotificationHub>("/notificationHub");

// Map các controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
