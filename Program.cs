using MongoDB.Driver;
using Microsoft.Extensions.Options;
using AdminDashboard.Models;
using AdminDashboard.Repositories;
using AdminDashboard.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using AdminDashboard.Services; // Thêm namespace chứa WebSocket services

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

// Cấu hình bộ nhớ lưu trữ session
builder.Services.AddDistributedMemoryCache();
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
        options.LoginPath = "/Login/Login";
        options.LogoutPath = "/Login/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
    });

// Đăng ký MongoDbContext
builder.Services.AddScoped<MongoDbContext>();

// Đăng ký các Repository và Services
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<EventRepository>();
builder.Services.AddScoped<CategoryEventRepository>();
builder.Services.AddScoped<AdminRepository>();
builder.Services.AddScoped<NotificationService>();

// Đăng ký các dịch vụ WebSocket
builder.Services.AddSingleton<WebSocketConnectionManager>();
//builder.Services.AddSingleton<WebSocketMessageService>();
//builder.Services.AddSingleton<WebSocketHandler>();

// Đăng ký các dịch vụ MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Cấu hình middleware WebSocket
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles(); // Đảm bảo các tệp tĩnh như CSS, JS, hình ảnh có thể được tải
app.UseRouting();

// Cấu hình WebSocket endpoint
app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        //var webSocketHandler = app.Services.GetRequiredService<WebSocketHandler>();
        //await webSocketHandler.HandleWebSocketConnectionAsync(context, webSocket);
    }
    else
    {
        context.Response.StatusCode = 400; // Bad Request nếu không phải WebSocket
    }
});

// Cấu hình các route controller khác
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
