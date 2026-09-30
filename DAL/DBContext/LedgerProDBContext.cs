using DAL.UserModels;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace DAL.DBContext
{
    public class LedgerProDBContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public LedgerProDBContext(DbContextOptions<LedgerProDBContext> options)
           : base(options)
        {
        }

        public DbSet<TbAuditLog> TbAuditLogs { get; set; }
        public DbSet<TbBillingPeriod> TbBillingPeriods { get; set; }
        public DbSet<TbCancellationRecord> TbCancellationRecords { get; set; }
        public DbSet<TbGracePeriod> TbGracePeriods { get; set; }
        public DbSet<TbInvoice> TbInvoices { get; set; }
        public DbSet<TbNotification> TbNotifications { get; set; }
        public DbSet<TbOrganization> TbOrganizations { get; set; }
        public DbSet<TbPayment> TbPayments { get; set; }
        public DbSet<TbPaymentAttempt> TbPaymentAttempts { get; set; }
        public DbSet<TbPendingPlanChange> TbPendingPlanChanges { get; set; }
        public DbSet<TbPlan> TbPlans { get; set; }
        public DbSet<TbPlanPricing> TbPlanPricing { get; set; }
        public DbSet<TbRefreshToken> TbRefreshTokens { get; set; }
        public DbSet<TbRefund> TbRefunds { get; set; }
        public DbSet<TbSubscription> TbSubscriptions { get; set; }
        public DbSet<TbSystemConfig> TbSystemConfigs { get; set; }
        public DbSet<TbTrial> TbTrials { get; set; }
        public DbSet<TbUserSeat> TbUserSeats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===================== BaseTable Audit Fields =====================
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseTable).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasOne(typeof(ApplicationUser))
                        .WithMany()
                        .HasForeignKey(nameof(BaseTable.CreatedBy))
                        .OnDelete(DeleteBehavior.Restrict);

                    modelBuilder.Entity(entityType.ClrType)
                        .HasOne(typeof(ApplicationUser))
                        .WithMany()
                        .HasForeignKey(nameof(BaseTable.UpdatedBy))
                        .OnDelete(DeleteBehavior.Restrict);
                }
            }

            // ===================== ApplicationUser =====================
            modelBuilder.Entity<ApplicationUser>(e =>
            {
                e.HasOne(u => u.Organization)
                    .WithMany()
                    .HasForeignKey(u => u.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== TbOrganization =====================
            modelBuilder.Entity<TbOrganization>(e =>
            {
                e.Property(o => o.OrganizationName).IsRequired().HasMaxLength(250);
            });

            // ===================== TbPlan =====================
            modelBuilder.Entity<TbPlan>(e =>
            {
                e.Property(p => p.PlanName).IsRequired().HasMaxLength(150);
                e.Property(p => p.CurrentPrice).HasPrecision(18, 2);
                e.Property(p => p.Currency).HasMaxLength(3);
            });

            // ===================== TbPlanPricing =====================
            modelBuilder.Entity<TbPlanPricing>(e =>
            {
                e.HasOne(a => a.Plan)
                    .WithMany(a => a.PlanPricing)
                    .HasForeignKey(a => a.PlanId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.Property(p => p.Price).HasPrecision(18, 2);
                e.Property(p => p.Currency).HasMaxLength(3);
            });

            // ===================== TbSubscription =====================
            modelBuilder.Entity<TbSubscription>(e =>
            {
                e.HasOne(a => a.Organization)
                    .WithMany(a => a.Subscription)
                    .HasForeignKey(a => a.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.Plan)
                    .WithMany(a => a.Subscription)
                    .HasForeignKey(a => a.PlanId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== TbBillingPeriod =====================
            modelBuilder.Entity<TbBillingPeriod>(e =>
            {
                e.HasOne(a => a.Subscription)
                    .WithMany(a => a.BillingPeriod)
                    .HasForeignKey(a => a.SubscriptionId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.Plan)
                    .WithMany(a => a.BillingPeriod)
                    .HasForeignKey(a => a.PlanId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.Property(b => b.Amount).HasPrecision(18, 2);
                e.Property(b => b.Currency).HasMaxLength(3);
            });

            // ===================== TbTrial =====================
            modelBuilder.Entity<TbTrial>(e =>
            {
                e.HasOne(a => a.Organization)
                    .WithMany(a => a.Trial)
                    .HasForeignKey(a => a.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== TbInvoice =====================
          
            modelBuilder.Entity<TbInvoice>(e =>
            {
                e.HasOne(a => a.Organization)
                    .WithMany(a => a.Invoice)
                    .HasForeignKey(a => a.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.Subscription)
                    .WithMany(a => a.Invoice)
                    .HasForeignKey(a => a.SubscriptionId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.Plan)
                    .WithMany(a => a.Invoice)
                    .HasForeignKey(a => a.PlanId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.BillingPeriod)
                    .WithMany(a => a.Invoice)
                    .HasForeignKey(a => a.BillingPeriodId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.Property(i => i.Amount).HasPrecision(18, 2);
                e.Property(i => i.Currency).HasMaxLength(3);
            });

            // ===================== TbPayment =====================
            modelBuilder.Entity<TbPayment>(e =>
            {
                e.HasOne(p => p.Invoice)
                    .WithOne(i => i.Payment)
                    .HasForeignKey<TbPayment>(p => p.InvoiceId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.Property(p => p.Amount).HasPrecision(18, 2);
                e.Property(p => p.Currency).HasMaxLength(3);

                e.HasIndex(p => p.ProviderReference)
                    .IsUnique()
                    .HasFilter("[ProviderReference] IS NOT NULL");
            });

            // ===================== TbPaymentAttempt =====================
            modelBuilder.Entity<TbPaymentAttempt>(e =>
            {
                e.HasOne(a => a.Invoice)
                    .WithMany(a => a.PaymentAttempt)
                    .HasForeignKey(a => a.InvoiceId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.Property(p => p.Amount).HasPrecision(18, 2);

                e.HasIndex(a => a.ProviderReference)
                    .IsUnique()
                    .HasFilter("[ProviderReference] IS NOT NULL");
            });

            // ===================== TbRefund =====================
            modelBuilder.Entity<TbRefund>(e =>
            {
                e.HasOne(a => a.Payment)
                    .WithMany(a => a.Refund)
                    .HasForeignKey(a => a.PaymentId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.Property(r => r.RefundAmount).HasPrecision(18, 2);
                e.Property(r => r.Currency).HasMaxLength(3);
            });

            // ===================== TbGracePeriod =====================
            modelBuilder.Entity<TbGracePeriod>(e =>
            {
                e.HasOne(a => a.Subscription)
                    .WithMany(a => a.GracePeriod)
                    .HasForeignKey(a => a.SubscriptionId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.Invoice)
                    .WithMany(a => a.GracePeriod)
                    .HasForeignKey(a => a.InvoiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== TbCancellationRecord =====================
            modelBuilder.Entity<TbCancellationRecord>(e =>
            {
                e.HasOne(a => a.Organization)
                    .WithMany(a => a.CancellationRecord)
                    .HasForeignKey(a => a.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.Subscription)
                    .WithMany(a => a.CancellationRecord)
                    .HasForeignKey(a => a.SubscriptionId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<ApplicationUser>()
                    .WithMany(u => u.CancellationRecord)
                    .HasForeignKey(x => x.PerformedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== TbPendingPlanChange =====================
            modelBuilder.Entity<TbPendingPlanChange>(e =>
            {
              
                e.HasOne(a => a.Subscription)
                    .WithOne(s => s.PendingPlanChange)
                    .HasForeignKey<TbPendingPlanChange>(p => p.SubscriptionId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.Plan)
                    .WithMany(a => a.PendingPlanChange)
                    .HasForeignKey(a => a.TargetPlanId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== TbUserSeat =====================
            modelBuilder.Entity<TbUserSeat>(e =>
            {
                e.HasOne<ApplicationUser>()
                    .WithMany(u => u.UserSeat)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.Organization)
                    .WithMany(a => a.UserSeat)
                    .HasForeignKey(a => a.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.Subscription)
                    .WithMany(a => a.UserSeat)
                    .HasForeignKey(a => a.SubscriptionId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => new
                {
                    x.UserId,
                    x.OrganizationId,
                    x.SubscriptionId
                })
                .IsUnique()
                .HasFilter("[CurrentState] > 0");
            });

            // ===================== TbNotification =====================
            modelBuilder.Entity<TbNotification>(e =>
            {
                e.HasOne(a => a.Organization)
                    .WithMany(a => a.Notification)
                    .HasForeignKey(a => a.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<ApplicationUser>()
                    .WithMany(u => u.Notification)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== TbAuditLog =====================
            modelBuilder.Entity<TbAuditLog>(e =>
            {
              
                e.HasIndex(x => x.UserId);

                e.HasOne<ApplicationUser>()
                    .WithMany(u => u.AuditLog)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== TbRefreshToken =====================
            modelBuilder.Entity<TbRefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("NEWID()");

                entity.HasOne<ApplicationUser>()
                    .WithMany(u => u.RefreshToken)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.CurrentState)
                    .HasDefaultValue(1)
                    .IsRequired();

                entity.Property(e => e.CreatedBy)
                    .IsRequired();

                entity.Property(e => e.CreatedDate)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}