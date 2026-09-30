using BL.Contract;
using BL.Dtos;
using BL.Exceptions;
using LedgerPro.SubscriptionBilling.Api.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LedgerPro.SubscriptionBilling.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Platform Admin,Customer Admin")]
    public class CancellationRecordApiController : ControllerBase
    {
        private readonly ICancellationRecord _cancellationRecordService;

        public CancellationRecordApiController(
            ICancellationRecord cancellationRecordService)
        {
            _cancellationRecordService = cancellationRecordService;
        }

        // ============================================================
        // Get Cancellation Record By Id
        // ============================================================
        [HttpGet("{id:guid}")]
        [ProducesResponseType(
            typeof(ApiResponse<CancellationRecordDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<CancellationRecordDto>),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<CancellationRecordDto>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<CancellationRecordDto>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<CancellationRecordDto>),
            StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(
    typeof(ApiResponse<CancellationRecordDto>),
    StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result =
                    await _cancellationRecordService.GetByIdAsync(id);

                return StatusCode(
                    StatusCodes.Status200OK,
                    ApiResponse<CancellationRecordDto>.Ok(
                        result,
                        "Cancellation record retrieved successfully.",
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status401Unauthorized,
                    ApiResponse<CancellationRecordDto>.Unauthorized(
                        ex.Message,
                        HttpContext.TraceIdentifier));
            }
            catch (KeyNotFoundException ex)
            {
                return StatusCode(
                    StatusCodes.Status404NotFound,
                    ApiResponse<CancellationRecordDto>.NotFound(
                        ex.Message,
                        HttpContext.TraceIdentifier));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<CancellationRecordDto>.Forbidden(
                        ex.Message,
                        HttpContext.TraceIdentifier));
            }
            catch (ArgumentException ex)
            {
                return StatusCode(
                    StatusCodes.Status400BadRequest,
                    ApiResponse<CancellationRecordDto>.BadRequest(
                        ex.Message,
                        [
                            new ApiError(
                    ApiErrorCodes.InvalidValue,
                    ex.Message)
                        ],
                        HttpContext.TraceIdentifier));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<CancellationRecordDto>.ServerError(
                        "An unexpected error occurred while retrieving the cancellation record.",
                        HttpContext.TraceIdentifier));
            }
        }


        // ============================================================
        // Get Cancellation Records By Subscription
        // ============================================================
        [HttpGet("subscription/{subscriptionId:guid}")]
        [ProducesResponseType(
            typeof(ApiResponse<IEnumerable<CancellationRecordDto>>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<IEnumerable<CancellationRecordDto>>),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<IEnumerable<CancellationRecordDto>>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<IEnumerable<CancellationRecordDto>>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<IEnumerable<CancellationRecordDto>>),
            StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(
    typeof(ApiResponse<IEnumerable<CancellationRecordDto>>),
    StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBySubscriptionId(
            Guid subscriptionId)
        {
            try
            {
                var result =
                    await _cancellationRecordService
                        .GetBySubscriptionIdAsync(subscriptionId);

                return StatusCode(
                    StatusCodes.Status200OK,
                    ApiResponse<IEnumerable<CancellationRecordDto>>.Ok(
                        result,
                        "Cancellation records retrieved successfully.",
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status401Unauthorized,
                    ApiResponse<IEnumerable<CancellationRecordDto>>.Unauthorized(
                        ex.Message,
                        HttpContext.TraceIdentifier));
            }
            catch (KeyNotFoundException ex)
            {
                return StatusCode(
                    StatusCodes.Status404NotFound,
                    ApiResponse<IEnumerable<CancellationRecordDto>>.NotFound(
                        ex.Message,
                        HttpContext.TraceIdentifier));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<IEnumerable<CancellationRecordDto>>.Forbidden(
                        ex.Message,
                        HttpContext.TraceIdentifier));
            }
            catch (ArgumentException ex)
            {
                return StatusCode(
                    StatusCodes.Status400BadRequest,
                    ApiResponse<IEnumerable<CancellationRecordDto>>.BadRequest(
                        ex.Message,
                        [
                            new ApiError(
                    ApiErrorCodes.InvalidValue,
                    ex.Message)
                        ],
                        HttpContext.TraceIdentifier));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<IEnumerable<CancellationRecordDto>>.ServerError(
                        "An unexpected error occurred while retrieving cancellation records.",
                        HttpContext.TraceIdentifier));
            }
        }
    }
}
