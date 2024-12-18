using MongoDB.Driver;
using Microsoft.Extensions.Options;
using AdminDashboard.Models;
using AdminDashboard.Repositories;
using AdminDashboard.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;

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

// Cấu hình Session (Sử dụng bộ nhớ để lưu trữ Session)
builder.Services.AddDistributedMemoryCache(); // Dùng bộ nhớ để lưu trữ session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn của session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Đảm bảo session hoạt động ngay cả khi không có cookie
});

// Cấu hình dịch vụ xác thực cookie (Authentication)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login"; // Đường dẫn đăng nhập
        options.LogoutPath = "/Login/Logout"; // Đường dẫn đăng xuất
        options.AccessDeniedPath = "/Home/AccessDenied"; // Đường dẫn truy cập bị từ chối
        options.Cookie.Name = "AppCookie"; // Đặt tên cookie
        options.Cookie.SameSite = SameSiteMode.Strict; // Xác định chiến lược SameSite cookie (khuyến nghị Strict)
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Đảm bảo chỉ gửi cookie qua HTTPS
    });

// Đăng ký các repository (Các lớp để truy xuất dữ liệu từ MongoDB)
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
    app.UseExceptionHandler("/Home/Error"); // Xử lý lỗi trong môi trường sản xuất
    app.UseHsts(); // Chuyển hướng tất cả yêu cầu HTTP tới HTTPS
}

app.UseHttpsRedirection(); // Tự động chuyển hướng HTTP sang HTTPS
app.UseStaticFiles(); // Cung cấp các tệp tĩnh (JS, CSS, hình ảnh, v.v.)
app.UseRouting(); // Định tuyến các yêu cầu đến controller hoặc endpoint

app.UseSession(); // Kích hoạt Session (đặt trước Authentication)
app.UseAuthentication(); // Kích hoạt xác thực
app.UseAuthorization(); // Kích hoạt phân quyền

// Map các controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}"); // Đặt controller mặc định là Login

// Chạy ứng dụng
app.Run();
