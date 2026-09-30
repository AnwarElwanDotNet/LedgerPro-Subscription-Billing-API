using BL.Contract;
using BL.Services;
using DAL.Contracts;
using DAL.DBContext;
using DAL.Repositories;
using DAL.UserModels;
using Domain;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LedgerPro.SubscriptionBilling.Api.Service
{
    public class RegisterServciesHelper
    {
        public static void RegisteredServices(WebApplicationBuilder builder)
        {
            #region DbContext (Identity)

            builder.Services.AddDbContext<LedgerProDBContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")
                )
            );

            #endregion

            #region Identity

            builder.Services
                .AddIdentityCore<ApplicationUser>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireDigit = true;

                    options.User.RequireUniqueEmail = true;

                    options.Lockout.DefaultLockoutTimeSpan =
                        TimeSpan.FromMinutes(15);

                    options.Lockout.MaxFailedAccessAttempts = 5;

                    options.Lockout.AllowedForNewUsers = true;
                })
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<LedgerProDBContext>()
                .AddSignInManager()
                .AddRoleManager<RoleManager<IdentityRole<Guid>>>()
                .AddDefaultTokenProviders();

            #endregion

            #region AutoMapper
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            #endregion


            #region Repositories
            builder.Services.AddScoped(typeof(ITableRepository<>), typeof(TableRepository<>));
            builder.Services.AddScoped(typeof(IViewRepository<>), typeof(ViewRepository<>));
            #endregion

            #region Base Service
            builder.Services.AddScoped(typeof(IBaseService<,>), typeof(BaseService<,>));
            #endregion

            #region Token Service
            builder.Services.AddScoped<TokenService>();
            #endregion


            builder.Services.AddScoped<IAuditLog, AuditLogService>();
            builder.Services.AddScoped<IBillingPeriod, BillingPeriodService>();
            builder.Services.AddScoped<ICancellationRecord, CancellationRecordService>();
            builder.Services.AddScoped<IGracePeriod, GracePeriodService>();
            builder.Services.AddScoped<IInvoice, InvoiceService>();
            builder.Services.AddScoped<INotification, NotificationService>();
            builder.Services.AddScoped<IOrganization, OrganizationService>();
            builder.Services.AddScoped<IPayment, PaymentService>();
            builder.Services.AddScoped<IPaymentAttempt, PaymentAttemptService>();
            builder.Services.AddScoped<IPendingPlanChange, PendingPlanChangeService>();
            builder.Services.AddScoped<IPlan, PlanService>();
            builder.Services.AddScoped<IPlanPricing, PlanPricingService>();
            builder.Services.AddScoped<IRefreshToken, RefreshTokenService>();
            builder.Services.AddScoped<IRefreshTokenRetriver, RefreshTokenRetriverService>();
            builder.Services.AddScoped<IRefund, RefundService>();
            builder.Services.AddScoped<ISubscription, SubscriptionService>();
            builder.Services.AddScoped<ISystemConfig, SystemConfigService>();
            builder.Services.AddScoped<ITrial, TrialService>();
            builder.Services.AddScoped<IUserSeat, UserSeatService>();
            builder.Services.AddScoped<IUserService, UserService>();


            builder.Services.AddValidatorsFromAssemblyContaining<BL.Class1>();

        }
    }
}