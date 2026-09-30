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
    [Authorize]
    public class UserSeatApiController : ControllerBase
    {
        private readonly IUserSeat _userSeatService;

        public UserSeatApiController(
            IUserSeat userSeatService)
        {
            _userSeatService = userSeatService;
        }


        // =========================================================
        // GET ALL USER SEATS
        // =========================================================

        [HttpGet]
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<UserSeatDto>>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<UserSeatDto>>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<UserSeatDto>>),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<UserSeatDto>>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<UserSeatDto>>),
            StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PagedResult<UserSeatDto>>>>
            GetAllAsync(
                [FromQuery] int pageNumber,
                [FromQuery] int pageSize)
        {
            var traceId = HttpContext.TraceIdentifier;

            var validationErrors =
                new List<ApiError>();

            if (pageNumber <= 0)
            {
                validationErrors.Add(
                    new ApiError(
                        ApiErrorCodes.InvalidValue,
                        "Page number must be greater than zero.",
                        "pageNumber"));
            }

            if (pageSize <= 0)
            {
                validationErrors.Add(
                    new ApiError(
                        ApiErrorCodes.InvalidValue,
                        "Page size must be greater than zero.",
                        "pageSize"));
            }

            if (validationErrors.Count > 0)
            {
                return BadRequest(
                    ApiResponse<PagedResult<UserSeatDto>>.Validation(
                        validationErrors,
                        traceId: traceId));
            }

            try
            {
                var result =
                    await _userSeatService.GetAllAsync(
                        pageNumber,
                        pageSize);

                return Ok(
                    ApiResponse<PagedResult<UserSeatDto>>.Ok(
                        result,
                        "User seats retrieved successfully.",
                        traceId: traceId));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<PagedResult<UserSeatDto>>.Unauthorized(
                        ex.Message,
                        traceId));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<PagedResult<UserSeatDto>>.Forbidden(
                        ex.Message,
                        traceId));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    ApiResponse<PagedResult<UserSeatDto>>.BadRequest(
                        ex.Message,
                        new List<ApiError>
                        {
                            new ApiError(
                                ApiErrorCodes.InvalidValue,
                                ex.Message)
                        },
                        traceId));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<PagedResult<UserSeatDto>>.ServerError(
                        traceId: traceId));
            }
        }


        // =========================================================
        // GET USER SEAT BY ID
        // =========================================================

        [HttpGet("{id:guid}")]
        [ProducesResponseType(
            typeof(ApiResponse<UserSeatDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<UserSeatDto>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<UserSeatDto>),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<UserSeatDto>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<UserSeatDto>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<UserSeatDto>),
            StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserSeatDto>>>
            GetByIdAsync(Guid id)
        {
            var traceId = HttpContext.TraceIdentifier;

            if (id == Guid.Empty)
            {
                return BadRequest(
                    ApiResponse<UserSeatDto>.BadRequest(
                        "User seat ID is required.",
                        new List<ApiError>
                        {
                            new ApiError(
                                ApiErrorCodes.Required,
                                "User seat ID is required.",
                                "id")
                        },
                        traceId));
            }

            try
            {
                var result =
                    await _userSeatService.GetByIdAsync(id);

                if (result == null)
                {
                    return NotFound(
                        ApiResponse<UserSeatDto>.NotFound(
                            "User seat could not be found.",
                            traceId));
                }

                return Ok(
                    ApiResponse<UserSeatDto>.Ok(
                        result,
                        "User seat retrieved successfully.",
                        traceId: traceId));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<UserSeatDto>.Unauthorized(
                        ex.Message,
                        traceId));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<UserSeatDto>.Forbidden(
                        ex.Message,
                        traceId));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    ApiResponse<UserSeatDto>.BadRequest(
                        ex.Message,
                        new List<ApiError>
                        {
                            new ApiError(
                                ApiErrorCodes.InvalidValue,
                                ex.Message,
                                "id")
                        },
                        traceId));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<UserSeatDto>.ServerError(
                        traceId: traceId));
            }
        }


        // =========================================================
        // ADD USER SEAT
        // =========================================================

        [HttpPost]
        [ProducesResponseType(
            typeof(ApiResponse<UserSeatDto>),
            StatusCodes.Status201Created)]
        [ProducesResponseType(
            typeof(ApiResponse<UserSeatDto>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<UserSeatDto>),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<UserSeatDto>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<UserSeatDto>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<UserSeatDto>),
            StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(
            typeof(ApiResponse<UserSeatDto>),
            StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserSeatDto>>>
            AddUserSeatAsync(
                [FromQuery] Guid organizationId,
                [FromQuery] Guid subscriptionId,
                [FromQuery] Guid userId)
        {
            var traceId = HttpContext.TraceIdentifier;

            var validationErrors =
                new List<ApiError>();

            if (organizationId == Guid.Empty)
            {
                validationErrors.Add(
                    new ApiError(
                        ApiErrorCodes.Required,
                        "Organization ID is required.",
                        "organizationId"));
            }

            if (subscriptionId == Guid.Empty)
            {
                validationErrors.Add(
                    new ApiError(
                        ApiErrorCodes.Required,
                        "Subscription ID is required.",
                        "subscriptionId"));
            }

            if (userId == Guid.Empty)
            {
                validationErrors.Add(
                    new ApiError(
                        ApiErrorCodes.Required,
                        "User ID is required.",
                        "userId"));
            }

            if (validationErrors.Count > 0)
            {
                return BadRequest(
                    ApiResponse<UserSeatDto>.Validation(
                        validationErrors,
                        traceId: traceId));
            }

            try
            {
                var result =
                    await _userSeatService.AddUserSeatAsync(
                        organizationId,
                        subscriptionId,
                        userId);

                return StatusCode(
                    StatusCodes.Status201Created,
                    ApiResponse<UserSeatDto>.Created(
                        result,
                        "User seat added successfully.",
                        traceId));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<UserSeatDto>.Unauthorized(
                        ex.Message,
                        traceId));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<UserSeatDto>.Forbidden(
                        ex.Message,
                        traceId));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<UserSeatDto>.NotFound(
                        ex.Message,
                        traceId));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    ApiResponse<UserSeatDto>.BadRequest(
                        ex.Message,
                        new List<ApiError>
                        {
                            new ApiError(
                                ApiErrorCodes.InvalidValue,
                                ex.Message)
                        },
                        traceId));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(
                    ApiResponse<UserSeatDto>.UnprocessableEntity(
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
                    ApiResponse<UserSeatDto>.ServerError(
                        traceId: traceId));
            }
        }


        // =========================================================
        // REMOVE USER SEAT
        // =========================================================

        [HttpDelete("{userSeatId:guid}")]
        [ProducesResponseType(
            typeof(ApiResponse<UserSeatDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<UserSeatDto>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<UserSeatDto>),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<UserSeatDto>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<UserSeatDto>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<UserSeatDto>),
            StatusCodes.Status409Conflict)]
        [ProducesResponseType(
            typeof(ApiResponse<UserSeatDto>),
            StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserSeatDto>>>
            RemoveUserSeatAsync(Guid userSeatId)
        {
            var traceId = HttpContext.TraceIdentifier;

            if (userSeatId == Guid.Empty)
            {
                return BadRequest(
                    ApiResponse<UserSeatDto>.BadRequest(
                        "User seat ID is required.",
                        new List<ApiError>
                        {
                            new ApiError(
                                ApiErrorCodes.Required,
                                "User seat ID is required.",
                                "userSeatId")
                        },
                        traceId));
            }

            try
            {
                var result =
                    await _userSeatService.RemoveUserSeatAsync(
                        userSeatId);

                return Ok(
                    ApiResponse<UserSeatDto>.Ok(
                        result,
                        "User seat removed successfully.",
                        traceId: traceId));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<UserSeatDto>.Unauthorized(
                        ex.Message,
                        traceId));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<UserSeatDto>.Forbidden(
                        ex.Message,
                        traceId));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<UserSeatDto>.NotFound(
                        ex.Message,
                        traceId));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    ApiResponse<UserSeatDto>.BadRequest(
                        ex.Message,
                        new List<ApiError>
                        {
                            new ApiError(
                                ApiErrorCodes.InvalidValue,
                                ex.Message,
                                "userSeatId")
                        },
                        traceId));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(
                    ApiResponse<UserSeatDto>.Conflict(
                        ex.Message,
                        new List<ApiError>
                        {
                            new ApiError(
                                ApiErrorCodes.OperationNotAllowed,
                                ex.Message)
                        },
                        traceId));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<UserSeatDto>.ServerError(
                        traceId: traceId));
            }
        }
    }
}