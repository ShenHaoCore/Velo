using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using VeloApp.Api.Features.Tasks;
using VeloApp.Api.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

// MediatR：扫描本程序集中的 Handler / NotificationHandler
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// 连接串来自配置；内存库需 keepalive，避免共享缓存被回收
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=VeloApp;Mode=Memory;Cache=Shared";
var useInMemorySqlite = connectionString.Contains("Mode=Memory", StringComparison.OrdinalIgnoreCase);

if (useInMemorySqlite)
{
    var keepAliveConnection = new SqliteConnection(connectionString);
    keepAliveConnection.Open();
    builder.Services.AddSingleton(keepAliveConnection);
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// OpenAPI 文档 + Scalar UI
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        if (error is BadHttpRequestException badRequest)
        {
            await Results.Problem(
                    title: badRequest.Message,
                    statusCode: badRequest.StatusCode)
                .ExecuteAsync(context);
            return;
        }

        await Results.Problem(
                title: "An unexpected error occurred.",
                statusCode: StatusCodes.Status500InternalServerError)
            .ExecuteAsync(context);
    });
});

// 内存库用 EnsureCreated；文件/服务器库用 Migrate（见 Migrations/）
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (useInMemorySqlite)
        await db.Database.EnsureCreatedAsync();
    else
        await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // 默认 /scalar，读取 /openapi/v1.json
}

// Vertical Slice：各功能自行注册路由，直接映射到 MediatR
app.MapCreateTask();
app.MapGetTasks();
app.MapCompleteTask();
app.MapDeleteTask();

app.Run();

// 供集成测试引用
public partial class Program;
