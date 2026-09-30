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
    public class SystemConfigApiController : ControllerBase
    {
        private readonly ISystemConfig _systemConfig;

        public SystemConfigApiController(
            ISystemConfig systemConfig)
        {
            _systemConfig = systemConfig;
        }

        // =====================================================================
        // GET /api/SystemConfigApi
        // =====================================================================

        [HttpGet]
        [ProducesResponseType(
            typeof(ApiResponse<SystemConfigDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<SystemConfigDto>),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<SystemConfigDto>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<SystemConfigDto>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<SystemConfigDto>),
            StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<SystemConfigDto>>> Get()
        {
            var traceId = HttpContext.TraceIdentifier;

            try
            {
                var result =
                    await _systemConfig.GetAsync();

                return Ok(
                    ApiResponse<SystemConfigDto>.Ok(
                        result,
                        "System configuration retrieved successfully.",
                        null,
                        traceId));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<SystemConfigDto>.Unauthorized(
                        ex.Message,
                        traceId));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<SystemConfigDto>.Forbidden(
                        ex.Message,
                        traceId));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<SystemConfigDto>.NotFound(
                        ex.Message,
                        traceId));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<SystemConfigDto>.ServerError(
                        "An unexpected error occurred.",
                        traceId));
            }
        }

        // =====================================================================
        // PUT /api/SystemConfigApi
        // =====================================================================

        [HttpPut]
        [ProducesResponseType(
            typeof(ApiResponse<SystemConfigDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<SystemConfigDto>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<SystemConfigDto>),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<SystemConfigDto>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<SystemConfigDto>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<SystemConfigDto>),
            StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(
            typeof(ApiResponse<SystemConfigDto>),
            StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<SystemConfigDto>>> Update(
            [FromBody] SystemConfigDto dto)
        {
            var traceId = HttpContext.TraceIdentifier;

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
                    ApiResponse<SystemConfigDto>.Validation(
                        errors,
                        "Validation failed",
                        traceId));
            }

            try
            {
                var result =
                    await _systemConfig.UpdateAsync(dto);

                return Ok(
                    ApiResponse<SystemConfigDto>.Ok(
                        result,
                        "System configuration updated successfully.",
                        null,
                        traceId));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<SystemConfigDto>.Unauthorized(
                        ex.Message,
                        traceId));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<SystemConfigDto>.Forbidden(
                        ex.Message,
                        traceId));
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(
                    ApiResponse<SystemConfigDto>.BadRequest(
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
                    ApiResponse<SystemConfigDto>.NotFound(
                        ex.Message,
                        traceId));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(
                    ApiResponse<SystemConfigDto>.UnprocessableEntity(
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
                    ApiResponse<SystemConfigDto>.ServerError(
                        "An unexpected error occurred.",
                        traceId));
            }
        }
    }
}