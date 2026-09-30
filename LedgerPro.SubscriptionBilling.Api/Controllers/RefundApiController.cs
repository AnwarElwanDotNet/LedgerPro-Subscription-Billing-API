using BL.Contract;
using BL.Dtos;
using BL.Exceptions;
using LedgerPro.SubscriptionBilling.Api.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class RefundApiController : ControllerBase
    {
        private readonly IRefund _RefundService;

        public RefundApiController(IRefund refundService)
        {
            _RefundService = refundService;
        }

        // =========================================================
        // PROCESS REFUND
        // =========================================================

        [HttpPost("process")]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status409Conflict)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessRefund(
            [FromBody] RefundDto dto)
        {
            var traceId =
                HttpContext.TraceIdentifier;

            // =========================================================
            // Model Validation
            // =========================================================

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x =>
                        x.Value!.Errors.Select(error =>
                            new ApiError(
                                ApiErrorCodes.ValidationError,
                                string.IsNullOrWhiteSpace(error.ErrorMessage)
                                    ? "Invalid value."
                                    : error.ErrorMessage,
                                x.Key)))
                    .ToList();

                return BadRequest(
                    ApiResponse<object>.Validation(
                        errors,
                        "Validation failed",
                        traceId));
            }

            try
            {
              

                await _RefundService.ProcessRefundAsync(dto);

               

                return Ok(
                    ApiResponse<object>.NoContent(
                        "Refund processed successfully",
                        traceId));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<object>.Unauthorized(
                        ex.Message,
                        traceId));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Forbidden(
                        ex.Message,
                        traceId));
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(
                    ApiResponse<object>.BadRequest(
                        ex.Message,
                        new List<ApiError>
                        {
                            new ApiError(
                                ApiErrorCodes.BadRequest,
                                ex.Message,
                                ex.ParamName)
                        },
                        traceId));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<object>.NotFound(
                        ex.Message,
                        traceId));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(
                    ApiResponse<object>.UnprocessableEntity(
                        ex.Message,
                        new List<ApiError>
                        {
                            new ApiError(
                                ApiErrorCodes.BusinessRuleViolation,
                                ex.Message)
                        },
                        traceId));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.ServerError(
                        "An unexpected error occurred.",
                        traceId));
            }
        }
    }
}