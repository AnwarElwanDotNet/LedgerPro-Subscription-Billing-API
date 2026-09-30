using BL.Dtos;
using Domain;
using System;
using System.Threading.Tasks;

namespace BL.Contract
{

    public interface ISubscription : IBaseService<TbSubscription, SubscriptionDto>
    {

        Task CreateSubscriptionAsync(SubscriptionDto dto);

        Task<SubscriptionDto> GetSubscriptionByIdAsync(Guid subscriptionId);

        Task<SubscriptionDto> GetSubscriptionByOrganizationIdAsync(Guid organizationId);

        Task CancelSubscriptionAsync(Guid subscriptionId, string reason = null);

        Task CancelSubscriptionImmediatelyAsync(Guid subscriptionId, string reason = null);

        Task UpgradeSubscriptionAsync(Guid subscriptionId, Guid targetPlanId);

        Task RequestDowngradeAsync(Guid subscriptionId, Guid targetPlanId);

        Task<bool> CheckEntitlementAsync(Guid organizationId);


        // Payment / System Workflow

        Task ActivateSubscriptionAsync(Guid invoiceId);

        Task HandleInitialPaymentFailureAsync(Guid invoiceId);

        Task ApplySuccessfulUpgradeAsync(Guid invoiceId);

        Task CompleteSuccessfulRenewalAsync(Guid invoiceId);

        Task RenewSubscriptionAsync(Guid subscriptionId);

        Task HandleRenewalFailureAsync(Guid invoiceId);

        Task EnterGracePeriodAsync(Guid invoiceId);

        Task SuspendSubscriptionAsync(Guid subscriptionId);

        Task ExpireSubscriptionAsync(Guid subscriptionId);
    }
}