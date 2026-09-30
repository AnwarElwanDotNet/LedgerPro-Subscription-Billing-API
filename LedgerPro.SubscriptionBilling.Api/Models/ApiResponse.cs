using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace LedgerPro.SubscriptionBilling.Api.Models.Responses
{
    public sealed class ApiResponse<T>
    {
        public bool Success { get; init; }

        public string Message { get; init; } = string.Empty;

        public T? Data { get; init; }

        public IReadOnlyList<ApiError> Errors { get; init; }
            = Array.Empty<ApiError>();

        public ApiMeta? Meta { get; init; }

        public int StatusCode { get; init; }

        public DateTimeOffset Timestamp { get; init; }

        public string TraceId { get; init; } = string.Empty;


        // =========================================================
        // SUCCESS
        // =========================================================

        public static ApiResponse<T> Ok(
            T data,
            string message = "Request successful",
            ApiMeta? meta = null,
            string? traceId = null)
        {
            return Create(
                success: true,
                message: message,
                data: data,
                statusCode: StatusCodes.Status200OK,
                errors: null,
                meta: meta,
                traceId: traceId);
        }


        // =========================================================
        // CREATED
        // =========================================================

        public static ApiResponse<T> Created(
            T data,
            string message = "Resource created successfully",
            string? traceId = null)
        {
            return Create(
                success: true,
                message: message,
                data: data,
                statusCode: StatusCodes.Status201Created,
                errors: null,
                meta: null,
                traceId: traceId);
        }


        // =========================================================
        // NO CONTENT
        // =========================================================

        public static ApiResponse<T> NoContent(
            string message = "Request completed successfully",
            string? traceId = null)
        {
            return Create(
                success: true,
                message: message,
                data: default,
                statusCode: StatusCodes.Status204NoContent,
                errors: null,
                meta: null,
                traceId: traceId);
        }


        // =========================================================
        // BAD REQUEST
        // =========================================================

        public static ApiResponse<T> BadRequest(
            string message = "Bad request",
            IEnumerable<ApiError>? errors = null,
            string? traceId = null)
        {
            return Create(
                success: false,
                message: message,
                data: default,
                statusCode: StatusCodes.Status400BadRequest,
                errors: errors,
                meta: null,
                traceId: traceId);
        }


        // =========================================================
        // VALIDATION
        // =========================================================

        public static ApiResponse<T> Validation(
            IEnumerable<ApiError> errors,
            string message = "Validation failed",
            string? traceId = null)
        {
            var errorList = NormalizeErrors(
                errors,
                ApiErrorCodes.ValidationError,
                message);

            return Create(
                success: false,
                message: message,
                data: default,
                statusCode: StatusCodes.Status400BadRequest,
                errors: errorList,
                meta: null,
                traceId: traceId);
        }


        // =========================================================
        // UNAUTHORIZED
        // =========================================================

        public static ApiResponse<T> Unauthorized(
            string message = "Authentication is required",
            string? traceId = null)
        {
            return Create(
                success: false,
                message: message,
                data: default,
                statusCode: StatusCodes.Status401Unauthorized,
                errors:
                [
                    new ApiError(
                        ApiErrorCodes.Unauthorized,
                        message)
                ],
                meta: null,
                traceId: traceId);
        }


        // =========================================================
        // FORBIDDEN
        // =========================================================

        public static ApiResponse<T> Forbidden(
            string message = "You do not have permission to perform this operation",
            string? traceId = null)
        {
            return Create(
                success: false,
                message: message,
                data: default,
                statusCode: StatusCodes.Status403Forbidden,
                errors:
                [
                    new ApiError(
                        ApiErrorCodes.Forbidden,
                        message)
                ],
                meta: null,
                traceId: traceId);
        }


        // =========================================================
        // NOT FOUND
        // =========================================================

        public static ApiResponse<T> NotFound(
            string message = "Resource not found",
            string? traceId = null)
        {
            return Create(
                success: false,
                message: message,
                data: default,
                statusCode: StatusCodes.Status404NotFound,
                errors:
                [
                    new ApiError(
                        ApiErrorCodes.NotFound,
                        message)
                ],
                meta: null,
                traceId: traceId);
        }


        // =========================================================
        // CONFLICT
        // =========================================================

        public static ApiResponse<T> Conflict(
            string message = "The request conflicts with the current state of the resource",
            IEnumerable<ApiError>? errors = null,
            string? traceId = null)
        {
            var errorList = NormalizeErrors(
                errors,
                ApiErrorCodes.Conflict,
                message);

            return Create(
                success: false,
                message: message,
                data: default,
                statusCode: StatusCodes.Status409Conflict,
                errors: errorList,
                meta: null,
                traceId: traceId);
        }


        // =========================================================
        // UNPROCESSABLE ENTITY
        // =========================================================

        public static ApiResponse<T> UnprocessableEntity(
            string message = "The request could not be processed",
            IEnumerable<ApiError>? errors = null,
            string? traceId = null)
        {
            var errorList = NormalizeErrors(
                errors,
                ApiErrorCodes.UnprocessableEntity,
                message);

            return Create(
                success: false,
                message: message,
                data: default,
                statusCode: StatusCodes.Status422UnprocessableEntity,
                errors: errorList,
                meta: null,
                traceId: traceId);
        }


        // =========================================================
        // TOO MANY REQUESTS
        // =========================================================

        public static ApiResponse<T> TooManyRequests(
            string message = "Too many requests",
            string? traceId = null)
        {
            return Create(
                success: false,
                message: message,
                data: default,
                statusCode: StatusCodes.Status429TooManyRequests,
                errors:
                [
                    new ApiError(
                        ApiErrorCodes.TooManyRequests,
                        message)
                ],
                meta: null,
                traceId: traceId);
        }


        // =========================================================
        // SERVER ERROR
        // =========================================================

        public static ApiResponse<T> ServerError(
            string message = "An unexpected error occurred",
            string? traceId = null)
        {
            return Create(
                success: false,
                message: message,
                data: default,
                statusCode: StatusCodes.Status500InternalServerError,
                errors:
                [
                    new ApiError(
                        ApiErrorCodes.InternalServerError,
                        message)
                ],
                meta: null,
                traceId: traceId);
        }


        // =========================================================
        // SERVICE UNAVAILABLE
        // =========================================================

        public static ApiResponse<T> ServiceUnavailable(
            string message = "The service is temporarily unavailable",
            string? traceId = null)
        {
            return Create(
                success: false,
                message: message,
                data: default,
                statusCode: StatusCodes.Status503ServiceUnavailable,
                errors:
                [
                    new ApiError(
                        ApiErrorCodes.ServiceUnavailable,
                        message)
                ],
                meta: null,
                traceId: traceId);
        }


        // =========================================================
        // INTERNAL CREATION
        // =========================================================

        private static ApiResponse<T> Create(
            bool success,
            string message,
            T? data,
            int statusCode,
            IEnumerable<ApiError>? errors,
            ApiMeta? meta,
            string? traceId)
        {
            return new ApiResponse<T>
            {
                Success = success,
                Message = message,
                Data = data,
                Errors = NormalizeErrors(errors),
                Meta = meta,
                StatusCode = statusCode,
                Timestamp = DateTimeOffset.UtcNow,
                TraceId = traceId
                    ?? Activity.Current?.Id
                    ?? string.Empty
            };
        }


        // =========================================================
        // ERROR NORMALIZATION
        // =========================================================

        private static IReadOnlyList<ApiError> NormalizeErrors(
            IEnumerable<ApiError>? errors,
            string? fallbackCode = null,
            string? fallbackMessage = null)
        {
            if (errors == null)
            {
                if (string.IsNullOrWhiteSpace(fallbackCode) &&
                    string.IsNullOrWhiteSpace(fallbackMessage))
                {
                    return Array.Empty<ApiError>();
                }

                return
                [
                    new ApiError(
                        fallbackCode ?? ApiErrorCodes.BadRequest,
                        fallbackMessage ?? "Request failed")
                ];
            }

            var list = errors
                .Where(error => error != null)
                .Select(error => new ApiError(
                    string.IsNullOrWhiteSpace(error.Code)
                        ? fallbackCode ?? ApiErrorCodes.BadRequest
                        : error.Code,
                    string.IsNullOrWhiteSpace(error.Message)
                        ? fallbackMessage ?? "Request failed"
                        : error.Message,
                    error.Field))
                .ToList();

            return list;
        }
    }
}