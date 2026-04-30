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
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IDegreeRepository, DegreeRepository>();
builder.Services.AddScoped<IContractTemplateRepository, ContractTemplateRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<ISalaryPolicyRepository, SalaryPolicyRepository>();
builder.Services.AddScoped<ITaxBracketRepository, TaxBracketRepository>();
builder.Services.AddScoped<IDeductionPolicyRepository, DeductionPolicyRepository>();
builder.Services.AddScoped<IEmployeeTaxProfileRepository, EmployeeTaxProfileRepository>();
builder.Services.AddScoped<ISalaryPeriodRepository, SalaryPeriodRepository>();
builder.Services.AddScoped<IPayrollRepository, PayrollRepository>();
builder.Services.AddScoped<IPayrollItemRepository, PayrollItemRepository>();
builder.Services.AddScoped<IPayrollSnapshotRepository, PayrollSnapshotRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IAllowanceRepository, AllowanceRepository>();
builder.Services.AddScoped<IBusinessTripRepository, BusinessTripRepository>();
builder.Services.AddScoped<IEvaluationRepository, EvaluationRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();

// Đăng ký Service
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IDegreeService, DegreeService>();
builder.Services.AddScoped<IContractTemplateService, ContractTemplateService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<ISalaryPolicyService, SalaryPolicyService>();
builder.Services.AddScoped<ITaxBracketService, TaxBracketService>();
builder.Services.AddScoped<IDeductionPolicyService, DeductionPolicyService>();
builder.Services.AddScoped<IEmployeeTaxProfileService, EmployeeTaxProfileService>();
builder.Services.AddScoped<ISalaryPeriodService, SalaryPeriodService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.AddScoped<IPayrollItemService, PayrollItemService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IAllowanceService, AllowanceService>();
builder.Services.AddScoped<IBusinessTripService, BusinessTripService>();
builder.Services.AddScoped<IEvaluationService, EvaluationService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IPositionService, PositionService>();

//Thêm Distributed Memory Cache và Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

//Cho phép gọi api từ localhost:5173 với credentials
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowLocalhost5173",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
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

app.UseCors("AllowLocalhost5173");
app.UseMiddleware<ValidateExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllers();

app.Run();
