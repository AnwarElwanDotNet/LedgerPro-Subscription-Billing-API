
using BL.Contract;
using BL.Dtos;
using LedgerPro.SubscriptionBilling.Api.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace LedgerPro.SubscriptionBilling.Api.Controllers
{
    [Route("api/subscriptions")]
    [ApiController]
    [Authorize]
    public class SubscriptionApiController : ControllerBase
    {
        private readonly ISubscription _subscriptionService;

        public SubscriptionApiController(ISubscription subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

       
        [HttpPost]
        [Authorize(Roles = "Customer Admin")]
        public async Task<IActionResult> CreateSubscription(
            [FromBody] SubscriptionDto dto)
        {
            if (dto == null)
            {
                return BuildResponse(
                    ApiResponse<object>.BadRequest(
                        "Request body is required.",
                        traceId: HttpContext.TraceIdentifier));
            }

            try
            {
                await _subscriptionService.CreateSubscriptionAsync(dto);

                return BuildResponse(
                    ApiResponse<object>.Created(
                        data: null,
                        message: "Subscription and initial invoice created successfully.",
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                return BuildResponse(MapException<object>(ex));
            }
        }

        [HttpGet("{subscriptionId:guid}")]
        [Authorize(Roles = "Platform Admin,Customer Admin")]
        public async Task<IActionResult> GetSubscriptionById(
            [FromRoute] Guid subscriptionId)
        {
            if (subscriptionId == Guid.Empty)
            {
                return BuildResponse(
                    ApiResponse<SubscriptionDto>.BadRequest(
                        "subscriptionId is required.",
                        traceId: HttpContext.TraceIdentifier));
            }

            try
            {
                var result =
                    await _subscriptionService.GetSubscriptionByIdAsync(
                        subscriptionId);

                return BuildResponse(
                    ApiResponse<SubscriptionDto>.Ok(
                        data: result,
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                return BuildResponse(
                    MapException<SubscriptionDto>(ex));
            }
        }

      
        [HttpGet("organization/{organizationId:guid}")]
        [Authorize(Roles = "Platform Admin,Customer Admin")]
        public async Task<IActionResult> GetSubscriptionByOrganizationId(
            [FromRoute] Guid organizationId)
        {
            if (organizationId == Guid.Empty)
            {
                return BuildResponse(
                    ApiResponse<SubscriptionDto>.BadRequest(
                        "organizationId is required.",
                        traceId: HttpContext.TraceIdentifier));
            }

            try
            {
                var result =
                    await _subscriptionService
                        .GetSubscriptionByOrganizationIdAsync(
                            organizationId);

                return BuildResponse(
                    ApiResponse<SubscriptionDto>.Ok(
                        data: result,
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                return BuildResponse(
                    MapException<SubscriptionDto>(ex));
            }
        }

      
        [HttpPost("{subscriptionId:guid}/cancel")]
        [Authorize(Roles = "Customer Admin")]
        public async Task<IActionResult> CancelSubscription(
            [FromRoute] Guid subscriptionId,
            [FromBody] CancelSubscriptionRequest? request)
        {
            if (subscriptionId == Guid.Empty)
            {
                return BuildResponse(
                    ApiResponse<object>.BadRequest(
                        "subscriptionId is required.",
                        traceId: HttpContext.TraceIdentifier));
            }

            try
            {
                await _subscriptionService.CancelSubscriptionAsync(
                    subscriptionId,
                    request?.Reason);

                return BuildResponse(
                    ApiResponse<object>.Ok(
                        data: null,
                        message: "Subscription cancellation requested successfully.",
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                return BuildResponse(
                    MapException<object>(ex));
            }
        }

    
        [HttpPost("{subscriptionId:guid}/cancel-immediately")]
        [Authorize(Roles = "Platform Admin")]
        public async Task<IActionResult> CancelSubscriptionImmediately(
            [FromRoute] Guid subscriptionId,
            [FromBody] CancelSubscriptionRequest? request)
        {
            if (subscriptionId == Guid.Empty)
            {
                return BuildResponse(
                    ApiResponse<object>.BadRequest(
                        "subscriptionId is required.",
                        traceId: HttpContext.TraceIdentifier));
            }

            try
            {
                await _subscriptionService
                    .CancelSubscriptionImmediatelyAsync(
                        subscriptionId,
                        request?.Reason);

                return BuildResponse(
                    ApiResponse<object>.Ok(
                        data: null,
                        message: "Subscription cancelled immediately.",
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                return BuildResponse(
                    MapException<object>(ex));
            }
        }

      
        [HttpPost("{subscriptionId:guid}/upgrade")]
        [Authorize(Roles = "Customer Admin")]
        public async Task<IActionResult> UpgradeSubscription(
            [FromRoute] Guid subscriptionId,
            [FromBody] PlanChangeRequest request)
        {
            if (subscriptionId == Guid.Empty)
            {
                return BuildResponse(
                    ApiResponse<object>.BadRequest(
                        "subscriptionId is required.",
                        traceId: HttpContext.TraceIdentifier));
            }

            if (request == null ||
                request.TargetPlanId == Guid.Empty)
            {
                return BuildResponse(
                    ApiResponse<object>.BadRequest(
                        "targetPlanId is required.",
                        traceId: HttpContext.TraceIdentifier));
            }

            try
            {
                await _subscriptionService.UpgradeSubscriptionAsync(
                    subscriptionId,
                    request.TargetPlanId);

                return BuildResponse(
                    ApiResponse<object>.Created(
                        data: null,
                        message: "Proration invoice created for the requested upgrade.",
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                return BuildResponse(
                    MapException<object>(ex));
            }
        }

    
        [HttpPost("{subscriptionId:guid}/downgrade")]
        [Authorize(Roles = "Customer Admin")]
        public async Task<IActionResult> RequestDowngrade(
            [FromRoute] Guid subscriptionId,
            [FromBody] PlanChangeRequest request)
        {
            if (subscriptionId == Guid.Empty)
            {
                return BuildResponse(
                    ApiResponse<object>.BadRequest(
                        "subscriptionId is required.",
                        traceId: HttpContext.TraceIdentifier));
            }

            if (request == null ||
                request.TargetPlanId == Guid.Empty)
            {
                return BuildResponse(
                    ApiResponse<object>.BadRequest(
                        "targetPlanId is required.",
                        traceId: HttpContext.TraceIdentifier));
            }

            try
            {
                await _subscriptionService.RequestDowngradeAsync(
                    subscriptionId,
                    request.TargetPlanId);

                return BuildResponse(
                    ApiResponse<object>.Created(
                        data: null,
                        message: "Downgrade scheduled for the start of the next billing period.",
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                return BuildResponse(
                    MapException<object>(ex));
            }
        }

       
        [HttpGet("organization/{organizationId:guid}/entitlement")]
        [Authorize(Roles = "Platform Admin,Customer Admin,Customer User")]
        public async Task<IActionResult> CheckEntitlement(
            [FromRoute] Guid organizationId)
        {
            if (organizationId == Guid.Empty)
            {
                return BuildResponse(
                    ApiResponse<bool>.BadRequest(
                        "organizationId is required.",
                        traceId: HttpContext.TraceIdentifier));
            }

            try
            {
                var result =
                    await _subscriptionService.CheckEntitlementAsync(
                        organizationId);

                return BuildResponse(
                    ApiResponse<bool>.Ok(
                        data: result,
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                return BuildResponse(
                    MapException<bool>(ex));
            }
        }

      
        [HttpPost("invoice/{invoiceId:guid}/activate")]
        [Authorize]
        public async Task<IActionResult> ActivateSubscription(
            [FromRoute] Guid invoiceId)
        {
            if (invoiceId == Guid.Empty)
            {
                return BuildResponse(
                    ApiResponse<object>.BadRequest(
                        "invoiceId is required.",
                        traceId: HttpContext.TraceIdentifier));
            }

            try
            {
                await _subscriptionService.ActivateSubscriptionAsync(
                    invoiceId);

                return BuildResponse(
                    ApiResponse<object>.Ok(
                        data: null,
                        message: "Subscription activated successfully.",
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                return BuildResponse(
                    MapException<object>(ex));
            }
        }

      
        [HttpPost("invoice/{invoiceId:guid}/initial-payment-failure")]
        [Authorize]
        public async Task<IActionResult> HandleInitialPaymentFailure(
            [FromRoute] Guid invoiceId)
        {
            if (invoiceId == Guid.Empty)
            {
                return BuildResponse(
                    ApiResponse<object>.BadRequest(
                        "invoiceId is required.",
                        traceId: HttpContext.TraceIdentifier));
            }

            try
            {
                await _subscriptionService
                    .HandleInitialPaymentFailureAsync(invoiceId);

                return BuildResponse(
                    ApiResponse<object>.Ok(
                        data: null,
                        message: "Initial payment failure recorded.",
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                return BuildResponse(
                    MapException<object>(ex));
            }
        }

       
        [HttpPost("invoice/{invoiceId:guid}/apply-upgrade")]
        [Authorize]
        public async Task<IActionResult> ApplySuccessfulUpgrade(
            [FromRoute] Guid invoiceId)
        {
            if (invoiceId == Guid.Empty)
            {
                return BuildResponse(
                    ApiResponse<object>.BadRequest(
                        "invoiceId is required.",
                        traceId: HttpContext.TraceIdentifier));
            }

            try
            {
                await _subscriptionService
                    .ApplySuccessfulUpgradeAsync(invoiceId);

                return BuildResponse(
                    ApiResponse<object>.Ok(
                        data: null,
                        message: "Upgrade applied successfully.",
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                return BuildResponse(
                    MapException<object>(ex));
            }
        }

    
        [HttpPost("invoice/{invoiceId:guid}/complete-renewal")]
        [Authorize]
        public async Task<IActionResult> CompleteSuccessfulRenewal(
            [FromRoute] Guid invoiceId)
        {
            if (invoiceId == Guid.Empty)
            {
                return BuildResponse(
                    ApiResponse<object>.BadRequest(
                        "invoiceId is required.",
                        traceId: HttpContext.TraceIdentifier));
            }

            try
            {
                await _subscriptionService
                    .CompleteSuccessfulRenewalAsync(invoiceId);

                return BuildResponse(
                    ApiResponse<object>.Ok(
                        data: null,
                        message: "Renewal completed successfully.",
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                return BuildResponse(
                    MapException<object>(ex));
            }
        }

       
        [HttpPost("{subscriptionId:guid}/renew")]
        [Authorize]
        public async Task<IActionResult> RenewSubscription(
            [FromRoute] Guid subscriptionId)
        {
            if (subscriptionId == Guid.Empty)
            {
                return BuildResponse(
                    ApiResponse<object>.BadRequest(
                        "subscriptionId is required.",
                        traceId: HttpContext.TraceIdentifier));
            }

            try
            {
                await _subscriptionService
                    .RenewSubscriptionAsync(subscriptionId);

                return BuildResponse(
                    ApiResponse<object>.Created(
                        data: null,
                        message: "Renewal invoice created successfully.",
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                return BuildResponse(
                    MapException<object>(ex));
            }
        }

      
        [HttpPost("invoice/{invoiceId:guid}/renewal-failure")]
        [Authorize]
        public async Task<IActionResult> HandleRenewalFailure(
            [FromRoute] Guid invoiceId)
        {
            if (invoiceId == Guid.Empty)
            {
                return BuildResponse(
                    ApiResponse<object>.BadRequest(
                        "invoiceId is required.",
                        traceId: HttpContext.TraceIdentifier));
            }

            try
            {
                await _subscriptionService
                    .HandleRenewalFailureAsync(invoiceId);

                return BuildResponse(
                    ApiResponse<object>.Ok(
                        data: null,
                        message: "Renewal payment failure recorded.",
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                return BuildResponse(
                    MapException<object>(ex));
            }
        }

      
        [HttpPost("invoice/{invoiceId:guid}/enter-grace-period")]
        [Authorize]
        public async Task<IActionResult> EnterGracePeriod(
            [FromRoute] Guid invoiceId)
        {
            if (invoiceId == Guid.Empty)
            {
                return BuildResponse(
                    ApiResponse<object>.BadRequest(
                        "invoiceId is required.",
                        traceId: HttpContext.TraceIdentifier));
            }

            try
            {
                await _subscriptionService
                    .EnterGracePeriodAsync(invoiceId);

                return BuildResponse(
                    ApiResponse<object>.Ok(
                        data: null,
                        message: "Subscription moved into grace period.",
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                return BuildResponse(
                    MapException<object>(ex));
            }
        }

     
        [HttpPost("{subscriptionId:guid}/suspend")]
        [Authorize]
        public async Task<IActionResult> SuspendSubscription(
            [FromRoute] Guid subscriptionId)
        {
            if (subscriptionId == Guid.Empty)
            {
                return BuildResponse(
                    ApiResponse<object>.BadRequest(
                        "subscriptionId is required.",
                        traceId: HttpContext.TraceIdentifier));
            }

            try
            {
                await _subscriptionService
                    .SuspendSubscriptionAsync(subscriptionId);

                return BuildResponse(
                    ApiResponse<object>.Ok(
                        data: null,
                        message: "Subscription suspended.",
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                return BuildResponse(
                    MapException<object>(ex));
            }
        }

      
        [HttpPost("{subscriptionId:guid}/expire")]
        [Authorize]
        public async Task<IActionResult> ExpireSubscription(
            [FromRoute] Guid subscriptionId)
        {
            if (subscriptionId == Guid.Empty)
            {
                return BuildResponse(
                    ApiResponse<object>.BadRequest(
                        "subscriptionId is required.",
                        traceId: HttpContext.TraceIdentifier));
            }

            try
            {
                await _subscriptionService
                    .ExpireSubscriptionAsync(subscriptionId);

                return BuildResponse(
                    ApiResponse<object>.Ok(
                        data: null,
                        message: "Subscription expired.",
                        traceId: HttpContext.TraceIdentifier));
            }
            catch (Exception ex)
            {
                return BuildResponse(
                    MapException<object>(ex));
            }
        }

        // =========================================================
        // RESPONSE HELPERS
        // =========================================================

        private IActionResult BuildResponse<T>(
            ApiResponse<T> response)
        {
            return StatusCode(
                response.StatusCode,
                response);
        }

        private ApiResponse<T> MapException<T>(
            Exception ex)
        {
            var traceId = HttpContext.TraceIdentifier;

            switch (ex)
            {
                case UnauthorizedAccessException:
                    return ApiResponse<T>.Forbidden(
                        "You are not authorized to perform this operation.",
                        traceId);

                case KeyNotFoundException:
                    return ApiResponse<T>.NotFound(
                        ex.Message,
                        traceId);

                case ArgumentException:
                    return ApiResponse<T>.BadRequest(
                        ex.Message,
                        traceId: traceId);

                case InvalidOperationException:
                    return ApiResponse<T>.Conflict(
                        ex.Message,
                        traceId: traceId);

                default:
                    return ApiResponse<T>.ServerError(
                        "An unexpected error occurred while processing the request.",
                        traceId);
            }
        }
    }

    // =============================================================
    // REQUEST MODELS
    // =============================================================

    public sealed class CancelSubscriptionRequest
    {
        public string? Reason { get; init; }
    }

    public sealed class PlanChangeRequest
    {
        [Required(ErrorMessage = "targetPlanId is required.")]
        public Guid TargetPlanId { get; init; }
    }
}
