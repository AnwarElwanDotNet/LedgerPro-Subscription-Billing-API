using BL.Contract;
using BL.Dtos;
using BL.Exceptions;
using Domain;
using LedgerPro.SubscriptionBilling.Api.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

namespace LedgerPro.SubscriptionBilling.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentAttemptApiController : ControllerBase
    {
        private readonly IPaymentAttempt _paymentAttemptService;

        public PaymentAttemptApiController(
            IPaymentAttempt paymentAttemptService)
        {
            _paymentAttemptService = paymentAttemptService;
        }

        // =========================================================
        // CREATE / RECORD PAYMENT ATTEMPT
        // =========================================================

        [HttpPost]
        [ProducesResponseType(
            typeof(ApiResponse<Guid>),
            StatusCodes.Status201Created)]

        [ProducesResponseType(
            typeof(ApiResponse<Guid>),
            StatusCodes.Status400BadRequest)]

        [ProducesResponseType(
            typeof(ApiResponse<Guid>),
            StatusCodes.Status401Unauthorized)]

        [ProducesResponseType(
            typeof(ApiResponse<Guid>),
            StatusCodes.Status403Forbidden)]

        [ProducesResponseType(
            typeof(ApiResponse<Guid>),
            StatusCodes.Status404NotFound)]

        [ProducesResponseType(
            typeof(ApiResponse<Guid>),
            StatusCodes.Status422UnprocessableEntity)]

        [ProducesResponseType(
            typeof(ApiResponse<Guid>),
            StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<ApiResponse<Guid>>> Add(
            [FromBody] PaymentAttemptDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(
                        ApiResponse<Guid>.BadRequest(
                            "Invalid payment attempt data.",
                            traceId: HttpContext.TraceIdentifier));
                }

                var id =
                    await _paymentAttemptService.AddAsync(dto);

                return StatusCode(
                    StatusCodes.Status201Created,
                    ApiResponse<Guid>.Created(
                        id,
                        "Payment attempt recorded successfully.",
                        HttpContext.TraceIdentifier));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status401Unauthorized,
                    ApiResponse<Guid>.Unauthorized(
                        ex.Message,
                        HttpContext.TraceIdentifier));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<Guid>.Forbidden(
                        ex.Message,
                        HttpContext.TraceIdentifier));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<Guid>.NotFound(
                        ex.Message,
                        HttpContext.TraceIdentifier));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    ApiResponse<Guid>.BadRequest(
                        ex.Message,
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(
                    ApiResponse<Guid>.UnprocessableEntity(
                        ex.Message,
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<Guid>.ServerError(
                        "An unexpected error occurred while recording the payment attempt.",
                        traceId: HttpContext.TraceIdentifier));
            }
        }

        // =========================================================
        // GET PAYMENT ATTEMPT BY ID
        // =========================================================

        [HttpGet("{id:guid}")]
        [ProducesResponseType(
            typeof(ApiResponse<PaymentAttemptDto>),
            StatusCodes.Status200OK)]

        [ProducesResponseType(
            typeof(ApiResponse<PaymentAttemptDto>),
            StatusCodes.Status400BadRequest)]

        [ProducesResponseType(
            typeof(ApiResponse<PaymentAttemptDto>),
            StatusCodes.Status401Unauthorized)]

        [ProducesResponseType(
            typeof(ApiResponse<PaymentAttemptDto>),
            StatusCodes.Status403Forbidden)]

        [ProducesResponseType(
            typeof(ApiResponse<PaymentAttemptDto>),
            StatusCodes.Status404NotFound)]

        [ProducesResponseType(
            typeof(ApiResponse<PaymentAttemptDto>),
            StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<ApiResponse<PaymentAttemptDto>>> GetById(
            Guid id)
        {
            try
            {
                var result =
                    await _paymentAttemptService.GetByIdAsync(id);

                if (result == null)
                {
                    return NotFound(
                        ApiResponse<PaymentAttemptDto>.NotFound(
                            "Payment attempt not found.",
                            HttpContext.TraceIdentifier));
                }

                return Ok(
                    ApiResponse<PaymentAttemptDto>.Ok(
                        result,
                        "Payment attempt retrieved successfully.",
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status401Unauthorized,
                    ApiResponse<PaymentAttemptDto>.Unauthorized(
                        ex.Message,
                        HttpContext.TraceIdentifier));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<PaymentAttemptDto>.Forbidden(
                        ex.Message,
                        HttpContext.TraceIdentifier));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    ApiResponse<PaymentAttemptDto>.BadRequest(
                        ex.Message,
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<PaymentAttemptDto>.ServerError(
                        "An unexpected error occurred while retrieving the payment attempt.",
                        traceId: HttpContext.TraceIdentifier));
            }
        }

        // =========================================================
        // GET PAYMENT ATTEMPTS BY INVOICE
        // =========================================================

        [HttpGet("invoice/{invoiceId:guid}")]
        [ProducesResponseType(
            typeof(ApiResponse<List<PaymentAttemptDto>>),
            StatusCodes.Status200OK)]

        [ProducesResponseType(
            typeof(ApiResponse<List<PaymentAttemptDto>>),
            StatusCodes.Status400BadRequest)]

        [ProducesResponseType(
            typeof(ApiResponse<List<PaymentAttemptDto>>),
            StatusCodes.Status401Unauthorized)]

        [ProducesResponseType(
            typeof(ApiResponse<List<PaymentAttemptDto>>),
            StatusCodes.Status403Forbidden)]

        [ProducesResponseType(
            typeof(ApiResponse<List<PaymentAttemptDto>>),
            StatusCodes.Status404NotFound)]

        [ProducesResponseType(
            typeof(ApiResponse<List<PaymentAttemptDto>>),
            StatusCodes.Status500InternalServerError)]

        public async Task<
            ActionResult<ApiResponse<List<PaymentAttemptDto>>>>
            GetByInvoiceId(Guid invoiceId)
        {
            try
            {
                var result =
                    await _paymentAttemptService.GetByInvoiceIdAsync(
                        invoiceId);

                return Ok(
                    ApiResponse<List<PaymentAttemptDto>>.Ok(
                        result,
                        "Payment attempts retrieved successfully.",
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status401Unauthorized,
                    ApiResponse<List<PaymentAttemptDto>>.Unauthorized(
                        ex.Message,
                        HttpContext.TraceIdentifier));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<List<PaymentAttemptDto>>.Forbidden(
                        ex.Message,
                        HttpContext.TraceIdentifier));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<List<PaymentAttemptDto>>.NotFound(
                        ex.Message,
                        HttpContext.TraceIdentifier));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    ApiResponse<List<PaymentAttemptDto>>.BadRequest(
                        ex.Message,
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<List<PaymentAttemptDto>>.ServerError(
                        "An unexpected error occurred while retrieving payment attempts.",
                        traceId: HttpContext.TraceIdentifier));
            }
        }

        // =========================================================
        // GET FAILED PAYMENT ATTEMPTS
        // PLATFORM ADMIN ONLY
        // =========================================================

        [HttpGet("failed")]
        [ProducesResponseType(
            typeof(ApiResponse<List<PaymentAttemptDto>>),
            StatusCodes.Status200OK)]

        [ProducesResponseType(
            typeof(ApiResponse<List<PaymentAttemptDto>>),
            StatusCodes.Status401Unauthorized)]

        [ProducesResponseType(
            typeof(ApiResponse<List<PaymentAttemptDto>>),
            StatusCodes.Status403Forbidden)]

        [ProducesResponseType(
            typeof(ApiResponse<List<PaymentAttemptDto>>),
            StatusCodes.Status500InternalServerError)]

        public async Task<
            ActionResult<ApiResponse<List<PaymentAttemptDto>>>>
            GetFailed()
        {
            try
            {
                var result =
                    await _paymentAttemptService.GetFailedAsync();

                return Ok(
                    ApiResponse<List<PaymentAttemptDto>>.Ok(
                        result,
                        "Failed payment attempts retrieved successfully.",
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status401Unauthorized,
                    ApiResponse<List<PaymentAttemptDto>>.Unauthorized(
                        ex.Message,
                        HttpContext.TraceIdentifier));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<List<PaymentAttemptDto>>.Forbidden(
                        ex.Message,
                        HttpContext.TraceIdentifier));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<List<PaymentAttemptDto>>.ServerError(
                        "An unexpected error occurred while retrieving failed payment attempts.",
                        traceId: HttpContext.TraceIdentifier));
            }
        }
    }
}
