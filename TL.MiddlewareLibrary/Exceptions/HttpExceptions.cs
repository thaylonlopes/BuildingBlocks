using System;
using TL.BaseContracts;

namespace TL.MiddlewareLibrary.Exceptions
{
    /// <summary>
    /// Exceção base de requisição inválida (HTTP 400 Bad Request).
    /// </summary>
    public class BadRequestException : Exception
    {
        /// <summary>
        /// Obtém o contrato de erro associado, se disponível.
        /// </summary>
        public Error? Error { get; }

        public BadRequestException(string message) : base(message) { }
        public BadRequestException(string message, Exception innerException) : base(message, innerException) { }
        public BadRequestException(Error error) : base(error?.Message)
        {
            Error = error;
        }
    }

    /// <summary>
    /// Exceção especializada para falhas de validação de dados de entrada com detalhamento por campo (HTTP 400).
    /// </summary>
    public class ValidationException : BadRequestException
    {
        /// <summary>
        /// Obtém o contrato de validação detalhado.
        /// </summary>
        public ValidationError? ValidationError => Error as ValidationError;

        public ValidationException(string message) : base(message) { }
        public ValidationException(ValidationError validationError) : base(validationError) { }
        public ValidationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exceção para falhas de autenticação de credenciais (HTTP 401 Unauthorized).
    /// </summary>
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
        public UnauthorizedException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exceção para acessos negados por falta de permissão (HTTP 403 Forbidden).
    /// </summary>
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
        public ForbiddenException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exceção para recursos não encontrados no servidor (HTTP 404 Not Found).
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exceção para tipos de conteúdo não aceitáveis pelo cliente (HTTP 406 Not Acceptable).
    /// </summary>
    public class NotAcceptableException : Exception
    {
        public NotAcceptableException(string message) : base(message) { }
        public NotAcceptableException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exceção para conflito de estado ou duplicidade de recurso (HTTP 409 Conflict).
    /// </summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
        public ConflictException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exceção para formato de mídia não suportado no payload (HTTP 415 Unsupported Media Type).
    /// </summary>
    public class UnsupportedMediaTypeException : Exception
    {
        public UnsupportedMediaTypeException(string message) : base(message) { }
        public UnsupportedMediaTypeException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exceção para recursos bloqueados contra edição concorrente (HTTP 423 Locked).
    /// </summary>
    public class LockedException : Exception
    {
        public LockedException(string message) : base(message) { }
        public LockedException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exceção para estouro do limite de requisições por IP ou cliente (HTTP 429 Too Many Requests).
    /// </summary>
    public class TooManyRequestsException : Exception
    {
        public TooManyRequestsException(string message) : base(message) { }
        public TooManyRequestsException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exceção para respostas inválidas de serviços upstream (HTTP 502 Bad Gateway).
    /// </summary>
    public class BadGatewayException : Exception
    {
        public BadGatewayException(string message) : base(message) { }
        public BadGatewayException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exceção para expiração do tempo de resposta de gateway ou proxy upstream (HTTP 504 Gateway Timeout).
    /// </summary>
    public class GatewayTimeoutException : Exception
    {
        public GatewayTimeoutException(string message) : base(message) { }
        public GatewayTimeoutException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exceção para insuficiência de armazenamento em disco no servidor (HTTP 507 Insufficient Storage).
    /// </summary>
    public class InsufficientStorageException : Exception
    {
        public InsufficientStorageException(string message) : base(message) { }
        public InsufficientStorageException(string message, Exception innerException) : base(message, innerException) { }
    }
}
