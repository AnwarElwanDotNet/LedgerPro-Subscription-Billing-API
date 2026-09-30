# LedgerPro Subscription and Billing API

LedgerPro Subscription and Billing API is a backend-only REST API responsible for managing the commercial subscription and billing lifecycle of LedgerPro, a fictional cloud accounting SaaS product.

The system manages plans, pricing, customer organizations, trials, subscriptions, billing periods, invoices, payments, payment attempts, renewals, retry policies, grace periods, suspension, plan changes, cancellation, refunds, notifications, auditing, authentication, authorization, and entitlement decisions.

The system follows a prepaid subscription model:

> **No successful payment means no active paid subscription, which means no paid access.**

---

## Table of Contents

1. [System Scope](#1-system-scope)
2. [Actors and Access](#2-actors-and-access)
3. [Core Domain Concepts](#3-core-domain-concepts)
4. [Functional Requirements](#4-functional-requirements)
5. [Subscription Lifecycle](#5-subscription-lifecycle)
6. [Invoice and Payment Lifecycle](#6-invoice-and-payment-lifecycle)
7. [Billing and Subscription Policies](#7-billing-and-subscription-policies)
8. [Entitlement and User Limits](#8-entitlement-and-user-limits)
9. [Notifications and Audit](#9-notifications-and-audit)
10. [Historical Financial Integrity](#10-historical-financial-integrity)
11. [Authentication and Authorization](#11-authentication-and-authorization)
12. [Technical Requirements](#12-technical-requirements)
13. [API Requirements](#13-api-requirements)
14. [Business Rules](#14-business-rules)
15. [Implementation Constraints](#15-implementation-constraints)

---

# 1. System Scope

## 1.1 In Scope

The API is responsible for the commercial side of LedgerPro, including:

* Plans and pricing
* Customer organizations
* Trial periods
* Subscriptions
* Billing periods
* Subscription invoices
* Payments and payment attempts
* Renewals
* Retry policies
* Grace periods
* Subscription suspension and expiration
* Subscription upgrades and downgrades
* Cancellation
* Refunds
* Notifications
* Audit logging
* Authentication and authorization
* Entitlement decisions
* User limits

The API is backend-only and does not provide a customer-facing MVC application, mobile application, or other user interface.

## 1.2 Out of Scope

The following are explicitly outside the system scope:

* General accounting
* General ledger
* Journal entries
* Accounting reports
* Inventory
* Payroll
* Taxes
* Customer invoices issued by LedgerPro customers to their own customers
* MVC or other UI applications
* Mobile applications

## 1.3 Subscription Invoice Definition

An invoice within LedgerPro represents a subscription or billing invoice issued by LedgerPro for the use of LedgerPro itself.

It is not an accounting invoice issued by a LedgerPro customer to that customer's customers.

---

# 2. Actors and Access

## 2.1 Platform Administrator

The Platform Administrator may:

* Manage plans.
* Activate or deactivate plans.
* Configure pricing and trial rules.
* Review customer organizations.
* Review subscriptions, invoices, payments, and failed payment attempts.
* Process eligible refunds.
* Review audit logs.
* Manage platform settings.
* Perform authorized administrative operations, including immediate cancellation where permitted.

## 2.2 Customer Admin

A Customer Admin operates within a customer organization and may:

* View the organization's subscription.
* View available plans.
* Start subscription operations.
* Manage customer users within the applicable plan limit.
* Request an upgrade.
* Request a downgrade.
* Request cancellation.
* View invoices.
* View payment history.

## 2.3 Customer User

A Customer User has restricted access within their own organization.

A Customer User must never be able to access another organization's subscription, invoice, payment, or user data, including by changing an identifier supplied in a request.

Ownership and organization access must always be verified server-side.

## 2.4 Payment Provider

Payment processing may depend on an external payment provider, including a Sandbox or Mock provider during development.

The system must not assume that every payment attempt succeeds.

---

# 3. Core Domain Concepts

## 3.1 Plan

A Plan represents a commercial offer and contains:

* Name
* Description
* Price
* Currency
* Duration
* Maximum number of users
* Active or inactive status
* Creation information
* Update information

A Plan is not a Subscription.

## 3.2 Subscription

A Subscription represents the commercial entitlement of a specific customer organization to use LedgerPro under a selected Plan.

A subscription is governed by its lifecycle, billing periods, payment state, plan changes, and applicable cancellation rules.

## 3.3 Billing Period

A Billing Period represents one paid service period.

The default duration is exactly 30 days and is calculated from the exact timestamp rather than from calendar months.

Each billing period must remain historically traceable.

## 3.4 Invoice

An Invoice represents an amount payable for a subscription transaction.

An invoice is created before payment succeeds and initially has a `Pending` status.

## 3.5 Payment

A Payment represents the financial payment associated with an invoice.

A successful payment is the condition required to activate a pending paid subscription.

## 3.6 Payment Attempt

A Payment Attempt represents an individual attempt to process a payment, including failed attempts.

Payment attempts must remain traceable independently of whether the attempt ultimately succeeds or fails.

## 3.7 Refund

A Refund represents money returned against a previous successful payment.

Refund processing is independent of cancellation.

## 3.8 Trial

A Trial represents a temporary access period that does not require a successful paid transaction.

## 3.9 Grace Period

A Grace Period represents a temporary billing state following exhausted renewal retries.

It is a billing policy and is not an additional paid billing period.

---

# 4. Functional Requirements

## 4.1 Plans

The initial seed plans are:

| Plan         | Price | Currency | Duration | Maximum Users |
| ------------ | ----: | -------- | -------: | ------------: |
| Basic        |    10 | USD      |  30 days |             3 |
| Professional |    25 | USD      |  30 days |            10 |
| Enterprise   |    60 | USD      |  30 days |            25 |

These values are seed data and must not be hard-coded into business logic.

### Plan Rules

* A new paid subscription may use only an active plan.
* An inactive plan cannot be selected for a new subscription.
* Deactivating a plan does not automatically affect existing subscriptions.
* A plan price change affects future transactions only.
* Historical transactions must retain the price applicable when they were created.
* Current plan configuration must never rewrite historical financial records.

---

## 4.2 Trials

Trials are optional and configurable.

The default trial duration is exactly 14 days.

The system must:

* Calculate the trial from its exact start timestamp.
* Provide temporary access without requiring a successful paid transaction.
* End access at the exact trial end timestamp.
* Never automatically convert the trial into a paid subscription.
* Require a paid subscription to continue paid access after the trial ends.
* Verify trial eligibility before starting a trial.
* Prevent unrestricted repeated trials for the same product within the same organization.
* Maintain a record of the trial period.

---

## 4.3 Subscriptions

A subscription begins in `PendingPayment` when payment is required before activation.

A paid subscription may become active only after confirmed payment success.

The system must not activate a subscription because:

* A plan was selected.
* An invoice was created.
* A subscription record was created.
* A payment attempt was created.
* A payment is pending.
* A payment failed.

Only a confirmed successful payment may activate a pending paid subscription.

---

## 4.4 Invoices

An invoice must:

* Exist before payment.
* Initially have `Pending` status.
* Preserve its transaction amount.
* Preserve its currency.
* Preserve its plan context.
* Preserve its billing-period context.
* Preserve its creation timestamp.
* Remain historically accurate after future plan changes.
* Become `Paid` only when a corresponding successful payment exists.
* Remain financially immutable after finalization except through explicit supported financial workflows.

A pending invoice grants no paid access.

---

## 4.5 Payments

Payment statuses are:

```text
Pending
Succeeded
Failed
Cancelled
Refunded
PartiallyRefunded
```

The system must:

* Match the payment amount to the invoice amount unless partial payment is explicitly supported.
* Prevent failed payments from activating subscriptions.
* Allow only successful payments to activate pending subscriptions.
* Store the payment provider reference.
* Preserve payment history for reconciliation and auditing.
* Prevent duplicate payment callbacks from producing duplicate financial effects.
* Process repeatable financial operations idempotently.
* Prevent refunds from exceeding the refundable amount.

---

## 4.6 Payment Attempts

The system must maintain traceability of payment attempts, including unsuccessful attempts.

Payment attempts must support the billing lifecycle by allowing the system to:

* Record each payment processing attempt.
* Distinguish failed attempts from successful financial outcomes.
* Support retry processing.
* Preserve payment-provider references where applicable.
* Prevent repeated processing from creating duplicate financial effects.

A failed payment attempt must never by itself create paid entitlement.

---

# 5. Subscription Lifecycle

## 5.1 Subscription Statuses

```text
PendingPayment
PaymentFailed
Active
ActiveUntilPeriodEnd
PastDue
GracePeriod
Suspended
Cancelled
Expired
```

### Status Definitions

| Status                 | Meaning                                                                        |
| ---------------------- | ------------------------------------------------------------------------------ |
| `PendingPayment`       | Payment is required and paid access is not available.                          |
| `PaymentFailed`        | The initial payment failed and paid access is not available.                   |
| `Active`               | Paid access is currently allowed.                                              |
| `ActiveUntilPeriodEnd` | Cancellation has been requested and the current paid period remains valid.     |
| `PastDue`              | A renewal payment failed and the system is awaiting retry processing.          |
| `GracePeriod`          | Renewal retries have been exhausted and the configured grace period is active. |
| `Suspended`            | The payment obligation was not resolved during the grace period.               |
| `Cancelled`            | The subscription was explicitly cancelled.                                     |
| `Expired`              | Subscription entitlement has ended permanently.                                |

## 5.2 Primary Lifecycle

```text
PendingPayment
      |
      | Successful payment
      v
   Active
      |
      | Period-end cancellation requested
      v
ActiveUntilPeriodEnd
      |
      | Period ends
      v
  Expired
```

## 5.3 Initial Payment Failure

```text
PendingPayment
      |
      | Payment fails
      v
PaymentFailed
```

An initial payment failure never grants paid access.

## 5.4 Renewal Failure

```text
Active
   |
   | Renewal payment fails
   v
PastDue
   |
   | Retries exhausted
   v
GracePeriod
   |
   | Grace period ends without successful payment
   v
Suspended
   |
   | Final expiry rule
   v
Expired
```

## 5.5 Immediate Cancellation

Authorized immediate cancellation may result in:

```text
Active -> Cancelled
```

Immediate cancellation is exceptional and subject to authorization and audit requirements.

## 5.6 State Transition Protection

The system must enforce business workflows rather than allowing arbitrary client-controlled status changes.

In particular:

* `PendingPayment -> Active` requires confirmed successful payment.
* `PendingPayment -> Refunded` is invalid.
* `Expired -> Active` must never occur through a generic update operation.
* Clients must not directly submit arbitrary subscription statuses to bypass business rules.
* State transitions must occur through authorized business operations.

---

# 6. Invoice and Payment Lifecycle

## 6.1 Invoice Statuses

```text
Pending
Paid
PaymentFailed
Void
Refunded
PartiallyRefunded
```

## 6.2 Payment Statuses

```text
Pending
Succeeded
Failed
Cancelled
Refunded
PartiallyRefunded
```

## 6.3 Payment Processing

Payment processing must be idempotent.

Duplicate callbacks or repeated payment notifications must not:

* Activate the same subscription more than once.
* Apply the same financial effect more than once.
* Create overlapping paid periods.
* Create duplicate financial records for the same provider event.

A payment provider reference must be retained for reconciliation.

## 6.4 Successful Payment Effects

A confirmed successful payment must produce the required financial state transition atomically.

Where the payment represents the initial payment for a pending subscription:

```text
Payment = Succeeded
Invoice = Paid
Subscription = Active
```

The related state changes must remain transactionally consistent.

---

# 7. Billing and Subscription Policies

## 7.1 Retry Policy

The default retry schedule is:

| Attempt         | Day |
| --------------- | --: |
| Initial attempt |   0 |
| Retry 1         |   1 |
| Retry 2         |   3 |
| Retry 3         |   5 |

The retry schedule is configurable and must not be treated as an immutable hard-coded business rule.

## 7.2 Grace Period

The default grace period is 3 days and is configurable.

The system must record:

* Grace period start
* Grace period end
* Reason
* Related invoice
* Related subscription

The grace period is not an additional paid billing period.

Whether paid access remains available during the grace period depends on the configured entitlement policy.

## 7.3 Renewal

A renewal:

* Continues the existing subscription.
* Creates another billing period.
* Does not create a new customer relationship.
* Uses the plan price applicable when the new billing transaction is created.
* Permanently records the transaction price.
* Must not create overlapping paid periods.
* Requires successful payment for continued paid entitlement.

The subscription history must preserve all billing periods and renewals.

## 7.4 Upgrade

An upgrade:

* Takes effect immediately.
* Keeps the same subscription.
* Applies the new plan immediately.
* Supports proration.
* Calculates proration using the exact timestamps and remaining time in the current period.
* Uses an explicit financial rounding rule.
* Does not rewrite historical billing records.

## 7.5 Downgrade

A downgrade:

* Takes effect at the beginning of the next billing period.
* Does not change the current period immediately.
* Preserves the current plan for the remainder of the current period.
* Records the requested future plan change.
* Must not rewrite historical billing records.

## 7.6 Cancellation

The default cancellation behavior is period-end cancellation.

Cancellation:

* Requires authentication.
* Requires authorization.
* Records the cancellation request.
* Records its timestamp.
* May record a reason.
* Does not automatically imply a refund.
* Must not silently delete the subscription.
* Must preserve historical billing records.

### Immediate Cancellation

Immediate cancellation is limited to authorized administrative operations or exceptional cases.

When supported, the system must record:

* Who performed the cancellation.
* When it was performed.
* The reason.
* Whether access ended immediately.
* Whether refund processing was initiated.

## 7.7 Refunds

A refund:

* Must reference a previous successful payment.
* Must not exceed the refundable amount.
* Is independent of cancellation.
* Must be idempotent.
* Must be auditable.
* Must not silently modify the original payment amount.
* Must distinguish full and partial refund outcomes.

---

# 8. Entitlement and User Limits

## 8.1 Entitlement

The entitlement decision must consider at least:

* Organization
* Subscription
* Subscription status
* Current time
* Current billing period
* Successful payment state
* Plan
* Applicable limits

### No Paid Entitlement

Paid entitlement does not exist when:

* The initial payment failed.
* The subscription is suspended.
* The subscription is expired.
* The subscription is cancelled and its paid period has ended.

### Potential Entitlement

Entitlement may exist during:

* `Active`
* `ActiveUntilPeriodEnd`
* A valid `Trial`
* `GracePeriod`, when the configured policy allows access

## 8.2 User Limits

The maximum user limit belongs to the selected plan:

| Plan         | Maximum Users |
| ------------ | ------------: |
| Basic        |             3 |
| Professional |            10 |
| Enterprise   |            25 |

The system must:

* Prevent activation of a new user when the organization has reached its plan limit.
* Require an upgrade when additional capacity is needed.
* Count currently active users only.
* Treat removal or deactivation of a user as freeing a seat.
* Enforce the limit server-side.
* Protect the limit against concurrent additions.

---

# 9. Notifications and Audit

## 9.1 Notifications

The system must create notifications for the following billing events:

1. Trial started
2. Trial approaching its end
3. Trial ended
4. Invoice created
5. Payment succeeded
6. Payment failed
7. Renewal approaching
8. Renewal succeeded
9. Subscription entered grace period
10. Subscription suspended
11. Subscription expired
12. Cancellation requested
13. Plan changed
14. Refund processed

Notification delivery may use a separate delivery mechanism such as email.

Financial state must never depend on successful notification delivery.

A failed notification must not stop or alter a financial operation.

## 9.2 Audit Logging

Security-sensitive and business-critical operations must be auditable, including:

* Plan creation
* Plan update
* Plan deactivation
* Subscription creation
* Subscription activation
* Subscription cancellation
* Subscription upgrade
* Subscription downgrade
* Payment success
* Payment failure
* Refund creation
* User addition
* User removal
* Administrative overrides

Audit records must contain:

* Actor
* Action
* Entity type
* Entity identifier
* Timestamp
* Relevant metadata
* Trace or correlation identifier where available

Audit records must not expose secrets or sensitive authentication credentials.

---

# 10. Historical Financial Integrity

LedgerPro must preserve historical financial truth.

For example:

```text
Professional plan price at transaction time: 25 USD

Later:
Professional plan price changes to 30 USD

Historical invoice:
25 USD
```

The historical invoice must remain 25 USD.

Financial transactions must store the values applicable when the transaction occurred rather than relying only on the current state of the related plan.

Historical transaction data includes, where applicable:

* Amount
* Currency
* Billing period context
* Selected plan context
* Refund amount
* Payment provider reference
* Transaction timestamps

Historical financial records must not be rewritten when current plan configuration changes.

Historical invoices, payments, and refunds must remain auditable.

---

# 11. Authentication and Authorization

## 11.1 Authentication

Authentication is token-based and must support:

* Secure login
* Password hashing
* Access tokens with expiration
* Refresh token mechanism
* Logout and token revocation
* Secure password rules

Passwords must never be stored or exposed as plain text.

## 11.2 Authorization

Authorization is role-based with three permission groups:

* Platform Administrator
* Customer Admin
* Customer User

## 11.3 Multi-Tenancy Isolation

Customer-owned data must be associated with the correct organization.

Ownership verification must occur server-side.

A customer user must not be able to access another organization's data by manipulating identifiers supplied in requests.

## 11.4 Sensitive Information

The API must never expose:

* Password hashes
* Refresh token secrets
* Payment credentials
* Internal exception details
* Sensitive payment provider credentials

---

# 12. Technical Requirements

## 12.1 Monetary Values

Financial calculations must:

* Use `decimal`.
* Use explicit precision and scale.
* Never use `float` or `double` for financial calculations.
* Preserve finalized historical amounts.
* Change finalized financial values only through explicit supported correction or refund workflows.

## 12.2 Time

All timestamps must use UTC.

Billing calculations must use:

```text
Start timestamp + exact configured number of days
```

rather than calendar-month calculations.

For example:

```text
2026-01-31 10:15:00 + 30 days
=
2026-03-02 10:15:00
```

## 12.3 Transactional Consistency

Financial state changes must preserve transactional consistency.

Operations that require atomic financial state changes must be executed transactionally.

For example, a successful initial payment and its corresponding invoice and subscription state changes must not leave the system in a partially updated financial state.

## 12.4 Idempotency

Financial operations must be idempotent wherever requests, callbacks, or notifications may be repeated.

Repeated processing must not duplicate financial effects.

---

# 13. API Requirements

## 13.1 Response Contract

API responses use a standardized response structure containing:

```json
{
  "success": true,
  "message": "Request successful",
  "data": {},
  "errors": [],
  "traceId": "..."
}
```

The response contract must support:

* Success indication
* Human-readable message
* Response data
* Structured errors
* Trace or correlation information

## 13.2 HTTP Status Codes

The API uses standard HTTP status codes, including:

| Category      | Status Codes                 |
| ------------- | ---------------------------- |
| Success       | 200, 201, 204                |
| Client errors | 400, 401, 403, 404, 409, 422 |
| Server error  | 500                          |

## 13.3 Authentication Operations

The authentication API must support the required authentication lifecycle, including:

* Registration
* Login
* Token refresh
* Logout and revocation
* Password reset
* Current authenticated-user access
* Profile management
* Email change
* Password change

Authentication operations must respect the defined authentication and authorization requirements.

---

# 14. Business Rules

| ID     | Rule                                                                                            |
| ------ | ----------------------------------------------------------------------------------------------- |
| BR-001 | An active plan must be selected to start a paid subscription.                                   |
| BR-002 | An invoice is created before payment.                                                           |
| BR-003 | A pending invoice grants no paid access.                                                        |
| BR-004 | A subscription becomes active only after successful payment is confirmed.                       |
| BR-005 | An initial payment failure never grants paid access.                                            |
| BR-006 | The paid period equals the configured duration exactly, with 30 days as the default.            |
| BR-007 | Billing duration is timestamp-based, not calendar-month-based.                                  |
| BR-008 | Historical billing amounts remain unchanged after future plan price changes.                    |
| BR-009 | A successful payment marks the related invoice as paid.                                         |
| BR-010 | A refund cannot exceed the refundable amount.                                                   |
| BR-011 | Cancellation does not imply an automatic refund.                                                |
| BR-012 | Standard cancellation takes effect at the end of the current paid period.                       |
| BR-013 | Upgrades take effect immediately.                                                               |
| BR-014 | Downgrades take effect in the next billing period.                                              |
| BR-015 | Immediate upgrades support proration according to the configured calculation and rounding rule. |
| BR-016 | Renewal requires successful payment for continued entitlement.                                  |
| BR-017 | Failed renewal payments enter the configured retry flow.                                        |
| BR-018 | Exhausted renewal retries lead to the configured grace period.                                  |
| BR-019 | A grace period ending without successful payment leads to suspension.                           |
| BR-020 | A suspended subscription has no paid entitlement.                                               |
| BR-021 | A subscription cannot be activated through a generic client status change.                      |
| BR-022 | Financial operations are idempotent wherever repeated processing is possible.                   |
| BR-023 | Customer users can access only their own organization's data.                                   |
| BR-024 | The server enforces the plan's user limit.                                                      |
| BR-025 | Removing or deactivating a user frees the consumed seat.                                        |
| BR-026 | Trial access ends at the exact trial end timestamp.                                             |
| BR-027 | A trial does not automatically convert to a paid subscription.                                  |
| BR-028 | Unlimited repeated trials for the same product within an organization are not permitted.        |
| BR-029 | Historical invoices, payments, and refunds remain auditable.                                    |
| BR-030 | A refund must reference an original successful payment.                                         |
| BR-031 | Duplicate payment notifications must not repeat financial effects.                              |
| BR-032 | A successful renewal must not create overlapping paid periods.                                  |
| BR-033 | Current plan configuration changes must not rewrite historical billing records.                 |
| BR-034 | Billing calculations use UTC timestamps consistently.                                           |
| BR-035 | Financial state changes must preserve transactional consistency.                                |

---

# 15. Implementation Constraints

The implementation must respect the following constraints:

1. Core business rules must not be placed in API controllers.
2. Business logic must remain separated from data access.
3. Data access must remain separated from business logic.
4. No table or data structure should be introduced merely to increase project size.
5. Every implemented feature must have a corresponding documented business requirement.
6. Subscription and financial state transitions must be controlled by business workflows rather than unrestricted generic status updates.
7. Financial operations must use transactions wherever transactional consistency requires them.
8. Historical financial information must not be rewritten by changes to current commercial configuration.
9. Server-side authorization and ownership validation must not depend on client-supplied organization identifiers.
10. Sensitive credentials and internal security information must never be exposed through API responses.

Before introducing any additional data required by the system, the following must be established:

* Which business requirement requires the data?
* What is the business meaning of the data?
* Is the capability part of the defined LedgerPro commercial domain?

---

# Requirement Integrity Principle

The system must remain strictly aligned with the defined LedgerPro commercial requirements.

Business requirements must take precedence over implementation convenience.

Database entities, fields, relationships, APIs, services, and supporting structures must exist because they support an actual documented business requirement, not merely because they increase implementation complexity or project size.

The final implementation must preserve the core principles of LedgerPro:

* Successful payment is required for paid entitlement.
* Financial history must remain accurate and immutable.
* Subscription state transitions must follow controlled business workflows.
* Payment processing must be idempotent.
* Customer data must remain isolated by organization.
* User limits must be enforced server-side.
* Trial, billing, renewal, cancellation, and refund behavior must follow their defined business rules.
* Notifications must remain independent from financial state.
* Security-sensitive and business-critical operations must remain auditable.
