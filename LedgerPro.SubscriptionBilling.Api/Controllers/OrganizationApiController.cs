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
    [Authorize(Roles = "Platform Admin")]
    public class OrganizationApiController : ControllerBase
    {
        private readonly IOrganization _organizationService;

        public OrganizationApiController(
            IOrganization organizationService)
        {
            _organizationService = organizationService;
        }

        // =========================================================
        // GET ALL
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var organizations =
                    await _organizationService.GetAllAsync();

                var response =
                    ApiResponse<List<OrganizationDto>>.Ok(
                        organizations,
                        "Organizations retrieved successfully.",
                        traceId: HttpContext.TraceIdentifier);

                return StatusCode(
                    response.StatusCode,
                    response);
            }
            catch (UnauthorizedAccessException ex)
            {
                var response =
      ApiResponse<List<OrganizationDto>>.Unauthorized(
          ex.Message,
          HttpContext.TraceIdentifier);

                return StatusCode(
                    response.StatusCode,
                    response);
            }
            catch (ForbiddenAccessException ex)
            {
                var response =
    ApiResponse<List<OrganizationDto>>.Forbidden(
        ex.Message,
        HttpContext.TraceIdentifier);

                return StatusCode(
                    response.StatusCode,
                    response);
            }
            catch (Exception)
            {
                var response =
                    ApiResponse<List<OrganizationDto>>.ServerError(
                        traceId: HttpContext.TraceIdentifier);

                return StatusCode(
                    response.StatusCode,
                    response);
            }
        }

        // =========================================================
        // GET BY ID
        // =========================================================

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    var badRequestResponse =
                        ApiResponse<OrganizationDto>.BadRequest(
                            "Organization ID is required.",
                            traceId: HttpContext.TraceIdentifier);

                    return StatusCode(
                        badRequestResponse.StatusCode,
                        badRequestResponse);
                }

                var organization =
                    await _organizationService.GetByIdAsync(id);

                if (organization == null)
                {
                    var notFoundResponse =
                        ApiResponse<OrganizationDto>.NotFound(
                            "Organization not found.",
                            HttpContext.TraceIdentifier);

                    return StatusCode(
                        notFoundResponse.StatusCode,
                        notFoundResponse);
                }

                var response =
                    ApiResponse<OrganizationDto>.Ok(
                        organization,
                        "Organization retrieved successfully.",
                        traceId: HttpContext.TraceIdentifier);

                return StatusCode(
                    response.StatusCode,
                    response);
            }
            catch (UnauthorizedAccessException ex)
            {
                var response =
                   ApiResponse<OrganizationDto>.Unauthorized(
    ex.Message,
    HttpContext.TraceIdentifier);

                return StatusCode(
                    response.StatusCode,
                    response);
            }
            catch (ForbiddenAccessException ex)
            {
                var response =
                   ApiResponse<OrganizationDto>.Forbidden(
    ex.Message,
    HttpContext.TraceIdentifier);

                return StatusCode(
                    response.StatusCode,
                    response);
            }
            catch (Exception)
            {
                var response =
                    ApiResponse<OrganizationDto>.ServerError(
                        traceId: HttpContext.TraceIdentifier);

                return StatusCode(
                    response.StatusCode,
                    response);
            }
        }

        // =========================================================
        // CREATE
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] OrganizationDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x =>
                            x.Value != null &&
                            x.Value.Errors.Count > 0)
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

                    var validationResponse =
                        ApiResponse<Guid>.Validation(
                            errors,
                            "Validation failed.",
                            HttpContext.TraceIdentifier);

                    return StatusCode(
                        validationResponse.StatusCode,
                        validationResponse);
                }

                dto.Id = Guid.Empty;

                var id =
                    await _organizationService.SaveAsync(dto);

                var response =
                    ApiResponse<Guid>.Created(
                        id,
                        "Organization created successfully.",
                        HttpContext.TraceIdentifier);

                return StatusCode(
                    response.StatusCode,
                    response);
            }
            catch (UnauthorizedAccessException ex)
            {
                var response =
                   ApiResponse<Guid>.Unauthorized(
    ex.Message,
    HttpContext.TraceIdentifier);

                return StatusCode(
                    response.StatusCode,
                    response);
            }
            catch (ForbiddenAccessException ex)
            {
                var response =
                   ApiResponse<Guid>.Forbidden(
    ex.Message,
    HttpContext.TraceIdentifier);

                return StatusCode(
                    response.StatusCode,
                    response);
            }
            catch (ArgumentNullException ex)
            {
                var response =
                    ApiResponse<Guid>.BadRequest(
                        ex.Message,
                        traceId: HttpContext.TraceIdentifier);

                return StatusCode(
                    response.StatusCode,
                    response);
            }
            catch (ArgumentException ex)
            {
                var response =
                    ApiResponse<Guid>.Validation(
                        new[]
                        {
                            new ApiError(
                                ApiErrorCodes.ValidationError,
                                ex.Message,
                                nameof(
                                    OrganizationDto.OrganizationName))
                        },
                        "Validation failed.",
                        HttpContext.TraceIdentifier);

                return StatusCode(
                    response.StatusCode,
                    response);
            }
            catch (Exception)
            {
                var response =
                    ApiResponse<Guid>.ServerError(
                        traceId: HttpContext.TraceIdentifier);

                return StatusCode(
                    response.StatusCode,
                    response);
            }
        }

        // =========================================================
        // UPDATE
        // =========================================================

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] OrganizationDto dto)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    var badRequestResponse =
                        ApiResponse<Guid>.BadRequest(
                            "Organization ID is required.",
                            traceId: HttpContext.TraceIdentifier);

                    return StatusCode(
                        badRequestResponse.StatusCode,
                        badRequestResponse);
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x =>
                            x.Value != null &&
                            x.Value.Errors.Count > 0)
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

                    var validationResponse =
                        ApiResponse<Guid>.Validation(
                            errors,
                            "Validation failed.",
                            HttpContext.TraceIdentifier);

                    return StatusCode(
                        validationResponse.StatusCode,
                        validationResponse);
                }

                dto.Id = id;

                var organization =
                    await _organizationService.GetByIdAsync(id);

                if (organization == null)
                {
                    var notFoundResponse =
                        ApiResponse<Guid>.NotFound(
                            "Organization not found.",
                            HttpContext.TraceIdentifier);

                    return StatusCode(
                        notFoundResponse.StatusCode,
                        notFoundResponse);
                }

                var updatedId =
                    await _organizationService.SaveAsync(dto);

                var response =
                    ApiResponse<Guid>.Ok(
                        updatedId,
                        "Organization updated successfully.",
                        traceId: HttpContext.TraceIdentifier);

                return StatusCode(
                    response.StatusCode,
                    response);
            }
            catch (UnauthorizedAccessException ex)
            {
                var response =
                  ApiResponse<Guid>.Unauthorized(
    ex.Message,
    HttpContext.TraceIdentifier);

                return StatusCode(
                    response.StatusCode,
                    response);
            }
            catch (ForbiddenAccessException ex)
            {
                var response =
                  ApiResponse<Guid>.Forbidden(
    ex.Message,
    HttpContext.TraceIdentifier);

                return StatusCode(
                    response.StatusCode,
                    response);
            }
            catch (ArgumentNullException ex)
            {
                var response =
                    ApiResponse<Guid>.BadRequest(
                        ex.Message,
                        traceId: HttpContext.TraceIdentifier);

                return StatusCode(
                    response.StatusCode,
                    response);
            }
            catch (ArgumentException ex)
            {
                var response =
                    ApiResponse<Guid>.Validation(
                        new[]
                        {
                            new ApiError(
                                ApiErrorCodes.ValidationError,
                                ex.Message,
                                nameof(
                                    OrganizationDto.OrganizationName))
                        },
                        "Validation failed.",
                        HttpContext.TraceIdentifier);

                return StatusCode(
                    response.StatusCode,
                    response);
            }
            catch (KeyNotFoundException ex)
            {
                var response =
                    ApiResponse<Guid>.NotFound(
                        ex.Message,
                        HttpContext.TraceIdentifier);

                return StatusCode(
                    response.StatusCode,
                    response);
            }
            catch (Exception)
            {
                var response =
                    ApiResponse<Guid>.ServerError(
                        traceId: HttpContext.TraceIdentifier);

                return StatusCode(
                    response.StatusCode,
                    response);
            }
        }
    }
}