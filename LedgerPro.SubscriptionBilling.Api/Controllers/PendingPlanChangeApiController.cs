
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
    public class PendingPlanChangeApiController : ControllerBase
    {
        private readonly IPendingPlanChange _pendingPlanChangeService;

        public PendingPlanChangeApiController(
            IPendingPlanChange pendingPlanChangeService)
        {
            _pendingPlanChangeService = pendingPlanChangeService;
        }

        // =========================================================
        // CREATE PENDING PLAN CHANGE
        // =========================================================

        [HttpPost]
        [ProducesResponseType(
            typeof(ApiResponse<Guid>),
            StatusCodes.Status201Created)]
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
            StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create(
            [FromBody] PendingPlanChangeDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest(
                        ApiResponse<object>.BadRequest(
                            "Request body is required.",
                            new[]
                            {
                                new ApiError(
                                    ApiErrorCodes.Required,
                                    "Request body is required.")
                            }));
                }

                var id =
                    await _pendingPlanChangeService
                        .AddAsync(dto);

                return StatusCode(
                    StatusCodes.Status201Created,
                    ApiResponse<Guid>.Created(
                        id,
                        "Pending plan change created successfully."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<object>.Unauthorized(ex.Message));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Forbidden(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<object>.NotFound(ex.Message));
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(
                    ApiResponse<object>.BadRequest(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    ApiResponse<object>.BadRequest(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(
                    ApiResponse<object>.Conflict(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.ServerError());
            }
        }

        // =========================================================
        // GET PENDING PLAN CHANGE BY SUBSCRIPTION
        // =========================================================

        [HttpGet("subscription/{subscriptionId:guid}")]
        [ProducesResponseType(
            typeof(ApiResponse<PendingPlanChangeDto>),
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
            StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBySubscriptionId(
            Guid subscriptionId)
        {
            try
            {
                var result =
                    await _pendingPlanChangeService
                        .GetBySubscriptionIdAsync(subscriptionId);

                if (result == null)
                {
                    return NotFound(
                        ApiResponse<object>.NotFound(
                            "No pending plan change was found for this subscription."));
                }

                return Ok(
                    ApiResponse<PendingPlanChangeDto>.Ok(
                        result,
                        "Pending plan change retrieved successfully."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<object>.Unauthorized(ex.Message));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Forbidden(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<object>.NotFound(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    ApiResponse<object>.BadRequest(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.ServerError());
            }
        }

        // =========================================================
        // APPLY PENDING PLAN CHANGE
        // =========================================================

        [HttpPost("subscription/{subscriptionId:guid}/apply")]
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
            StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Apply(
            Guid subscriptionId)
        {
            try
            {
                await _pendingPlanChangeService
                    .ApplyAsync(subscriptionId);

                return Ok(
                    ApiResponse<object>.Ok(
                        null,
                        "Pending plan change applied successfully."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<object>.Unauthorized(ex.Message));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Forbidden(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<object>.NotFound(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    ApiResponse<object>.BadRequest(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(
                    ApiResponse<object>.Conflict(ex.Message));
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
