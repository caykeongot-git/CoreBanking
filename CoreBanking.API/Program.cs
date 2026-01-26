using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CoreBanking.BLL.Interfaces;
using CoreBanking.BLL.Services;
using CoreBanking.DAL.Data;
using CoreBanking.DAL.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ==================================================================
// CẤU HÌNH CORE BANKING
// ==================================================================

// A. Database Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CoreBankingDbContext>(options =>
    options.UseSqlServer(connectionString));

// B. Data Access Layer (DAL)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// C. Business Logic Layer (BLL)
// Đăng ký các Service để Controller có thể sử dụng
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IAccountService, AccountService>();

// ==================================================================

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

try
{
    app.MapControllers();
}
catch (System.Reflection.ReflectionTypeLoadException ex)
{
    foreach (var loaderException in ex.LoaderExceptions)
    {
        Console.WriteLine($"--- LOADER ERROR: {loaderException?.Message}");
    }
    throw;
}

app.Run();
