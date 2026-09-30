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
    public class PlanPricingApiController : ControllerBase
    {
        private readonly IPlanPricing _planPricingService;

        public PlanPricingApiController(IPlanPricing planPricingService)
        {
            _planPricingService = planPricingService;
        }

        // =========================================================
        // ADD PLAN PRICING
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
            StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddAsync(
            [FromBody] PlanPricingDto dto)
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
                                string.IsNullOrWhiteSpace(error.ErrorMessage)
                                    ? "Invalid value."
                                    : error.ErrorMessage,
                                x.Key)))
                    .ToList();

                return BadRequest(
                    ApiResponse<Guid>.Validation(
                        errors,
                        "Validation failed.",
                        traceId));
            }

            try
            {
                var id =
                    await _planPricingService.AddAsync(dto);

                return StatusCode(
                    StatusCodes.Status201Created,
                    ApiResponse<Guid>.Created(
                        id,
                        "Plan pricing created successfully.",
                        traceId));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<Guid>.Unauthorized(
                        ex.Message,
                        traceId));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<Guid>.Forbidden(
                        ex.Message,
                        traceId));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<Guid>.NotFound(
                        ex.Message,
                        traceId));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<Guid>.ServerError(
                        "An unexpected error occurred while creating plan pricing.",
                        traceId));
            }
        }

        // =========================================================
        // GET PLAN PRICING HISTORY BY PLAN ID
        // =========================================================

        [HttpGet("plan/{planId:guid}")]
        [ProducesResponseType(
            typeof(ApiResponse<List<PlanPricingDto>>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<List<PlanPricingDto>>),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<List<PlanPricingDto>>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<List<PlanPricingDto>>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<List<PlanPricingDto>>),
            StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByPlanIdAsync(
            Guid planId)
        {
            var traceId = HttpContext.TraceIdentifier;

            try
            {
                var result =
                    await _planPricingService
                        .GetByPlanIdAsync(planId);

                return Ok(
                    ApiResponse<List<PlanPricingDto>>.Ok(
                        result,
                        "Plan pricing history retrieved successfully.",
                        traceId: traceId));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<List<PlanPricingDto>>.Unauthorized(
                        ex.Message,
                        traceId));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<List<PlanPricingDto>>.Forbidden(
                        ex.Message,
                        traceId));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<List<PlanPricingDto>>.NotFound(
                        ex.Message,
                        traceId));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<List<PlanPricingDto>>.ServerError(
                        "An unexpected error occurred while retrieving plan pricing.",
                        traceId));
            }
        }
    }
}