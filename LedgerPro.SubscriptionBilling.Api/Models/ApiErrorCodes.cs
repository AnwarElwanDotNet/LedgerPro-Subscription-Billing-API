namespace LedgerPro.SubscriptionBilling.Api.Models.Responses
{
    public static class ApiErrorCodes
    {
        // =========================
        // General
        // =========================

        public const string BadRequest =
            "BAD_REQUEST";

        public const string ValidationError =
            "VALIDATION_ERROR";

        public const string Unauthorized =
            "UNAUTHORIZED";

        public const string Forbidden =
            "FORBIDDEN";

        public const string NotFound =
            "NOT_FOUND";

        public const string Conflict =
            "CONFLICT";

        public const string UnprocessableEntity =
            "UNPROCESSABLE_ENTITY";

        public const string TooManyRequests =
            "TOO_MANY_REQUESTS";

        public const string InternalServerError =
            "INTERNAL_SERVER_ERROR";

        public const string ServiceUnavailable =
            "SERVICE_UNAVAILABLE";

        // =========================
        // Validation
        // =========================

        public const string Required =
            "REQUIRED";

        public const string InvalidFormat =
            "INVALID_FORMAT";

        public const string InvalidValue =
            "INVALID_VALUE";

        // =========================
        // Authentication
        // =========================

        public const string InvalidCredentials =
            "INVALID_CREDENTIALS";

        public const string InvalidToken =
            "INVALID_TOKEN";

        public const string TokenExpired =
            "TOKEN_EXPIRED";

        // =========================
        // Business
        // =========================

        public const string BusinessRuleViolation =
            "BUSINESS_RULE_VIOLATION";

        public const string DuplicateResource =
            "DUPLICATE_RESOURCE";

        public const string OperationNotAllowed =
            "OPERATION_NOT_ALLOWED";
    }
}