using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using TL.BaseContracts;

namespace TL.MiddlewareLibrary.Models
{
    /// <summary>
    /// Modelo imutável de resposta de erro padronizado conforme a especificação RFC 7807 (ProblemDetails).
    /// </summary>
    public sealed record ProblemDetailsResponse
    {
        /// <summary>
        /// URI de referência que identifica o tipo do problema.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; init; }

        /// <summary>
        /// Resumo legível do tipo de problema.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; init; }

        /// <summary>
        /// Código de status HTTP gerado pelo servidor para esta ocorrência.
        /// </summary>
        [JsonPropertyName("status")]
        public int Status { get; init; }

        /// <summary>
        /// Explicação detalhada específica para esta ocorrência do problema.
        /// </summary>
        [JsonPropertyName("detail")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Detail { get; init; }

        /// <summary>
        /// Referência URI que identifica a ocorrência específica do problema (ex: caminho do endpoint).
        /// </summary>
        [JsonPropertyName("instance")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Instance { get; init; }

        /// <summary>
        /// Identificador único de correlação e rastreabilidade da requisição.
        /// </summary>
        [JsonPropertyName("traceId")]
        public string TraceId { get; init; }

        /// <summary>
        /// Código padronizado e legível por máquina do erro de negócio ou de infraestrutura.
        /// </summary>
        [JsonPropertyName("code")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Code { get; init; }

        /// <summary>
        /// Dicionário de mensagens de erro detalhadas agrupadas por campo quando for uma falha de validação.
        /// </summary>
        [JsonPropertyName("errors")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IReadOnlyDictionary<string, string[]>? Errors { get; init; }

        /// <summary>
        /// Construtor de inicialização do ProblemDetailsResponse.
        /// </summary>
        public ProblemDetailsResponse(
            string type,
            string title,
            int status,
            string? detail,
            string? instance,
            string traceId,
            string? code = null,
            IReadOnlyDictionary<string, string[]>? errors = null)
        {
            Type = type ?? "https://tools.ietf.org/html/rfc9110#section-15.6.1";
            Title = title ?? "Internal Server Error";
            Status = status;
            Detail = detail;
            Instance = instance;
            TraceId = traceId ?? string.Empty;
            Code = code;
            Errors = errors;
        }

        /// <summary>
        /// Cria uma instância de ProblemDetailsResponse com base em parâmetros primitivos.
        /// </summary>
        public static ProblemDetailsResponse Create(
            int statusCode,
            string title,
            string? detail,
            string? instance,
            string traceId,
            string? code = null,
            IReadOnlyDictionary<string, string[]>? errors = null)
        {
            var typeUri = ResolveRfcTypeUri(statusCode);
            return new ProblemDetailsResponse(typeUri, title, statusCode, detail, instance, traceId, code, errors);
        }

        /// <summary>
        /// Cria uma resposta de erro estruturada a partir de um contrato <see cref="Error"/> de TL.BaseContracts.
        /// </summary>
        public static ProblemDetailsResponse FromError(
            Error error,
            string? instance,
            string traceId,
            int? overrideStatusCode = null)
        {
            ArgumentNullException.ThrowIfNull(error);

            if (error is ValidationError validationError)
            {
                return FromValidationError(validationError, instance, traceId);
            }

            int statusCode = overrideStatusCode ?? ResolveStatusCodeFromErrorType(error.Type);
            string title = ResolveTitleFromStatusCode(statusCode);

            return Create(
                statusCode,
                title,
                error.Message,
                instance,
                traceId,
                string.IsNullOrWhiteSpace(error.Code) ? null : error.Code);
        }

        /// <summary>
        /// Cria uma resposta estruturada de validação a partir de um contrato <see cref="ValidationError"/> de TL.BaseContracts.
        /// </summary>
        public static ProblemDetailsResponse FromValidationError(
            ValidationError validationError,
            string? instance,
            string traceId)
        {
            ArgumentNullException.ThrowIfNull(validationError);

            const int statusCode = 400;
            return Create(
                statusCode,
                "Bad Request",
                validationError.Message,
                instance,
                traceId,
                string.IsNullOrWhiteSpace(validationError.Code) ? "Validation.General" : validationError.Code,
                validationError.Errors);
        }

        /// <summary>
        /// Cria uma resposta de erro a partir de uma exceção não tratada.
        /// </summary>
        public static ProblemDetailsResponse FromException(
            Exception exception,
            string? instance,
            string traceId)
        {
            ArgumentNullException.ThrowIfNull(exception);

            const int statusCode = 500;
            return Create(
                statusCode,
                "Internal Server Error",
                "Ocorreu um erro interno inesperado ao processar a solicitação.",
                instance,
                traceId,
                "Server.InternalError");
        }

        private static int ResolveStatusCodeFromErrorType(ErrorType errorType) => errorType switch
        {
            ErrorType.Validation => 400,
            ErrorType.Unauthorized => 401,
            ErrorType.Forbidden => 403,
            ErrorType.NotFound => 404,
            ErrorType.Conflict => 409,
            _ => 500
        };

        private static string ResolveTitleFromStatusCode(int statusCode) => statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            406 => "Not Acceptable",
            408 => "Request Timeout",
            409 => "Conflict",
            415 => "Unsupported Media Type",
            423 => "Locked",
            429 => "Too Many Requests",
            500 => "Internal Server Error",
            502 => "Bad Gateway",
            503 => "Service Unavailable",
            504 => "Gateway Timeout",
            507 => "Insufficient Storage",
            _ => "Error"
        };

        private static string ResolveRfcTypeUri(int statusCode) => statusCode switch
        {
            400 => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            401 => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
            403 => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            404 => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            406 => "https://tools.ietf.org/html/rfc9110#section-15.5.7",
            408 => "https://tools.ietf.org/html/rfc9110#section-15.5.9",
            409 => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
            415 => "https://tools.ietf.org/html/rfc9110#section-15.5.16",
            423 => "https://tools.ietf.org/html/rfc4918#section-11.2",
            429 => "https://tools.ietf.org/html/rfc6585#section-4",
            502 => "https://tools.ietf.org/html/rfc9110#section-15.6.3",
            504 => "https://tools.ietf.org/html/rfc9110#section-15.6.5",
            507 => "https://tools.ietf.org/html/rfc4918#section-11.5",
            _ => "https://tools.ietf.org/html/rfc9110#section-15.6.1"
        };
    }
}
