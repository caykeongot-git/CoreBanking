# -----------------------------------------------------------------------------
# CORE BANKING SOLUTION INITIALIZATION SCRIPT (.NET 8)
# Architect: Senior Lead
# -----------------------------------------------------------------------------

Write-Host ">>> KHOI TAO SOLUTION CORE BANKING..." -ForegroundColor Cyan

# 1. Tao Solution
dotnet new sln -n CoreBanking

# 2. Tao cac Project thanh phan (3-Layer + WinUI)
Write-Host ">>> Dang tao cac Project..." -ForegroundColor Yellow
dotnet new classlib -n CoreBanking.DAL
dotnet new classlib -n CoreBanking.BLL
dotnet new webapi -n CoreBanking.API --use-controllers
dotnet new winforms -n CoreBanking.WinUI

# 3. Add Project vao Solution
Write-Host ">>> Add Project vao Solution..." -ForegroundColor Yellow
dotnet sln add CoreBanking.DAL/CoreBanking.DAL.csproj
dotnet sln add CoreBanking.BLL/CoreBanking.BLL.csproj
dotnet sln add CoreBanking.API/CoreBanking.API.csproj
dotnet sln add CoreBanking.WinUI/CoreBanking.WinUI.csproj

# 4. Thiet lap Dependency (Tham chieu)
Write-Host ">>> Thiet lap References..." -ForegroundColor Yellow

# BLL su dung DAL
dotnet add CoreBanking.BLL/CoreBanking.BLL.csproj reference CoreBanking.DAL/CoreBanking.DAL.csproj

# API su dung BLL (Logic) va DAL (de Config DI/DBContext)
dotnet add CoreBanking.API/CoreBanking.API.csproj reference CoreBanking.BLL/CoreBanking.BLL.csproj
dotnet add CoreBanking.API/CoreBanking.API.csproj reference CoreBanking.DAL/CoreBanking.DAL.csproj

# WinUI su dung BLL (Logic) - Khong tham chieu truc tiep DAL de dam bao bao mat
dotnet add CoreBanking.WinUI/CoreBanking.WinUI.csproj reference CoreBanking.BLL/CoreBanking.BLL.csproj

# 5. Cai dat NuGet Packages (Entity Framework Core & SQL Server)
Write-Host ">>> Cai dat NuGet Packages..." -ForegroundColor Yellow

# Cho DAL: Core packages
dotnet add CoreBanking.DAL/CoreBanking.DAL.csproj package Microsoft.EntityFrameworkCore.SqlServer
dotnet add CoreBanking.DAL/CoreBanking.DAL.csproj package Microsoft.EntityFrameworkCore.Design

# Cho API: Tools de chay Migration
dotnet add CoreBanking.API/CoreBanking.API.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add CoreBanking.API/CoreBanking.API.csproj package Microsoft.EntityFrameworkCore.Tools

Write-Host ">>> HOAN TAT! SOLUTION DA SAN SANG." -ForegroundColor Green
Write-Host ">>> Cau truc hien tai:"
tree /F