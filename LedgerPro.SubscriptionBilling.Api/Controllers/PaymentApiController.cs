using BL.Contract;
using BL.Dtos;
using BL.Exceptions;
using LedgerPro.SubscriptionBilling.Api.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LedgerPro.SubscriptionBilling.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentApiController : ControllerBase
    {
        private readonly IPayment _paymentService;

        public PaymentApiController(IPayment paymentService)
        {
            _paymentService = paymentService;
        }

        // =========================================================
        // GET: api/PaymentApi/{id}
        // Get payment by ID
        // =========================================================
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<PaymentDto>>> GetById(
            Guid id)
        {
            var traceId = HttpContext.TraceIdentifier;

            if (id == Guid.Empty)
            {
                return BadRequest(
                    ApiResponse<PaymentDto>.BadRequest(
                        "Payment ID is required.",
                        traceId: traceId));
            }

            try
            {
                var payment =
                    await _paymentService.GetByIdAsync(id);

                if (payment == null)
                {
                    return NotFound(
                        ApiResponse<PaymentDto>.NotFound(
                            "Payment not found.",
                            traceId: traceId));
                }

                return Ok(
                    ApiResponse<PaymentDto>.Ok(
                        payment,
                        "Payment retrieved successfully.",
                        traceId: traceId));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status401Unauthorized,
                    ApiResponse<PaymentDto>.Unauthorized(
                        ex.Message,
                        traceId: traceId));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<PaymentDto>.Forbidden(
                        ex.Message,
                        traceId: traceId));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<PaymentDto>.NotFound(
                        ex.Message,
                        traceId: traceId));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(
                    ApiResponse<PaymentDto>.UnprocessableEntity(
                        ex.Message,
                        traceId: traceId));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<PaymentDto>.ServerError(
                        traceId: traceId));
            }
        }

        // =========================================================
        // GET: api/PaymentApi/invoice/{invoiceId}
        // Get payments by invoice
        // =========================================================
        [HttpGet("invoice/{invoiceId:guid}")]
        public async Task<ActionResult<ApiResponse<List<PaymentDto>>>>
            GetByInvoiceId(Guid invoiceId)
        {
            var traceId = HttpContext.TraceIdentifier;

            if (invoiceId == Guid.Empty)
            {
                return BadRequest(
                    ApiResponse<List<PaymentDto>>.BadRequest(
                        "Invoice ID is required.",
                        traceId: traceId));
            }

            try
            {
                var payments =
                    await _paymentService
                        .GetByInvoiceIdAsync(invoiceId);

                return Ok(
                    ApiResponse<List<PaymentDto>>.Ok(
                        payments,
                        "Payments retrieved successfully.",
                        traceId: traceId));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status401Unauthorized,
                    ApiResponse<List<PaymentDto>>.Unauthorized(
                        ex.Message,
                        traceId: traceId));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<List<PaymentDto>>.Forbidden(
                        ex.Message,
                        traceId: traceId));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<List<PaymentDto>>.NotFound(
                        ex.Message,
                        traceId: traceId));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(
                    ApiResponse<List<PaymentDto>>.UnprocessableEntity(
                        ex.Message,
                        traceId: traceId));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<List<PaymentDto>>.ServerError(
                        traceId: traceId));
            }
        }

        // =========================================================
        // GET: api/PaymentApi/organization/{organizationId}
        // Get payments by organization
        // =========================================================
        [HttpGet("organization/{organizationId:guid}")]
        public async Task<ActionResult<ApiResponse<List<PaymentDto>>>>
            GetByOrganizationId(Guid organizationId)
        {
            var traceId = HttpContext.TraceIdentifier;

            if (organizationId == Guid.Empty)
            {
                return BadRequest(
                    ApiResponse<List<PaymentDto>>.BadRequest(
                        "Organization ID is required.",
                        traceId: traceId));
            }

            try
            {
                var payments =
                    await _paymentService
                        .GetByOrganizationIdAsync(organizationId);

                return Ok(
                    ApiResponse<List<PaymentDto>>.Ok(
                        payments,
                        "Payments retrieved successfully.",
                        traceId: traceId));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status401Unauthorized,
                    ApiResponse<List<PaymentDto>>.Unauthorized(
                        ex.Message,
                        traceId: traceId));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<List<PaymentDto>>.Forbidden(
                        ex.Message,
                        traceId: traceId));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<List<PaymentDto>>.NotFound(
                        ex.Message,
                        traceId: traceId));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(
                    ApiResponse<List<PaymentDto>>.UnprocessableEntity(
                        ex.Message,
                        traceId: traceId));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<List<PaymentDto>>.ServerError(
                        traceId: traceId));
            }
        }

        // =========================================================
        // POST: api/PaymentApi/process
        // Process payment
        // =========================================================
        [HttpPost("process")]
        public async Task<ActionResult<ApiResponse<Guid>>> ProcessPayment(
            [FromBody] PaymentDto dto)
        {
            var traceId = HttpContext.TraceIdentifier;

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value != null)
                    .SelectMany(x =>
                        x.Value!.Errors.Select(error =>
                            new ApiError(
                                ApiErrorCodes.ValidationError,
                                string.IsNullOrWhiteSpace(
                                    error.ErrorMessage)
                                    ? "Invalid value."
                                    : error.ErrorMessage,
                                string.IsNullOrWhiteSpace(x.Key)
                                    ? null
                                    : x.Key)))
                    .ToList();

                return BadRequest(
                    ApiResponse<Guid>.Validation(
                        errors,
                        traceId: traceId));
            }

            try
            {
                var paymentId =
                    await _paymentService
                        .ProcessPaymentAsync(dto);

                return Ok(
                    ApiResponse<Guid>.Ok(
                        paymentId,
                        "Payment processed successfully.",
                        traceId: traceId));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status401Unauthorized,
                    ApiResponse<Guid>.Unauthorized(
                        ex.Message,
                        traceId: traceId));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<Guid>.Forbidden(
                        ex.Message,
                        traceId: traceId));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<Guid>.NotFound(
                        ex.Message,
                        traceId: traceId));
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(
                    ApiResponse<Guid>.BadRequest(
                        ex.Message,
                        traceId: traceId));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    ApiResponse<Guid>.BadRequest(
                        ex.Message,
                        traceId: traceId));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(
                    ApiResponse<Guid>.UnprocessableEntity(
                        ex.Message,
                        traceId: traceId));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<Guid>.ServerError(
                        traceId: traceId));
            }
        }

        // =========================================================
        // POST: api/PaymentApi/result
        // Handle payment provider result / callback
        // =========================================================
        [HttpPost("result")]
        public async Task<ActionResult<ApiResponse<bool>>>
            HandlePaymentResult([FromBody] PaymentDto dto)
        {
            var traceId = HttpContext.TraceIdentifier;

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value != null)
                    .SelectMany(x =>
                        x.Value!.Errors.Select(error =>
                            new ApiError(
                                ApiErrorCodes.ValidationError,
                                string.IsNullOrWhiteSpace(
                                    error.ErrorMessage)
                                    ? "Invalid value."
                                    : error.ErrorMessage,
                                string.IsNullOrWhiteSpace(x.Key)
                                    ? null
                                    : x.Key)))
                    .ToList();

                return BadRequest(
                    ApiResponse<bool>.Validation(
                        errors,
                        traceId: traceId));
            }

            try
            {
                var result =
                    await _paymentService
                        .HandlePaymentResultAsync(dto);

                return Ok(
                    ApiResponse<bool>.Ok(
                        result,
                        "Payment result handled successfully.",
                        traceId: traceId));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status401Unauthorized,
                    ApiResponse<bool>.Unauthorized(
                        ex.Message,
                        traceId: traceId));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<bool>.Forbidden(
                        ex.Message,
                        traceId: traceId));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<bool>.NotFound(
                        ex.Message,
                        traceId: traceId));
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(
                    ApiResponse<bool>.BadRequest(
                        ex.Message,
                        traceId: traceId));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    ApiResponse<bool>.BadRequest(
                        ex.Message,
                        traceId: traceId));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(
                    ApiResponse<bool>.UnprocessableEntity(
                        ex.Message,
                        traceId: traceId));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<bool>.ServerError(
                        traceId: traceId));
            }
        }
    }
}