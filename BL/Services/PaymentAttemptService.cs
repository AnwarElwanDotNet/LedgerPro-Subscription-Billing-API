
using AutoMapper;
using BL.Contract;
using BL.Dtos;
using BL.Exceptions;
using DAL.Contracts;
using Domain;
using Domain.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
namespace BL.Services
{
    public class PaymentAttemptService
        : BaseService<TbPaymentAttempt, PaymentAttemptDto>, IPaymentAttempt
    {
        private readonly ITableRepository<TbPaymentAttempt> _paymentAttemptRepo;
        private readonly ITableRepository<TbInvoice> _invoiceRepo;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public PaymentAttemptService(
            ITableRepository<TbPaymentAttempt> paymentAttemptRepo,
            ITableRepository<TbInvoice> invoiceRepo,
            IMapper mapper,
            IUserService userService)
            : base(paymentAttemptRepo, mapper, userService)
        {
            _paymentAttemptRepo = paymentAttemptRepo;
            _invoiceRepo = invoiceRepo;
            _mapper = mapper;
            _userService = userService;
        }

        // ============================================================
        // ADD PAYMENT ATTEMPT
        // ============================================================

        public async Task<Guid> AddAsync(PaymentAttemptDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.Status == PaymentAttemptStatus.Succeeded)
            {
                throw new InvalidOperationException(
                    "Succeeded payment attempts must be recorded through the payment processing workflow.");
            }

            return await RecordAttemptAsync(dto);
        }

        // ============================================================
        // RECORD PAYMENT ATTEMPT
        // ============================================================


        public async Task<Guid> RecordAsync(PaymentAttemptDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            return await RecordAttemptAsync(dto);
        }



        // ============================================================
        // INTERNAL PAYMENT ATTEMPT RECORDING
        // ============================================================


        private async Task<Guid> RecordAttemptAsync(PaymentAttemptDto dto)
        {
            if (dto.InvoiceId == Guid.Empty)
                throw new ArgumentException("Invoice ID is required.");

            if (dto.Amount <= 0)
                throw new ArgumentException(
                    "Payment attempt amount must be greater than zero.");

            var currentUserId = _userService.GetLoggedInUser();

            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "Authenticated user is required.");

            var currentUser =
                await _userService.GetUserByIdAsync(
                    currentUserId.ToString());

            if (currentUser == null)
                throw new UnauthorizedAccessException(
                    "Authenticated user was not found.");

            var isPlatformAdmin =
                currentUser.Roles.Contains("Platform Admin");

            var isCustomerAdmin =
                currentUser.Roles.Contains("Customer Admin");

            if (!isPlatformAdmin && !isCustomerAdmin)
                throw new ForbiddenAccessException(
                    "You are not authorized to create a payment attempt.");

            // ========================================================
            // GET ACTIVE INVOICE
            // ========================================================

            var invoice =
                await _invoiceRepo.GetFirstOrDefaultAsync(
                    x =>
                        x.Id == dto.InvoiceId &&
                        x.CurrentState > 0);

            if (invoice == null)
                throw new KeyNotFoundException(
                    "Invoice not found.");

            // ========================================================
            // TENANT ISOLATION
            // ========================================================

            if (!isPlatformAdmin)
            {
                if (currentUser.OrganizationId == null)
                    throw new UnauthorizedAccessException(
                        "User organization was not found.");

                if (invoice.OrganizationId != currentUser.OrganizationId)
                    throw new ForbiddenAccessException(
                        "You are not authorized to access this invoice.");
            }

            // ========================================================
            // PAYMENT ATTEMPT MUST BELONG TO A PAYABLE INVOICE
            // ========================================================

            if (invoice.Status != InvoiceStatus.Pending &&
                invoice.Status != InvoiceStatus.PaymentFailed)
            {
                throw new InvalidOperationException(
                    "A payment attempt can only be recorded for a pending or payment-failed invoice.");
            }

            // ========================================================
            // AMOUNT MUST MATCH THE INVOICE
            // ========================================================

            if (dto.Amount != invoice.Amount)
            {
                throw new InvalidOperationException(
                    "Payment attempt amount must match the invoice amount.");
            }

            // ========================================================
            // CURRENCY MUST MATCH THE INVOICE
            // ========================================================

            if (string.IsNullOrWhiteSpace(invoice.Currency))
                throw new InvalidOperationException(
                    "Invoice currency is required.");

            if (string.IsNullOrWhiteSpace(dto.Currency))
                throw new ArgumentException(
                    "Payment attempt currency is required.");

            if (!string.Equals(
                    dto.Currency.Trim(),
                    invoice.Currency.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Payment attempt currency must match the invoice currency.");
            }

            // ========================================================
            // ONLY VALID PAYMENT ATTEMPT STATUSES ARE ALLOWED
            // ========================================================

            if (dto.Status != PaymentAttemptStatus.Pending &&
                dto.Status != PaymentAttemptStatus.Succeeded &&
                dto.Status != PaymentAttemptStatus.Failed)
            {
                throw new InvalidOperationException(
                    "Invalid payment attempt status.");
            }

            // ========================================================
            // ATTEMPTED AT MUST BE UTC
            // ========================================================

            var attemptedAt = dto.AttemptedAt == default
                ? DateTime.UtcNow
                : dto.AttemptedAt;

            if (attemptedAt.Kind == DateTimeKind.Local)
            {
                attemptedAt = attemptedAt.ToUniversalTime();
            }
            else if (attemptedAt.Kind == DateTimeKind.Unspecified)
            {
                attemptedAt = DateTime.SpecifyKind(
                    attemptedAt,
                    DateTimeKind.Utc);
            }

            // ========================================================
            // PROVIDER REFERENCE
            // ========================================================

            if (string.IsNullOrWhiteSpace(dto.ProviderReference))
                throw new ArgumentException(
                    "Provider reference is required.");

            // ========================================================
            // CREATE PAYMENT ATTEMPT RECORD
            // ========================================================

            var attempt =
                _mapper.Map<TbPaymentAttempt>(dto);

            attempt.Id = Guid.NewGuid();
            attempt.InvoiceId = invoice.Id;
            attempt.Amount = invoice.Amount;
            attempt.AttemptedAt = attemptedAt;
            attempt.ProviderReference =
                dto.ProviderReference.Trim();

            await _paymentAttemptRepo.AddAsync(
                attempt,
                currentUserId);

            return attempt.Id;
        }



        // ============================================================
        // GET BY ID
        // ============================================================


        public override async Task<PaymentAttemptDto?> GetByIdAsync(Guid id)
        {
            // ========================================================
            // VALIDATE PAYMENT ATTEMPT ID
            // ========================================================

            if (id == Guid.Empty)
                throw new ArgumentException(
                    "Payment attempt ID is required.");

            // ========================================================
            // AUTHENTICATION
            // ========================================================

            var currentUserId = _userService.GetLoggedInUser();

            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "Authenticated user is required.");

            var currentUser =
                await _userService.GetUserByIdAsync(
                    currentUserId.ToString());

            if (currentUser == null)
                throw new UnauthorizedAccessException(
                    "Authenticated user was not found.");

            // ========================================================
            // AUTHORIZATION
            // ========================================================

            var isPlatformAdmin =
                currentUser.Roles.Contains("Platform Admin");

            var isCustomerAdmin =
                currentUser.Roles.Contains("Customer Admin");

            if (!isPlatformAdmin && !isCustomerAdmin)
                throw new ForbiddenAccessException(
                    "You are not authorized to access payment attempts.");

            // ========================================================
            // GET ACTIVE PAYMENT ATTEMPT
            // ========================================================

            var attempt =
                await _paymentAttemptRepo
                    .GetTable()
                    .Include(x => x.Invoice)
                    .ThenInclude(x => x.Plan)
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id == id &&
                            x.CurrentState > 0 &&
                            x.Invoice != null &&
                            x.Invoice.CurrentState > 0);

            // ========================================================
            // NOT FOUND
            // ========================================================

            if (attempt == null)
                return null;

            // ========================================================
            // TENANT ISOLATION
            // ========================================================

            if (!isPlatformAdmin)
            {
                if (currentUser.OrganizationId == null)
                    throw new UnauthorizedAccessException(
                        "User organization was not found.");

                if (attempt.Invoice!.OrganizationId !=
                    currentUser.OrganizationId)
                {
                    throw new ForbiddenAccessException(
                        "You are not authorized to access this payment attempt.");
                }
            }

            // ========================================================
            // MAP RESULT
            // ========================================================

            return _mapper.Map<PaymentAttemptDto>(attempt);
        }



        // ============================================================
        // GET BY INVOICE
        // ============================================================


        public async Task<List<PaymentAttemptDto>> GetByInvoiceIdAsync(
            Guid invoiceId)
        {
            // ========================================================
            // VALIDATE INVOICE ID
            // ========================================================

            if (invoiceId == Guid.Empty)
                throw new ArgumentException(
                    "Invoice ID is required.");

            // ========================================================
            // AUTHENTICATION
            // ========================================================

            var currentUserId = _userService.GetLoggedInUser();

            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "Authenticated user is required.");

            var currentUser =
                await _userService.GetUserByIdAsync(
                    currentUserId.ToString());

            if (currentUser == null)
                throw new UnauthorizedAccessException(
                    "Authenticated user was not found.");

            // ========================================================
            // AUTHORIZATION
            // ========================================================

            var isPlatformAdmin =
                currentUser.Roles.Contains("Platform Admin");

            var isCustomerAdmin =
                currentUser.Roles.Contains("Customer Admin");

            if (!isPlatformAdmin && !isCustomerAdmin)
                throw new ForbiddenAccessException(
                    "You are not authorized to access payment attempts.");

            // ========================================================
            // GET ACTIVE INVOICE
            // ========================================================

            var invoice =
                await _invoiceRepo.GetFirstOrDefaultAsync(
                    x =>
                        x.Id == invoiceId &&
                        x.CurrentState > 0);

            if (invoice == null)
                throw new KeyNotFoundException(
                    "Invoice not found.");

            // ========================================================
            // TENANT ISOLATION
            // ========================================================

            if (!isPlatformAdmin)
            {
                if (currentUser.OrganizationId == null)
                    throw new UnauthorizedAccessException(
                        "User organization was not found.");

                if (invoice.OrganizationId !=
                    currentUser.OrganizationId)
                {
                    throw new ForbiddenAccessException(
                        "You are not authorized to access this invoice.");
                }
            }

            // ========================================================
            // GET ACTIVE PAYMENT ATTEMPTS
            // ========================================================

            var attempts =
                await _paymentAttemptRepo
                    .GetTable()
                    .Include(x => x.Invoice)
                    .ThenInclude(x => x.Plan)
                    .Where(x =>
                        x.CurrentState > 0 &&
                        x.InvoiceId == invoiceId &&
                        x.Invoice != null &&
                        x.Invoice.CurrentState > 0)
                    .OrderByDescending(x => x.AttemptedAt)
                    .ToListAsync();

            // ========================================================
            // RETURN RESULTS
            // ========================================================

            return _mapper.Map<List<PaymentAttemptDto>>(attempts);
        }




        // ============================================================
        // GET FAILED ATTEMPTS
        // ============================================================


        public async Task<List<PaymentAttemptDto>> GetFailedAsync()
        {
            // ========================================================
            // AUTHENTICATION
            // ========================================================

            var currentUserId = _userService.GetLoggedInUser();

            if (currentUserId == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "Authenticated user is required.");

            var currentUser =
                await _userService.GetUserByIdAsync(
                    currentUserId.ToString());

            if (currentUser == null)
                throw new UnauthorizedAccessException(
                    "Authenticated user was not found.");

            // ========================================================
            // AUTHORIZATION
            // FAILED PAYMENT ATTEMPTS ARE REVIEWED BY PLATFORM ADMIN
            // ========================================================

            if (!currentUser.Roles.Contains("Platform Admin"))
                throw new ForbiddenAccessException(
                    "Only Platform Admin can review failed payment attempts.");

            // ========================================================
            // GET ACTIVE FAILED PAYMENT ATTEMPTS
            // ========================================================

            var attempts =
                await _paymentAttemptRepo
                    .GetTable()
                    .Include(x => x.Invoice)
                    .ThenInclude(x => x.Plan)
                    .Where(x =>
                        x.CurrentState > 0 &&
                        x.Status == PaymentAttemptStatus.Failed &&
                        x.Invoice != null &&
                        x.Invoice.CurrentState > 0)
                    .OrderByDescending(x => x.AttemptedAt)
                    .ToListAsync();

            // ========================================================
            // RETURN RESULTS
            // ========================================================

            return _mapper.Map<List<PaymentAttemptDto>>(attempts);
        }


    }
}
