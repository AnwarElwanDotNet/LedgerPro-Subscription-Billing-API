
using BL.Contract;
using BL.Dtos;
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
    [Authorize(Roles = "Platform Admin,Customer Admin")]
    public class InvoiceApiController : ControllerBase
    {
        private readonly IInvoice _invoiceService;

        public InvoiceApiController(IInvoice invoiceService)
        {
            _invoiceService = invoiceService;
        }

        // ============================================================
        // GET BY ID
        // ============================================================

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(
                    ApiResponse<InvoiceDto>.BadRequest(
                        "Invoice ID is required."));
            }
            var invoice = await _invoiceService.GetByIdAsync(id);

            if (invoice == null)
            {
                return NotFound(
                    ApiResponse<InvoiceDto>.NotFound(
                        "Invoice was not found."));
            }

            return Ok(
                ApiResponse<InvoiceDto>.Ok(
                    invoice,
                    "Invoice retrieved successfully."));
        }

        // ============================================================
        // GET BY SUBSCRIPTION
        // ============================================================

        [HttpGet("subscription/{subscriptionId:guid}")]
        public async Task<IActionResult> GetBySubscriptionId(
            Guid subscriptionId)
        {
            if (subscriptionId == Guid.Empty)
            {
                return BadRequest(
                    ApiResponse<List<InvoiceDto>>.BadRequest(
                        "Subscription ID is required."));
            }
            var invoices =
                await _invoiceService.GetBySubscriptionIdAsync(
                    subscriptionId);

            var data = invoices.ToList();

            return Ok(
                ApiResponse<List<InvoiceDto>>.Ok(
                    data,
                    "Subscription invoices retrieved successfully."));
        }

        // ============================================================
        // GET BY ORGANIZATION
        // ============================================================

        [HttpGet("organization/{organizationId:guid}")]
        public async Task<IActionResult> GetByOrganizationId(
            Guid organizationId)
        {
            if (organizationId == Guid.Empty)
            {
                return BadRequest(
                    ApiResponse<List<InvoiceDto>>.BadRequest(
                        "Organization ID is required."));
            }
            var invoices =
                await _invoiceService.GetByOrganizationIdAsync(
                    organizationId);

            var data = invoices.ToList();

            return Ok(
                ApiResponse<List<InvoiceDto>>.Ok(
                    data,
                    "Organization invoices retrieved successfully."));
        }
    }
}

