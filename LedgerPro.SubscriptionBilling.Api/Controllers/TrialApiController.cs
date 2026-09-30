using BL.Contract;
using BL.Dtos;
using BL.Exceptions;
using LedgerPro.SubscriptionBilling.Api.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
namespace LedgerPro.SubscriptionBilling.Api.Controllers
{
    [Route("api/trials")]
    [ApiController]
    [Authorize]
    public class TrialApiController : ControllerBase
    {
        private readonly ITrial _trialService;

        public TrialApiController(ITrial trialService)
        {
            _trialService = trialService;
        }

        // =========================================================
        // START TRIAL
        // =========================================================

        [HttpPost("{organizationId:guid}/start")]
        [ProducesResponseType(
            typeof(ApiResponse<TrialDto>),
            StatusCodes.Status201Created)]
        [ProducesResponseType(
            typeof(ApiResponse<TrialDto>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<TrialDto>),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<TrialDto>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<TrialDto>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<TrialDto>),
            StatusCodes.Status409Conflict)]
        [ProducesResponseType(
            typeof(ApiResponse<TrialDto>),
            StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<TrialDto>>> StartTrial(
    Guid organizationId)
        {
            var traceId = HttpContext.TraceIdentifier;

            try
            {
                var result = await _trialService.StartTrialAsync(organizationId);

                return StatusCode(
                    StatusCodes.Status201Created,
                    ApiResponse<TrialDto>.Created(
                        result,
                        "Trial started successfully.",
                        traceId));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<TrialDto>.Unauthorized(
                        ex.Message,
                        traceId));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<TrialDto>.Forbidden(
                        ex.Message,
                        traceId));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    ApiResponse<TrialDto>.BadRequest(
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
                    ApiResponse<TrialDto>.NotFound(
                        ex.Message,
                        traceId));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(
                    ApiResponse<TrialDto>.Conflict(
                        ex.Message,
                        new List<ApiError>
                        {
                    new ApiError(
                        ApiErrorCodes.Conflict,
                        ex.Message)
                        },
                        traceId));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<TrialDto>.ServerError(
                        "An unexpected error occurred.",
                        traceId));
            }
        }


        // =========================================================
        // GET CURRENT TRIAL
        // =========================================================

        [HttpGet("{organizationId:guid}/current")]
        [ProducesResponseType(
            typeof(ApiResponse<TrialDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<TrialDto>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<TrialDto>),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<TrialDto>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<TrialDto>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<TrialDto>),
            StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<TrialDto>>> GetCurrentTrial(
    Guid organizationId)
        {
            var traceId = HttpContext.TraceIdentifier;

            try
            {
                var result =
                    await _trialService.GetCurrentTrialAsync(organizationId);

                return Ok(
                    ApiResponse<TrialDto>.Ok(
                        result,
                        "Current trial retrieved successfully.",
                        traceId: traceId));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<TrialDto>.Unauthorized(
                        ex.Message,
                        traceId));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<TrialDto>.Forbidden(
                        ex.Message,
                        traceId));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    ApiResponse<TrialDto>.BadRequest(
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
                    ApiResponse<TrialDto>.NotFound(
                        ex.Message,
                        traceId));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<TrialDto>.ServerError(
                        "An unexpected error occurred.",
                        traceId));
            }
        }


        // =========================================================
        // EXPIRE TRIAL
        // =========================================================

        [HttpPost("{trialId:guid}/expire")]
        [ProducesResponseType(
            typeof(ApiResponse<TrialDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<TrialDto>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<TrialDto>),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<TrialDto>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<TrialDto>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<TrialDto>),
            StatusCodes.Status409Conflict)]
        [ProducesResponseType(
            typeof(ApiResponse<TrialDto>),
            StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<TrialDto>>> ExpireTrial(
    Guid trialId)
        {
            var traceId = HttpContext.TraceIdentifier;

            try
            {
                var result =
                    await _trialService.ExpireTrialAsync(trialId);

                return Ok(
                    ApiResponse<TrialDto>.Ok(
                        result,
                        "Trial expired successfully.",
                        traceId: traceId));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<TrialDto>.Unauthorized(
                        ex.Message,
                        traceId));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<TrialDto>.Forbidden(
                        ex.Message,
                        traceId));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    ApiResponse<TrialDto>.BadRequest(
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
                    ApiResponse<TrialDto>.NotFound(
                        ex.Message,
                        traceId));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(
                    ApiResponse<TrialDto>.Conflict(
                        ex.Message,
                        new List<ApiError>
                        {
                    new ApiError(
                        ApiErrorCodes.Conflict,
                        ex.Message)
                        },
                        traceId));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<TrialDto>.ServerError(
                        "An unexpected error occurred.",
                        traceId));
            }
        }
    }
}