using AutoMapper;
using BL.Contract;
using BL.Dtos;
using BL.Exceptions;
using DAL.Contracts;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BL.Services
{
    public class RefundService : BaseService<TbRefund, RefundDto>, IRefund
    {
        private readonly ITableRepository<TbRefund> _RefundRepo;
        private readonly ITableRepository<TbPayment> _PaymentRepo;
        private readonly ITableRepository<TbInvoice> _InvoiceRepo;
        private readonly IAuditLog _AuditLog;
        private readonly INotification _Notification;
        private readonly IMapper _MapperRepo;
        private readonly IUserService _UserServiceRepo;

        public RefundService(
     ITableRepository<TbRefund> tableRepository,
     ITableRepository<TbPayment> paymentRepository,
     ITableRepository<TbInvoice> invoiceRepository,
     IAuditLog auditLog,
     INotification notification,
     IMapper mapper,
     IUserService userService)
     : base(tableRepository, mapper, userService)
        {
            _RefundRepo = tableRepository;
            _PaymentRepo = paymentRepository;
            _InvoiceRepo = invoiceRepository;
            _AuditLog = auditLog;
            _Notification = notification;

            _MapperRepo = mapper;
            _UserServiceRepo = userService;
        }

        public async Task ProcessRefundAsync(RefundDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // =========================================================
            // Current User
            // =========================================================

            var userId = _UserServiceRepo.GetLoggedInUser();

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");
            }

            var user =
                await _UserServiceRepo.GetUserByIdAsync(
                    userId.ToString());

            if (user == null)
            {
                throw new UnauthorizedAccessException(
                    "User not found.");
            }

            // =========================================================
            // Refund Authorization
            // =========================================================

            if (user.Role != "Platform Admin")
            {
                throw new ForbiddenAccessException(
                    "Only Platform Admin can process refunds.");
            }

            // =========================================================
            // Refund Result Validation
            // =========================================================

            if (dto.Status != RefundStatus.Succeeded)
            {
                throw new InvalidOperationException(
                    "Only a successful refund can be processed.");
            }

            // =========================================================
            // Transaction
            // =========================================================

            await _RefundRepo.ExecuteInTransactionAsync(
                async () =>
                {
                   

                    if (dto.Id == Guid.Empty)
                    {
                        throw new InvalidOperationException(
                            "Refund ID is required for idempotent processing.");
                    }

                    var existingRefund =
                        await _RefundRepo.GetFirstOrDefaultAsync(
                            x => x.Id == dto.Id);

                    if (existingRefund != null)
                    {
                        if (existingRefund.PaymentId != dto.PaymentId ||
                            existingRefund.RefundAmount != dto.RefundAmount ||
                            !string.Equals(
                                existingRefund.Currency,
                                dto.Currency,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException(
                                "This refund ID has already been used for a different refund.");
                        }

                        return;
                    }

                   

                    var payment =
                        await _PaymentRepo
                            .GetTable()
                            .Include(x => x.Invoice)
                            .FirstOrDefaultAsync(
                                x => x.Id == dto.PaymentId);

                    if (payment == null)
                    {
                        throw new KeyNotFoundException(
                            "Payment not found.");
                    }

                   

                    if (payment.Status != PaymentStatus.Succeeded &&
                        payment.Status != PaymentStatus.PartiallyRefunded)
                    {
                        throw new InvalidOperationException(
                            "Refund can only be processed against a successful payment.");
                    }

                

                    if (payment.Invoice == null)
                    {
                        throw new InvalidOperationException(
                            "Invoice associated with payment was not found.");
                    }

                    var invoice = payment.Invoice;

                

                    if (!string.Equals(
                            dto.Currency,
                            payment.Currency,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            "Refund currency must match the payment currency.");
                    }

               

                    if (dto.RefundAmount <= 0)
                    {
                        throw new InvalidOperationException(
                            "Refund amount must be greater than zero.");
                    }

               

                    var refundedAmount =
                        await _RefundRepo
                            .GetTable()
                            .Where(x =>
                                x.PaymentId == payment.Id &&
                                x.Status == RefundStatus.Succeeded)
                            .SumAsync(x => (decimal?)x.RefundAmount)
                            ?? 0m;

                    var refundableAmount =
                        payment.Amount - refundedAmount;

                    if (dto.RefundAmount > refundableAmount)
                    {
                        throw new InvalidOperationException(
                            "Refund amount cannot exceed the refundable amount.");
                    }

                  

                    var refundDate = DateTime.UtcNow;

                    var refund = new TbRefund
                    {
                        Id = dto.Id,
                        PaymentId = payment.Id,
                        RefundAmount = dto.RefundAmount,
                        Currency = payment.Currency,
                        Status = RefundStatus.Succeeded,
                        RefundDate = refundDate,
                        CurrentState = 1,
                        CreatedDate = refundDate
                    };

                    await _RefundRepo.AddAsync(
                        refund,
                        userId);


                    var totalRefundedAmount =
                        refundedAmount + dto.RefundAmount;


                    if (totalRefundedAmount == payment.Amount)
                    {
                        payment.Status =
                            PaymentStatus.Refunded;

                        invoice.Status =
                            InvoiceStatus.Refunded;
                    }
                  

                    else
                    {
                        payment.Status =
                            PaymentStatus.PartiallyRefunded;

                        invoice.Status =
                            InvoiceStatus.PartiallyRefunded;
                    }

                   

                    await _PaymentRepo.UpdateAsync(
                        payment,
                        userId);

                    await _InvoiceRepo.UpdateAsync(
                        invoice,
                        userId);

                    await _Notification.AddAsync(
    new NotificationDto
    {
        OrganizationId = invoice.OrganizationId,

        UserId = null,

        Description =
            $"Refund processed successfully. " +
            $"PaymentId={payment.Id}; " +
            $"RefundId={refund.Id}; " +
            $"RefundAmount={refund.RefundAmount}; " +
            $"Currency={refund.Currency}",

        RelatedEntityType = EntityType.Refund,
        RelatedEntityId = refund.Id,

        IsRead = false,

        Type = NotificationType.RefundProcessed
    });

                 

                    await _AuditLog.AddAsync(
                     new AuditLogDto
                     {
                         Action = "Create Refund",
                         EntityType = EntityType.Refund,
                         EntityId = refund.Id,
                         CorrelationId = Guid.NewGuid(),
                         Metadata =
                             $"PaymentId={payment.Id}; " +
                             $"RefundAmount={refund.RefundAmount}; " +
                             $"Currency={refund.Currency}; " +
                             $"Status={refund.Status}"
                     });
                },
                System.Data.IsolationLevel.Serializable);
        }
    }
}