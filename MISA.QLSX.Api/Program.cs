using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;
using MISA.QLSX.Core.Services;
using MISA.QLSX.Infrastructure.Connection;
using MISA.QLSX.Infrastructure.Repositories;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

// Đăng ký MySQL connection factory
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddSingleton<MySqlConnectionFactory>(new MySqlConnectionFactory(connectionString));

// Đăng ký Repository
builder.Services.AddScoped<IShiftRepository, ShiftRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IContractTemplateRepository, ContractTemplateRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<ISalaryPolicyRepository, SalaryPolicyRepository>();
builder.Services.AddScoped<ITaxBracketRepository, TaxBracketRepository>();
builder.Services.AddScoped<IDeductionPolicyRepository, DeductionPolicyRepository>();
builder.Services.AddScoped<IEmployeeTaxProfileRepository, EmployeeTaxProfileRepository>();

// Đăng ký Service
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IContractTemplateService, ContractTemplateService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<ISalaryPolicyService, SalaryPolicyService>();
builder.Services.AddScoped<ITaxBracketService, TaxBracketService>();
builder.Services.AddScoped<IDeductionPolicyService, DeductionPolicyService>();
builder.Services.AddScoped<IEmployeeTaxProfileService, EmployeeTaxProfileService>();

//Cho phép gọi api không cần ktra
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
    );
});

//set license code cho EPPlus
ExcelPackage.License.SetNonCommercialPersonal("thaiminhhieu");
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseMiddleware<ValidateExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
