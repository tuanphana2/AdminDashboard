using MongoDB.Driver;
using Microsoft.Extensions.Options;
using AdminDashboard.Models;
using AdminDashboard.Repositories;
using AdminDashboard.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

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

builder.Services.AddControllersWithViews();

// Cấu hình dịch vụ xác thực cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login"; // Đường dẫn đăng nhập
        options.LogoutPath = "/Login/Logout"; // Đường dẫn đăng xuất
        options.AccessDeniedPath = "/Home/AccessDenied"; // Đường dẫn truy cập bị từ chối
    });

// Cấu hình MVC cho ứng dụng
builder.Services.AddControllersWithViews();

// Đăng ký UserRepository và EventRepository
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<EventRepository>();
builder.Services.AddScoped<CategoryEventRepository>();
builder.Services.AddScoped<AdminRepository>();
//builder.Services.AddScoped<NotificationRepository>();

// Thêm dịch vụ MVC cho các trang và API
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Cấu hình pipeline xử lý yêu cầu
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles(); // Đảm bảo các tệp tĩnh như CSS, JS, hình ảnh có thể được tải
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
