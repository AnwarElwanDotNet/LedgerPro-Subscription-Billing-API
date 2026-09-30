
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
    public class PlanApiController : ControllerBase
    {
        private readonly IPlan _planService;

        public PlanApiController(IPlan planService)
        {
            _planService = planService;
        }

        // =========================================================
        // GET ACTIVE PLANS
        // =========================================================

        [HttpGet("active")]
        [Authorize(Roles = "Platform Admin,Customer Admin")]
        [ProducesResponseType(
            typeof(ApiResponse<List<PlanDto>>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetActivePlans()
        {
            try
            {
                var plans =
                    await _planService.GetActivePlansAsync();

                return Ok(
                    ApiResponse<List<PlanDto>>.Ok(
                        plans,
                        "Active plans retrieved successfully."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<object>.Unauthorized(
                        ex.Message));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Forbidden(
                        ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.ServerError());
            }
        }

        // =========================================================
        // ACTIVATE PLAN
        // =========================================================

        [HttpPost("{id:guid}/activate")]
        [Authorize(Roles = "Platform Admin")]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status200OK)]
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
            StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Activate(Guid id)
        {
            try
            {
                await _planService.ActivateAsync(id);

                return Ok(
                    ApiResponse<object>.Ok(
                        null,
                        "Plan activated successfully."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<object>.Unauthorized(
                        ex.Message));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Forbidden(
                        ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<object>.NotFound(
                        ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.ServerError());
            }
        }

        // =========================================================
        // DEACTIVATE PLAN
        // =========================================================

        [HttpPost("{id:guid}/deactivate")]
        [Authorize(Roles = "Platform Admin")]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status200OK)]
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
            StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            try
            {
                await _planService.DeactivateAsync(id);

                return Ok(
                    ApiResponse<object>.Ok(
                        null,
                        "Plan deactivated successfully."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<object>.Unauthorized(
                        ex.Message));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Forbidden(
                        ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<object>.NotFound(
                        ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.ServerError());
            }
        }
    }
}

