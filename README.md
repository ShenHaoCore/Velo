# VeloApp

ASP.NET Core Minimal API + MediatR + EF Core 的模块化单体（Vertical Slice）脚手架。

## 快速开始

```bash
dotnet run --project src/VeloApp.Api
```

- API：http://localhost:5213  
- Scalar：http://localhost:5213/scalar  
- OpenAPI：http://localhost:5213/openapi/v1.json  

```bash
dotnet test
```

也可用 `src/VeloApp.Api/VeloApp.Api.http` 发请求。

## 安装为 `dotnet new` 模板

```bash
dotnet new install <本仓库路径>
dotnet new velo -n MyApp
cd MyApp
dotnet run --project src/MyApp.Api
```

卸载：`dotnet new uninstall <本仓库路径>`

> **说明：** 模板 `sourceName` 为 `VeloApp`（不是 `Velo`），避免替换时误伤 `Development`（其中含有 `velo` 子串）。`shortName` 仍为 `velo`。

## 架构约定

- 按功能切片：每个功能的 Command/Query + Handler + Endpoint 放同一文件/文件夹
- Minimal API 路由直接绑定 `ISender.Send`，无 Controller
- Handler 直接注入 `AppDbContext`，不引入仓储
- 实体 `internal`；跨模块用 MediatR `INotification`
- 统一返回 `Result<T>`（成功/失败）

直接 `Send` 时，业务失败也返回 HTTP 200，通过 `isSuccess` / `error` 表达；若要映射 400/201，可自行包一层 `ToHttpResult()`。

## 技术栈

| 组件 | 版本 |
|------|------|
| .NET | 9 |
| MediatR | 14.x |
| EF Core + SQLite（内存） | 9.x |
| Microsoft.AspNetCore.OpenApi | 9.x |
| Scalar.AspNetCore | 2.x |

MediatR 14 在开发环境可能提示许可证警告；生产使用请参见 [Lucky Penny](https://luckypennysoftware.com)。本地 Demo 可忽略。

## 目录结构

```
VeloApp/
├── src/VeloApp.Api/
│   ├── Features/Tasks|Notifications
│   ├── Shared/          # AppDbContext、Result
│   └── Program.cs
├── tests/VeloApp.Api.Tests/
└── .template.config/
```
