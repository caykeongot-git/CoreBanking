Write-Host ">>> BAT DAU HA CAP TOAN BO SOLUTION VE .NET 8.0 (STABLE)..." -ForegroundColor Cyan

# 1. Danh sach cac project
$projects = @(
    "CoreBanking.DAL/CoreBanking.DAL.csproj",
    "CoreBanking.BLL/CoreBanking.BLL.csproj",
    "CoreBanking.API/CoreBanking.API.csproj",
    "CoreBanking.WinUI/CoreBanking.WinUI.csproj"
)

# 2. Sua file .csproj: Doi net9.0 -> net8.0
foreach ($proj in $projects) {
    if (Test-Path $proj) {
        Write-Host "Creating backup & Updating framework for: $proj" -ForegroundColor Yellow
        (Get-Content $proj).Replace('net9.0', 'net8.0') | Set-Content $proj
    }
}

# 3. Go bo cac goi gay loi (Version 9.x, 10.x)
Write-Host ">>> DANG GO BO CAC GOI LOI..." -ForegroundColor Magenta
dotnet remove CoreBanking.WinUI/CoreBanking.WinUI.csproj package Microsoft.Extensions.Hosting
dotnet remove CoreBanking.WinUI/CoreBanking.WinUI.csproj package Microsoft.Extensions.DependencyInjection
dotnet remove CoreBanking.WinUI/CoreBanking.WinUI.csproj package Microsoft.EntityFrameworkCore.Design
dotnet remove CoreBanking.DAL/CoreBanking.DAL.csproj package Microsoft.EntityFrameworkCore.SqlServer
dotnet remove CoreBanking.DAL/CoreBanking.DAL.csproj package Microsoft.EntityFrameworkCore.Tools

# 4. Cai dat lai phien ban 8.0.0 (SIEU ON DINH)
Write-Host ">>> DANG CAI DAT LAI PHIEN BAN 8.0.0..." -ForegroundColor Green

# --- DAL ---
dotnet add CoreBanking.DAL/CoreBanking.DAL.csproj package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0
dotnet add CoreBanking.DAL/CoreBanking.DAL.csproj package Microsoft.EntityFrameworkCore.Tools --version 8.0.0

# --- WinUI ---
dotnet add CoreBanking.WinUI/CoreBanking.WinUI.csproj package Microsoft.EntityFrameworkCore.Design --version 8.0.0
dotnet add CoreBanking.WinUI/CoreBanking.WinUI.csproj package Microsoft.Extensions.Hosting --version 8.0.0
dotnet add CoreBanking.WinUI/CoreBanking.WinUI.csproj package Microsoft.Extensions.DependencyInjection --version 8.0.0

# 5. Clean & Build
Write-Host ">>> CLEAN & BUILD..." -ForegroundColor Cyan
dotnet clean
dotnet build

Write-Host ">>> HOAN TAT! HAY CHAY THU PROJECT." -ForegroundColor Green