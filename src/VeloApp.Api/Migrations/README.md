# 将 ConnectionStrings:Default 改为文件库后启用迁移，例如：
# "Data Source=veloapp.db"
#
# 生成迁移：
#   dotnet ef migrations add <Name> --project src/VeloApp.Api
# 应用迁移（启动时非内存库会自动 Migrate）：
#   dotnet ef database update --project src/VeloApp.Api
