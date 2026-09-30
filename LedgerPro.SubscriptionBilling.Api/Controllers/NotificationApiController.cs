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
    public class NotificationApiController : ControllerBase
    {
        private readonly INotification _NotificationService;

        public NotificationApiController(
            INotification notificationService)
        {
            _NotificationService = notificationService;
        }

        // =========================================================
        // GET BY ID
        // =========================================================

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<NotificationDto>>> GetById(
            Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest(
                        ApiResponse<NotificationDto>.BadRequest(
                            "Notification ID is required.",
                            new[]
                            {
                                new ApiError(
                                    ApiErrorCodes.Required,
                                    "Notification ID is required.",
                                    "id")
                            }));
                }

                var notification =
                    await _NotificationService.GetByIdAsync(id);

                if (notification == null)
                {
                    return NotFound(
                        ApiResponse<NotificationDto>.NotFound(
                            "Notification not found."));
                }

                return Ok(
                    ApiResponse<NotificationDto>.Ok(
                        notification,
                        "Notification retrieved successfully."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<NotificationDto>.Unauthorized(
                        ex.Message));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    403,
                    ApiResponse<NotificationDto>.Forbidden(
                        ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(
                    500,
                    ApiResponse<NotificationDto>.ServerError());
            }
        }

        // =========================================================
        // GET BY ORGANIZATION
        // =========================================================

        [HttpGet("organization/{organizationId:guid}")]
        public async Task<ActionResult<ApiResponse<List<NotificationDto>>>>
            GetByOrganizationId(Guid organizationId)
        {
            try
            {
                if (organizationId == Guid.Empty)
                {
                    return BadRequest(
                        ApiResponse<List<NotificationDto>>.BadRequest(
                            "Organization ID is required.",
                            new[]
                            {
                                new ApiError(
                                    ApiErrorCodes.Required,
                                    "Organization ID is required.",
                                    "organizationId")
                            }));
                }

                var notifications =
                    await _NotificationService
                        .GetByOrganizationIdAsync(organizationId);

                return Ok(
                    ApiResponse<List<NotificationDto>>.Ok(
                        notifications,
                        "Notifications retrieved successfully."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<List<NotificationDto>>.Unauthorized(
                        ex.Message));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    403,
                    ApiResponse<List<NotificationDto>>.Forbidden(
                        ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(
                    500,
                    ApiResponse<List<NotificationDto>>.ServerError());
            }
        }

        // =========================================================
        // CREATE
        // =========================================================

        [Authorize(Roles = "Platform Admin,Customer Admin")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<NotificationDto>>> Add(
            [FromBody] NotificationDto dto)
        {
            try
            {
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
                                    x.Key)))
                        .ToList();

                    return BadRequest(
                        ApiResponse<NotificationDto>.Validation(
                            errors));
                }

                var notification =
                    await _NotificationService.AddAsync(dto);

                return Created(
                    string.Empty,
                    ApiResponse<NotificationDto>.Created(
                        notification,
                        "Notification created successfully."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<NotificationDto>.Unauthorized(
                        ex.Message));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    403,
                    ApiResponse<NotificationDto>.Forbidden(
                        ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<NotificationDto>.NotFound(
                        ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    ApiResponse<NotificationDto>.BadRequest(
                        ex.Message,
                        new[]
                        {
                            new ApiError(
                                ApiErrorCodes.InvalidValue,
                                ex.Message)
                        }));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(
                    ApiResponse<NotificationDto>.BadRequest(
                        ex.Message,
                        new[]
                        {
                            new ApiError(
                                ApiErrorCodes.InvalidValue,
                                ex.Message)
                        }));
            }
            catch (Exception)
            {
                return StatusCode(
                    500,
                    ApiResponse<NotificationDto>.ServerError());
            }
        }
    }
}