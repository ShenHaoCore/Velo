using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace VeloApp.Api.Tests;

/// <summary>
/// 每个工厂实例使用独立的共享内存库名，避免测试间数据串扰。
/// </summary>
public sealed class VeloWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString =
        $"Data Source=VeloApp_Test_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", _connectionString);
        builder.UseEnvironment("Development");
    }
}
