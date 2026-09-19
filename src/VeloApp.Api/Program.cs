using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using VeloApp.Api.Features.Tasks;
using VeloApp.Api.Shared;

var builder = WebApplication.CreateBuilder(args);

// MediatR：扫描本程序集中的 Handler / NotificationHandler
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// SQLite 内存库：keepalive 连接仅防止库被回收；DbContext 各自用连接字符串开连接（避免共用同一实例的线程安全问题）
const string sqliteConnectionString = "Data Source=VeloApp;Mode=Memory;Cache=Shared";
var keepAliveConnection = new SqliteConnection(sqliteConnectionString);
keepAliveConnection.Open();
builder.Services.AddSingleton(keepAliveConnection);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(sqliteConnectionString));

// OpenAPI 文档 + Scalar UI
builder.Services.AddOpenApi();

var app = builder.Build();

// 启动时建表（内存库无迁移必要）
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // 默认 /scalar，读取 /openapi/v1.json
}

// Vertical Slice：各功能自行注册路由，直接映射到 MediatR
app.MapCreateTask();
app.MapGetTasks();

app.Run();

// 供集成测试引用
public partial class Program;
