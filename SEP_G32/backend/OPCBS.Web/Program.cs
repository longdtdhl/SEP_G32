using Microsoft.AspNetCore.Authentication.Cookies;
using OPCBS.Web.Helpers;
using OPCBS.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<JwtCookieService>();

// --- API Base URL ---
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5001/";

// --- Register all typed HTTP client services ---
void RegisterApi<TInterface, TImpl>(IServiceCollection services)
    where TInterface : class
    where TImpl : class, TInterface
{
    services.AddHttpClient<TInterface, TImpl>(client =>
        client.BaseAddress = new Uri(apiBaseUrl));
}

RegisterApi<IAuthApiService, AuthApiService>(builder.Services);
RegisterApi<IDoctorApiService, DoctorApiService>(builder.Services);
RegisterApi<IAppointmentApiService, AppointmentApiService>(builder.Services);
RegisterApi<IBlogApiService, BlogApiService>(builder.Services);
RegisterApi<IScheduleApiService, ScheduleApiService>(builder.Services);
RegisterApi<IConsultationRecordApiService, ConsultationRecordApiService>(builder.Services);
RegisterApi<ITreatmentPackageApiService, TreatmentPackageApiService>(builder.Services);
RegisterApi<IReviewApiService, ReviewApiService>(builder.Services);
RegisterApi<IVerificationApiService, VerificationApiService>(builder.Services);
RegisterApi<IServicePackageApiService, ServicePackageApiService>(builder.Services);
RegisterApi<ISubscriptionApiService, SubscriptionApiService>(builder.Services);
RegisterApi<IAdminApiService, AdminApiService>(builder.Services);
RegisterApi<ICustomerSupportApiService, CustomerSupportApiService>(builder.Services);
RegisterApi<IBusinessManagerApiService, BusinessManagerApiService>(builder.Services);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "OPCBS.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/403";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
