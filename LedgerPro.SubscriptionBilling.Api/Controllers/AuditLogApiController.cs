using BL.Contract;
using BL.Dtos;
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
    [Authorize(Roles = "Platform Admin")]
    public class AuditLogApiController : ControllerBase
    {
        private readonly IAuditLog _auditLogService;

        public AuditLogApiController(IAuditLog auditLogService)
        {
            _auditLogService = auditLogService;
        }


        // =========================================================
        // GET: api/AuditLogApi
        // =========================================================

        [HttpGet]
        [ProducesResponseType(
            typeof(ApiResponse<List<AuditLogDto>>),
            StatusCodes.Status200OK)]

        [ProducesResponseType(
            typeof(ApiResponse<List<AuditLogDto>>),
            StatusCodes.Status401Unauthorized)]

        [ProducesResponseType(
            typeof(ApiResponse<List<AuditLogDto>>),
            StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(
    typeof(ApiResponse<List<AuditLogDto>>),
    StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _auditLogService.GetAllAsync();

                return Ok(
                    ApiResponse<List<AuditLogDto>>.Ok(
                        result,
                        "Audit logs retrieved successfully",
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<List<AuditLogDto>>.ServerError(
                        "An unexpected error occurred while retrieving audit logs.",
                        HttpContext.TraceIdentifier));
            }
        }


        // =========================================================
        // GET: api/AuditLogApi/{id}
        // =========================================================

        [HttpGet("{id:guid}")]
        [ProducesResponseType(
            typeof(ApiResponse<AuditLogDto>),
            StatusCodes.Status200OK)]

        [ProducesResponseType(
            typeof(ApiResponse<AuditLogDto>),
            StatusCodes.Status400BadRequest)]

        [ProducesResponseType(
            typeof(ApiResponse<AuditLogDto>),
            StatusCodes.Status401Unauthorized)]

        [ProducesResponseType(
            typeof(ApiResponse<AuditLogDto>),
            StatusCodes.Status404NotFound)]

        [ProducesResponseType(
            typeof(ApiResponse<AuditLogDto>),
            StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(
    typeof(ApiResponse<AuditLogDto>),
    StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(
                    ApiResponse<AuditLogDto>.BadRequest(
                        "Audit log ID is required.",
                        [
                            new ApiError(
                                ApiErrorCodes.InvalidValue,
                                "Audit log ID cannot be empty.",
                                "id")
                        ],
                        HttpContext.TraceIdentifier));
            }

            try
            {
                var result = await _auditLogService.GetByIdAsync(id);

                if (result == null)
                {
                    return NotFound(
                        ApiResponse<AuditLogDto>.NotFound(
                            "Audit log was not found.",
                            HttpContext.TraceIdentifier));
                }

                return Ok(
                    ApiResponse<AuditLogDto>.Ok(
                        result,
                        "Audit log retrieved successfully",
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<AuditLogDto>.ServerError(
                        "An unexpected error occurred while retrieving the audit log.",
                        HttpContext.TraceIdentifier));
            }
        }


        // =========================================================
        // POST: api/AuditLogApi
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
            StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(
    typeof(ApiResponse<Guid>),
    StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Add([FromBody] AuditLogDto dto)
        {
            try
            {
                var id = await _auditLogService.AddAsync(dto);

                var response = ApiResponse<Guid>.Created(
                    id,
                    "Audit log created successfully",
                    HttpContext.TraceIdentifier);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id },
                    response);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(
                    ApiResponse<Guid>.BadRequest(
                        "Invalid audit log request.",
                        [
                            new ApiError(
                                ApiErrorCodes.BadRequest,
                                ex.Message)
                        ],
                        HttpContext.TraceIdentifier));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    ApiResponse<Guid>.BadRequest(
                        "Invalid audit log request.",
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
                    ApiResponse<Guid>.ServerError(
                        "An unexpected error occurred while creating the audit log.",
                        HttpContext.TraceIdentifier));
            }
        }
    }
}

