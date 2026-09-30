using AutoMapper;
using BL.Dtos;
using DAL.UserModels;
using Domain;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Mapping
{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {
            CreateMap<ApplicationUser, UserDto>().ReverseMap();
            CreateMap<TbAuditLog, AuditLogDto>().ReverseMap();
            CreateMap<TbBillingPeriod, BillingPeriodDto>()
    .ForMember(
        dest => dest.OrganizationName,
        opt => opt.MapFrom(src => src.Subscription.Organization.OrganizationName))
    .ForMember(
        dest => dest.PlanName,
        opt => opt.MapFrom(src => src.Plan.PlanName))
    .ForMember(
        dest => dest.Description,
        opt => opt.MapFrom(src => src.Plan.Description));

            CreateMap<BillingPeriodDto, TbBillingPeriod>()
                .ForMember(dest => dest.Subscription, opt => opt.Ignore())
                .ForMember(dest => dest.Plan, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore());
            CreateMap<TbCancellationRecord, CancellationRecordDto>()
     .ForMember(
         dest => dest.OrganizationName,
         opt => opt.MapFrom(src => src.Organization.OrganizationName))
     .ForMember(
         dest => dest.PlanName,
         opt => opt.MapFrom(src => src.Subscription.Plan.PlanName))
     .ForMember(
         dest => dest.Description,
         opt => opt.MapFrom(src => src.Subscription.Plan.Description))
     .ForMember(
         dest => dest.CurrentPrice,
         opt => opt.MapFrom(src => src.Subscription.Plan.CurrentPrice));

            CreateMap<CancellationRecordDto, TbCancellationRecord>()
                .ForMember(dest => dest.Organization, opt => opt.Ignore())
                .ForMember(dest => dest.Subscription, opt => opt.Ignore());
            // ===================== GRACE PERIOD =====================
            CreateMap<TbGracePeriod, GracePeriodDto>()
                .ForMember(
                    dest => dest.OrganizationName,
                    opt => opt.MapFrom(src => src.Subscription.Organization.OrganizationName))
                .ForMember(
                    dest => dest.PlanName,
                    opt => opt.MapFrom(src => src.Subscription.Plan.PlanName))
                .ForMember(
                    dest => dest.Duration,
                    opt => opt.MapFrom(src => src.Invoice.Duration))
                .ForMember(
                    dest => dest.PlanNameSnapshot,
                    opt => opt.MapFrom(src => src.Invoice.PlanNameSnapshot))
                .ForMember(
                    dest => dest.Amount,
                    opt => opt.MapFrom(src => src.Invoice.Amount))
                .ForMember(
                    dest => dest.Currency,
                    opt => opt.MapFrom(src => src.Invoice.Currency));

            CreateMap<GracePeriodDto, TbGracePeriod>()
                .ForMember(dest => dest.Subscription, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore());
            // ===================== INVOICE =====================
            CreateMap<TbInvoice, InvoiceDto>()
                .ForMember(
                    dest => dest.OrganizationName,
                    opt => opt.MapFrom(src => src.Organization.OrganizationName))
                .ForMember(
                    dest => dest.SubscriptionStatus,
                    opt => opt.MapFrom(src => src.Subscription.Status))
                .ForMember(
                    dest => dest.PlanName,
                    opt => opt.MapFrom(src => src.Plan.PlanName))
                .ForMember(
                    dest => dest.PlanDescription,
                    opt => opt.MapFrom(src => src.Plan.Description))
                .ForMember(
                    dest => dest.BillingPeriodStart,
                    opt => opt.MapFrom(src =>
                        src.BillingPeriod != null
                            ? src.BillingPeriod.Start
                            : (DateTime?)null))
                .ForMember(
                    dest => dest.BillingPeriodEnd,
                    opt => opt.MapFrom(src =>
                        src.BillingPeriod != null
                            ? src.BillingPeriod.End
                            : (DateTime?)null))
                .ForMember(
                    dest => dest.BillingPeriodStatus,
                    opt => opt.MapFrom(src =>
                        src.BillingPeriod != null
                            ? src.BillingPeriod.Status
                            : (BillingPeriodStatus?)null));

            CreateMap<InvoiceDto, TbInvoice>()
                .ForMember(dest => dest.Organization, opt => opt.Ignore())
                .ForMember(dest => dest.Subscription, opt => opt.Ignore())
                .ForMember(dest => dest.Plan, opt => opt.Ignore())
                .ForMember(dest => dest.BillingPeriod, opt => opt.Ignore())
                .ForMember(dest => dest.GracePeriod, opt => opt.Ignore())
                .ForMember(dest => dest.Payment, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentAttempt, opt => opt.Ignore());
            // ===================== NOTIFICATION =====================
            CreateMap<TbNotification, NotificationDto>()
                .ForMember(
                    dest => dest.OrganizationName,
                    opt => opt.MapFrom(src => src.Organization.OrganizationName));

            CreateMap<NotificationDto, TbNotification>()
                .ForMember(dest => dest.Organization, opt => opt.Ignore());
            CreateMap<TbOrganization, OrganizationDto>().ReverseMap();
          
// ===================== PAYMENT =====================
CreateMap<TbPayment, PaymentDto>()
    .ForMember(
        dest => dest.InvoiceStatus,
        opt => opt.MapFrom(src => src.Invoice != null
            ? src.Invoice.Status
            : (InvoiceStatus?)null))
    .ForMember(
        dest => dest.InvoiceType,
        opt => opt.MapFrom(src => src.Invoice != null
            ? src.Invoice.InvoiceType
            : (InvoiceType?)null))
    .ForMember(
        dest => dest.PlanNameSnapshot,
        opt => opt.MapFrom(src => src.Invoice != null
            ? src.Invoice.PlanNameSnapshot
            : null))
    .ForMember(
        dest => dest.PlanName,
        opt => opt.MapFrom(src => src.Invoice != null && src.Invoice.Plan != null
            ? src.Invoice.Plan.PlanName
            : null));

            CreateMap<PaymentDto, TbPayment>()
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.Refund, opt => opt.Ignore());


            // ===================== PAYMENT ATTEMPT =====================
            CreateMap<TbPaymentAttempt, PaymentAttemptDto>()
                .ForMember(
                    dest => dest.PlanNameSnapshot,
                    opt => opt.MapFrom(src => src.Invoice != null
                        ? src.Invoice.PlanNameSnapshot
                        : null))
                .ForMember(
                    dest => dest.InvoiceAmount,
                    opt => opt.MapFrom(src => src.Invoice != null
                        ? src.Invoice.Amount
                        : (decimal?)null))
                .ForMember(
                    dest => dest.PlanName,
                    opt => opt.MapFrom(src => src.Invoice != null && src.Invoice.Plan != null
                        ? src.Invoice.Plan.PlanName
                        : null))
                .ForMember(
                    dest => dest.InvoiceStatus,
                    opt => opt.MapFrom(src => src.Invoice != null
                        ? src.Invoice.Status
                        : (InvoiceStatus?)null))
                .ForMember(
                    dest => dest.InvoiceType,
                    opt => opt.MapFrom(src => src.Invoice != null
                        ? src.Invoice.InvoiceType
                        : (InvoiceType?)null))
                .ForMember(
                    dest => dest.Currency,
                    opt => opt.MapFrom(src => src.Invoice != null
                        ? src.Invoice.Currency
                        : null));

            CreateMap<PaymentAttemptDto, TbPaymentAttempt>()
                .ForMember(dest => dest.Invoice, opt => opt.Ignore());
            // ===================== PENDING PLAN CHANGE =====================
            CreateMap<TbPendingPlanChange, PendingPlanChangeDto>()
                .ForMember(
                    dest => dest.OrganizationName,
                    opt => opt.MapFrom(src => src.Subscription.Organization.OrganizationName))
                .ForMember(
                    dest => dest.SubscriptionPlanName,
                    opt => opt.MapFrom(src => src.Subscription.Plan.PlanName))
                .ForMember(
                    dest => dest.Status,
                    opt => opt.MapFrom(src => src.Subscription.Status))
                .ForMember(
                    dest => dest.TargetPlanName,
                    opt => opt.MapFrom(src => src.Plan.PlanName))
                .ForMember(
                    dest => dest.TargetPlanDescription,
                    opt => opt.MapFrom(src => src.Plan.Description))
                .ForMember(
                    dest => dest.TargetPlanPrice,
                    opt => opt.MapFrom(src => src.Plan.CurrentPrice))
                .ForMember(
                    dest => dest.TargetPlanCurrency,
                    opt => opt.MapFrom(src => src.Plan.Currency))
                .ForMember(
                    dest => dest.TargetPlanDuration,
                    opt => opt.MapFrom(src => src.Plan.Duration))
                .ForMember(
                    dest => dest.TargetPlanMaxUsers,
                    opt => opt.MapFrom(src => src.Plan.MaxUsers))
                .ForMember(
                    dest => dest.TargetPlanIsActive,
                    opt => opt.MapFrom(src => src.Plan.IsActive));

            CreateMap<PendingPlanChangeDto, TbPendingPlanChange>()
                .ForMember(dest => dest.Subscription, opt => opt.Ignore())
                .ForMember(dest => dest.Plan, opt => opt.Ignore());
            CreateMap<TbPlan, PlanDto>().ReverseMap();
            // ===================== PLAN PRICING =====================
            CreateMap<TbPlanPricing, PlanPricingDto>()
                .ForMember(
                    dest => dest.PlanName,
                    opt => opt.MapFrom(src => src.Plan.PlanName))
                .ForMember(
                    dest => dest.Description,
                    opt => opt.MapFrom(src => src.Plan.Description))
                .ForMember(
                    dest => dest.CurrentPrice,
                    opt => opt.MapFrom(src => src.Plan.CurrentPrice))
                .ForMember(
                    dest => dest.PlanCurrency,
                    opt => opt.MapFrom(src => src.Plan.Currency))
                .ForMember(
                    dest => dest.Duration,
                    opt => opt.MapFrom(src => src.Plan.Duration))
                .ForMember(
                    dest => dest.MaxUsers,
                    opt => opt.MapFrom(src => src.Plan.MaxUsers))
                .ForMember(
                    dest => dest.IsActive,
                    opt => opt.MapFrom(src => src.Plan.IsActive));

            CreateMap<PlanPricingDto, TbPlanPricing>()
                .ForMember(
                    dest => dest.Plan,
                    opt => opt.Ignore());
            // ===================== REFUND =====================
            CreateMap<TbRefund, RefundDto>()
                .ForMember(
                    dest => dest.PaymentStatus,
                    opt => opt.MapFrom(src => src.Payment != null
                        ? src.Payment.Status
                        : (PaymentStatus?)null))
                .ForMember(
                    dest => dest.InvoiceStatus,
                    opt => opt.MapFrom(src => src.Payment != null && src.Payment.Invoice != null
                        ? src.Payment.Invoice.Status
                        : (InvoiceStatus?)null))
                .ForMember(
                    dest => dest.InvoiceType,
                    opt => opt.MapFrom(src => src.Payment != null && src.Payment.Invoice != null
                        ? src.Payment.Invoice.InvoiceType
                        : (InvoiceType?)null))
                .ForMember(
                    dest => dest.OrganizationName,
                    opt => opt.MapFrom(src =>
                        src.Payment != null &&
                        src.Payment.Invoice != null &&
                        src.Payment.Invoice.Organization != null
                            ? src.Payment.Invoice.Organization.OrganizationName
                            : null))
                .ForMember(
                    dest => dest.PlanName,
                    opt => opt.MapFrom(src =>
                        src.Payment != null &&
                        src.Payment.Invoice != null &&
                        src.Payment.Invoice.Plan != null
                            ? src.Payment.Invoice.Plan.PlanName
                            : null));

            CreateMap<RefundDto, TbRefund>()
                .ForMember(dest => dest.Payment, opt => opt.Ignore());
            // ===================== SUBSCRIPTION =====================
            CreateMap<TbSubscription, SubscriptionDto>()
                .ForMember(
                    dest => dest.OrganizationName,
                    opt => opt.MapFrom(src =>
                        src.Organization != null
                            ? src.Organization.OrganizationName
                            : null))
                .ForMember(
                    dest => dest.PlanName,
                    opt => opt.MapFrom(src =>
                        src.Plan != null
                            ? src.Plan.PlanName
                            : null))
                .ForMember(
                    dest => dest.Description,
                    opt => opt.MapFrom(src =>
                        src.Plan != null
                            ? src.Plan.Description
                            : null))
                .ForMember(
                    dest => dest.CurrentPrice,
                    opt => opt.MapFrom(src =>
                        src.Plan != null
                            ? src.Plan.CurrentPrice
                            : (decimal?)null))
                .ForMember(
                    dest => dest.Currency,
                    opt => opt.MapFrom(src =>
                        src.Plan != null
                            ? src.Plan.Currency
                            : null))
                .ForMember(
                    dest => dest.Duration,
                    opt => opt.MapFrom(src =>
                        src.Plan != null
                            ? src.Plan.Duration
                            : (int?)null))
                .ForMember(
                    dest => dest.MaxUsers,
                    opt => opt.MapFrom(src =>
                        src.Plan != null
                            ? src.Plan.MaxUsers
                            : (int?)null))
                .ForMember(
                    dest => dest.IsActive,
                    opt => opt.MapFrom(src =>
                        src.Plan != null
                            ? src.Plan.IsActive
                            : (bool?)null));

            CreateMap<SubscriptionDto, TbSubscription>()
                .ForMember(dest => dest.Organization, opt => opt.Ignore())
                .ForMember(dest => dest.Plan, opt => opt.Ignore())
                .ForMember(dest => dest.BillingPeriod, opt => opt.Ignore())
                .ForMember(dest => dest.CancellationRecord, opt => opt.Ignore())
                .ForMember(dest => dest.GracePeriod, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.PendingPlanChange, opt => opt.Ignore())
                .ForMember(dest => dest.UserSeat, opt => opt.Ignore());
            CreateMap<TbSystemConfig, SystemConfigDto>().ReverseMap();
            // ===================== TRIAL =====================
            CreateMap<TbTrial, TrialDto>()
                .ForMember(
                    dest => dest.OrganizationName,
                    opt => opt.MapFrom(src =>
                        src.Organization != null
                            ? src.Organization.OrganizationName
                            : null));

            CreateMap<TrialDto, TbTrial>()
                .ForMember(dest => dest.Organization, opt => opt.Ignore());
            // ===================== USER SEAT =====================
            CreateMap<TbUserSeat, UserSeatDto>()
                .ForMember(
                    dest => dest.OrganizationName,
                    opt => opt.MapFrom(src =>
                        src.Organization != null
                            ? src.Organization.OrganizationName
                            : null))
                .ForMember(
                    dest => dest.SubscriptionStatus,
                    opt => opt.MapFrom(src =>
                        src.Subscription != null
                            ? src.Subscription.Status
                            : (SubscriptionStatus?)null))
                .ForMember(
                    dest => dest.PlanName,
                    opt => opt.MapFrom(src =>
                        src.Subscription != null &&
                        src.Subscription.Plan != null
                            ? src.Subscription.Plan.PlanName
                            : null))
                .ForMember(
                    dest => dest.Description,
                    opt => opt.MapFrom(src =>
                        src.Subscription != null &&
                        src.Subscription.Plan != null
                            ? src.Subscription.Plan.Description
                            : null))
                .ForMember(
                    dest => dest.Currency,
                    opt => opt.MapFrom(src =>
                        src.Subscription != null &&
                        src.Subscription.Plan != null
                            ? src.Subscription.Plan.Currency
                            : null))
                .ForMember(
                    dest => dest.FirstName,
                    opt => opt.Ignore())
                .ForMember(
                    dest => dest.LastName,
                    opt => opt.Ignore())
                .ForMember(
                    dest => dest.UserName,
                    opt => opt.Ignore())
                .ForMember(
                    dest => dest.Email,
                    opt => opt.Ignore())
                .ForMember(
                    dest => dest.PhoneNumber,
                    opt => opt.Ignore());

            CreateMap<UserSeatDto, TbUserSeat>()
                .ForMember(dest => dest.Organization, opt => opt.Ignore())
                .ForMember(dest => dest.Subscription, opt => opt.Ignore());

            // ================= REFRESH TOKEN =================
         
            CreateMap<RefreshTokenDto, TbRefreshToken>()
      .ForMember(
          dest => dest.UserId,
          opt => opt.MapFrom(src => Guid.Parse(src.UserId)))
      .ForMember(
          dest => dest.ExpiresAt,
          opt => opt.MapFrom(src => src.Expires));

            CreateMap<TbRefreshToken, RefreshTokenDto>()
                .ForMember(
                    dest => dest.UserId,
                    opt => opt.MapFrom(src => src.UserId.ToString()))
                .ForMember(
                    dest => dest.Expires,
                    opt => opt.MapFrom(src => src.ExpiresAt));

        }
    }
}