using Microsoft.AspNetCore.Authentication.Cookies;
using OPCBS.Domain.Constants;
using OPCBS.Web.Helpers;
using OPCBS.Web.Services;

var builder = WebApplication.CreateBuilder(args);

const string customerSupportPolicy = "CustomerSupportPortal";

// Add services to the container.
builder.Services.AddRazorPages(options =>
    options.Conventions.AuthorizeFolder("/CustomerSupport", customerSupportPolicy));
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
RegisterApi<IConsultationNoteApiService, ConsultationNoteApiService>(builder.Services);
RegisterApi<IPatientRecordApiService, PatientRecordApiService>(builder.Services);
RegisterApi<ITreatmentPackageApiService, TreatmentPackageApiService>(builder.Services);
RegisterApi<IReviewApiService, ReviewApiService>(builder.Services);
RegisterApi<IVerificationApiService, VerificationApiService>(builder.Services);
RegisterApi<IServicePackageApiService, ServicePackageApiService>(builder.Services);
RegisterApi<ISubscriptionApiService, SubscriptionApiService>(builder.Services);
RegisterApi<IAdminApiService, AdminApiService>(builder.Services);
RegisterApi<ICustomerSupportApiService, CustomerSupportApiService>(builder.Services);
RegisterApi<IBusinessManagerApiService, BusinessManagerApiService>(builder.Services);
RegisterApi<IPsychometricApiService, PsychometricApiService>(builder.Services);
RegisterApi<INotificationApiService, NotificationApiService>(builder.Services);
RegisterApi<ITherapyApiService, TherapyApiService>(builder.Services);
RegisterApi<IFavoriteApiService, FavoriteApiService>(builder.Services);
RegisterApi<IMessagingApiService, MessagingApiService>(builder.Services);
RegisterApi<ITreatmentCaseApiService, TreatmentCaseApiService>(builder.Services);
RegisterApi<IViolationReportApiService, ViolationReportApiService>(builder.Services);
RegisterApi<IDoctorRevenueApiService, DoctorRevenueApiService>(builder.Services);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "OPCBS.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/403";
        options.Events.OnValidatePrincipal = context =>
        {
            var jwtCookies = context.HttpContext.RequestServices.GetRequiredService<JwtCookieService>();
            if (string.IsNullOrWhiteSpace(jwtCookies.GetToken()) &&
                string.IsNullOrWhiteSpace(jwtCookies.GetRefreshToken()))
            {
                context.RejectPrincipal();
                context.HttpContext.Response.Cookies.Delete(
                    options.Cookie.Name!,
                    new CookieOptions { Path = "/" });
            }

            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization(options =>
    options.AddPolicy(customerSupportPolicy, policy =>
        policy.RequireRole(RoleConstants.CustomerSupport, RoleConstants.SystemAdmin)));

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
