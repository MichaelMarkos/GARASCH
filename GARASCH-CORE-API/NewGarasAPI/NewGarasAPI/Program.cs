using Microsoft.EntityFrameworkCore;
using NewGarasAPI.Models;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using NewGarasAPI.Hubs;
using Microsoft.AspNetCore.SignalR;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure;
using NewGaras.Domain.Interfaces.ServicesInterfaces;
using NewGaras.Domain.Services;
using NewGaras.Domain.Mappers;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces;
using NewGaras.Infrastructure.Models.Mail;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.ProjectManagment;
using NewGaras.Domain.Services.ProjectManagment;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.BYCompany;
using NewGaras.Domain.Services.BYCompany;
using Org.BouncyCastle.Crypto.Tls;
using NewGaras.Domain.Services.Inventory;
using NewGaras.Domain.Services.Purchasing;
using NewGaras.Infrastructure.Models.EmailTool;
using NewGaras.Domain.Services.ContractExtensionJob;
using NewGaras.Domain.Services.ApplicationsVersion;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.Medical;
using NewGaras.Domain.Services.Medical;
using NewGaras.Infrastructure.Interfaces.Library;
using NewGaras.Domain.Services.Library;
using NewGaras.Domain.Services.Family;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {

            //you can configure your custom policy
            builder.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
        });
});
// Add services to the container.
//builder.Services.AddDbContext<GarasTestContext>(options =>
//options.UseSqlServer(builder.Configuration.GetConnectionString("GarasTest"))
//);
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ITenantService, TenantService>();
builder.Services.Configure<TenantSettings>(builder.Configuration.GetSection(nameof(TenantSettings)));
TenantSettings options = new();
builder.Configuration.GetSection(nameof(TenantSettings)).Bind(options);

var defaultDbProvider = options.Defaults.DBProvider;

builder.Services.AddDbContext<GarasTestContext>((serviceProvider, options) =>
{
    var tenantResolver = serviceProvider.GetRequiredService<ITenantService>();
    var tenant = tenantResolver.GetTenant();
    options.UseSqlServer(tenant.ConnectionString);
});
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.Configure<AuthDataInit>(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ISalaryService, SalaryService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IDepartmentService,DepartmentService>();
builder.Services.AddScoped<ILogService,LogService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IVacationTypeService, VacationTypeService>();
builder.Services.AddScoped<IAttendanceService,AttendanceService>();
builder.Services.AddScoped<IBranchSettingService, BranchSettingService>();
builder.Services.AddScoped<IOverTimeAndDeductionRateService, OverTimeAndDeductionRateService>();
builder.Services.AddScoped<IVacationOverTimeAndDeductionRate, VacationOverTimeAndDeductionRateService>();
builder.Services.AddScoped<IVacationDayService, VacationDayService>();
builder.Services.AddScoped<ISalesOfferService, SalesOfferService>();
builder.Services.AddScoped<IAccountMovementService, AccountMovementService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IInventoryItemCategoryService, InventoryItemCategoryService>();
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();
//builder.Services.AddSingleton<IHubNotificationHelper,HubNotificationHelper>();
builder.Services.AddScoped<IHrUserService, HrUserService>();
builder.Services.AddScoped<IJobTitleService, JobTitleService>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<ITaskExpensisService, TaskExpensisService>();
builder.Services.AddScoped<IWorkFlowService, WorkFlowService>();
builder.Services.AddScoped<ISprintService, SprintService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IProjectChequeService, ProjectChequeService>();
builder.Services.AddScoped<IProjectInvoiceService, ProjectInvoiceService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IProjectProgressService, ProjectProgressService>();
builder.Services.AddScoped<IInsuranceCompanyNamesService, InsuranceCompanyNamesService>();
builder.Services.AddScoped<IInventoryMatrialReleaseForBYService, InventoryMatrialReleaseForBYService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IAccountAndFinanceService, AccountAndFinanceService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailToolService,  EmailToolService>();
builder.Services.AddScoped<ITaskMangerProjectService, TaskMangerProjectService>();
builder.Services.AddScoped<IInventoryOpeningBalanceService, InventoryOpeningBalanceService>();
builder.Services.AddScoped<IInventoryMaterialRequestService, InventoryMaterialRequestService>();
builder.Services.AddScoped<IInventoryItemMatrialAddingAndExternalOrderService, InventoryItemMatrialAddingAndExternalOrderService>();
builder.Services.AddScoped<IInventoryTransferOrderService, InventoryTransferOrderService>();
builder.Services.AddScoped<IInventoryInternalBackOrderService, InventoryInternalBackOrderService>();
builder.Services.AddScoped<IPOSService, POSService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IInventoryMateriaReleaseService, InventoryMaterialReleaseService>();
builder.Services.AddScoped<IInventoryItemService, InventoryItemService>();
builder.Services.AddScoped<IPurchesRequestService, PurchesRequestService>();
builder.Services.AddScoped<IItemsPricingService, ItemsPricingService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<ICrmService, CrmService>();
builder.Services.AddScoped<IInventoryStoreReportsService, InventoryStoreReportsService>();
builder.Services.AddScoped<IInternalTicketService, InternalTicketService>();
builder.Services.AddScoped<INotificationSubscriptionService, NotificationSubscriptionService>();
builder.Services.AddSingleton<IGraphAuthService, GraphAuthService>();
builder.Services.AddScoped<IPoInvoiceService, PoInvoiceService>();
builder.Services.AddDbContext<GarasTestContext>();
builder.Services.AddScoped<IDDLService,  DDLService>();
builder.Services.AddScoped<ISupplierService,  SupplierService>();
builder.Services.AddScoped<IApplicationVersionService,  ApplicationVersionService>();
builder.Services.AddScoped<IFamilyService, FamilyService>();

builder.Services.AddScoped<IWeekDayService , WeekDayService>();
//-----------------------------------------------Medical------------------------------

builder.Services.AddScoped<IMedicalService, MedicalService>();
builder.Services.AddScoped<IDoctorScheduleService, DoctorScheduleService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IMedicalFinancialService, MedicalFinancialService>();
//-----------------------------------------------Library------------------------------
builder.Services.AddScoped<ILibraryService, LibraryService>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddSignalR().AddHubOptions<NotificationsHub>(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(30);
    options.KeepAliveInterval = TimeSpan.FromMinutes(15);
    //options.Hubs.EnableDetailedErrors = true;
});

var app = builder.Build();
app.UseCors();





//  For Periti Only
// Configure CORS policy to allow a specific URL (e.g., Flutter app)
//var myAllowSpecificOrigins = "_myAllowSpecificOrigins";

////builder.Services.AddCors(options =>
////{
////    options.AddPolicy(name: myAllowSpecificOrigins,
////        builder =>
////        {
////            // Allow the Flutter app URL
////            builder.WithOrigins("http://192.168.1.68:8080")
////                   .AllowAnyHeader()
////                   .AllowAnyMethod()
////                   .AllowCredentials();  // Optional if you need cookies or credentials
////        });
////});
//// Enable the configured CORS policy globally
//app.UseCors(myAllowSpecificOrigins);
//var app = builder.Build();









// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseStaticFiles();
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.MapHub<NotificationsHub>("/Notifications");
app.Run();





