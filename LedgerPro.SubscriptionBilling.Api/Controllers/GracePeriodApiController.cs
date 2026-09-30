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
    [Authorize]
    public class GracePeriodApiController : ControllerBase
    {
        private readonly IGracePeriod _gracePeriodService;

        public GracePeriodApiController(
            IGracePeriod gracePeriodService)
        {
            _gracePeriodService = gracePeriodService;
        }


        // =========================================================
        // GET BY ID
        // =========================================================

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<GracePeriodDto>>>
            GetById(Guid id)
        {
            try
            {
                var result =
                    await _gracePeriodService.GetByIdAsync(id);

                return Ok(
                    ApiResponse<GracePeriodDto>.Ok(
                        result,
                        "Grace period retrieved successfully."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<GracePeriodDto>.Unauthorized(
                        ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<GracePeriodDto>.NotFound(
                        ex.Message));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<GracePeriodDto>.Forbidden(
                        ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    ApiResponse<GracePeriodDto>.BadRequest(
                        ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<GracePeriodDto>.ServerError());
            }
        }


        // =========================================================
        // GET BY SUBSCRIPTION ID
        // =========================================================

        [HttpGet("subscription/{subscriptionId:guid}")]
        public async Task<
            ActionResult<ApiResponse<IEnumerable<GracePeriodDto>>>>
            GetBySubscriptionId(Guid subscriptionId)
        {
            try
            {
                var result =
                    await _gracePeriodService
                        .GetBySubscriptionIdAsync(subscriptionId);

                return Ok(
                    ApiResponse<IEnumerable<GracePeriodDto>>.Ok(
                        result,
                        "Grace periods retrieved successfully."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(
                    ApiResponse<IEnumerable<GracePeriodDto>>.Unauthorized(
                        ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(
                    ApiResponse<IEnumerable<GracePeriodDto>>.NotFound(
                        ex.Message));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<IEnumerable<GracePeriodDto>>.Forbidden(
                        ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(
                    ApiResponse<IEnumerable<GracePeriodDto>>.BadRequest(
                        ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<IEnumerable<GracePeriodDto>>.ServerError());
            }
        }
    }
}