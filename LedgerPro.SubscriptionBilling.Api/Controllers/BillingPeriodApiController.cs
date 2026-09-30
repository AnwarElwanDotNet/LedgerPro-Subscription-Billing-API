using BL.Contract;
using BL.Dtos;
using BL.Exceptions;
using LedgerPro.SubscriptionBilling.Api.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LedgerPro.SubscriptionBilling.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Platform Admin,Customer Admin")]
    public class BillingPeriodApiController : ControllerBase
    {
        private readonly IBillingPeriod _billingPeriodService;

        public BillingPeriodApiController(
            IBillingPeriod billingPeriodService)
        {
            _billingPeriodService = billingPeriodService;
        }

        // =========================================================
        // Get Billing Period By Id
        // =========================================================
        [HttpGet("{id:guid}")]
        [ProducesResponseType(
            typeof(ApiResponse<BillingPeriodDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<BillingPeriodDto>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<BillingPeriodDto>),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<BillingPeriodDto>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<BillingPeriodDto>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<BillingPeriodDto>),
            StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(
            typeof(ApiResponse<BillingPeriodDto>),
            StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            string traceId = HttpContext.TraceIdentifier;

            try
            {
                BillingPeriodDto billingPeriod =
                    await _billingPeriodService.GetByIdAsync(id);

                return Ok(
                    ApiResponse<BillingPeriodDto>.Ok(
                        billingPeriod,
                        "Billing period retrieved successfully.",
                        traceId: traceId));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    ApiResponse<BillingPeriodDto>.BadRequest(
                        ex.Message,
                        [
                            new ApiError(
                                ApiErrorCodes.InvalidValue,
                                ex.Message)
                        ],
                        traceId));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<BillingPeriodDto>.Unauthorized(
                        ex.Message,
                        traceId));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<BillingPeriodDto>.NotFound(
                        ex.Message,
                        traceId));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(
                    ApiResponse<BillingPeriodDto>.UnprocessableEntity(
                        ex.Message,
                        [
                            new ApiError(
                                ApiErrorCodes.BusinessRuleViolation,
                                ex.Message)
                        ],
                        traceId));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<BillingPeriodDto>.Forbidden(
                        ex.Message,
                        traceId));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<BillingPeriodDto>.ServerError(
                        "An unexpected error occurred.",
                        traceId));
            }
           
        }

        // =========================================================
        // Get All Billing Periods For A Subscription
        // =========================================================
        [HttpGet("subscription/{subscriptionId:guid}")]
        [ProducesResponseType(
            typeof(ApiResponse<IEnumerable<BillingPeriodDto>>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<IEnumerable<BillingPeriodDto>>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<IEnumerable<BillingPeriodDto>>),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<IEnumerable<BillingPeriodDto>>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<IEnumerable<BillingPeriodDto>>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<IEnumerable<BillingPeriodDto>>),
            StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(
            typeof(ApiResponse<IEnumerable<BillingPeriodDto>>),
            StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBySubscriptionId(
            Guid subscriptionId)
        {
            string traceId = HttpContext.TraceIdentifier;

            try
            {
                IEnumerable<BillingPeriodDto> billingPeriods =
                    await _billingPeriodService
                        .GetBySubscriptionIdAsync(subscriptionId);

                return Ok(
                    ApiResponse<IEnumerable<BillingPeriodDto>>.Ok(
                        billingPeriods,
                        "Billing periods retrieved successfully.",
                        traceId: traceId));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    ApiResponse<IEnumerable<BillingPeriodDto>>.BadRequest(
                        ex.Message,
                        [
                            new ApiError(
                                ApiErrorCodes.InvalidValue,
                                ex.Message)
                        ],
                        traceId));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<IEnumerable<BillingPeriodDto>>.Unauthorized(
                        ex.Message,
                        traceId));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<IEnumerable<BillingPeriodDto>>.NotFound(
                        ex.Message,
                        traceId));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(
                    ApiResponse<IEnumerable<BillingPeriodDto>>.UnprocessableEntity(
                        ex.Message,
                        [
                            new ApiError(
                                ApiErrorCodes.BusinessRuleViolation,
                                ex.Message)
                        ],
                        traceId));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<IEnumerable<BillingPeriodDto>>.Forbidden(
                        ex.Message,
                        traceId));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<IEnumerable<BillingPeriodDto>>.ServerError(
                        "An unexpected error occurred.",
                        traceId));
            }
         
        }
    }
}