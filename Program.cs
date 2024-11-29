using MongoDB.Driver;
using Microsoft.Extensions.Options;
using AdminDashboard.Models;
using AdminDashboard.Repositories;
using AdminDashboard.Data;

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
app.UseStaticFiles(); // Đảm bảo các tệp tĩnh như CSS, JS, hình ảnh có thể được tải
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
